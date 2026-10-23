# AGENTS.md

ASP.NET Core 8 online-store backend (Clean Architecture) plus a Vite + React 19 + TypeScript
frontend in `frontend/`. Root solution: `Backend_for_an_online_store.sln`.

## Architecture

Dependency direction is strict and the build enforces it:

```
WebApi -> Infrastructure -> Application -> Domain
```

- `Domain/` — entities, enums, domain exceptions. No dependencies.
- `Application/` — MediatR CQRS (`Commands/`, `Queries/`, `Behaviors/`), AutoMapper, DTOs,
  interfaces implemented by Infrastructure. References only `Domain` (no EF Core).
- `Infrastructure/` — EF Core (`AppDbContext`), repositories, `UnitOfWork`, and services
  (JWT, email, payments, IP geolocation, AI agent). Migrations live in
  `Infrastructure/Migrations` and target the `Infrastructure` assembly.
- `WebApi/` — controllers under `api/*`, JWT Bearer auth, Swagger (Development only), Serilog,
  startup DB seeding.

Request flow: controller -> `IMediator.Send(command/query)` -> handler -> repository / `IUnitOfWork`.
Controllers never touch EF or repositories directly.

## Backend setup

- Requires PostgreSQL at `localhost:5432`, database `WebStoreProjectDb`, user `postgres`
  (see `WebApi/appsettings.json`).
- `DB_PASSWORD` environment variable is **required**: `Infrastructure/DependencyInjection.cs`
  throws at startup if it is missing.
  PowerShell: `$env:DB_PASSWORD='your-password'`
- `JWT__KEY` environment variable is **required** and must be at least 32 bytes (256 bits):
  `JwtOptions.Load` in `Program.cs` fails fast if it is missing or too short. Do **not** put the
  key back in `appsettings.json`; `JWT__KEY` maps to `Jwt:Key` and overrides any file value.
  PowerShell: `$env:JWT__KEY='<random-base64-at-least-32-bytes>'`
- Docker Compose reads the same variable from `.env` and passes it to the `api` container as
  `JWT__KEY`; the legacy `JWT_KEY` name is still accepted as a fallback (see
  `docker-compose.yml`, `.env.example`).
- The AI agent needs `GROQ_API_KEY`; sending email needs `Smtp:Password` in `appsettings.json`.
  These are optional for startup but required for those features.
- `Program.cs` seeds the database on startup (admin, categories, 1000 products) and rethrows on
  failure, so a broken DB connection prevents startup.
- Seeded admin: `admin@store.com` / `Admin123!`.

### Run

```powershell
dotnet run --project WebApi --launch-profile https   # https://localhost:7197
```

Use the `https` profile: the Vite dev proxy targets `https://localhost:7197`. The default launch
profile is `http` (port 5277), which the frontend proxy does **not** use. Swagger:
`https://localhost:7197/swagger` (Development only).

### EF migrations

`Program.cs` does **not** apply migrations automatically — run them manually. Because EF tooling
executes `Program.cs` up to `builder.Build()`, the JWT fail-fast check also runs, so export the
required secrets first:

```powershell
$env:DB_PASSWORD='your-password'
$env:JWT__KEY='<random-base64-at-least-32-bytes>'
dotnet ef database update --project Infrastructure --startup-project WebApi
dotnet ef migrations add <Name> --project Infrastructure --startup-project WebApi
```

`Microsoft.EntityFrameworkCore.Design` is referenced only by `WebApi`, and there is no design-time
factory, so always pass `--startup-project WebApi`.

## Frontend

Everything is self-contained in `frontend/`:

```powershell
cd frontend
npm install
npm run dev      # Vite dev server, proxies /api -> https://localhost:7197
npm run build    # tsc -b && vite build
npm run lint
```

- Source is organized by feature (`src/features/<name>/`), with shared API/UI in `src/shared/`.
- Axios client base URL is `/api`; it reads the JWT from `localStorage["access_token"]`.
- **Gotcha:** `frontend/eslint.config.js` is a copy of `vite.config.ts` (imports `defineConfig`
  from `vite`). `npm run lint` will fail until it is rewritten as a real ESLint flat config.

## Tests

```powershell
dotnet test Backend_for_an_online_store.sln
dotnet test WebAPITests --filter "FullyQualifiedName~AuthFlowTests"   # single class
dotnet test DomainTests                                             # unit project only
```

- Source projects target `net8.0`; all four test projects target `net9.0`.
- `WebAPITests` integration tests use **Testcontainers.PostgreSql + Respawn** and need Docker;
  they are slow. Unit suites (`DomainTests`, `ApplicationTests`, `InfrastructureTests`) use EF
  InMemory / Moq and need no external services.
- **Gotcha:** `WebAPITests/Infrastructure/IntegrationTestBase.LoginAsAdminAsync` logs in with
  `Admin123!@#`, but `Infrastructure/Data/SeedData.cs` hashes `Admin123!`. Admin-based integration
  tests will fail until these are reconciled.

## Stray / misleading files

- `OrderTests.txt` at the repo root is a loose `.txt` file, not part of any project (never compiled).
- `WebApi/web_store.db`, `WebApi/web_store.sql`, `WebApi/data.sql` are leftover SQLite artifacts;
  the application uses PostgreSQL.
- `ApplicationTests` references (and in the `.sln` depends on) `Infrastructure` — unusual for a
  unit-test project; assume Application-layer tests may still pull in infrastructure types.
