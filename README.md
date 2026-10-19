# Gin Pair App

<p align="center">
    <img width="680" height="286" alt="GinPair Index page" src="https://github.com/user-attachments/assets/d527b9f6-e0d0-4de2-894f-1d0143d481e7" />
</p>

A lightweight web application built using the MVC design pattern with .NET 10 and Entity Framework Core.  
The app works with either PostgreSQL or SQL Server and allows users to browse, add, and delete gins, tonics, and pairing suggestions.  
Enter the name of a gin and the app returns a recommended tonic pairing based on the stored data.

## Prerequisites
1. Install .NET SDK
2. Either install PostgreSQL OR SQL Server
3. Create an empty database
4. Install Entity Framework Core CLI: `dotnet tool install --global dotnet-ef`
5. [optional] Install Fallout for CI/CD local execution: `dotnet tool install --global Fallout.GlobalTool`

## Getting Started
1. Clone the GinPair repository

2. Configure the database provider - in appsettings.json, change the "DatabaseProvider" setting to either "Postgres" or "SqlServer" depending on the database you are using.

3. Configure the database connection string - add the connection string in appsettings.json. For PostgreSQL database, set the "PostgresConnection". For SQL Server database, set the "SqlServerConnection" value.

4. Apply ef migrations - in the solution root, create the database schema and apply existing migrations.

    1. For PostgreSQL run: `dotnet ef database update  --context GinPairDbContext  --project GinPair.Migrations.Postgres  --startup-project GinPair`.

    2. For SQL Server run: `dotnet ef database update  --context GinPairDbContext  --project GinPair.Migrations.SqlServer  --startup-project GinPair`.

5. Run the app - press F5 in Visual Studio or, using the command line, navigate to the folder which contains the solution `GinPair/GinPair.sln` and run: `dotnet run`. Navigate to http://localhost:5260

Sample data for the Gins and Tonics tables is included in the _Dependencies project.

**Note:** The application will automatically initialise the database schema on startup if migrations have been applied. The app will be available immediately, with database initialisation completing in the background.

## Unit Testing
Unit tests are located in the GinPair.Tests project and are written using xUnit and Shouldly for assertions.

To run the unit tests, navigate to the folder which contains the solution `GinPair/GinPair.sln` and run: `dotnet test`

## Observability & Logging
The application uses Serilog for structured logging and writes logs to the console. Logging configuration can be customised in appsettings.json.

### Optional: Azure Monitor & OpenTelemetry Integration
The application uses **OpenTelemetry** (`Azure.Monitor.OpenTelemetry.AspNetCore`) for distributed tracing, runtime metrics, and log export to Azure Application Insights. When enabled, Serilog logs are automatically correlated with incoming HTTP request traces and SQL dependencies.

If an Application Insights connection string is not configured, the application runs normally with console-only logging.

#### Enabling Azure Monitor / Application Insights:
Provide the connection string using any of the following methods:
1. **Azure App Service (Production):** Set the `APPLICATIONINSIGHTS_CONNECTION_STRING` environment variable in the Azure Portal.
2. **Local Development:** Set via .NET User Secrets or in `appsettings.json`:
   ```bash
   dotnet user-secrets set "ConnectionStrings:ApplicationInsights" "InstrumentationKey=...;IngestionEndpoint=..."
   ```

#### Optional Configuration:
- **Cloud Role Name / Service Name:** Customise the application role name using either `ApplicationInsights:CloudRoleName` in configuration or the standard `OTEL_SERVICE_NAME` environment variable (defaults to `"GinPair"`).

Example `appsettings.json` configuration:
```json
{
  "ConnectionStrings": {
    "ApplicationInsights": "InstrumentationKey=your-key;IngestionEndpoint=https://..."
  },
  "ApplicationInsights": {
    "CloudRoleName": "GinPair-Production"
  }
}
```

## Troubleshooting

### Database Connection Issues
- Ensure your database server is running
- Verify the connection string format is correct for your database provider
- Check that the database exists and is accessible
- Review the console logs for specific error messages

### Migration Issues
- Ensure the correct migration project is specified for your database provider
- Verify the Entity Framework Core CLI tools are installed: `dotnet ef --version`

## Notes
Database migrations are located in separate GinPair.Migrations projects to allow for multiple database providers. The migrations for PostgreSQL are located in the GinPair.Migrations.Postgres project and the migrations for SQL Server are located in the GinPair.Migrations.SqlServer project.

To add additional migrations, 

- Postgres: `dotnet ef migrations add <MigrationName> --context GinPairDbContext --project GinPair.Migrations.Postgres --startup-project GinPair --output-dir Migrations`. 
- SqlServer: `dotnet ef migrations add <MigrationName> --context GinPairDbContext --project GinPair.Migrations.SqlServer --startup-project GinPair --output-dir Migrations`.

## CI/CD

To run the build pipeline locally using FALLOUT, use the following commands based on your operating system:

```bash
# Windows
.\build.cmd 

# Linux/macOS
./build.sh 

# Using Fallout global tool
fallout
```

The project uses GitHub Actions for continuous integration and continuous deployment.

To deploy using the Fallout pipeline locally, install the Azure CLI. 
Add AzureResourceGroup and AzureWebAppName to user secrets.
Then run:

```
az login

fallout ReleaseStep
```
