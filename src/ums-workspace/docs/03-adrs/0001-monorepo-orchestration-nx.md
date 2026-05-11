# ADR 0001: Monorepo Orchestration with Nx and npm Workspaces

## Status
Accepted

## Date
2026-05-08

## Context
As the UMS monorepo grows, running sequential builds, lints, and tests across different applications (`apps/api` and `apps/web`) becomes a significant bottleneck for developers and CI/CD pipelines. We initially set up plain npm Workspaces, which handles hoisting and symlinking of packages perfectly but lacks smart task execution, parallelism, and caching capabilities.

Additionally, our development team is constrained to **Node v18.20.6**, which prevents the use of modern engines like `rolldown` or newer `@nx/vite` automatic plugins that depend on Node 20+ utilities (such as `styleText` in `node:util`).

## Decision
We decided to adopt a **hybrid monorepo architecture**:
1. Use **npm Workspaces** as the physical package and dependency manager (managing hoisting and symlinks).
2. Use **Nx** as the smart, high-performance task runner on top of standard package scripts.
3. Configure **`nx.json`** with custom `targetDefaults` to enable caching for `build`, `test`, and `lint` operations.
4. Disable automatic plugin dependency injections (keeping `plugins: []` empty in `nx.json`) to guarantee 100% compatibility with **Node v18** by running package-level scripts.

## Consequences

### Positive (Pros)
* **Blazing Fast Builds**: Second-run builds are retrieved instantly from the local cache (`[existing outputs match the cache, left as is]`), reducing compilation time to under 1 second.
* **Task Parallelism**: Nx automatically executes tasks concurrently across different workspaces (e.g., building `api` and `apps-web` together).
* **Node 18 Compatibility**: By running native scripts, we completely bypass the Node 20+ requirements of modern Nx plugins, achieving absolute stability.
* **No Vendor Lock-in**: Developers can still run standard `npm run build` or native workspace commands if needed.

### Negative (Cons)
* Slight learning curve for developers to use `npx nx run-many --target=<task>` instead of plain npm commands.
* Cache files must be managed and ignored in Git (already handled in `.gitignore`).
