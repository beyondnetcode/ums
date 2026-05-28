# UMS - Sistema de Gestion de Usuarios Empresarial

> [Abrir estandares](./STANDARDS.es.md) | [Indice maestro](./MASTER_INDEX.es.md) | [Portal de arquitectura](./architecture/index.es.md) | [Read in English](../README.md)

UMS es la referencia ejecutable oficial de producto para Evolith. Demuestra como aplicar estandares Evolith en un monolito modular real para identidad, autorizacion, auditoria, gobierno y gestion empresarial de usuarios.

UMS posee la evidencia de implementacion especifica del producto. Evolith posee los estandares empresariales reutilizables.

---

## Empieza aqui

| Necesidad | Ir a |
|---|---|
| Encontrar estandares React, Web, C# o .NET | [Acceso rapido a estandares](./STANDARDS.es.md) |
| Entender la arquitectura | [Portal de arquitectura](./architecture/index.es.md) |
| Revisar decisiones arquitectonicas UMS | [Registro ADR](./architecture/adrs/index.md) |
| Entender el alcance del producto | [Vision del producto](./governance/product-es/product-vision.md) |
| Revisar requerimientos | [Indice de requerimientos](./governance/requirements-es/index.md) |
| Revisar plan de entrega | [MVP Product Backlog](./governance/project-es/mvp-product-backlog.md) |
| Explorar diseno de dominio | [Portal de construccion](./governance/construction/index.es.md) |
| Operar u observar UMS | [Portal de operaciones](./operations/index.md) |
| Explorar todos los documentos | [Indice maestro](./MASTER_INDEX.es.md) |

---

## Rutas rapidas por rol

| Rol | Empezar con | Luego leer |
|---|---|---|
| Ingeniero backend | [Referencia aplicada API .NET UMS](./architecture/api-dotnet/ums-api-dotnet-applied-reference.es.md) | [Portal de construccion](./governance/construction/index.es.md) |
| Ingeniero frontend | [Referencia aplicada React Web UMS](./architecture/web-frontend/ums-react-applied-reference.es.md) | [Estandar Web Frontend React Evolith](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/governance/standards/engineering/web-frontend/react/react-web-frontend-standard.es.md) |
| Arquitecto | [Portal de arquitectura](./architecture/index.es.md) | [Matriz de trazabilidad](./architecture/traceability-matrix.md) |
| Product owner o QA | [Historias funcionales](./governance/requirements-es/functional-stories/index.md) | [MVP Product Backlog](./governance/project-es/mvp-product-backlog.md) |
| DevOps u operaciones | [Portal de operaciones](./operations/index.md) | [Dashboard de metricas](./operations/metrics/index.md) |
| Revisor de gobierno | [Acceso rapido a estandares](./STANDARDS.es.md) | [Portal de arquitectura](./architecture/index.es.md) |

---

## Estandares y referencias aplicadas

| Area | Referencia aplicada UMS | Estandar upstream Evolith |
|---|---|---|
| Frontend React Web | [Referencia aplicada React Web UMS](./architecture/web-frontend/ums-react-applied-reference.es.md) | [Estandar Web Frontend React Evolith](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/governance/standards/engineering/web-frontend/react/react-web-frontend-standard.es.md) |
| Backend API .NET | [Referencia aplicada API .NET UMS](./architecture/api-dotnet/ums-api-dotnet-applied-reference.es.md) | [Estandar API .NET Evolith](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/governance/standards/engineering/api-dotnet/api-dotnet-standard.es.md) |
| Todos los estandares | [Acceso rapido a estandares UMS](./STANDARDS.es.md) | [Acceso rapido por stack Evolith](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/quick-access/README.es.md) |

---

## UMS vs Evolith

| Pregunta | UMS | Evolith |
|---|---|---|
| Que pertenece aqui? | Evidencia de fuente especifica del producto, ejemplos aplicados, decisiones locales, rutas, modulos, schemas, valores runtime | Estandares empresariales reutilizables, principios, ADRs, patrones canonicos, quality gates |
| Que no debe generalizarse aqui? | Decisiones locales que solo aplican a UMS | Evidencia de implementacion especifica de producto |
| Como una practica UMS se vuelve estandar? | Aporta evidencia de implementacion | Se promueve mediante ADR, estandar de gobierno o patron canonico |

---

## Snapshot tecnologico

| Capa | Tecnologia |
|---|---|
| Backend | .NET 10, HotChocolate GraphQL, Minimal APIs REST |
| Frontend | React 18, Vite 5, TypeScript, TailwindCSS, Zustand, TanStack Query |
| Base de datos | SQL Server 2022, Entity Framework Core |
| Monorepo | Nx, npm Workspaces |
| Metodologia | BMAD-METHOD, Arquitectura Limpia, DDD |

---

## Inicio rapido

### Prerrequisitos

- Node.js 20+
- .NET 10 SDK
- SQL Server 2022 o Docker

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

## Mapa del repositorio

| Area | Entrada | Proposito |
|---|---|---|
| Estandares | [docs/STANDARDS.es.md](./STANDARDS.es.md) | Enlaces directos a estandares Evolith y referencias aplicadas UMS |
| Arquitectura | [docs/architecture](./architecture/index.es.md) | ADRs, blueprints, referencias aplicadas API/Web, trazabilidad |
| Gobernanza | [docs/governance](./governance/index.es.md) | Vision, alcance, requerimientos y documentacion de entrega |
| Construccion | [docs/governance/construction](./governance/construction/index.es.md) | Diseno DDD, bounded contexts, agregados, eventos, comandos |
| Operaciones | [docs/operations](./operations/index.md) | Metricas, runbooks y documentacion operacional |
| Fuente | [src/apps](../src/apps) | Codigo fuente API y Web |
| Navegacion completa | [docs/MASTER_INDEX.es.md](./MASTER_INDEX.es.md) | Arbol documental completo |

---

## Notas de seguridad y calidad

- CSP, CSRF, security headers y validacion runtime forman parte del baseline de implementacion.
- Las decisiones React Web y API .NET estan documentadas como referencias aplicadas en `docs/architecture/`.
- Los estandares empresariales reutilizables deben promoverse upstream a Evolith.

---

## Contribucion y gobierno

Antes de cambiar arquitectura o documentacion, revisa:

- [Acceso rapido a estandares](./STANDARDS.es.md)
- [Portal de arquitectura](./architecture/index.es.md)
- [Indice maestro](./MASTER_INDEX.es.md)
- [AGENTS.md](../AGENTS.md)
