# Gin Pair App
This project is a web app built using MVC design pattern with .NET8.0 and Entity Framework Core. The app is compatible with a PostgreSQL or SQLSever database.
The app enables users to input the name of a gin and receive back a suggested tonic pairing. Users can also add pairings to the database.

## Prerequisites
1. Install .Net SDK
2. Either install PostgreSQL and pgAdmin OR SqlServer
3. Create an empty database
3. Install Entity Framework Core CLI: `dotnet tool install --global dotnet-ef`

## Getting Started
1. Clone the GinPair repository

2. Configure the database provider - in appsettings.json, change the "DatabaseProvider" setting to either "Postgres" or "SqlServer" depending on the database you are using.

3. Configure the database connection string - add the connection string in appsettings.json. For PostgreSQL database, set the "PostgresConnection". For SqlServer database, set the "SqlServerConnection" value.

4. Apply ef migrations - in the solution root, create the database schema and apply existing migrations for Postgresql by running: `dotnet ef database update  --context GinPairDbContext  --project GinPair.Migrations.SqlServer  --startup-project GinPair`. 
For SqlServer run: `dotnet ef database update  --context GinPairDbContext  --project GinPair.Migrations.Postgres  --startup-project GinPair`

5. Run the app - press F5 on visual studio or, using the command line, navigate to the folder which contains the solution `GinPair/GinPair.sln` and run: `dotnet run`. Navigate to http://localhost:5260

Sample data for the Gins and Tonics tables is included in the _Dependencies project

## Unit tests
Unit tests are located in the GinPair.Tests project and are written using xUnit.
To run the unit tests, navigate to the folder which contains the solution `GinPair/GinPair.sln` and run: `dotnet test`

## Notes
Database migrations are located in separate GinPair.Migrations projects to allow for multiple database providers. The migrations for PostgreSQL are located in the GinPair.Migrations.Postgres project and the migrations for SQL Server are located in the GinPair.Migrations.SqlServer project.
To add additional migrations, for Postgres use the command: `dotnet ef migrations add <MigrationName> --context GinPairDbContext --project GinPair.Migrations.Postgres --startup-project GinPair --output-dir Migrations`. Or for SqlServer: `dotnet ef migrations add <MigrationName> --context GinPairDbContext --project GinPair.Migrations.SqlServer --startup-project GinPair --output-dir Migrations`.
