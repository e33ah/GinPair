namespace GinPair.Data;

public static class ServiceConfiguration {
    public static IServiceCollection AddConfiguredDbContext(this IServiceCollection services, IConfiguration config) {
        string connectionString;

        string provider = config["DatabaseProvider"] ?? throw new InvalidOperationException("Database provider not found in configuration.");
        if (provider != "Postgres" && provider != "SqlServer") {
            throw new InvalidOperationException("Unsupported database provider.");
        }

        // Use the connection string from the configuration
        if (provider == "Postgres") {
            connectionString = config.GetConnectionString("PostgresConnection") ?? throw new InvalidOperationException("Postgres connection string not found.");
        } else if (provider == "SqlServer") {
            connectionString = config.GetConnectionString("SqlServerConnection") ?? throw new InvalidOperationException("SQL Server connection string not found.");
        } else {
            throw new InvalidOperationException("Unsupported database provider.");
        }

        // Configure the DbContext with the appropriate provider and connection string
        services.AddDbContext<GinPairDbContext>(options => {
            if (provider == "Postgres") {
                options.UseNpgsql(connectionString, npgsqlOptionsAction => {
                    npgsqlOptionsAction.MigrationsAssembly("GinPair.Migrations.Postgres");
                })
                .UseSnakeCaseNamingConvention();
            } else if (provider == "SqlServer") {
                options.UseSqlServer(connectionString, sqlServerOptionsAction => {
                    sqlServerOptionsAction.MigrationsAssembly("GinPair.Migrations.SqlServer")
                    .EnableRetryOnFailure();
                })
                ;
            }
        });

        return services;
    }
}
