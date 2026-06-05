# UMS Web Console

> Language: [English](./README.md) | [Español](./README.es.md)

UMS Web Console is the React 18 portal for the User Management System administrative experience. It is the primary frontend for operators and administrators who manage tenants, authorization, configuration, approvals, and audit workflows.

## Quick Links

| Need                         | Open this                                                         |
| ---------------------------- | ----------------------------------------------------------------- |
| Root README                  | [Repository overview](../../../README.md)                         |
| English documentation portal | [docs/README.md](../../../docs/README.md)                         |
| Spanish documentation portal | [docs/README.es.md](../../../docs/README.es.md)                   |
| Architecture portal          | [docs/architecture/index.md](../../../docs/architecture/index.md) |
| Governance portal            | [docs/governance/index.md](../../../docs/governance/index.md)     |

## At a Glance

| Area           | Choice                                              |
| -------------- | --------------------------------------------------- |
| Runtime        | React 18 + Vite + TypeScript                        |
| State and data | Zustand, TanStack Query                             |
| Styling        | Tailwind CSS                                        |
| Purpose        | Administrative portal for UMS operators and tenants |

## Local Workflow

All technical commands should be run from `src/`.

```bash
cd src
npm install
npx nx run app-web:dev
```

## Notes

- This module README stays short and navigable.
- Product and architecture detail belongs in `docs/`.
- Bilingual documentation should remain synchronized with the Spanish mirror.
