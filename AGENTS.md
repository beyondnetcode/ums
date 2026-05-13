## Project
Enterprise Monorepo for User Management System (UMS). An authorization block prototype capable of working with third-party Identity Providers or operating standalone, using NestJS, React, and PostgreSQL.

## Build & Run
All commands must be run from within the `./src/ums-workspace/` directory:
- Build: `npm run build`
- Test: `npm run test`
- Lint: `npm run lint`
- Start API: `npx nx run api:serve`
- Start Client: `npx nx run apps-web:dev`
- Setup Docs Context (Context7): `npx ctx7 setup`

## Architecture
- Runtime: Node.js v20+ (LTS) with NestJS v10 (Backend) and React v18 + Vite (Frontend).
- Monorepo: Managed via Nx & npm Workspaces.
- DB: PostgreSQL 16 + TypeORM.
- Key Modules: `apps/api` (Backend), `apps/web` (Frontend), `libs/aop` (Aspect-Oriented Programming for Observability).
- Pattern: Clean Architecture (Hexagonal), SOLID, Explicit Bounded Contexts.

## Conventions
- Adhere to the **bMAD Method** for numerical sequential documentation (Phases 00 to 05).
- Strictly isolate domain rules from external frameworks (Hexagonal boundaries).
- Enforce strict TypeScript standards and type safety.
- Every PR must pass static analysis (SonarJS) and dependency security gates.

## Agent Rules
- NEVER delete or bypass existing tests to make a fix pass.
- Before updating dependencies, verify strict dependency pinning.
- If modifying core logic, ensure architectural traceability to approved ADRs.
- Keep formatting clean, adhering to ESLint and Prettier configs in the workspace.
- **Context Retrieval:** Always use **Context7** (`npx ctx7`) to fetch updated, version-specific documentation for third-party libraries before implementing complex external integrations to eliminate API hallucinations.

## Out of Bounds
- DO NOT modify CI/CD GitHub workflows.
- DO NOT edit Git hooks (Husky configuration).
- DO NOT modify core corporate library standards unless authorized.
