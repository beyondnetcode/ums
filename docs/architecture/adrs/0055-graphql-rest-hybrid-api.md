# ADR-0055: GraphQL/REST Hybrid API Pattern

> **Implements Evolith:** [Evolith ADR-0012 — Advanced Authorization, RBAC/ABAC](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/architecture/adrs/nodejs/0012-advanced-authorization-rbac-abac.md) and [Evolith ADR-0008 — Progressive Multimodule Evolution & Gateway BFF](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/architecture/adrs/nodejs/0008-progressive-multimodule-evolution-gateway-bff.md). UMS retains this document as its GraphQL/REST hybrid API design record.

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

## Alternatives Considered

### Alternative 1: Separate API Tiers (Query Tier + Command Tier)

Split queries and commands into two independently deployed API services — a dedicated GraphQL query service and a dedicated REST command service.

**Rejected because:**

- UMS is a modular monolith. Splitting into deployment tiers before extraction criteria are met violates ADR-0054 (Shell Library Isolation) and the modular monolith evolution playbook.
- CQRS separates read and write *models*, not *deployment units*. The existing separation is already enforced at three levels: protocol (GraphQL vs REST), code (distinct handlers, distinct clients), and routing (`/graphql` vs `/api/v1/...`).
- Operational cost doubles: two Dockerfiles, two health checks, two scaling policies, two sets of connection pools — with no measurable benefit at current load.

**When this decision should be revisited:**

This alternative becomes valid when any of the following conditions are met:

| Trigger | Explanation |
|---|---|
| Read throughput consistently 10x write throughput | Independent horizontal scaling of the query tier becomes justified |
| Separate teams own query vs command surfaces | Conway's Law makes the split natural, not forced |
| Migration toward microservices is initiated | Tier separation is a prerequisite step |
| Incompatible technology requirements emerge | e.g., query tier needs a different runtime or caching strategy |

**SaaS-specific consideration — tenant load isolation:**

In a multi-tenant SaaS context, heavy GraphQL queries from a large tenant could impact command latency (login, provisioning) if both share the same process. This risk is mitigated in the current architecture by:

1. GraphQL query complexity limits enforced at the HotChocolate schema level.
2. Differentiated timeouts per operation type.
3. Per-tenant rate limiting at the API Gateway layer (see [TE-07: YARP API Gateway](../blueprints/technical-enablers/te-07-yarp-api-gateway.md)).

Tier separation remains the correct escalation path if these controls prove insufficient at scale.

---

### Alternative 2: GraphQL for Both Queries and Mutations

Use GraphQL exclusively — queries and mutations — eliminating the REST layer.

**Rejected because:**

- GraphQL mutation semantics do not map cleanly to HTTP idempotency, retry, and status code conventions required for command operations.
- CSRF protection requires explicit handling for GraphQL mutations; REST POST/PUT/DELETE endpoints get this boundary naturally.
- REST is the established standard for webhook callbacks, external integrations, and mobile clients that may not support a GraphQL client.

---

## Related

- ADR-0056: Zustand + TanStack Query State Management
- ADR-0058: API Gateway Evolution — YARP for Multi-Client SaaS
- Evolith ADR-0012: API Gateway Pattern
- [TE-07: YARP API Gateway](../blueprints/technical-enablers/te-07-yarp-api-gateway.md)
