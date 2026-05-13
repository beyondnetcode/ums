# User Management System (UMS)

> **Language:** [English](./README.md) | [Español](./README.es.md)

Multi-tenant authorization kernel built with .NET 8 LTS and React, following Clean Architecture and Domain-Driven Design.

## Tech Stack

| Component | Technology |
|-----------|-----------|
| Backend | .NET 8 LTS / ASP.NET Core / EF Core |
| Frontend | React 18 / Vite / Zustand / TanStack Query |
| Database | PostgreSQL 16 with Row-Level Security |
| Monorepo | Nx / npm Workspaces / .NET SLN |
| Quality | CodeQL / ESLint / SonarJS / CSharpier |

## Quick Start

```bash
cd src/ums-workspace

# Frontend
npm install
npx nx run apps-web:dev

# Backend (from apps/api-dotnet/)
dotnet build
dotnet run --project Ums.Presentation
```

## Documentation

- [Master Index](./MASTER_INDEX.md) -- Phase-based navigation hub
- [English Knowledge Base](./src/ums-workspace/docs/en/index.md)
- [Base de Conocimientos en Espanol](./src/ums-workspace/docs/es/index.md)

## Architectural References

This project is built upon the [Arc32 Progressive Monolith](https://github.com/beyondnetcode/arc32_progresive_monolith) corporate reference architecture and follows the **BMAD-METHOD** spec-driven AI strategy (Phases 00-05) for documentation, design, and implementation governance.
