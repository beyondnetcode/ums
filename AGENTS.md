## Project
Enterprise Monorepo for User Management System (UMS). An authorization block prototype capable of working with third-party Identity Providers or operating standalone, using .NET 8 LTS, React, and PostgreSQL.

## Build & Run
> [!IMPORTANT]
> The technical engine of this monorepo is located in `src/`. All technical commands must be executed relative to that directory.

Commands for Frontend (run from `src/`):
- Frontend Install: `npm install`
- Frontend Start: `npx nx run app-web:dev`
- Setup Docs Context (Context7): `npx ctx7 setup`
- Markdown Encoding Sanitation: `python ../.bmad-core/scripts/cleanup_markdown_encoding.py`

Commands for Backend (run from within `./src/apps/app-api-dotnet/` or root solution directory):
- Backend Build: `dotnet build`
- Backend Test: `dotnet test`
- Backend Run: `dotnet run`

## Architecture
- Runtime: **.NET 8 LTS** (Backend) and React v18 + Vite (Frontend).
- Monorepo: Managed via Nx, npm Workspaces (Frontend) and standard .NET SLN.
- DB: PostgreSQL 16 + Entity Framework Core (EF Core).
- Key Modules: `src/apps/app-api-dotnet` (.NET Backend), `src/apps/app-web` (Frontend React Portal).
- Pattern: Clean Architecture (Hexagonal), SOLID, Explicit Bounded Contexts.

## Conventions
- **Engineering Patterns**: Adhere to **Clean Architecture** (Hexagonal), **SOLID** principles, and strict **DDD** bounded contexts.
- **AI-Driven Strategy**: Utilize the **BMAD-METHOD** for spec-driven development and numerical sequential documentation (Phases 00 to 05).
- **Domain Purity**: Strictly isolate Domain rules from external frameworks.
  - JS/TS: Hexagonal boundaries and strict linting.
  - C#: `{BoundedContext}.Domain` project must be pure POCOs with zero NuGet references.
- **Flow Control**: Utilize the **Result Pattern** instead of throwing application exceptions for domain flow control.
- Enforce strict TypeScript and C# types with static analysis gates (SonarJS).

## Agent Rules
- NEVER delete or bypass existing tests to make a fix pass.
- Before updating dependencies, verify strict dependency pinning.
- If modifying core logic, ensure architectural traceability to approved ADRs.
- Keep formatting clean, adhering to ESLint and Prettier configs in the workspace.
- **BMAD Rule Compliance:** Any agent working on this repository MUST read, prioritize, and strictly enforce the 14 rules defined in `.bmad-core/rules/global-rules.md`, `.bmad-core/rules/structuring-standard.md` (R-13), and `.harness/rules/project-rules.yaml`. If encoding artifacts (mojibake) or non-standard decorative characters (emojis/icons) are detected, the agent MUST run the appropriate cleanup utilities immediately to enforce rules R-03 and R-14. No code or documentation commits are permitted without validating against these rules.
- **Multi-language Synchronization & Diagram Validation:** Whenever documentation is updated, the agent MUST ensure that both English and Spanish versions are synchronized in content, technical precision, and clarity. Additionally, all Mermaid diagrams MUST be validated for syntax and structural correctness before any commit.
- **Context Retrieval:** Always use **Context7** (`npx ctx7`) to fetch updated, version-specific documentation for third-party libraries before implementing complex external integrations.
- **Corporate Standards Alignment:** Any agent making architectural design decisions MUST query the **Corporate Reference** via Context7 (`use context7 for beyondnetcode/arc32_progresive_monolith`) to ensure absolute compliance with baseline polyglot standards and authoritative patterns.

## Out of Bounds
- DO NOT modify CI/CD GitHub workflows.
- DO NOT edit Git hooks (Husky configuration).
- DO NOT modify core corporate library standards unless authorized.
