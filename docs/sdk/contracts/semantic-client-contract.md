# Semantic Auth Graph Client Contract

> **Language:** English | [Español](../../sdk-es/contracts/semantic-client-contract.md)

This document defines the semantic-first client contract for the `AuthorizationGraph`.

The goal is to make client integrations stable, business-readable, and free from default reliance on internal GUIDs.

The current canonical client contract is the one documented here. The structural legacy schema overview remains available as reference in [Schema Overview](./schema-overview.md).

---

## Contract Goals

Client systems should be able to:

1. Decide access without depending on database identifiers.
2. Render business-facing UI using stable codes.
3. Cache the graph safely until `validUntil`.
4. Re-authenticate instead of performing partial token refreshes when the graph expires.
5. Consume the same contract across web, mobile, backend, and integration services.

---

## Top-Level Shape

```jsonc
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
  "authentication": {
    "method": "Local",
    "provider": {
      "code": "AZURE_AD_LOGISTICS",
      "value": "Azure AD - Logistics",
      "strategy": "AZURE_AD"
    },
    "mfaRequired": true,
    "issuedAt": "2026-06-04T12:00:00Z",
    "sessionExpiresAt": "2026-06-04T13:00:00Z"
  },
  "permissions": [
    {
      "resourceCode": "INVENTORY",
      "actionCode": "VIEW",
      "effect": "Allow",
      "source": "Template"
    }
  ],
  "featureFlags": [
    {
      "flagCode": "NEW_MENU_EXPERIMENT",
      "isEnabled": true
    }
  ],
  "effectiveConfig": {
    "sessionTimeoutMinutes": 60,
    "accessTokenDurationMs": 3600000,
    "authUseExternalIdp": false
  },
  "scopes": [
    "INVENTORY.VIEW"
  ],
  "generatedAt": "2026-06-04T12:00:00Z",
  "validUntil": "2026-06-04T13:00:00Z"
}
```

The exact payload may carry additional semantic sections, but the default client-facing contract should remain code-first and ID-optional.

---

## Allowed Fields

### Context

- `tenant.code`
- `tenant.value`
- `tenant.status`
- `tenant.isManagementOwner`
- `systemSuite.code`
- `systemSuite.value`
- `systemSuite.status`
- `role.code`
- `role.value`
- `role.hierarchyLevel`
- `profile.scope`
- `profile.isActive`
- `branch.code`
- `branch.value`

### Authentication

- `authentication.method`
- `authentication.provider.code`
- `authentication.provider.value`
- `authentication.provider.strategy`
- `authentication.mfaRequired`
- `authentication.issuedAt`
- `authentication.sessionExpiresAt`

### Authorization

- `permissions[].resourceCode`
- `permissions[].actionCode`
- `permissions[].effect`
- `permissions[].source`
- `featureFlags[].flagCode`
- `featureFlags[].isEnabled`
- `effectiveConfig.sessionTimeoutMinutes`
- `effectiveConfig.accessTokenDurationMs`
- `effectiveConfig.authUseExternalIdp`
- `scopes[]`
- `generatedAt`
- `validUntil`

### Support-only field

- `graphId` is optional and exists for support correlation, not authorization logic.

---

## Prohibited Fields in the Default Surface

The following fields should not be part of the normal external client contract:

- raw GUIDs for tenant, user, role, profile, branch, system suite, resource, or action identifiers
- database primary keys
- foreign keys
- internal table names
- SQL schema names
- password hashes
- signing keys
- refresh tokens from external IDPs
- access tokens from external IDPs
- SAML assertions from external IDPs
- infrastructure secrets
- connection strings

If a support workflow needs an identifier for correlation, it must use explicit diagnostic mode rather than the default client payload.

---

## Diagnostic Mode

Diagnostic mode is reserved for internal support and admin workflows.

When enabled, it may include:

- internal GUIDs
- correlation metadata
- preview-only diffs
- trace markers useful for debugging

Diagnostic mode must never become the default client contract.

Recommended triggers:

- `includeIds=true`
- `mode=diagnostic`
- `mode=preview`

---

## Migration Path

### Phase 1

- Keep the current graph payload available.
- Add the semantic projection alongside the current graph.
- Allow internal support flows to request diagnostic mode.

### Phase 2

- Update external clients to consume the semantic contract by `code` and `value`.
- Remove any dependency on GUIDs from client decision logic.
- Keep backend correlation identifiers internally.

### Phase 3

- Make the semantic contract the default for external integrations.
- Reserve GUID exposure for support-only tooling and previews.

---

## Client Decision Rules

Clients should evaluate access with the following precedence:

1. Check `validUntil`.
2. Check tenant scope and status.
3. Check denied permissions first.
4. Check allowed permissions.
5. Evaluate feature flags only after the graph is still valid.

Example:

```ts
function canExecute(graph, resourceCode, actionCode) {
  if (new Date(graph.validUntil) <= new Date()) return false;

  const deny = graph.permissions.find(
    p => p.resourceCode === resourceCode && p.actionCode === actionCode && p.effect === 'Deny'
  );
  if (deny) return false;

  return graph.permissions.some(
    p => p.resourceCode === resourceCode && p.actionCode === actionCode && p.effect === 'Allow'
  );
}
```

---

## Implementation Notes

| Area | Guidance |
|---|---|
| Public contract | Prefer semantic fields and stable codes |
| Internal model | Keep GUIDs inside backend aggregates and audit events |
| SDKs | Deserialize semantic fields first, IDs only in diagnostic mode |
| Diagnostics | Use explicit support-only flags to expose IDs |
| Versioning | `schemaVersion` must reflect the semantic contract revision |

---

## References

- [Auth Graph Schema Overview](./schema-overview.md)
- [Auth Graph Versioning Policy](./versioning.md)
- [Auth Graph (domain doc)](../../domain/identity/auth-graph.md)
- [ADR-0081: Semantic Auth Graph Client Contract](../../architecture/adrs/0081-semantic-auth-graph-client-contract.md)
