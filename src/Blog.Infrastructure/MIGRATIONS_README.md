EF Core Migrations

To create the initial migration (after creating the solution and adding packages):

1. Install dotnet-ef if you don't have it:
   dotnet tool install --global dotnet-ef

2. From the repository root run:
   dotnet ef migrations add InitialCreate -p src/Blog.Infrastructure -s src/Blog.Web -o src/Blog.Infrastructure/Migrations

3. Apply the migration:
   dotnet ef database update -p src/Blog.Infrastructure -s src/Blog.Web

Note: Development uses SQLite by default. To use PostgreSQL, change the connection string in appsettings.Production.json or set environment variables.
