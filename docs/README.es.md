# UMS — Sistema Empresarial de Gestión de Usuarios

> **[ABRIR ESTÁNDARES](./STANDARDS.md) • [ABRIR ÍNDICE MAESTRO](./MASTER_INDEX.es.md) • [READ IN ENGLISH](../README.md) • [PORTAL DE ARQUITECTURA](./architecture/index.md)**
> *Nota: GitHub muestra los archivos fuente primero. Para saltar el código y leer la documentación, haz clic en los enlaces superiores.*

---

> **Monolito modular estandarizado para identidad y autorización unificada.**
>
> ![Status](https://img.shields.io/badge/Status-Activo-success) ![Architecture](https://img.shields.io/badge/Architecture-Modular_Monolith-blue) ![Methodology](https://img.shields.io/badge/Methodology-BMAD--METHOD-success)

---

## Acceso Rápido a Estándares

| Necesidad | Ruta directa |
| :--- | :--- |
| Referencia aplicada React Web de UMS | [UMS React Web Applied Reference](./architecture/web-frontend/ums-react-applied-reference.md) |
| Referencia aplicada .NET API de UMS | [UMS API .NET Applied Reference](./architecture/api-dotnet/ums-api-dotnet-applied-reference.md) |
| Estándar React upstream de Evolith | [Evolith React Web Frontend Standard](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/governance/standards/engineering/web-frontend/react/react-web-frontend-standard.md) |
| Estándar .NET API upstream de Evolith | [Evolith .NET API Standard](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/governance/standards/engineering/api-dotnet/api-dotnet-standard.md) |
| Todos los estándares en una página | [Standards Quick Access](./STANDARDS.md) |

---

## Índice Maestro de Navegación

Comienza aquí si eres nuevo en UMS. Este índice ofrece a cada lector una ruta rápida dentro del repositorio sin necesidad de conocer la estructura de carpetas.

### Ruta rápida por persona

| Soy… | Empiezo aquí | Luego leo |
| :--- | :--- | :--- |
| **Backend Engineer** | [Standards Quick Access](./STANDARDS.md) · [Portal de Construcción](./governance/construction/index.md) · [Índice de Agregados de Dominio](./domain/index.md) | [UMS API .NET Applied Reference](./architecture/api-dotnet/ums-api-dotnet-applied-reference.md) → [Portal de Diseño DDD](./governance/construction/ddd-design/index.md) |
| **Frontend Engineer** | [Standards Quick Access](./STANDARDS.md) · [UMS React Web Applied Reference](./architecture/web-frontend/ums-react-applied-reference.md) | [Evolith React Web Frontend Standard](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/governance/standards/engineering/web-frontend/react/react-web-frontend-standard.md) |
| **Arquitecto** | [Portal de Arquitectura](./architecture/index.md) · [Registro ADR](./architecture/adrs/index.md) | [Standards Quick Access](./STANDARDS.md) → [Matriz de Trazabilidad](./architecture/traceability-matrix.md) |
| **Product Owner / QA** | [Índice Maestro](./MASTER_INDEX.es.md) · [Historias Funcionales](./governance/requirements/functional-stories/index.md) | [MVP Backlog](./governance/project/mvp-product-backlog.md) → [Glosario](./governance/requirements/glossary.md) |
| **DevOps / Ops** | [Portal de Operaciones](./operations/index.md) · [Runbooks](./operations/runbooks/) | [ADR-0053 OpenTelemetry](./architecture/adrs/0053-opentelemetry-observability.md) → [ADR-0054 Shell Library Isolation](./architecture/adrs/0054-shell-library-isolation.md) |

### Quiero…

| Objetivo | Empiezo aquí | Luego leo |
| :--- | :--- | :--- |
| Encontrar estándares para React, Web, C# o .NET | [Standards Quick Access](./STANDARDS.md) | [Portal de Arquitectura](./architecture/index.md) |
| Entender el producto | [Visión del Producto](./governance/product-es/product-vision.md) | [Contexto de Negocio](./governance/product-es/business-context.md) → [Alcance](./governance/product-es/scope.md) |
| Ver épicas y prioridades | [MVP Product Backlog](./governance/project-es/mvp-product-backlog.md) | [Índice de Requerimientos](./governance/requirements/index.md) → [Historias Funcionales](./governance/requirements/functional-stories/index.md) |
| Revisar requerimientos funcionales | [Índice de Requerimientos](./governance/requirements/index.md) | [Historias Funcionales](./governance/requirements/functional-stories/index.md) → [Glosario](./governance/requirements/glossary.md) |
| Validar el modelo de datos y dominio | [Modelo de Datos Conceptual](./governance/requirements/conceptual-data-model.md) | [Revisión de Consistencia del Modelo de Datos](./architecture/blueprints/data-model-consistency-review.md) → [Diseño de Base de Datos ER](./architecture/blueprints/database-design-er.md) |
| Entender la arquitectura | [Portal de Arquitectura](./architecture/index.md) | [Overview de Arquitectura](./architecture/overview.md) → [Diseño de Base de Datos ER](./architecture/blueprints/database-design-er.md) |
| Explorar todo | [Índice Maestro](./MASTER_INDEX.es.md) | Árbol completo de documentos por fase del ciclo de vida. |

---

## Resumen de Arquitectura

### Stack Tecnológico

| Capa | Tecnología |
| :--- | :--- |
| **Backend** | .NET 10, HotChocolate (GraphQL), Minimal APIs (REST) |
| **Frontend** | React 18, Vite 5, TypeScript, TailwindCSS, Zustand, TanStack Query |
| **Base de datos** | SQL Server 2022, Entity Framework Core |
| **Monorepo** | Nx, npm Workspaces |
| **Metodología** | BMAD-METHOD, Arquitectura Limpia (Hexagonal), DDD |

### Estructura del Proyecto

```text
src/
├── apps/
│   ├── ums.api/                    # Backend .NET (Arquitectura Limpia)
│   │   ├── Domain/                 # Modelo DDD puro: Aggregate Roots, Entities, Value Objects, Domain Events; cero dependencias de framework
│   │   ├── Application/            # Casos de uso, interfaces
│   │   ├── Infrastructure/         # EF Core, servicios externos
│   │   └── Presentation/           # Endpoints GraphQL/REST
│   └── ums.web-app/                # Frontend React (Arquitectura Limpia)
│       ├── src/
│       │   ├── domain/             # Tipos de dominio cliente, value objects y modelo de negocio orientado a UI
│       │   ├── application/        # Hooks, stores, casos de uso
│       │   ├── infrastructure/     # Clientes HTTP, cliente GraphQL
│       │   └── presentation/       # Componentes, pantallas, layouts
│       └── ...
└── ...
```

### Decisiones Arquitectónicas Clave

- **GraphQL para consultas, REST para comandos**: Todas las operaciones de lectura usan HotChocolate GraphQL; las escrituras usan REST Minimal APIs para claridad transaccional.
- **Arquitectura Limpia + DDD**: Límites estrictos entre capas. La capa de Dominio contiene el modelo DDD puro — Aggregate Roots, Entities, Value Objects, Domain Events e invariantes — sin dependencias de framework. Application contiene casos de uso y puertos. Infrastructure gestiona persistencia y preocupaciones externas.
- **Patrón Result**: Sin excepciones para control de flujo. Todas las operaciones retornan `Result<T>` para manejo explícito de errores.
- **Bounded Contexts**: Identity, Authorization, Configuration, Approvals, Compliance, IGA, Audit, Cache y Console. Cada contexto gobierna su modelo de agregados, contratos y reglas de integración.

---

## Inicio Rápido (Engine Room)

### Prerrequisitos

- Node.js 20+
- .NET 10 SDK
- SQL Server 2022 o Docker:

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123!" -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
```

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
# Terminal 1 — Backend (puerto 7114)
cd src/apps/ums.api && dotnet run

# Terminal 2 — Frontend (puerto 5173)
cd src/apps/ums.web-app && npm run dev
```

---

## Comandos de Desarrollo

| Comando | Descripción |
| :--- | :--- |
| `npm install` | Instalar todas las dependencias frontend desde `src/apps/ums.web-app` |
| `npm run dev` | Iniciar servidor de desarrollo frontend (puerto 5173) |
| `npm run build` | Compilar frontend para producción |
| `npm run lint` | Ejecutar ESLint |
| `npm run test` | Ejecutar tests Vitest |
| `dotnet build` | Compilar solución backend desde `src/apps/ums.api` |
| `dotnet test` | Ejecutar tests backend |
| `dotnet run` | Iniciar API backend (puerto 7114) |

---

## Centro de Conocimiento

| Dominio | Índice del portal | Contenido |
| :--- | :--- | :--- |
| **Estándares** | [Standards Quick Access](./STANDARDS.md) | Enlaces directos a estándares Evolith y referencias aplicadas UMS para React Web y .NET API. |
| **Gobernanza** | [Portal de Gobernanza](./governance/index.md) | Dirección del producto, alcance de negocio y requerimientos funcionales. |
| **Entrega del proyecto** | [Backlog del Proyecto](./governance/project/index.md) | Épicas MVP, historias de usuario y diseño funcional de módulos core. |
| **Requerimientos** | [Índice de Requerimientos](./governance/requirements/index.md) | Historias funcionales, glosario de negocio y modelo de datos conceptual. |
| **Arquitectura** | [Portal de Arquitectura](./architecture/index.md) | Diseño ER de base de datos, mapas de entidades, revisión de consistencia de datos y visores interactivos. |
| **Construcción** | [Portal de Construcción](./governance/construction/index.md) | Diseño DDD de la capa de dominio: bounded contexts, agregados, eventos y comandos. |
| **Métricas** | [Dashboard de Métricas de Solución](./operations/metrics/index.md) | Métricas de ingeniería por tipo de solución: código, seguridad, calidad, pruebas y uso de IA. |

---

## Seguridad y Cumplimiento

- **Content Security Policy**: CSP restrictiva con `unsafe-eval` removido, lista para producción.
- **Protección CSRF**: Patrón double-submit cookie con refresco de token.
- **Headers de seguridad**: HSTS, X-Frame-Options, X-Content-Type-Options y Referrer-Policy vía Nginx.
- **Validación de inputs**: Schemas Zod como fuente única de verdad para validación runtime.

---

## Contribución y Gobernanza

- **Flujo de trabajo**: Este repositorio utiliza [BMAD-METHOD](../AGENTS.md) para documentación orientada a especificaciones.
- **Navegación**: Visita el [Índice Maestro](./MASTER_INDEX.es.md) para el árbol completo de documentos.
- **Estándares de código**: ESLint + TypeScript strict mode. Cero errores requeridos antes de commit.
- **Testing**: Vitest con React Testing Library. Los umbrales se gobiernan por los quality gates activos del proyecto.
