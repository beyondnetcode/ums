# Workspace del Monorepo UMS

> **Idioma:** [English](./README.md) | [Espanol](./README.es.md)

Workspace Nx para el Sistema de Gestion de Usuarios -- backend .NET 8 y frontend React.

## Stack Tecnologico

- **Frontend:** React 18 / Vite / Zustand / TanStack Query
- **Backend:** .NET 8 LTS / ASP.NET Core / MediatR / FluentValidation
- **Datos:** PostgreSQL 16 / EF Core (Npgsql)
- **Herramientas:** Nx / npm Workspaces / CSharpier / ESLint / CodeQL

## Inicio Rapido

### Frontend
```bash
npm install
npx nx run apps-web:dev
```

### Backend (desde apps/api-dotnet/)
```bash
dotnet build
dotnet test
dotnet run --project Ums.Presentation
```

## Documentacion

- [Base de Conocimiento en Ingles](./docs/en/index.md)
- [Base de Conocimientos en Espanol](./docs/es/index.md)
