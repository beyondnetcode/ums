# TE-02: Permission Graph Compiler

| Field | Value |
|-------|-------|
| **TE ID** | TE-02 |
| **Status** | Approved |
| **ADR Reference** | ADR-0012 (RBAC/ABAC Guards), ADR-0021 (Auth Graph Compilation), ADR-0022 (Contextual Projections) |
| **Satisfies** | FS-02 (Create Authorization Template), FS-05 (Create Profile), FS-07 (Visual Graph Resolver), FS-14 (Delegated Management), FS-16 (Access Enforcement Policy) |
| **Owner** | Platform Team |
| **Date** | 2026-05-18 |

---

## Problem

A user may hold multiple `Profile` objects across different branch scopes. Each Profile links to one or more `PermissionTemplate` objects plus optional per-permission overrides. Evaluating "can user X perform action Y on system Z in branch B?" requires combining all active Profiles, applying DENY-dominance (Axiom A3), and resolving branch-scope precedence (Axiom A4) — at request time, with sub-millisecond latency.

## Solution: Compiled Auth Graph + Read-Aside Cache

The compiler pre-aggregates all active Profiles for a user into a single flat `AuthGraph` structure persisted in Redis. The Policy Enforcement Point (PEP) at the API gateway reads this compiled graph rather than querying the database on every request.

```
Write Path (mutation triggers recompile)
────────────────────────────────────────
ProfileCreatedEvent / PermissionMutatedEvent / TemplateLinkedToProfileEvent
    │
    ▼
┌──────────────────────────────────────────────────────┐
│  AuthGraphCompilerService (Application Layer)         │
│  1. Load all active Profiles for (UserId, TenantId)   │
│  2. Resolve each Profile's effective permissions:     │
│     a. Expand PermissionTemplate items               │
│     b. Apply per-permission overrides (ALLOW/DENY)   │
│     c. Tag with scope (ORG_WIDE / BRANCH_SCOPED)     │
│  3. Apply axioms:                                    │
│     A1: start with DENY for all actions              │
│     A2: union all ALLOW across active Profiles       │
│     A3: any DENY overrides all ALLOWs                │
│     A4: BRANCH_SCOPED supersedes ORG_WIDE per branch  │
│  4. Serialize → AuthGraph JSON                       │
│  5. ICachePort.Set(key, graph, TTL)                  │
└──────────────────────────────────────────────────────┘

Read Path (per-request authorization)
──────────────────────────────────────
API Request → PEP (gateway or middleware)
    │
    ├─► ICachePort.Get(auth_graph:{tenantId}:{userId})
    │       ├── Hit  → evaluate inline (< 1 ms)
    │       └── Miss → load from DB, recompile, cache
    │
    └─► Check: graph.Allows(systemCode, actionCode, branchId?)
```

## AuthGraph Schema

```json
{
  "userId": "<guid>",
  "tenantId": "<guid>",
  "compiledAt": "2026-05-18T10:00:00Z",
  "entries": [
    {
      "systemCode": "ERP",
      "actionCode": "USER_CREATE",
      "effect": "ALLOW",
      "scope": "ORG_WIDE",
      "branchId": null
    },
    {
      "systemCode": "ERP",
      "actionCode": "USER_DELETE",
      "effect": "DENY",
      "scope": "ORG_WIDE",
      "branchId": null
    }
  ]
}
```

## Cache Key Convention

```
auth_graph:{tenantId}:{userId}
```

TTL: 900 seconds (configurable per tenant via BC-C `IConfigCachePort`).

Invalidation: on any domain event that mutates Profiles or Templates for the affected user.

## Axiom Evaluation Order

| Step | Axiom | Rule |
|------|-------|------|
| 1 | A1 — Deny-by-default | Initialize all actions as DENY |
| 2 | A2 — Permissive union | ALLOW from any active Profile grants the action |
| 3 | A3 — Explicit deny dominance | DENY in any Profile overrides all ALLOWs for that action |
| 4 | A4 — Branch scope precedence | For a given branch, BRANCH_SCOPED entry overrides ORG_WIDE |

## Port Contract

```csharp
public interface ICachePort
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
```

## Visual Graph Resolver (FS-07)

The Console PAP calls a dedicated read projection endpoint that returns the full compiled graph for rendering the visual permission explorer. This is served from the same Redis key using TE-06 (CQRS Projection Rebuild) for historical snapshots.

---

**[Technical Enablers Index](./index.md)** | **[Traceability Matrix](../../traceability-matrix.md)**
