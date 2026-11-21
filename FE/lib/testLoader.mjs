import { registerHooks } from "node:module";
import { existsSync } from "node:fs";
import { resolve as resolvePath } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";

const rootDir = resolvePath(fileURLToPath(new URL(".", import.meta.url)), "..");

function resolveCandidate(basePath) {
  const candidates = [
    basePath,
    `${basePath}.ts`,
    `${basePath}.tsx`,
    `${basePath}.js`,
    resolvePath(basePath, "index.ts"),
    resolvePath(basePath, "index.tsx"),
  ];

  for (const candidate of candidates) {
    if (existsSync(candidate)) {
      return pathToFileURL(candidate).href;
    }
  }

  return null;
}

registerHooks({
  resolve(specifier, context, nextResolve) {
    if (specifier.startsWith("@/")) {
      const targetPath = resolvePath(rootDir, specifier.slice(2));
      const resolvedUrl = resolveCandidate(targetPath);
      if (resolvedUrl) {
        return { url: resolvedUrl, shortCircuit: true };
      }
    }

    return nextResolve(specifier, context);
  },
});
