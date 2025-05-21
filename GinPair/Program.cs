namespace GinPair;

public class Program {
    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        // Add database context
        builder.Services.AddConfiguredDbContext(builder.Configuration);

        var app = builder.Build();

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GinPairDbContext>();
        try {
            dbContext.Database.EnsureCreated();
        } catch (InvalidOperationException) {
            if (!app.Environment.IsDevelopment()) {
                app.UseExceptionHandler("/Home/Error");
            } else {
                throw;
            }
        }
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapControllers();

        app.Run();
    }
}
