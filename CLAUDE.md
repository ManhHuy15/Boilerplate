# CLAUDE.md

Read and strictly follow [AGENTS.md](./AGENTS.md), [`.agents/rules/BE.md`](./.agents/rules/BE.md), and [`.agents/rules/FE.md`](./.agents/rules/FE.md).

## Quick Commands

- **Full High-Trust Verification Gate (BE + FE + Contracts):**
  ```powershell
  powershell -NoProfile -ExecutionPolicy Bypass -File .\verify.ps1
  ```
- **Backend Build & Tests (`BE/`):**
  ```powershell
  cd BE && dotnet build Boilerplate.slnx --configuration Release && dotnet test Boilerplate.Tests/Boilerplate.Tests.csproj --configuration Release --no-build
  ```
- **Frontend Lint, Typecheck, Tests & Build (`FE/`):**
  ```powershell
  cd FE && npm run lint && npm run typecheck && npm test && npm run build
  ```
- **Local PostgreSQL (`docker-compose.yml`):**
  ```powershell
  docker compose up -d
  ```

Never bypass compiler warnings (`TreatWarningsAsErrors`), `.editorconfig`, ESLint architectural boundary rules (`FE/eslint.config.mjs`), or architecture/contract tests (`BE/Boilerplate.Tests` and `FE/**/*.test.ts`). Never hand-edit auto-generated files (`*.Designer.cs`, `ApplicationDbContextModelSnapshot.cs`, `FE/next-env.d.ts`, `bin/`, `obj/`, `.next/`, or `package-lock.json` without a `package.json` change).
