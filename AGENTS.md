# AGENTS.md — High-Trust Agent Instructions

This repository operates as a **High-Trust Environment** inspired by the *Inverse Trust Pyramid* ("All Code Is Slop" by Lauren Tan). Instead of relying on prose guidelines alone, architectural boundaries, coding conventions, and cross-stack contracts are enforced deterministically by compilers, linters, automated architecture tests, and pre-commit gates.

Every AI coding agent (Antigravity, Claude Code, Cursor, GitHub Copilot, Codex, Windsurf, etc.) working in this repository **MUST** read and follow the domain-specific rules and pass all deterministic verification gates before completing a task.

---

## 1. Mandatory Rule Files

Before planning or editing any file, load and obey the detailed specification for the target stack:

- **Backend (`BE/`):** [`.agents/rules/BE.md`](./.agents/rules/BE.md) — ASP.NET Core (.NET 10) controller-based Web API, Clean Architecture (`Domain`, `Application`, `Infrastructure`, `API`, `Tests`), EF Core Code First + PostgreSQL (`citext`), JWT + Rotating `HttpOnly` Refresh Token Cookie.
- **Frontend (`FE/`):** [`.agents/rules/FE.md`](./.agents/rules/FE.md) — Next.js 16 App Router, React 19, TypeScript strict, Tailwind CSS v4, HeroUI, Zustand, feature-sliced architecture (`app/`, `features/`, `components/`, `shared/`, `lib/`), and runtime boundary validation (`DataValidator<T>`).

---

## 2. Core High-Trust Invariants (Never Violate or Bypass)

### No Escape Hatches & Never Edit Auto-Generated Files
- **Never** add `#pragma warning disable`, `[SuppressMessage]`, `// eslint-disable`, `@ts-ignore`, `@ts-expect-error`, or `any` to silence a compiler or linter error. Fix the underlying design violation.
- **Never** weaken `BE/Directory.Build.props`, `BE/.editorconfig`, `FE/eslint.config.mjs`, `FE/tsconfig.json`, or delete/skip tests in `BE/Boilerplate.Tests` or `FE/**/*.test.ts` to make a build pass.
- **Never** hand-edit auto-generated files or build outputs (enforced by `verify.ps1` and `ArchitectureConventionsTests`):
  - EF Core migration designer files (`BE/**/Data/Migrations/*.Designer.cs`) and `ApplicationDbContextModelSnapshot.cs` — always generate schema changes via `dotnet ef migrations add`.
  - Next.js & TypeScript generated files (`FE/next-env.d.ts`, `FE/.next/**`, `*.tsbuildinfo`).
  - Build and package directories (`BE/**/bin/**`, `BE/**/obj/**`, `*.csproj.user`, `FE/node_modules/**`, `FE/package-lock.json` without a `FE/package.json` dependency change).

### Backend (`BE/`) Hard Boundaries
1. **Dependency Direction:** `Domain` has zero dependencies. `Application` depends only on `Domain`. `Infrastructure` depends on `Application` and `Domain`. `API` wires DI and depends on `Application` and `Infrastructure`, but **Controllers must never inject `ApplicationDbContext` or any `Boilerplate.Infrastructure.*` type** (enforced by `ArchitectureConventionsTests`).
2. **Class & Code Style:** Every concrete class across all layers must be `sealed` unless inheritance is intentional (`BaseEntity`, `BaseController`). Use file-scoped namespaces, explicit constructors for DI, `private readonly _camelCase` fields, and `PascalCase` public members (enforced at compile time by `.editorconfig` + `TreatWarningsAsErrors`).
3. **DTOs & Entities:** Controllers must inherit `BaseController`, return `ActionResult<ApiResponse<T>>`, and **never** accept or return `BaseEntity` / EF Core entities directly (enforced by `ArchitectureConventionsTests`).
4. **Auth & Security:** Raw refresh tokens must have `[JsonIgnore]` on DTOs and only travel via `HttpOnly` cookies. Store only SHA-256 refresh token hashes in the database. Never put internal numeric database IDs in JWT claims (`sub` is username).

### Frontend (`FE/`) Hard Boundaries
1. **Import Direction (`FE/eslint.config.mjs`):**
   - `components/**`, `shared/**`, and `lib/**` must **never** import from `@/features/*`.
   - Presentational components and screens (`components/**`, `features/**/components/**`, `*Page.tsx`, `*Screen.tsx`) must **never** call `fetch()`, `apiRequest()`, or import `@/features/*/services/*` directly. All orchestration goes through feature hooks (`features/**/hooks/*`).
   - Feature services (`features/**/services/**`) are strictly transport-only: they call `apiRequest` with a runtime type guard and must **never** import `react`, `next/navigation`, `zustand`, stores, hooks, or UI components.
2. **Runtime Boundary Validation:** Every `apiRequest<T>(path, validateData, options)` call requires an explicit runtime type guard (`(value: unknown) => value is T`). Never trust external JSON payloads with `as T`.

### Cross-Boundary Contract Synchronization (`BE` <-> `FE`)
Whenever you modify an API endpoint, request/response DTO, enum (`AccountRole`), error envelope (`ApiResponse<T>`), or validation constraint (`[StringLength]`):
1. Update the C# DTO/Enum in `BE/Boilerplate.Application` or `BE/Boilerplate.Domain`.
2. Update the TypeScript type (`features/**/types/*.ts`), constants (`features/**/constants/*.ts`), and runtime type guard (`features/**/utils/*Guards.ts`) in `FE/`.
3. Update both `BE/Boilerplate.Tests/Architecture/CrossBoundaryContractTests.cs` and `FE/features/auth/utils/authContract.test.ts` so both stacks prove contract compatibility.

---

## 3. Mandatory Verification Gate (Definition of Done)

A task is **NOT** complete until the deterministic verification gate passes with zero errors and zero warnings.

### Single-Command Full Verification (from repository root)
```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\verify.ps1
```

### Individual Stack Commands

**Backend (`cd BE`):**
```powershell
dotnet build Boilerplate.slnx --configuration Release
dotnet test Boilerplate.Tests/Boilerplate.Tests.csproj --configuration Release --no-build
```

**Frontend (`cd FE`):**
```powershell
npm run lint
npm run typecheck
npm test
npm run build
```

**Database Migrations (when modifying EF Core entities or configurations in `BE/`):**
```powershell
docker compose up -d
cd BE
dotnet build Boilerplate.slnx --configuration Debug
dotnet ef migrations has-pending-model-changes --project Boilerplate.Infrastructure --startup-project Boilerplate.API
```

> **Rule of Thumb ("Making Corrections Stick"):** If an agent or developer makes a mistake that passes `./verify.ps1`, do not just fix the code—add an ESLint rule in `FE/eslint.config.mjs`, an `.editorconfig` rule in `BE/.editorconfig`, or an assertion in `BE/Boilerplate.Tests` / `FE/**/*.test.ts` so the mistake can never recur silently.
