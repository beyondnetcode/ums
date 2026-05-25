# UMS — Enterprise User Management System

> **[ 📚 OPEN MASTER INDEX ](./docs/MASTER_INDEX.md) • [ 🇪🇸 LEER EN ESPAÑOL ](./docs/README.es.md) • [ ⚙️ ARCHITECTURE PORTAL ](./docs/architecture/index.md)**  
> *Note: GitHub displays source files first. To skip the code and read the documentation, click the links above.*

---

> **Standardized Modular Monolith for Unified Identity & Authorization.**
>
> ![Status](https://img.shields.io/badge/Status-Active-success) ![Architecture](https://img.shields.io/badge/Architecture-Modular_Monolith-blue) ![Methodology](https://img.shields.io/badge/Methodology-BMAD--METHOD-success)

---

## Master Navigation Index
Start here if you are new to UMS. This index gives each reader a fast route into the repository without needing to know the folder structure.

### Quick Route by Persona

| I am a… | Start here | Then read |
| :--- | :--- | :--- |
| **Backend Engineer** | [Construction Portal](./docs/governance/construction/index.md) · [Domain Aggregate Index](./docs/domain/index.md) | [DDD Design Portal](./docs/governance/construction/ddd-design/index.md) → [Bounded Context Map](./docs/governance/construction/ddd-design/01-bounded-context-map.md) |
| **Architect** | [Architecture Portal](./docs/architecture/index.md) · [ADR Registry](./docs/architecture/adrs/index.md) | [Bounded Context Map](./docs/governance/construction/ddd-design/01-bounded-context-map.md) → [Traceability Matrix](./docs/architecture/traceability-matrix.md) |
| **Product Owner / QA** | [Master Index](./docs/MASTER_INDEX.md) · [Functional Stories](./docs/governance/requirements/functional-stories/index.md) | [MVP Backlog](./docs/governance/project/mvp-product-backlog.md) → [Glossary](./docs/governance/requirements/glossary.md) |
| **DevOps / Ops** | [Operations Portal](./docs/operations/index.md) · [Runbooks](./docs/operations/runbooks/) | [ADR-0053 OpenTelemetry](./docs/architecture/adrs/0053-opentelemetry-observability.md) → [ADR-0054 Shell Library Isolation](./docs/architecture/adrs/0054-shell-library-isolation.md) |

### I want to…

| Goal | Start Here | Then Read |
| :--- | :--- | :--- |
| Understand the product | [Product Vision](./docs/governance/product/product-vision.md) | [Business Context](./docs/governance/product/business-context.md) → [Scope](./docs/governance/product/scope.md) |
| See Epics & Priorities | [MVP Product Backlog](./docs/governance/project/mvp-product-backlog.md) | [Requirements Index](./docs/governance/requirements/index.md) → [Functional Stories](./docs/governance/requirements/functional-stories/index.md) |
| Review functional requirements | [Requirements Index](./docs/governance/requirements/index.md) | [Functional Stories](./docs/governance/requirements/functional-stories/index.md) → [Glossary](./docs/governance/requirements/glossary.md) |
| Validate the data and domain model | [Conceptual Data Model](./docs/governance/requirements/conceptual-data-model.md) | [ER Export Formats](./docs/architecture/blueprints/er-export-formats.md) → [Database Design ER](./docs/architecture/blueprints/database-design-er.md) |
| Understand the architecture | [Architecture Portal](./docs/architecture/index.md) | [Database Design ER](./docs/architecture/blueprints/database-design-er.md) |
| Browse everything | [Master Index](./docs/MASTER_INDEX.md) | Complete document tree by lifecycle phase. |

---

## Architecture Overview

### Technology Stack
| Layer | Technology |
| :--- | :--- |
| **Backend** | .NET 8 LTS, HotChocolate (GraphQL), Minimal APIs (REST) |
| **Frontend** | React 18, Vite 5, TypeScript, TailwindCSS, Zustand, TanStack Query |
| **Database** | SQL Server 2022, Entity Framework Core 8 |
| **Monorepo** | Nx, npm Workspaces |
| **Methodology** | BMAD-METHOD, Clean Architecture (Hexagonal), DDD |

### Project Structure
```
src/
├── apps/
│   ├── ums.api/                    # .NET Backend (Clean Architecture)
│   │   ├── Domain/                 # Pure POCOs, zero NuGet references
│   │   ├── Application/            # Use cases, interfaces
│   │   ├── Infrastructure/         # EF Core, external services
│   │   └── Presentation/           # GraphQL/REST endpoints
│   └── ums.web-app/                # React Frontend (Clean Architecture)
│       ├── src/
│       │   ├── domain/             # Enterprise entities, value objects
│       │   ├── application/        # Hooks, stores, use cases
│       │   ├── infrastructure/     # HTTP clients, GraphQL client
│       │   └── presentation/       # Components, screens, layouts
│       └── ...
└── ...
```

### Key Architectural Decisions
- **GraphQL for Queries, REST for Commands**: All read operations use HotChocolate GraphQL; writes use REST Minimal APIs for transactional clarity.
- **Clean Architecture**: Strict layer boundaries. Domain layer is pure (no external dependencies). Application layer contains use cases and interfaces. Infrastructure handles external concerns.
- **Result Pattern**: No exceptions for flow control. All operations return `Result<T>` for explicit error handling.
- **Bounded Contexts**: Identity, Access, Audit, etc. Each context has its own aggregates, services, and presentation modules.

---

## Quick Start (Engine Room)

### Prerequisites
- Node.js 20+
- .NET 8 SDK
- SQL Server 2022 (or Docker: `docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123!" -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest`)

### Frontend
```bash
cd src/apps/ums.web-app
npm install
npm run dev
```

### Backend
```bash
cd src/apps/ums.api
dotnet build
dotnet run
```

### Full Stack (Frontend + Backend)
```bash
# Terminal 1 — Backend (port 7114)
cd src/apps/ums.api && dotnet run

# Terminal 2 — Frontend (port 5173)
cd src/apps/ums.web-app && npm run dev
```

---

## Development Commands

| Command | Description |
| :--- | :--- |
| `npm install` | Install all frontend dependencies (run from `src/apps/ums.web-app`) |
| `npm run dev` | Start frontend dev server (port 5173) |
| `npm run build` | Build frontend for production |
| `npm run lint` | Run ESLint |
| `npm run test` | Run Vitest tests |
| `dotnet build` | Build backend solution (run from `src/apps/ums.api`) |
| `dotnet test` | Run backend tests |
| `dotnet run` | Start backend API (port 7114) |

---

## Knowledge Hub
| Domain | Portal Index | Contents |
| :--- | :--- | :--- |
| **Governance** | [Governance Portal](./docs/governance/index.md) | Product direction, business scope, and functional requirements. |
| **Project Delivery** | [Project Backlog](./docs/governance/project/index.md) | MVP epics, user stories, and functional design of core modules. |
| **Requirements** | [Requirements Index](./docs/governance/requirements/index.md) | Functional stories, business glossary, and conceptual data model. |
| **Architecture** | [Architecture Portal](./docs/architecture/index.md) | Database ER design, entity maps, and interactive viewers. |
| **Construction** | [Construction Portal](./docs/governance/construction/index.md) | DDD domain layer design (bounded contexts, aggregates, events, commands). |
| **Metrics** | [Solution Metrics Dashboard](./docs/operations/metrics/index.md) | Engineering metrics by solution type: coding, security, quality, tests, AI usage. |

---

## Security & Compliance

- **Content Security Policy**: Restrictive CSP with `unsafe-eval` removed (production-ready).
- **CSRF Protection**: Double-submit cookie pattern with token refresh.
- **Security Headers**: HSTS, X-Frame-Options, X-Content-Type-Options, Referrer-Policy via Nginx.
- **Input Validation**: Zod schemas as single source of truth for runtime validation.

---

## Contribution & Governance
- **Workflow**: This repo uses [BMAD-METHOD](./AGENTS.md) for spec-driven documentation.
- **Navigation**: Visit the [**Master Index**](./docs/MASTER_INDEX.md) for the full document tree.
- **Code Standards**: ESLint + TypeScript strict mode. Zero errors required before commit.
- **Testing**: Vitest with React Testing Library. Coverage thresholds: 60% lines/statements.
