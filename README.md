# Gax

.NET 8 solution with two Entity Framework Core data contexts.

## Projects

- Gax.Domain — entity models (User, Role, Right, Resource + join tables UserRole, RoleRight, ResourceRight).
- Gax.Infrastructure — custom GaxDbContext (RBAC: User-Role-Right, Resource-Right) with Fluent API configurations.
- Gax.Identity — standard ASP.NET Core Identity via AppIdentityDbContext with ApplicationUser / ApplicationRole (int keys).
- Gax.Api — web host registering both contexts and Identity.

## Restore and build

    dotnet restore
    dotnet build

## Migrations

GaxDbContext:

    dotnet ef migrations add InitialCreate --project src/Gax.Infrastructure --startup-project src/Gax.Api --context GaxDbContext --output-dir Migrations/Gax
    dotnet ef database update --project src/Gax.Infrastructure --startup-project src/Gax.Api --context GaxDbContext

AppIdentityDbContext:

    dotnet ef migrations add InitialIdentity --project src/Gax.Identity --startup-project src/Gax.Api --context AppIdentityDbContext --output-dir Migrations/Identity
    dotnet ef database update --project src/Gax.Identity --startup-project src/Gax.Api --context AppIdentityDbContext

## Run

    dotnet run --project src/Gax.Api

Connection strings are in src/Gax.Api/appsettings.json (LocalDB by default).
