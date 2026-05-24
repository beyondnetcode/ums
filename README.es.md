# UMS — Sistema de Gestión de Usuarios Empresarial

> **[ 📚 ABRIR ÍNDICE MAESTRO ](./docs/MASTER_INDEX.es.md) • [ 🇬🇧 READ IN ENGLISH ](./README.md) • [ ⚙️ PORTAL DE ARQUITECTURA ](./docs/architecture/index.es.md)**  
> *Nota: GitHub muestra los archivos de código primero. Para saltar el código y leer la documentación, haz clic en los enlaces de arriba.*

---

> **Monolito Modular Estandarizado para Identidad y Autorización Unificada.**
>
> ![Status](https://img.shields.io/badge/Status-Activo-success) ![Architecture](https://img.shields.io/badge/Architecture-Modular_Monolith-blue) ![Methodology](https://img.shields.io/badge/Methodology-BMAD--METHOD-success)

---

## Índice Maestro de Navegación
Comience aquí si es nuevo en UMS. Este índice ofrece a cada lector una ruta rápida al repositorio sin necesidad de conocer la estructura de carpetas.

| Quiero... | Empezar Aquí | Luego Leer |
| :--- | :--- | :--- |
| Entender el producto | [Visión del Producto](./docs/governance/product-es/product-vision.md) | [Contexto de Negocio](./docs/governance/product-es/business-context.md) → [Alcance](./docs/governance/product-es/scope.md) |
| Ver Épicas y Prioridades | [MVP Product Backlog](./docs/governance/project-es/mvp-product-backlog.md) | [Índice de Requerimientos](./docs/governance/requirements-es/index.md) → [Historias Funcionales](./docs/governance/requirements-es/functional-stories/index.md) |
| Revisar requerimientos funcionales | [Índice de Requerimientos](./docs/governance/requirements-es/index.md) | [Historias Funcionales](./docs/governance/requirements-es/functional-stories/index.md) → [Glosario](./docs/governance/requirements-es/glossary.md) |
| Validar el modelo de datos y dominio | [Modelo de Datos Conceptual](./docs/governance/requirements-es/conceptual-data-model.md) | [Formatos de Exportación ER](./docs/architecture/blueprints-es/er-export-formats.md) → [Diseño de Base de Datos ER](./docs/architecture/blueprints-es/database-design-er.md) |
| Entender la arquitectura | [Portal de Arquitectura](./docs/architecture/index.es.md) | [Diseño de Base de Datos ER](./docs/architecture/blueprints-es/database-design-er.md) |
| Explorar todo | [Índice Maestro](./docs/MASTER_INDEX.es.md) | Árbol completo de documentos por fase del ciclo de vida. |

---

## Resumen de Arquitectura

### Stack Tecnológico
| Capa | Tecnología |
| :--- | :--- |
| **Backend** | .NET 8 LTS, HotChocolate (GraphQL), Minimal APIs (REST) |
| **Frontend** | React 18, Vite 5, TypeScript, TailwindCSS, Zustand, TanStack Query |
| **Base de Datos** | PostgreSQL 16, Entity Framework Core |
| **Monorepo** | Nx, npm Workspaces |
| **Metodología** | BMAD-METHOD, Arquitectura Limpia (Hexagonal), DDD |

### Estructura del Proyecto
```
src/
├── apps/
│   ├── ums.api/                    # Backend .NET (Arquitectura Limpia)
│   │   ├── Domain/                 # POCOs puros, sin referencias NuGet
│   │   ├── Application/            # Casos de uso, interfaces
│   │   ├── Infrastructure/         # EF Core, servicios externos
│   │   └── Presentation/           # Endpoints GraphQL/REST
│   └── ums.web-app/                # Frontend React (Arquitectura Limpia)
│       ├── src/
│       │   ├── domain/             # Entidades enterprise, value objects
│       │   ├── application/        # Hooks, stores, casos de uso
│       │   ├── infrastructure/     # Clientes HTTP, cliente GraphQL
│       │   └── presentation/       # Componentes, pantallas, layouts
│       └── ...
└── ...
```

### Decisiones Arquitectónicas Clave
- **GraphQL para Consultas, REST para Comandos**: Todas las operaciones de lectura usan HotChocolate GraphQL; las escrituras usan REST Minimal APIs.
- **Arquitectura Limpia**: Límites estrictos entre capas. La capa de dominio es pura (sin dependencias externas).
- **Patrón Result**: Sin excepciones para control de flujo. Todas las operaciones retornan `Result<T>`.
- **Bounded Contexts**: Identity, Access, Audit, etc. Cada contexto tiene sus propios agregados, servicios y presentación.

---

## Inicio Rápido (Engine Room)

### Prerrequisitos
- Node.js 20+
- .NET 8 SDK
- PostgreSQL 16

### Frontend
```bash
cd src
npm install
npx nx run app-web:dev
```

### Backend
```bash
cd src/apps/ums.api-dotnet
dotnet build
dotnet run
```

### Full Stack (Frontend + Backend)
```bash
cd src
npm install
npx nx run app-web:dev
# En otra terminal:
cd apps/ums.api-dotnet && dotnet run
```

---

## Comandos de Desarrollo

| Comando | Descripción |
| :--- | :--- |
| `npm install` | Instalar todas las dependencias |
| `npx nx run app-web:dev` | Iniciar servidor de desarrollo frontend (puerto 5173) |
| `npx nx run app-web:build` | Compilar frontend para producción |
| `npx nx run app-web:lint` | Ejecutar ESLint |
| `npx nx run app-web:test` | Ejecutar tests Vitest |
| `dotnet build` | Compilar solución backend |
| `dotnet test` | Ejecutar tests backend |
| `dotnet run` | Iniciar API backend (puerto 7114) |

---

## Centro de Conocimiento
| Dominio | Índice del Portal | Contenido |
| :--- | :--- | :--- |
| **Gobernanza** | [Portal de Gobernanza](./docs/governance/index.es.md) | Dirección del producto, alcance de negocio y requerimientos funcionales. |
| **Entrega del Proyecto** | [Backlog del Proyecto](./docs/governance/project-es/index.md) | Épicas MVP, historias de usuario y diseño funcional de módulos core. |
| **Requerimientos** | [Índice de Requerimientos](./docs/governance/requirements-es/index.md) | Historias funcionales, glosario de negocio y modelo de datos conceptual. |
| **Arquitectura** | [Portal de Arquitectura](./docs/architecture/index.es.md) | Diseño ER de base de datos, mapas de entidades y visores interactivos. |
| **Construccion** | [Portal de Construccion](./docs/governance/construction/index.es.md) | Diseño DDD de la capa de dominio (bounded contexts, agregados, eventos, comandos). |

---

## Seguridad y Cumplimiento

- **Content Security Policy**: CSP restrictiva sin `unsafe-eval` (lista para producción).
- **Protección CSRF**: Patrón double-submit cookie con refresco de token.
- **Headers de Seguridad**: HSTS, X-Frame-Options, X-Content-Type-Options, Referrer-Policy vía Nginx.
- **Validación de Inputs**: Schemas Zod como fuente única de verdad para validación runtime.

---

## Contribución y Gobernanza
- **Flujo de Trabajo**: Este repositorio utiliza [BMAD-METHOD](./AGENTS.md) para documentación orientada a especificaciones.
- **Navegación**: Visite el [**Índice Maestro**](./docs/MASTER_INDEX.es.md) para el árbol completo de documentos.
- **Estándares de Código**: ESLint + TypeScript strict mode. Cero errores requeridos antes de commit.
- **Testing**: Vitest con React Testing Library. Umbrales de cobertura: 60% líneas/sentencias.
