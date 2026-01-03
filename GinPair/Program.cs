using Serilog;
using Serilog.Events;

namespace GinPair;

public class Program {
    /// <summary>
    /// Application entry point that configures logging, telemetry, and runs the web host.
    /// Returns 0 when the application starts and runs successfully, and 1 when a fatal error occurs.
    /// This convention is important for deployment scenarios and health monitoring.
    /// </summary>
    /// <param name="args">Command line arguments passed to the application.</param>
    /// <returns>0 for successful execution; 1 when the application terminates with an error.</returns>
    public static int Main(string[] args) {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try {
            Log.Information("Starting web application");

            var builder = WebApplication.CreateBuilder(args);

            // Configure Application Insights
            string? appInsightsConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");
            if (!string.IsNullOrEmpty(appInsightsConnectionString)) {
                string cloudRoleName = builder.Configuration["ApplicationInsights:CloudRoleName"] ?? "GinPair";
                
                builder.Services.AddSingleton<Microsoft.ApplicationInsights.Extensibility.ITelemetryInitializer>(
                    new Infrastructure.CloudRoleNameTelemetryInitializer(cloudRoleName));
                
                builder.Services.AddApplicationInsightsTelemetry(options => {
                    options.ConnectionString = appInsightsConnectionString;
                });
            }

            builder.Host.UseSerilog((context, services, configuration) => {
                var loggerConfig = configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services);

                // Add Application Insights sink if connection string is available
                string? aiConnectionString = context.Configuration.GetConnectionString("ApplicationInsights");
                if (!string.IsNullOrEmpty(aiConnectionString)) {
                    var telemetryConfiguration = services.GetService<Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration>();
                    if (telemetryConfiguration != null) {
                        var minLevel = context.HostingEnvironment.IsDevelopment() ? LogEventLevel.Debug : LogEventLevel.Information;
                        loggerConfig.WriteTo.ApplicationInsights(
                            telemetryConfiguration,
                            new Serilog.Sinks.ApplicationInsights.TelemetryConverters.TraceTelemetryConverter(),
                            minLevel);
                    }
                }
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add database context
            builder.Services.AddConfiguredDbContext(builder.Configuration);

            // Add database initialisation state singleton
            builder.Services.AddSingleton<DatabaseInitializationState>();

            // Register the database ready filter
            builder.Services.AddScoped<DatabaseReadyFilter>();

            var app = builder.Build();

            // Log application context once at startup
            Log.Information("Application started - Application: {Application}, Environment: {Environment}, MachineName: {MachineName}", 
                "GinPair", 
                app.Environment.EnvironmentName, 
                Environment.MachineName);

            app.UseSerilogRequestLogging(options => {
                options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                options.GetLevel = (httpContext, elapsed, ex) => ex != null 
                    ? LogEventLevel.Error 
                    : elapsed > 1000 
                        ? LogEventLevel.Warning 
                        : LogEventLevel.Information;
            });

            if (!app.Environment.IsDevelopment()) {
                app.UseExceptionHandler("/Home/Error");
            }

            // Initialise the database in the background after the app has started
            var dbInitState = app.Services.GetRequiredService<DatabaseInitializationState>();
            var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStarted.Register(() => {
                Task.Run(() => {
                    using var scope = app.Services.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<GinPairDbContext>();
                    try {
                        Log.Information("Initialising database");
                        dbContext.Database.EnsureCreated();
                        dbInitState.IsDatabaseReady = true;
                        Log.Information("Database initialisation completed successfully");
                    } catch (InvalidOperationException ex) {
                        Log.Error(ex, "Database initialisation failed");
#if DEBUG
                        throw new InvalidOperationException("Database creation failed. Ensure the connection string is correct and the database server is running.");
#endif
                    }
                });
            });

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapControllers();

            app.Run();

            return 0;
        } catch (Exception ex) {
            Log.Fatal(ex, "Application terminated unexpectedly");
            return 1;
        } finally {
            Log.CloseAndFlush();
        }
    }
}
