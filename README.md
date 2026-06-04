# UMS - Enterprise User Management System

UMS is a modular monolith for identity, authorization, configuration, approvals, compliance, IGA, and audit. The repository follows the Evolith baseline and documents UMS-specific evidence, local decisions, and ADR-backed deviations in `docs/`.

## Acceso rápido por idioma

| Entry | Open this |
|---|---|
| Read in English | [docs/README.md](./docs/README.md) |
| Leer en Español | [docs/README.es.md](./docs/README.es.md) |
| Master Index | [docs/MASTER_INDEX.md](./docs/MASTER_INDEX.md) |
| Índice Maestro | [docs/MASTER_INDEX.es.md](./docs/MASTER_INDEX.es.md) |
| Documentation Portal | [docs/README.md](./docs/README.md) |
| Portal Documental | [docs/README.es.md](./docs/README.es.md) |
| Standards | [docs/STANDARDS.md](./docs/STANDARDS.md) |
| Estándares | [docs/STANDARDS.es.md](./docs/STANDARDS.es.md) |

## Fast Routes

| Need | Open this |
|---|---|
| Standards | [STANDARDS.md](./docs/STANDARDS.md) / [STANDARDS.es.md](./docs/STANDARDS.es.md) |
| Full English documentation map | [MASTER_INDEX.md](./docs/MASTER_INDEX.md) |
| Full Spanish documentation map | [MASTER_INDEX.es.md](./docs/MASTER_INDEX.es.md) |
| Short navigation by team or goal | [Quick Navigation](./docs/governance/quick-navigation.md) |
| Architecture portal | [Architecture Portal](./docs/architecture/index.md) |
| Governance portal | [Governance Portal](./docs/governance/index.md) |

## At a Glance

| Area | Authoritative choice |
|---|---|
| Backend | .NET 10, SQL Server 2022, EF Core |
| Frontend | React 18, Vite, TypeScript |
| Monorepo | Nx, npm workspaces |
| Delivery method | BMAD-METHOD, Clean Architecture, DDD |
| Multi-tenancy | Application-layer tenant filtering first, SQL Server RLS as secondary failsafe |

## Local Workflows

All technical commands should be run from `src/`.

### Frontend

```bash
cd src
npm install
npx nx run app-web:dev
```

### Backend

```bash
cd src/apps/ums.api
dotnet build
dotnet run
```

### Validation and context

```bash
cd src
python ../.bmad-core/scripts/validate_docs_consistency.py README.md docs/
```

## Documentation Map

| Portal | Purpose |
|---|---|
| [Standards Quick Access](./docs/STANDARDS.md) | UMS applied references and Evolith standards |
| [Master Index](./docs/MASTER_INDEX.md) | English lifecycle map of the documentation tree |
| [Índice Maestro](./docs/MASTER_INDEX.es.md) | Spanish lifecycle map of the documentation tree |
| [Governance Portal](./docs/governance/index.md) | Product direction, requirements, backlog, and construction |
| [Architecture Portal](./docs/architecture/index.md) | Architecture, blueprints, ADRs, and traceability |
| [Operations Portal](./docs/operations/index.md) | Runbooks, metrics, and operational guidance |

## Governance Notes

- Bilingual documentation must stay synchronized.
- Markdown files must remain clean, professional, and free of decorative icons.
- Architecture decisions must stay aligned with the approved ADRs and the Evolith baseline.
- Reusable enterprise standards belong to Evolith; UMS documents applied evidence and approved local deviations.
- Root documentation should stay short and navigable. Detailed content belongs in `docs/`.

## License

See [LICENSE](./LICENSE) and [NOTICE](./NOTICE).
