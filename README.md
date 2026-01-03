# Gin Pair App
This project is a web app built using MVC design pattern with .NET 10 and Entity Framework Core. The app is compatible with a PostgreSQL or SQL Server database.
The app enables users to input the name of a gin and receive back a suggested tonic pairing. Users can also add and delete gins, tonics, and pairings to the database.

## Prerequisites
1. Install .NET SDK
2. Either install PostgreSQL OR SQL Server
3. Create an empty database
4. Install Entity Framework Core CLI: `dotnet tool install --global dotnet-ef`

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

## Logging
The application uses Serilog for structured logging and writes logs to the console. Logging configuration can be customised in appsettings.json.

### Optional: Application Insights Integration
The application supports optional Azure Application Insights telemetry for production monitoring. If you don't configure Application Insights, the application will run normally with console-only logging.

To enable Application Insights:
1. Set the "ApplicationInsights" connection string in appsettings.json or use User Secrets for local development
2. Optionally configure the "CloudRoleName" in the ApplicationInsights section to identify your application instance

Example configuration:
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
