# Code First + DB Next

This API uses **Code First** (define models and DbContext in C#) and **DB Next** (database schema is created/updated from code via EF Core migrations).

## Workflow

1. **Code First** – Define or change entities in `Models/` and relationships in `Data/ApplicationDbContext.cs`.
2. **Generate migration** – EF Core generates migration files from the model.
3. **DB Next** – Apply migrations to create or update the database.

## Commands

### From project directory (MCS.WebApi)

```powershell
cd MCS.WebApi
```

**Add a new migration** (after changing models or DbContext):

```powershell
dotnet ef migrations add YourMigrationName
```

**Apply migrations to the database (DB Next):**

```powershell
dotnet ef database update
```

**Roll back one migration:**

```powershell
dotnet ef database update PreviousMigrationName
```

**Remove the last migration** (only if not applied):

```powershell
dotnet ef migrations remove
```

**List migrations:**

```powershell
dotnet ef migrations list
```

### From solution root

```powershell
dotnet ef migrations add InitialCreate --project MCS.WebApi
dotnet ef database update --project MCS.WebApi
```

Use `--startup-project MCS.WebApi` if the startup project is different.

### Connection string

- **Development:** `appsettings.Development.json` or `appsettings.json` → `ConnectionStrings:DefaultConnection`
- **Test:** Set `ASPNETCORE_ENVIRONMENT=Test` and use connection in `appsettings.Test.json`
- **Production:** Use `appsettings.Production.json` or environment variables; never commit secrets.

EF Core tools use the startup project’s config. A design-time factory in `Data/DesignTimeDbContextFactory.cs` is used when running tools so the correct connection string is loaded.

## Automatic migration on startup

The API applies pending migrations on every startup (no environment check):

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}
```

The database schema is updated automatically whenever the app starts and there are pending migrations.

## Typical flow

| Step | Action |
|------|--------|
| 1 | Create or edit entity classes in `Models/`. |
| 2 | Optionally adjust `ApplicationDbContext` (DbSets, `OnModelCreating`). |
| 3 | Run `dotnet ef migrations add DescriptiveName` (e.g. `AddLoansTable`). |
| 4 | Run `dotnet ef database update` (or rely on auto-migrate in Development). |
| 5 | Run the API and use the updated database. |

## Project layout

- **Models/** – Entity classes (Code First).
- **Data/ApplicationDbContext.cs** – DbContext and mapping (Code First).
- **Data/DesignTimeDbContextFactory.cs** – Used by EF tools for design-time connection.
- **Migrations/** – EF Core migration files (generated); do not edit by hand except in special cases.
- **Database/Scripts/** – Legacy or one-off SQL scripts (not used by EF migrations).

## Notes

- Tables are created in the default schema (**dbo**). Hardcoded schema names were removed from entities for portability.
- After pulling new migrations, run `dotnet ef database update` (or deploy with migrations applied) so the DB matches the code.
- To disable auto-migrate in a specific environment (e.g. run migrations only in deployment), add a config flag and check it before calling `Migrate()`.
