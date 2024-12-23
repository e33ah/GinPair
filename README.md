# Gin Pair App
This project is a web app built using MVC design pattern with .NET and Entity Framework Core with a PostgreSQL database.
The app enables users to input the name of a gin and receive back a suggested tonic pairing. Users can also add pairings to the database.

## Prerequisites
1. Install .Net SDK
2. Install PostgreSQL and pgAdmin
3. Install Entity Framework Core CLI:
        dotnet tool install --global dotnet-ef

## Getting Started
1. Clone the repo
2. Configure the database connecction string
Update the connection string in appsettings.json with your PostgreSQL database
3. Apply ef migrations
To create the database schema and apply existing migrations run:
        dotnet ef database update
4. Run the app
Press F5 on visual studio or using the command line, navigate to the folder which contains the solution and run:
        dotnet run
Navigate to http://localhost:5260

## Unit tests
*TODO*