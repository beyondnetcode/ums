# Sistema de Gestion de Usuarios (UMS)

> **Idioma:** [English](./README.md) | [Espanol](./README.es.md)

Nucleo de autorizacion multi-inquilino construido con .NET 8 LTS y React, siguiendo Arquitectura Limpia y Domain-Driven Design.

## Stack Tecnologico

| Componente | Tecnologia |
|-----------|-----------|
| Backend | .NET 8 LTS / ASP.NET Core / EF Core |
| Frontend | React 18 / Vite / Zustand / TanStack Query |
| Base de Datos | PostgreSQL 16 con Row-Level Security |
| Monorepo | Nx / npm Workspaces / .NET SLN |
| Calidad | CodeQL / ESLint / SonarJS / CSharpier |

## Inicio Rapido

```bash
cd src/ums-workspace

# Frontend
npm install
npx nx run apps-web:dev

# Backend (desde apps/api-dotnet/)
dotnet build
dotnet run --project Ums.Presentation
```

## Documentacion

- [Indice Maestro](./MASTER_INDEX.es.md) -- Hub de navegacion por fases
- [Base de Conocimiento en Ingles](./src/ums-workspace/docs/en/index.md)
- [Base de Conocimientos en Espanol](./src/ums-workspace/docs/es/index.md)

## Referencias Arquitectonicas

Este proyecto se basa en la [Arquitectura de Referencia Arc32 Progressive Monolith](https://github.com/beyondnetcode/arc32_progresive_monolith) corporativa y sigue la estrategia **BMAD-METHOD** (Fases 00-05) para la gobernanza de documentacion, diseno e implementacion.
