## Project
Enterprise Monorepo for User Management System (UMS). An authorization block prototype capable of working with third-party Identity Providers or operating standalone, using .NET 8 LTS, React, and PostgreSQL.

## Build & Run
Commands for Frontend (run from within `./src/ums-workspace/`):
- Frontend Install: `npm install`
- Frontend Start: `npx nx run apps-web:dev`
- Setup Docs Context (Context7): `npx ctx7 setup`

Commands for Backend (run from within `./src/ums-workspace/apps/api-dotnet/` or root solution directory):
- Backend Build: `dotnet build`
- Backend Test: `dotnet test`
- Backend Run: `dotnet run`

## Architecture
- Runtime: **.NET 8 LTS** (Backend) and React v18 + Vite (Frontend).
- Monorepo: Managed via Nx, npm Workspaces (Frontend) and standard .NET SLN.
- DB: PostgreSQL 16 + Entity Framework Core (EF Core).
- Key Modules: `apps/api-dotnet` (Pending - .NET Backend Solution), `apps/web` (Frontend React Portal).
- Pattern: Clean Architecture (Hexagonal), SOLID, Explicit Bounded Contexts.

## Conventions
- Adhere to the **spec-driven AI strategy BMAD-METHOD** for numerical sequential documentation (Phases 00 to 05).
- Strictly isolate Domain rules from external frameworks.
  - JS/TS: Hexagonal boundaries and strict linting.
  - C#: `{BoundedContext}.Domain` project must be pure POCOs with zero NuGet references.
- Utilize the **Result Pattern** instead of throwing application exceptions for domain flow control.
- Enforce strict TypeScript and C# types with static analysis gates (SonarJS).

## Agent Rules
- NEVER delete or bypass existing tests to make a fix pass.
- Before updating dependencies, verify strict dependency pinning.
- If modifying core logic, ensure architectural traceability to approved ADRs.
- Keep formatting clean, adhering to ESLint and Prettier configs in the workspace.
- **BMAD Rule Compliance:** Any agent working on this repository MUST read, prioritize, and strictly enforce the 12 rules defined in `bmad-core/rules/global-rules.md` and `.bmad/rules/project-rules.yaml`. No code or documentation commits are permitted without validating against these rules.
- **Context Retrieval:** Always use **Context7** (`npx ctx7`) to fetch updated, version-specific documentation for third-party libraries before implementing complex external integrations.
- **Corporate Standards Alignment:** Any agent making architectural design decisions MUST query the **Corporate Reference** via Context7 (`use context7 for beyondnetcode/arc32_progresive_monolith`) to ensure absolute compliance with baseline polyglot standards and authoritative patterns.

## Out of Bounds
- DO NOT modify CI/CD GitHub workflows.
- DO NOT edit Git hooks (Husky configuration).
- DO NOT modify core corporate library standards unless authorized.
