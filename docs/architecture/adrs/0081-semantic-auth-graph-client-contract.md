# ADR-0081: Semantic Auth Graph Client Contract — Code-First, ID-Optional

**Status:** Accepted  
**Date:** 2026-06-04  
**Decision Owner:** Architecture  
**Related:**
- [ADR-0071: Auth Graph Engine](./0071-auth-graph-engine.md)
- [ADR-0074: Auth Graph Schema Versioning Policy](./0074-auth-graph-schema-versioning.md)
- [ADR-0080: Auth Graph Preview — Internal vs External Pipeline](./0080-auth-graph-preview-internal-pipeline.md)

---

## Context

UMS's long-term goal is to act as a **clear authorization graph provider** for client systems. Clients should be able to activate, deactivate, execute, or deny options based on a contract that is stable, human-readable, and business-semantic.

The current graph already contains rich authorization data, but it also exposes internal technical identifiers (`Guid` values) in multiple sections. Those identifiers are useful inside the backend, for correlation, and for diagnostic workflows, but they are not ideal as the primary contract for downstream client systems.

The product direction is to make the client-facing graph:

1. Semantically meaningful
2. Stable across schema evolution
3. Independent from internal database identifiers
4. Safe for long-lived integrations
5. Easy to consume by product code, UI guards, and service-side policy checks

---

## Decision

UMS will treat the client-facing authorization graph as the current semantic contract and keep IDs out of the default client surface.

### 1. Default client contract is code-first

The graph exposed to client systems should rely primarily on:

- `code`
- `value`
- `description`
- `effect`
- `source`
- `scope`
- `validUntil`
- `schemaVersion`

Technical GUIDs remain internal implementation details unless a diagnostic or migration scenario requires them.

### 2. Internal identifiers are not part of the normal integration surface

The following identifiers should not be required by standard clients:

- `user.id`
- `tenant.id`
- `systemSuite.id`
- `role.id`
- `profile.id`
- `branch.id`
- permission and action GUIDs

These values may still exist in backend models, audit records, and internal admin views, but they should not be necessary for normal client authorization decisions.

### 3. Diagnostic mode may include IDs explicitly

An optional diagnostic contract can expose GUIDs only when:

- the caller is an authorized internal administrator or support operator
- the request is made through an internal preview or diagnostic endpoint
- the caller explicitly asks for `includeIds=true` or an equivalent support-only flag

This mode exists for troubleshooting, correlation, and migration support, not for standard client consumption.

### 4. Business meaning is expressed through stable codes

Client systems must use stable business codes to interpret the graph:

- tenant code
- system suite code
- role code
- branch code
- resource code
- action code
- permission effect
- scope string

This keeps client logic independent from database-generated identifiers.

### 5. Refresh behavior remains session-based, not graph-token-based

UMS does not introduce a separate refresh token contract for the semantic graph. The graph remains valid until its `validUntil` value. When it expires, the client re-authenticates and receives a fresh graph snapshot.

This avoids partial refresh states where a token is renewed but the authorization model is stale or mismatched.

### 6. The semantic graph is the client source of truth for access decisions

For client systems, the graph should be the authoritative decision payload during its validity window. The client should not need to infer access from backend GUIDs or re-query UMS for each action.

---

## Payload Shape

```json
{
  "schemaVersion": "1.0.0",
  "graphId": "optional-support-correlation-id",
  "context": {
    "tenant": {
      "code": "TECHNO",
      "value": "Techno Logistics",
      "status": "ACTIVE",
      "isManagementOwner": false
    },
    "systemSuite": {
      "code": "WMS_SUITE",
      "value": "Warehouse Management Suite",
      "status": "PUBLISHED"
    },
    "role": {
      "code": "WAREHOUSE_SUPERVISOR",
      "value": "Warehouse Supervisor",
      "hierarchyLevel": 3
    },
    "profile": {
      "scope": "BranchScoped",
      "isActive": true
    },
    "branch": {
      "code": "LIM-01",
      "value": "Lima Main Branch"
    }
  },
  "permissions": [
    {
      "resourceCode": "INVENTORY",
      "actionCode": "VIEW",
      "effect": "Allow",
      "source": "Template"
    }
  ],
  "scopes": [
    "INVENTORY.VIEW"
  ],
  "featureFlags": [
    {
      "flagCode": "NEW_MENU_EXPERIMENT",
      "isEnabled": true
    }
  ],
  "effectiveConfig": {
    "sessionTimeoutMinutes": 60,
    "accessTokenDurationMs": 3600000
  },
  "generatedAt": "2026-06-04T12:00:00Z",
  "validUntil": "2026-06-04T13:00:00Z"
}
```

The exact structure can evolve, but the contract must remain semantically readable and stable for client systems.

---

## Implementation Strategy

### Current baseline

- Use the semantic client contract as the default payload for all new integrations.
- Keep a diagnostic mode available for support and preview tooling.
- Retain internal GUIDs only in backend models and support flows.

---

## Consequences

### Positive

- Client integrations become easier to understand and maintain
- Product and business teams can reason about access using stable language
- Internal database identifiers are no longer leaked as integration dependencies
- Schema evolution becomes safer because clients depend on semantic fields, not surrogate keys

### Trade-offs

- Support tooling may need explicit diagnostic mode support
- The backend must keep internal identifiers even though client payloads do not depend on them

---

## Implementation Notes

| Area | Guidance |
|---|---|
| Graph builder | Produce semantic fields as the default client projection |
| Internal model | Keep GUIDs in backend aggregates and audit records |
| Client endpoints | Prefer code/value/description-based responses |
| Diagnostic mode | Expose IDs only to authenticated internal support flows |
| SDKs | Consume semantic fields as the canonical decision surface |
| Documentation | Keep English and Spanish contract docs synchronized |

---

**[ADR Registry](./index.md)**
