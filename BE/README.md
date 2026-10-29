# Boilerplate Backend

Controller-based ASP.NET Core Web API targeting .NET 10.

## Projects

- `Boilerplate.API`: HTTP boundary, controllers, middleware, and composition root.
- `Boilerplate.Application`: use cases, DTOs, and application contracts.
- `Boilerplate.Infrastructure`: persistence, EF Core, password hashing, and token generation.
- `Boilerplate.Domain`: entities, enums, and domain rules.

Project dependencies point inward:

```text
API --> Application --> Domain
 |                       ^
 +----> Infrastructure --+
             |
             +---------> Application
```

## Local configuration

Do not commit the PostgreSQL password or JWT signing key. Supply them through
environment variables or another configuration provider:

```powershell
Copy-Item Boilerplate.API/.env.example Boilerplate.API/.env
```

Use `Boilerplate.API/.env.example` as the configuration template. The real
`Boilerplate.API/.env` file is ignored by Git. The standard .NET CLI does not
automatically load `.env`, so import these values through your IDE,
container/deployment configuration, or set them in the current shell before
running the API:

```powershell
$env:Database__ConnectionString = "Host=localhost;Port=5432;Database=boilerplate;Username=postgres;Password=<password>"
$env:Jwt__SecretKey = "<random-secret-with-at-least-32-bytes>"
```

`appsettings.Development.json` contains the non-secret defaults. For a
cross-site frontend, set `Jwt__RefreshCookieSameSite=None` and keep
`Jwt__RefreshCookieSecure=true` under HTTPS.

## Run locally

From `BE/`:

```powershell
dotnet restore Boilerplate.slnx
dotnet build Boilerplate.slnx --no-restore
dotnet run --project Boilerplate.API
```

The API listens on `http://localhost:8080`. Health check:

```text
GET /api/health
```

In the Development environment, Swagger UI is available at:

```text
http://localhost:8080/swagger
```

Use the Swagger `Authorize` action with the access token returned by an Auth
endpoint to test protected APIs. Swagger is not exposed outside Development.

## Authentication

The reusable authentication flow uses:

- username/password registration and login;
- ASP.NET Core password hashing;
- short-lived JWT access tokens;
- rotating refresh tokens in an `HttpOnly` cookie;
- SHA-256 refresh-token hashes in PostgreSQL;
- `User` and `Admin` roles;
- the common `ApiResponse<T>` envelope for success and error responses.

Endpoints:

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/logout
GET  /api/auth/me          Authorization: Bearer <access-token>
```

`register` and `login` return the access token in the response body and set the
refresh-token cookie. The raw refresh token is never included in JSON.

## PostgreSQL and EF Core

The solution uses EF Core Code First with Npgsql. The first migration is
`InitialAuth`, which creates `Accounts` and `RefreshTokens`.

The API automatically applies pending EF Core migrations during every startup,
before it begins accepting requests. Startup fails when the database is
unavailable or a migration cannot be applied.

Manual database updates remain available when needed:

```powershell
dotnet tool restore
$env:ASPNETCORE_ENVIRONMENT = "Development"

dotnet ef database update `
  --project Boilerplate.Infrastructure `
  --startup-project Boilerplate.API
```

Create later migrations with:

```powershell
dotnet ef migrations add <MigrationName> `
  --project Boilerplate.Infrastructure `
  --startup-project Boilerplate.API `
  --output-dir Data/Migrations
```

Add domain entities to `Boilerplate.Domain`, EF configurations to
`Boilerplate.Infrastructure/Data/Configurations`, and expose required data
access through `IApplicationDbContext`.
