# ADR-0055: GraphQL/REST Hybrid API Pattern

| Field | Value |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-05-21 |
| **Context** | UMS Web App — API Communication Strategy |
| **Deciders** | Architecture Team |

## Problem

The UMS system needs to support both flexible data querying (with nested relationships, filtering, and field selection) and clear transactional command semantics. Using a single API pattern for both leads to either over-fetching (REST) or unclear mutation semantics (GraphQL).

## Decision

Adopt a **GraphQL for Queries, REST for Commands** hybrid pattern:

- **GraphQL (HotChocolate)**: All read operations (queries). Clients request exactly the fields they need, with nested relationships in a single round-trip.
- **REST Minimal APIs**: All write operations (commands/transactions). Clear HTTP semantics (POST, PUT, DELETE) with explicit status codes and idempotency guarantees.

### Client-Side Implementation

```
Frontend (React)
├── GraphQL Client (graphql-request v7)
│   ├── All queries use absolute URL: `${window.location.origin}/graphql`
│   ├── Typed queries generated from schema
│   └── Cached via TanStack Query
│
└── REST Client (Axios via httpClient.ts)
    ├── All mutations (POST, PUT, DELETE)
    ├── CSRF token injection for state-changing requests
    ├── Dev headers (X-User-Id, X-Language, X-Tenant-Id)
    └── Error normalization via interceptors
```

### Rationale

1. **Query flexibility**: GraphQL allows the frontend to request exactly what each screen needs without over-fetching or N+1 requests.
2. **Clear transaction semantics**: REST provides well-understood HTTP status codes, idempotency keys, and retry semantics for mutations.
3. **CSRF protection**: REST endpoints are naturally protected via CSRF tokens; GraphQL subscriptions/queries are read-only and CSRF-safe.
4. **TanStack Query integration**: GraphQL queries cache naturally with TanStack Query's query key system.

### Consequences

**Positive:**
- Reduced payload sizes (field selection)
- Single round-trip for nested data
- Clear separation of read vs write concerns
- Natural CSRF protection boundary

**Negative:**
- Two API clients to maintain
- Developers must know which pattern to use
- Vite proxy configuration needed for both `/api` and `/graphql`

## Implementation

- `src/infrastructure/http/httpClient.ts` — Axios instance for REST commands
- `src/infrastructure/http/graphqlClient.ts` — GraphQL client for queries
- `src/infrastructure/http/csrf.ts` — CSRF token management
- `vite.config.ts` — Proxy `/api` and `/graphql` to backend

## Related

- ADR-0056: Zustand + TanStack Query State Management
- arc32 ADR-0012: API Gateway Pattern
