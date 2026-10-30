---
trigger: always_on
---

# Frontend Rules — Reusable Next.js Boilerplate

## Purpose and stack discovery

- These rules apply to frontend code under `FE/` and are intentionally domain-neutral.
- Before changing code, inspect `package.json`, the lockfile, TypeScript and framework configuration, README, environment examples, and nearby features.
- The default stack is Next.js App Router, React, strict TypeScript, and Tailwind CSS. Use the versions and optional libraries declared by the repository; do not upgrade or replace them incidentally.
- Keep configured import aliases and use the package manager implied by the lockfile. Do not add another alias, barrel-export layer, or second lockfile merely for convenience.
- Run the scripts defined in `package.json`; for code changes this normally includes linting, type checking, relevant tests, and a production build.
- Do not add a dependency when the browser, React, Next.js, or an installed package already covers the requirement.

## Routing and rendering

- Follow App Router conventions. Keep `app/**/page.tsx` as an explicit, thin route component that imports and renders the owning feature page, for example `AppLoginPage` rendering `features/auth/LoginPage.tsx`; do not use re-export shorthand for page components. Keep page implementations directly in the owning feature by default; introduce a `pages/` subfolder only when the feature contains enough page files to justify another grouping level. Route-required layouts, loading, error, and not-found files remain under `app/`.
- Preserve the repository's route groups, dynamic segments, layouts, loading states, error boundaries, metadata, and not-found behavior.
- Default to Server Components. Add `'use client'` only to the smallest component that needs state, effects, browser APIs, event handlers, client stores, or client-only libraries.
- Fetch on the server when it improves security, caching, or initial rendering. Preserve the route's cache tags, revalidation policy, and dynamic/static rendering intent; invalidate affected data after mutations.
- Keep authentication setup centralized. Do not scatter session checks, token refresh, or request interceptors across pages and components.
- Enforce route guards consistently: protected screens redirect unauthenticated users to login; guest screens (such as login and register) redirect authenticated users to the dashboard.

## Folder ownership

```text
FE/
├── app/                         # routes, layouts, metadata, route-level revalidation
├── features/
│   └── <Feature>/               # screen, components/, hooks/, services/, types/, constants/, actions/, utils/
├── components/
│   ├── ui/                      # configured design-system primitives
│   └── shared/                  # reusable application components
├── shared/
│   ├── constants/               # endpoint constants and cross-feature constants
│   ├── lib/                     # shared API client
│   └── types/                   # API envelope and cross-feature types
├── lib/                         # small app-wide utilities with no feature ownership
└── public/                      # static assets
```

- Keep code local to its feature until at least two unrelated features need it. Do not create generic wrappers, providers, stores, or design-system layers speculatively.
- Use PascalCase for React component files and components; use kebab/camel names already established by the containing folder for non-component files. Match the style of the file being edited.

## TypeScript and React

- TypeScript is strict: do not use `any`, unchecked casts, or untyped API payloads. Use `unknown` at untrusted boundaries and narrow it deliberately.
- Import types with `import type`. Reuse domain types from the owning feature or shared types; do not duplicate API DTO shapes in components.
- Do not leave business limits, routes, API paths, HTTP methods, status values, role values, timeouts, or other reusable configuration as magic numbers or magic strings. Give them descriptive names in the owning feature's `constants/` folder, or in `shared/constants/` when they are genuinely cross-feature.
- Derive literal union types from constant objects when practical so runtime values and TypeScript types cannot drift apart. Do not extract self-explanatory one-off UI copy, CSS dimensions, SVG coordinates, or basic loop/index arithmetic merely to satisfy this rule.
- Keep screen components focused on presentation and composition. Put request orchestration, complex form state, validation, and mutation state in the owning feature hook or server action.
- Use `useCallback` only when its identity is consumed by an effect, memoized child, or equivalent real dependency. Do not add memoization by default.
- Keep state as local as practical. Use a client store only for genuinely cross-component session state, and persist browser state only when the product requirement demands it.

## API, errors, and authentication

- Centralize API paths and cross-cutting HTTP behavior such as base URL, serialization, authentication, cancellation, and standard error parsing.
- Proxy backend API paths via Next.js rewrites (`/api/:path*`) to preserve same-origin cookie credentials and prevent CORS overhead during browser calls.
- Use the repository's shared request client for browser calls. Use native `fetch` on the server when Next.js caching or revalidation semantics are required.
- Validate both the HTTP status and expected response shape. Treat external data as unknown until it crosses the project's parsing or validation boundary.
- Avoid direct request calls in presentational components. Put them in the owning feature service, query hook, or server-side data function.
- Keep feature services transport-only: call endpoints, serialize requests, validate/return the API contract, and do not own UI state, navigation, form validation, or workflow decisions. Put client-side orchestration and business/UI logic in feature hooks; components should primarily render and bind hook outputs.
- Follow the backend's selected authentication transport. Do not invent another token store or attempt to read HttpOnly cookies from JavaScript.
- Only expose values intended for browsers through public environment variables; never expose secrets in the client bundle.
- Display safe, actionable errors in the product's configured language and use a feature-specific fallback for unknown failures.

## Components, UX, and accessibility

- Build components with a focused responsibility and a typed public API. Prefer composition over large collections of boolean props or duplicated variants.
- Every asynchronous screen must handle loading, empty, success, and error states. Preserve user input on recoverable failures and prevent accidental duplicate submissions.
- Use semantic HTML first. Ensure keyboard access, visible focus, associated labels, useful image alternative text, and sufficient contrast. Use ARIA only where native semantics are insufficient.
- Honor `prefers-reduced-motion` and avoid animation that blocks interaction or conveys essential information without an alternative.
- Confirm destructive or irreversible actions and show actionable feedback without exposing internal errors.

## Styling and UI

- Use the configured component library when it provides a matching accessible primitive. Do not create competing versions of buttons, inputs, dialogs, tables, alerts, or notifications.
- Use the established styling system for components and layouts. Add global CSS only for genuinely global tokens, resets, animations, or selectors; use inline styles only for data-driven values that cannot be expressed cleanly otherwise.
- Reuse existing semantic design tokens and shared primitives instead of introducing near-duplicate colors, gradients, controls, or layouts.
- Build responsive layouts mobile-first and check both narrow and wide viewports. Avoid fixed dimensions that cause clipping unless the design explicitly requires them.
- Keep visual structure in focused feature components, but do not add a component solely to wrap one element.

## Changes and verification

- Make the smallest complete change. Search for existing helpers and all callers before changing shared code; fix the shared root cause rather than patching one screen.
- Coordinate API contract changes with `BE/` rather than silently translating or guessing fields in the frontend.
- For non-trivial client logic, add the smallest focused test when the repository has a suitable setup; do not introduce a test framework solely for one change.
- Run the configured formatter or linter, type checker, relevant tests, and production build. If verification cannot run, report exactly what was not run and why.
- Do not modify generated output, `node_modules`, lockfiles without a dependency change, or unrelated files.
