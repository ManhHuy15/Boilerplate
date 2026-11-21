# Copilot & Agent Instructions

This repository enforces a **High-Trust Environment** with deterministic compiler, linter, architecture, and cross-boundary contract checks.

1. Always follow [`AGENTS.md`](../AGENTS.md) at the repository root.
2. When editing Backend files (`BE/**`), follow [`.agents/rules/BE.md`](../.agents/rules/BE.md):
   - Use file-scoped namespaces, `sealed` concrete classes, explicit DI constructors, and `private readonly _camelCase` fields.
   - Keep controllers thin (inheriting `BaseController`, returning `ActionResult<ApiResponse<T>>`, never injecting `DbContext` or exposing `BaseEntity`).
   - Ensure `dotnet build Boilerplate.slnx --configuration Release` and `dotnet test Boilerplate.Tests/Boilerplate.Tests.csproj --configuration Release` pass with 0 warnings and 0 errors.
3. When editing Frontend files (`FE/**`), follow [`.agents/rules/FE.md`](../.agents/rules/FE.md):
   - Respect strict TypeScript (`no-explicit-any`, `consistent-type-imports`) and ESLint architectural boundaries in `FE/eslint.config.mjs`.
   - Never import `@/features/*` from `components/`, `shared/`, or `lib/`.
   - Never call `fetch`, `apiRequest`, or feature services directly in presentational components/screens; keep feature services transport-only and validate all responses with runtime type guards.
   - Ensure `npm run lint`, `npm run typecheck`, `npm test`, and `npm run build` pass.
4. When changing API contracts or validation limits, synchronize C# DTOs, TypeScript types/guards, `BE/Boilerplate.Tests/Architecture/CrossBoundaryContractTests.cs`, and `FE/features/auth/utils/authContract.test.ts`.
5. Never hand-edit auto-generated files (`BE/**/Data/Migrations/*.Designer.cs`, `ApplicationDbContextModelSnapshot.cs`, `FE/next-env.d.ts`, `bin/`, `obj/`, `.next/`, `node_modules/`, or `FE/package-lock.json` without a `FE/package.json` dependency change).
