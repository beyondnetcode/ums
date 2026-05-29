# ADR-0056: Límites de Capas de Clean Architecture (Frontend)

| Campo | Valor |
|---|---|
| **Estado** | Aceptado |
| **Fecha** | 2026-05-21 |
| **Contexto** | UMS Web App — Arquitectura Frontend |
| **Decisores** | Equipo de Arquitectura |

## Problema

Las aplicaciones frontend frecuentemente mezclan concerns: lógica de negocio en componentes, llamadas HTTP en código de UI, y gestión de estado dispersa entre archivos. Esto hace el testing difícil, la reutilización imposible, y el codebase frágil al cambio.

## Decisión

Aplicar **Clean Architecture (Hexagonal)** al frontend React con límites de capa estrictos:

```
src/
├── domain/                     # Reglas de negocio empresariales (PURO)
│   ├── entities/               # Entidades empresariales (Tenant, Branch, IdP)
│   ├── value-objects/          # Value objects (Email, TenantCode)
│   ├── schemas/                # Schemas de validación Zod
│   └── constants/              # Constantes de dominio
│
├── application/                # Casos de uso y lógica de aplicación
│   ├── hooks/                  # React hooks (casos de uso)
│   ├── stores/                 # Zustand stores (estado)
│   ├── errors/                 # Utilidades de manejo de errores
│   ├── utils/                  # Utilidades de aplicación (logger, i18n)
│   └── i18n/                   # Internacionalización
│
├── infrastructure/             # Concerns externos
│   ├── http/                   # Cliente HTTP, Cliente GraphQL, CSRF
│   └── services/               # Adaptadores de servicios externos
│
└── presentation/               # Capa de UI
    ├── shared/                 # Componentes compartidos, layouts, tema
    └── identity/               # Presentación del bounded context
        ├── tenant/             # Pantallas del aggregate Tenant
        ├── profile/            # Pantallas del aggregate Profile
        └── hooks/              # Hooks específicos de contexto
```

### Regla de Dependencias

**Las dependencias fluyen hacia adentro**: Presentation → Application → Domain. Infrastructure se inyecta vía inversión de dependencias.

```
presentation ──▶ application ──▶ domain
                       ▲
                       │
               infrastructure (inyectado)
```

### Reglas

1. **La capa Domain es PURA**: Sin React, sin Zustand, sin Axios, sin librerías externas. Solo Zod para validación de schemas.
2. **La capa Application no sabe nada de UI**: Los hooks definen casos de uso, los stores gestionan estado. Sin manipulación de DOM.
3. **Infrastructure implementa ports**: Los clientes HTTP, clientes GraphQL, y servicios externos son adaptadores.
4. **Presentation compone**: Los componentes composed hooks y stores. Sin lógica de negocio en componentes.
5. **Sin imports cross-layer**: Domain nunca importa de Application. Application nunca importa de Presentation.

### Regla de Manipulación de DOM (C-2)

La manipulación de DOM (ej., `document.body.classList`) se realiza en la **capa de presentación**, no en stores. Los stores exponen estado; los componentes reaccionan a él.

## Consecuencias

**Positivas:**
- Lógica de negocio testeable (sin dependencias de UI)
- Hooks reutilizables entre pantallas
- Fronteras claras para code review
- Fácil swapping de infraestructura (ej., REST → gRPC)

**Negativas:**
- Más archivos y directorios
- Requiere disciplina para mantener fronteras
- Overhead de setup inicial

## Implementación

- ESLint `no-restricted-imports` puede enforce fronteras (futuro)
- Barrel exports (`index.ts`) definen APIs públicas por capa
- `AGENTS.md` documenta convenciones para agentes IA

## Relacionados

- ADR-0055: Patrón de API Híbrida GraphQL/REST
- ADR-0056: Gestión de Estado con Zustand + TanStack Query