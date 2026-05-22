# ADR-0056: Clean Architecture Layer Boundaries (Frontend)

| Field | Value |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-05-21 |
| **Context** | UMS Web App — Frontend Architecture |
| **Deciders** | Architecture Team |

## Problem

Frontend applications often mix concerns: business logic in components, HTTP calls in UI code, and state management scattered across files. This makes testing difficult, reuse impossible, and the codebase fragile to change.

## Decision

Apply **Clean Architecture (Hexagonal)** to the React frontend with strict layer boundaries:

```
src/
├── domain/                     # Enterprise business rules (PURE)
│   ├── entities/               # Enterprise entities (Tenant, Branch, IdP)
│   ├── value-objects/          # Value objects (Email, TenantCode)
│   ├── schemas/                # Zod validation schemas
│   └── constants/              # Domain constants
│
├── application/                # Use cases and application logic
│   ├── hooks/                  # React hooks (use cases)
│   ├── stores/                 # Zustand stores (state)
│   ├── errors/                 # Error handling utilities
│   ├── utils/                  # Application utilities (logger, i18n)
│   └── i18n/                   # Internationalization
│
├── infrastructure/             # External concerns
│   ├── http/                   # HTTP client, GraphQL client, CSRF
│   └── services/               # External service adapters
│
└── presentation/               # UI layer
    ├── shared/                 # Shared components, layouts, theme
    └── identity/               # Bounded context presentation
        ├── tenant/             # Tenant aggregate screens
        ├── profile/            # Profile aggregate screens
        └── hooks/              # Context-specific hooks
```

### Dependency Rule

**Dependencies flow inward**: Presentation → Application → Domain. Infrastructure is injected via dependency inversion.

```
presentation ──▶ application ──▶ domain
                      ▲
                      │
              infrastructure (injected)
```

### Rules

1. **Domain layer is PURE**: No React, no Zustand, no Axios, no external libraries. Only Zod for schema validation.
2. **Application layer knows nothing about UI**: Hooks define use cases, stores manage state. No DOM manipulation.
3. **Infrastructure implements ports**: HTTP clients, GraphQL clients, and external services are adapters.
4. **Presentation composes**: Components compose hooks and stores. No business logic in components.
5. **No cross-layer imports**: Domain never imports from Application. Application never imports from Presentation.

### DOM Manipulation Rule (C-2)

DOM manipulation (e.g., `document.body.classList`) is performed in the **presentation layer**, not in stores. Stores expose state; components react to it.

## Consequences

**Positive:**
- Testable business logic (no UI dependencies)
- Reusable hooks across screens
- Clear boundaries for code review
- Easy to swap infrastructure (e.g., REST → gRPC)

**Negative:**
- More files and directories
- Requires discipline to maintain boundaries
- Initial setup overhead

## Implementation

- ESLint `no-restricted-imports` can enforce boundaries (future)
- Barrel exports (`index.ts`) define public APIs per layer
- `AGENTS.md` documents conventions for AI agents

## Related

- ADR-0055: GraphQL/REST Hybrid API Pattern
- ADR-0056: Zustand + TanStack Query State Management
