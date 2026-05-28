# UMS — Sistema de Gestión de Usuarios Empresarial

> **[ABRIR ESTÁNDARES](./STANDARDS.es.md) • [ABRIR ÍNDICE MAESTRO](./MASTER_INDEX.es.md) • [READ IN ENGLISH](../README.md) • [PORTAL DE ARQUITECTURA](./architecture/index.es.md)**
> *Nota: GitHub muestra los archivos de código primero. Para saltar el código y leer la documentación, haz clic en los enlaces de arriba.*

---

> **Monolito modular estandarizado para identidad y autorización unificada.**
>
> ![Status](https://img.shields.io/badge/Status-Activo-success) ![Architecture](https://img.shields.io/badge/Architecture-Modular_Monolith-blue) ![Methodology](https://img.shields.io/badge/Methodology-BMAD--METHOD-success)

---

## Acceso rápido a estándares

| Necesidad | Ruta directa |
| :--- | :--- |
| Referencia aplicada React Web UMS | [Referencia Aplicada React Web UMS](./architecture/web-frontend/ums-react-applied-reference.es.md) |
| Referencia aplicada API .NET UMS | [Referencia Aplicada API .NET UMS](./architecture/api-dotnet/ums-api-dotnet-applied-reference.es.md) |
| Estándar React upstream Evolith | [Estandar Web Frontend React Evolith](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/governance/standards/engineering/web-frontend/react/react-web-frontend-standard.es.md) |
| Estándar API .NET upstream Evolith | [Estandar API .NET Evolith](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/governance/standards/engineering/api-dotnet/api-dotnet-standard.es.md) |
| Todos los estándares en una página | [Acceso Rápido a Estándares](./STANDARDS.es.md) |

---

## Índice maestro de navegación

Comience aquí si es nuevo en UMS. Este índice ofrece a cada lector una ruta rápida al repositorio sin necesidad de conocer la estructura de carpetas.

| Quiero... | Empezar aquí | Luego leer |
| :--- | :--- | :--- |
| Encontrar estándares React, Web, C# o .NET | [Acceso Rápido a Estándares](./STANDARDS.es.md) | [Portal de Arquitectura](./architecture/index.es.md) |
| Entender el producto | [Visión del Producto](./governance/product-es/product-vision.md) | [Contexto de Negocio](./governance/product-es/business-context.md) → [Alcance](./governance/product-es/scope.md) |
| Ver épicas y prioridades | [MVP Product Backlog](./governance/project-es/mvp-product-backlog.md) | [Índice de Requerimientos](./governance/requirements-es/index.md) → [Historias Funcionales](./governance/requirements-es/functional-stories/index.md) |
| Revisar requerimientos funcionales | [Índice de Requerimientos](./governance/requirements-es/index.md) | [Historias Funcionales](./governance/requirements-es/functional-stories/index.md) → [Glosario](./governance/requirements-es/glossary.md) |
| Validar el modelo de datos y dominio | [Modelo de Datos Conceptual](./governance/requirements-es/conceptual-data-model.md) | [Formatos de Exportación ER](./architecture/blueprints-es/er-export-formats.md) → [Diseño de Base de Datos ER](./architecture/blueprints-es/database-design-er.md) |
| Entender la arquitectura | [Portal de Arquitectura](./architecture/index.es.md) | [Diseño de Base de Datos ER](./architecture/blueprints-es/database-design-er.md) |
| Explorar todo | [Índice Maestro](./MASTER_INDEX.es.md) | Árbol completo de documentos por fase del ciclo de vida. |

---

## Resumen de arquitectura

### Stack tecnológico

| Capa | Tecnología |
| :--- | :--- |
| **Backend** | .NET 10, HotChocolate (GraphQL), Minimal APIs (REST) |
| **Frontend** | React 18, Vite 5, TypeScript, TailwindCSS, Zustand, TanStack Query |
| **Base de datos** | SQL Server 2022, Entity Framework Core |
| **Monorepo** | Nx, npm Workspaces |
| **Metodología** | BMAD-METHOD, Arquitectura Limpia (Hexagonal), DDD |

### Estructura del proyecto

```text
src/
├── apps/
│   ├── ums.api/                    # Backend .NET (Arquitectura Limpia)
│   │   ├── Domain/                 # POCOs puros, sin referencias NuGet
│   │   ├── Application/            # Casos de uso, interfaces
│   │   ├── Infrastructure/         # EF Core, servicios externos
│   │   └── Presentation/           # Endpoints GraphQL/REST
│   └── ums.web-app/                # Frontend React (Arquitectura Limpia)
│       ├── src/
│       │   ├── domain/             # Entidades empresariales, value objects
│       │   ├── application/        # Hooks, stores, casos de uso
│       │   ├── infrastructure/     # Clientes HTTP, cliente GraphQL
│       │   └── presentation/       # Componentes, pantallas, layouts
│       └── ...
└── ...
```

### Decisiones arquitectónicas clave

- **GraphQL para consultas, REST para comandos**: Todas las operaciones de lectura usan HotChocolate GraphQL; las escrituras usan REST Minimal APIs para claridad transaccional.
- **Arquitectura limpia**: Límites estrictos entre capas. La capa de dominio es pura, sin dependencias externas.
- **Patrón Result**: Sin excepciones para control de flujo. Todas las operaciones retornan `Result<T>`.
- **Bounded Contexts**: Identity, Access, Audit, etc. Cada contexto tiene sus propios agregados, servicios y módulos de presentación.

---

## Inicio rápido (Engine Room)

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

### Full stack (Frontend + Backend)

```bash
# Terminal 1 — Backend (puerto 7114)
cd src/apps/ums.api && dotnet run

# Terminal 2 — Frontend (puerto 5173)
cd src/apps/ums.web-app && npm install && npm run dev
```

---

## Comandos de desarrollo

| Comando | Descripción |
| :--- | :--- |
| `npm install` | Instalar dependencias frontend desde `src/apps/ums.web-app` |
| `npm run dev` | Iniciar servidor de desarrollo frontend (puerto 5173) |
| `npm run build` | Compilar frontend para producción |
| `npm run lint` | Ejecutar ESLint |
| `npm run test` | Ejecutar tests Vitest |
| `dotnet build` | Compilar backend desde `src/apps/ums.api` |
| `dotnet test` | Ejecutar tests backend |
| `dotnet run` | Iniciar API backend (puerto 7114) |

---

## Centro de conocimiento

| Dominio | Índice del portal | Contenido |
| :--- | :--- | :--- |
| **Estándares** | [Acceso Rápido a Estándares](./STANDARDS.es.md) | Enlaces directos a estándares Evolith y referencias aplicadas UMS para React Web y API .NET. |
| **Gobernanza** | [Portal de Gobernanza](./governance/index.es.md) | Dirección del producto, alcance de negocio y requerimientos funcionales. |
| **Entrega del proyecto** | [Backlog del Proyecto](./governance/project-es/index.md) | Épicas MVP, historias de usuario y diseño funcional de módulos core. |
| **Requerimientos** | [Índice de Requerimientos](./governance/requirements-es/index.md) | Historias funcionales, glosario de negocio y modelo de datos conceptual. |
| **Arquitectura** | [Portal de Arquitectura](./architecture/index.es.md) | Diseño ER de base de datos, entity maps y visores interactivos. |
| **Construcción** | [Portal de Construcción](./governance/construction/index.es.md) | Diseño DDD de la capa de dominio: bounded contexts, agregados, eventos y comandos. |

---

## Seguridad y cumplimiento

- **Content Security Policy**: CSP restrictiva sin `unsafe-eval`, lista para producción.
- **Protección CSRF**: Patrón double-submit cookie con refresco de token.
- **Headers de seguridad**: HSTS, X-Frame-Options, X-Content-Type-Options y Referrer-Policy vía Nginx.
- **Validación de inputs**: Schemas Zod como fuente única de verdad para validación runtime.

---

## Contribución y gobernanza

- **Flujo de trabajo**: Este repositorio utiliza [BMAD-METHOD](../AGENTS.md) para documentación orientada a especificaciones.
- **Navegación**: Visite el [Índice Maestro](./MASTER_INDEX.es.md) para el árbol completo de documentos.
- **Estándares aplicados**: Visite [Acceso Rápido a Estándares](./STANDARDS.es.md) para React Web, API .NET y referencias Evolith.
- **Estándares de código**: ESLint + TypeScript strict mode. Cero errores requeridos antes de commit.
- **Testing**: Vitest con React Testing Library. Umbrales de cobertura: 60% líneas/sentencias.
