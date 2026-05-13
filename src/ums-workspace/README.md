# UMS Monorepo Workspace

> **Language:** [English](./README.md) | [Espanol](./README.es.md)

Nx workspace for the User Management System -- .NET 8 backend and React frontend.

## Tech Stack

- **Frontend:** React 18 / Vite / Zustand / TanStack Query
- **Backend:** .NET 8 LTS / ASP.NET Core / MediatR / FluentValidation
- **Data:** PostgreSQL 16 / EF Core (Npgsql)
- **Tooling:** Nx / npm Workspaces / CSharpier / ESLint / CodeQL

## Quick Start

### Frontend
```bash
npm install
npx nx run apps-web:dev
```

### Backend (from apps/api-dotnet/)
```bash
dotnet build
dotnet test
dotnet run --project Ums.Presentation
```

## Documentation

- [English Knowledge Base](./docs/en/index.md)
- [Base de Conocimientos en Espanol](./docs/es/index.md)
