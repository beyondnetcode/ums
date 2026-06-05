# UMS - Enterprise User Management System

> Language: [English](./README.md) | [Español](./docs/README.es.md)

UMS is a modular monolith for identity, authorization, configuration, approvals, compliance, IGA, and audit. It is an applied implementation based on Evolith: reusable enterprise standards live upstream, while UMS documents product-specific evidence, local decisions, and ADR-backed deviations in `docs/`.

## Documentation Entry Points

| Need | Open this |
|---|---|
| English documentation portal | [docs/README.md](./docs/README.md) |
| Portal documental en español | [docs/README.es.md](./docs/README.es.md) |
| Master Index | [docs/MASTER_INDEX.md](./docs/MASTER_INDEX.md) |
| Índice Maestro | [docs/MASTER_INDEX.es.md](./docs/MASTER_INDEX.es.md) |
| Standards | [docs/STANDARDS.md](./docs/STANDARDS.md) |
| Estándares | [docs/STANDARDS.es.md](./docs/STANDARDS.es.md) |

## Fast Routes

| Need | Open this |
|---|---|
| Short navigation by team or goal | [Quick Navigation](./docs/governance/quick-navigation.md) |
| Architecture portal | [Architecture Portal](./docs/architecture/index.md) |
| Portal de Arquitectura | [Portal de Arquitectura](./docs/architecture/index.es.md) |
| Governance portal | [Governance Portal](./docs/governance/index.md) |
| Portal de Gobernanza | [Portal de Gobernanza](./docs/governance/index.es.md) |
| Operations portal | [Operations Portal](./docs/operations/index.md) |
| Portal de Operaciones | [Portal de Operaciones](./docs/operations/index.es.md) |

## At a Glance

| Area | Authoritative choice |
|---|---|
| Backend | .NET 10, SQL Server 2022, EF Core |
| Frontend | React 18, Vite, TypeScript |
| Monorepo | Nx, npm workspaces |
| Delivery method | BMAD-METHOD, Clean Architecture, DDD |
| Multi-tenancy | Application-layer tenant filtering first, SQL Server RLS as a secondary failsafe |

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

## What to Expect

- The root README stays short and executive.
- `docs/README.md` and `docs/README.es.md` are the bilingual home pages for documentation.
- `docs/MASTER_INDEX.md` and `docs/MASTER_INDEX.es.md` are the full lifecycle maps.
- UMS-specific evidence belongs in `docs/`, while reusable standards remain in Evolith.

## Governance Notes

- Bilingual documentation must stay synchronized.
- Markdown files must remain clean, professional, and free of decorative icons.
- Architecture decisions must stay aligned with the approved ADRs and the Evolith baseline.
- Reusable enterprise standards belong to Evolith; UMS documents applied evidence and approved local deviations.
- Root documentation should stay short and navigable. Detailed content belongs in `docs/`.

## License

See [LICENSE](./LICENSE) and [NOTICE](./NOTICE).
