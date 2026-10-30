---
trigger: always_on
---

# Backend Rules - .NET Boilerplate

## Scope and stack

- Apply these rules to `BE/`. Before editing, inspect the solution, project files, README, configuration, owning feature, callers, and latest migration.
- The stack is controller-based ASP.NET Core on .NET 10, EF Core Code First, Npgsql, and PostgreSQL. Use repository-declared versions; do not upgrade incidentally.
- Run commands from `BE/`. Do not add Minimal APIs, MediatR/CQRS, generic repositories, AutoMapper, another response wrapper, or extra layers without a demonstrated need.

## Architecture

```text
BE/
|-- Boilerplate.API/             Controllers, Common, Middlewares, Program.cs
|-- Boilerplate.Application/     InfraInterfaces, Services/<Feature>Services/DTOs, Settings
|-- Boilerplate.Infrastructure/  Data/Configurations, Data/Migrations, Security
`-- Boilerplate.Domain/          BaseEntity, Entities, Enums
```

```text
API --> Application --> Domain
 |                       ^
 `----> Infrastructure --+
             `---------> Application
```

- `Domain` has no project dependency. `Application` owns use cases and technical contracts. `Infrastructure` implements those contracts and owns EF/provider/SDK details. `API` owns HTTP concepts, controllers, cookies, claims, status codes, and composition.
- Register application services in Application's `DependencyInjection`; register DbContext and technical implementations in Infrastructure's `DependencyInjection`.
- Use feature folders: `Services/<Feature>Services/<Feature>Services.cs`, with feature DTOs under `DTOs/`. Keep namespaces aligned with folders.

## C# style

- Use file-scoped namespaces, nullable reference types, PascalCase public members, and camelCase parameters/locals.
- Dependency-injected classes use an explicit constructor, private `readonly` fields, and `_camelCase` field names. Constructor parameters do not use `_`.
- Prefer `sealed` concrete classes unless inheritance is intentional. Prefer clear, full code over compressed syntax.
- Propagate `CancellationToken` through async database and external I/O. Persist timestamps in UTC.

## Application and data access

- Application services own orchestration, business validation, mutations, and DTO mapping. Infrastructure services own technical mechanisms and framework/library implementations.
- Define technical boundaries in `Application/InfraInterfaces`; add interfaces only at real boundaries or testing seams.
- Use explicit request/response DTOs. Never expose EF entities or navigation properties through endpoints.
- Use Data Annotations for request shape; enforce business rules in Application. Trim input deliberately, but preserve display casing unless requirements say otherwise.
- Use `IApplicationDbContext`; do not add a repository/unit-of-work layer over EF Core.
- Expose explicit sets such as `DbSet<Account> Accounts`; application code uses `_dbContext.Accounts`, not `_dbContext.Set<Account>()`.
- Put mappings, lengths, types, relationships, indexes, and constraints in `Infrastructure/Data/Configurations`.
- Preserve `BaseEntity` audit fields, UTC timestamps, enum-to-string conversion, named soft-delete filters, and soft-delete behavior.
- Use no-tracking projections for reads, tracked entities for updates, set-based queries, and `Include` only when needed. Avoid N+1 queries.
- PostgreSQL usernames use `citext` plus a unique index: keep original casing while equality and uniqueness ignore casing. Do not add `NormalizedUsername`.
- Multi-step writes that form one operation must be atomic.

## Controllers, validation, and responses

- Controllers inherit `BaseController` and remain thin: bind input, authorize, read claims/cookies, call Application, and format the response.
- Use the `api/[controller]` route convention, `ActionResult<ApiResponse<T>>`, and relevant `ProducesResponseType` metadata unless a feature establishes another convention.
- Use only generic `ApiResponse<T>`. HTTP status stays on the HTTP response and is not duplicated in the body.
- `ApiError` has stable `Code`, safe `Message`, and optional `Field`; failures include `TraceId` when available.
- `Success` is HTTP 200 and `Created` is HTTP 201. Add success helpers only when needed. A 204 response uses `NoContent()` and has no body.
- `[ApiController]` and Data Annotations validate requests. Format invalid models through `InvalidModelStateResponseFactory`; do not repeat shape validation in actions.
- Map runtime failures through `ExceptionHandlingMiddleware`; configure JWT 401/403 centrally. Do not add controller helpers for every error.
- Never return stack traces, provider/SQL errors, secrets, or internal paths.

## Authentication

- Auth supports register, login, refresh rotation, logout/revoke, and `me`.
- `AuthService` and `RefreshTokenService` belong to Application because they implement workflows and token-state rules.
- `IPasswordService` and `ITokenService` belong to Application; their implementations belong to Infrastructure because they use framework/JWT/cryptographic mechanisms.
- Use the existing ASP.NET Core Identity `PasswordHasher<Account>`. Do not add BCrypt, a second hasher, custom salt storage, reversible encryption, or plaintext passwords.
- JWT validation includes issuer, audience, signature, lifetime, name, and role. Keep claims limited to `sub`, name, role, and `jti` unless required.
- Never put the internal numeric database ID in JWT claims. The current `sub` is username; if usernames become mutable, add a stable non-sequential public identifier such as a GUID.
- Access tokens may be returned only by the Auth contract. Never return raw refresh tokens in JSON.
- Generate refresh tokens cryptographically, store only SHA-256 hashes, and send raw values only through the configured `HttpOnly` cookie.
- Preserve refresh-cookie `Secure`, `SameSite`, path, expiration, and essential settings. Cross-site cookies require `SameSite=None`, `Secure=true`, HTTPS, and deliberate CORS credentials.
- Rotation revokes the old token, records its replacement hash, creates the new token, and saves atomically. Detect token replay/reuse: if an already-revoked or rotated token is presented again, immediately revoke all active refresh tokens of that account and abort. Logout is idempotent and always deletes the cookie.
- Prune expired and revoked refresh tokens periodically using a dedicated background service in Infrastructure.
- Public registration always assigns `User`; never accept privileged roles from the request. Authentication failures must not reveal whether an account exists.

## Configuration, migrations, and operations

- Bind sealed typed settings (`DatabaseSettings`, `JwtSettings`, `CorsSettings`) and validate required configuration at startup.
- Supply `Database__ConnectionString` and `Jwt__SecretKey` through environment variables or a secret store. Never commit real secrets; JWT keys require at least 32 UTF-8 bytes.
- Keep non-secret variable names and safe placeholders in `BE/Boilerplate.API/.env.example`, beside `appsettings.json`. Keep the real `.env` ignored; remember that the standard .NET CLI does not load dotenv files automatically.
- API port is `8080`. Development may disable cookie `Secure` only for local HTTP; production uses HTTPS.
- Register Swagger/OpenAPI and expose Swagger UI only in Development. Keep JWT Bearer authorization configured in Swagger for protected endpoint testing.
- Preserve middleware order: exception handling, HTTPS redirection, CORS, authentication, authorization, controller mapping.
- Use structured logs and trace IDs. Never log passwords, tokens, keys, connection strings, cookies, or unnecessary personal data.
- Create schema changes with the repository-local EF tool. Do not hand-edit migration designer or snapshot files. Review generated types, keys, delete behavior, indexes, and extensions.
- Build Debug before migration commands. Rebuild Debug after generating a migration before checking pending model changes, so EF does not use a stale snapshot.
- The API automatically calls `Database.MigrateAsync()` during every startup and before accepting requests in all environments. Keep this behavior unless the consuming project explicitly changes its deployment strategy.
- Migration or database connection failure must fail startup; never continue serving requests against an unknown schema.

## Verification

- Search all callers before changing contracts, namespaces, DTOs, claims, settings, entities, or fields. Preserve unrelated edits; never edit `bin/`, `obj/`, or user-specific project files.
- At minimum run `dotnet build Boilerplate.slnx --configuration Release --no-restore`.
- For EF changes, build Debug, review the migration, then run `dotnet ef migrations has-pending-model-changes` with required environment variables.
- For Auth/HTTP changes, smoke-test port 8080, health, affected Auth endpoints, and standardized unauthorized/validation responses when possible.
- Add focused tests when a test project exists; do not introduce a test framework only for one small change.
- If verification cannot run, report exactly what was not run and why.
