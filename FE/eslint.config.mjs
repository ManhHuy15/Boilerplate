import { defineConfig, globalIgnores } from "eslint/config";
import nextVitals from "eslint-config-next/core-web-vitals";
import nextTypeScript from "eslint-config-next/typescript";

export default defineConfig([
  ...nextVitals,
  ...nextTypeScript,
  {
    rules: {
      "@typescript-eslint/no-explicit-any": "error",
      "@typescript-eslint/consistent-type-imports": [
        "error",
        { prefer: "type-imports", fixStyle: "separate-type-imports" },
      ],
    },
  },
  globalIgnores([".next/**", "out/**", "next-env.d.ts"]),
  {
    files: ["**/*.{ts,tsx}"],
    rules: {
      "@typescript-eslint/no-explicit-any": "error",
      "@typescript-eslint/consistent-type-imports": [
        "error",
        {
          prefer: "type-imports",
          fixStyle: "inline-type-imports",
        },
      ],
    },
  },
  {
    // FE.md: "Keep code local to its feature... components/ui and components/shared must not depend on features/"
    files: ["components/**/*.{ts,tsx}", "shared/**/*.{ts,tsx}", "lib/**/*.{ts,tsx}"],
    rules: {
      "no-restricted-imports": [
        "error",
        {
          patterns: [
            {
              group: ["@/features/*", "@/features/**", "../features/*", "../../features/*"],
              message:
                "Shared/UI layers must not import from features/ (violates FE.md architectural boundary).",
            },
          ],
        },
      ],
    },
  },
  {
    // FE.md: "Avoid direct request calls in presentational components. Put them in the owning feature service, query hook, or server-side data function."
    files: [
      "components/**/*.{ts,tsx}",
      "features/**/components/**/*.{ts,tsx}",
      "features/**/*Page.tsx",
      "features/**/*Screen.tsx",
    ],
    rules: {
      "no-restricted-imports": [
        "error",
        {
          paths: [
            {
              name: "@/shared/lib/apiClient",
              importNames: ["apiRequest"],
              message:
                "Presentational components and screens must not call apiRequest directly. Put transport calls in feature services (FE.md).",
            },
          ],
          patterns: [
            {
              group: ["@/features/*/services/*"],
              message:
                "Presentational components and screens must not call feature services directly. Use a feature hook or server action (FE.md).",
            },
          ],
        },
      ],
      "no-restricted-globals": [
        "error",
        {
          name: "fetch",
          message:
            "Do not call fetch() directly inside presentational components or screens. Use feature services via hooks (FE.md).",
        },
      ],
    },
  },
  {
    // FE.md: "Keep feature services transport-only: call endpoints, serialize requests, validate/return the API contract, and do not own UI state, navigation, form validation, or workflow decisions."
    files: ["features/**/services/**/*.{ts,tsx}"],
    rules: {
      "no-restricted-imports": [
        "error",
        {
          paths: [
            {
              name: "react",
              message: "Feature services must remain transport-only and must not import React or own UI state (FE.md).",
            },
            {
              name: "next/navigation",
              message: "Feature services must remain transport-only and must not own navigation (FE.md).",
            },
            {
              name: "zustand",
              message: "Feature services must remain transport-only and must not own client stores (FE.md).",
            },
          ],
          patterns: [
            {
              group: ["@/shared/stores/*", "@/features/*/hooks/*", "@/components/*"],
              message: "Feature services must remain transport-only and must not depend on hooks, stores, or UI components (FE.md).",
            },
          ],
        },
      ],
    },
  },
  {
    // FE.md: "Follow App Router conventions. Keep app/**/page.tsx as an explicit, thin route component... Default to Server Components."
    files: ["app/**/page.tsx"],
    rules: {
      "no-restricted-imports": [
        "error",
        {
          paths: [
            {
              name: "react",
              importNames: ["useState", "useEffect", "useReducer", "useCallback", "useMemo", "useRef"],
              message:
                "app/**/page.tsx must remain a thin Server Component that renders the owning feature page (FE.md).",
            },
          ],
          patterns: [
            {
              group: ["@/features/*/hooks/*", "@/features/*/services/*", "@/shared/stores/*", "@/shared/lib/apiClient"],
              message:
                "app/**/page.tsx must remain a thin route component and only import the owning feature page component (FE.md).",
            },
          ],
        },
      ],
    },
  },
]);

