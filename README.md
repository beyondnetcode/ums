# UMS - Enterprise User Management System

> [Open standards](./docs/STANDARDS.md) | [Master index](./docs/MASTER_INDEX.md) | [Architecture portal](./docs/architecture/index.md) | [Leer en espanol](./docs/README.es.md)

UMS is the official executable product reference for Evolith. It demonstrates how Evolith standards can be applied in a real modular monolith for unified identity, authorization, audit, governance, and enterprise user management.

UMS owns product-specific implementation evidence. Evolith owns reusable enterprise standards.

---

## Start here

| Need | Go to |
|---|---|
| Find React, Web, C# or .NET standards | [Standards Quick Access](./docs/STANDARDS.md) |
| Understand the architecture | [Architecture Portal](./docs/architecture/index.md) |
| Review UMS architectural decisions | [ADR Registry](./docs/architecture/adrs/index.md) |
| Understand product scope | [Product Vision](./docs/governance/product/product-vision.md) |
| Review requirements | [Requirements Index](./docs/governance/requirements/index.md) |
| Review delivery plan | [MVP Product Backlog](./docs/governance/project/mvp-product-backlog.md) |
| Explore domain design | [Construction Portal](./docs/governance/construction/index.md) |
| Operate or observe UMS | [Operations Portal](./docs/operations/index.md) |
| Browse all documents | [Master Index](./docs/MASTER_INDEX.md) |

---

## Quick paths by role

| Role | Start with | Then read |
|---|---|---|
| Backend engineer | [UMS API .NET Applied Reference](./docs/architecture/api-dotnet/ums-api-dotnet-applied-reference.md) | [Construction Portal](./docs/governance/construction/index.md) |
| Frontend engineer | [UMS React Web Applied Reference](./docs/architecture/web-frontend/ums-react-applied-reference.md) | [Evolith React Web Frontend Standard](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/governance/standards/engineering/web-frontend/react/react-web-frontend-standard.md) |
| Architect | [Architecture Portal](./docs/architecture/index.md) | [Traceability Matrix](./docs/architecture/traceability-matrix.md) |
| Product owner or QA | [Functional Stories](./docs/governance/requirements/functional-stories/index.md) | [MVP Backlog](./docs/governance/project/mvp-product-backlog.md) |
| DevOps or operations | [Operations Portal](./docs/operations/index.md) | [Solution Metrics Dashboard](./docs/operations/metrics/index.md) |
| Governance reviewer | [Standards Quick Access](./docs/STANDARDS.md) | [Architecture Portal](./docs/architecture/index.md) |

---

## Standards and applied references

| Area | UMS applied reference | Upstream Evolith standard |
|---|---|---|
| React Web frontend | [UMS React Web Applied Reference](./docs/architecture/web-frontend/ums-react-applied-reference.md) | [Evolith React Web Frontend Standard](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/governance/standards/engineering/web-frontend/react/react-web-frontend-standard.md) |
| .NET API backend | [UMS API .NET Applied Reference](./docs/architecture/api-dotnet/ums-api-dotnet-applied-reference.md) | [Evolith .NET API Standard](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/governance/standards/engineering/api-dotnet/api-dotnet-standard.md) |
| All standards | [UMS Standards Quick Access](./docs/STANDARDS.md) | [Evolith Quick Access by Stack](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/quick-access/README.md) |

---

## UMS vs Evolith

| Question | UMS | Evolith |
|---|---|---|
| What belongs here? | Product-specific source evidence, applied examples, local decisions, routes, modules, schemas, runtime values | Reusable enterprise standards, principles, ADRs, canonical patterns, quality gates |
| What should not be generalized here? | Local implementation choices that only apply to UMS | Product-specific implementation evidence |
| How does a UMS practice become a standard? | It provides implementation evidence | It is promoted through ADR, governance standard, or canonical pattern |

---

## Technology snapshot

| Layer | Technology |
|---|---|
| Backend | .NET 10, HotChocolate GraphQL, Minimal APIs REST |
| Frontend | React 18, Vite 5, TypeScript, TailwindCSS, Zustand, TanStack Query |
| Database | SQL Server 2022, Entity Framework Core |
| Monorepo | Nx, npm Workspaces |
| Methodology | BMAD-METHOD, Clean Architecture, DDD |

---

## Quick start

### Prerequisites

- Node.js 20+
- .NET 10 SDK
- SQL Server 2022 or Docker

### Backend

```bash
cd src/apps/ums.api
dotnet build
dotnet run
```

### Frontend

```bash
cd src/apps/ums.web-app
npm install
npm run dev
```

### Full stack

```bash
# Terminal 1 - Backend
cd src/apps/ums.api && dotnet run

# Terminal 2 - Frontend
cd src/apps/ums.web-app && npm run dev
```

---

## Repository map

| Area | Entry point | Purpose |
|---|---|---|
| Standards | [docs/STANDARDS.md](./docs/STANDARDS.md) | Direct links to Evolith standards and UMS applied references |
| Architecture | [docs/architecture](./docs/architecture/index.md) | ADRs, blueprints, applied API/Web references, traceability |
| Governance | [docs/governance](./docs/governance/index.md) | Product vision, scope, requirements, delivery documentation |
| Construction | [docs/governance/construction](./docs/governance/construction/index.md) | DDD design, bounded contexts, aggregates, events, commands |
| Operations | [docs/operations](./docs/operations/index.md) | Metrics, runbooks, operational documentation |
| Source | [src/apps](./src/apps) | API and Web application source code |
| Full navigation | [docs/MASTER_INDEX.md](./docs/MASTER_INDEX.md) | Complete document tree |

---

## Security and quality notes

- CSP, CSRF, security headers, and runtime input validation are part of the implementation baseline.
- React Web and .NET API decisions are documented as applied references under `docs/architecture/`.
- Reusable enterprise standards must be promoted upstream to Evolith.

---

## Contribution and governance

Before changing architecture or documentation, check:

- [Standards Quick Access](./docs/STANDARDS.md)
- [Architecture Portal](./docs/architecture/index.md)
- [Master Index](./docs/MASTER_INDEX.md)
- [AGENTS.md](./AGENTS.md)
