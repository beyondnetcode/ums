# ADR-0079: Dependency Guard Policy — Blocking Operations on Active Dependencies

**Status:** Accepted  
**Date:** 2026-06-03  
**Decision Owner:** Architecture  
**Related:**
- [ADR-0077: Tenant Portal Management Authorization Boundary](./0077-tenant-portal-management-authorization-boundary.md)

---

## Context

Several state-change operations in UMS are unsafe when active dependencies exist. For example, suspending a tenant while active users are still logged in, or blocking a user while their profiles are actively used in authorization decisions.

Without a guard layer, these operations would either silently leave orphaned active data or require complex multi-step workflows to check constraints manually. We need a consistent, discoverable contract that:

1. Prevents the dangerous state change.
2. Returns a structured response explaining *why* it was blocked and *what* must be resolved first.
3. Does not place the guard logic in the domain model itself (which would require expensive cross-aggregate reads) or in the UI (which would be unreliable).

---

## Decision

### Placement: application command handlers

Dependency guards live in application command handlers, not in the domain aggregate or the UI. The handler queries the relevant repositories before dispatching the state-change command and rejects the operation if blocking dependencies are found.

This allows:
- Cross-aggregate reads without violating DDD aggregate boundaries.
- Testability via integration tests without loading the full domain model.
- Consistent enforcement regardless of which surface triggers the command.

### Error encoding: `BlockedOperationError` with `|` separator

Blocking errors are encoded as a string using `BlockedOperationError.Encode(errorCode, deps)` where multiple dependency entries are separated by `|`. The decoder `BlockedOperationError.TryDecode(error, out errorCode, out deps)` reconstructs the list in the presentation layer.

### HTTP contract: 409 Conflict with `BlockedOperationResponse`

When a guard fires, the endpoint returns **HTTP 409 Conflict** with the following JSON body:

```json
{
  "errorCode": "TENANT_HAS_ACTIVE_USERS",
  "message": "Human-readable message in configured locale",
  "brokenRule": "A tenant cannot be suspended while active users exist.",
  "blockingDependencies": [
    { "entityType": "UserAccount", "status": "Active", "count": 3 }
  ]
}
```

The `errorCode` values are defined in `DomainErrors` and mapped to messages/rules in `BlockedOperationMessages`.

### Guarded operations

| Operation | Error code | Blocking dependency |
|---|---|---|
| Suspend Tenant | `TENANT_HAS_ACTIVE_USERS` | Active `UserAccount` records |
| Suspend Tenant | `TENANT_HAS_ACTIVE_BRANCHES` | Active `Branch` records |
| Suspend Tenant | `TENANT_HAS_ACTIVE_IDP_CONFIG` | Active `IdpConfiguration` records |
| Block / Delete User | `USER_HAS_ACTIVE_PROFILES` | Active `Profile` records |
| Deactivate Role | `ROLE_HAS_ACTIVE_PROFILES` | Active `Profile` records |
| Deactivate Role | `ROLE_HAS_ACTIVE_CHILD_ROLES` | Active child `Role` records |
| Deprecate Template | `TEMPLATE_HAS_ACTIVE_PROFILES` | Active `Profile` records |
| Remove DomainResource | `DOMAIN_RESOURCE_HAS_TEMPLATE_ITEMS` | Active `PermissionTemplateItem` records |
| Remove Module | `MODULE_HAS_ACTIVE_MENUS` | Active `Menu` records |

---

## Consequences

- **Positive:** Consistent, machine-readable 409 response — UI can display actionable blocking information to users.
- **Positive:** Guard logic is centralized and tested independently of the domain model.
- **Negative:** Application layer must issue extra queries before each guarded operation; acceptable given low mutation frequency.
- **Neutral:** Adding new guarded operations requires: a new `DomainErrors` constant, a new `BlockedOperationMessages` entry, and a new check in the relevant command handler.
