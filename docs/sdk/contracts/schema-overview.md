# Auth Graph Schema — Overview

> **Language:** English | [Español](../../sdk-es/contracts/schema-overview.md)

This document describes the **canonical JSON Schema** for the `AuthorizationGraph` payload. It is the contract between the UMS server and all SDK runtimes (.NET, TypeScript, NestJS).

For the full runtime example and authorization semantics, see [`auth-graph.md`](../../domain/identity/auth-graph.md). This page focuses on the **schema shape, types, and contract guarantees**.

---

## 1. Location

| Artifact | Path |
|---|---|
| JSON Schema | `src/libs/sdk/contracts/auth-graph.schema.json` |
| Error codes catalog | `src/libs/sdk/contracts/error-codes.yaml` |
| Golden fixtures | `src/libs/sdk/contracts/fixtures/*.json` |
| Versioning policy | `src/libs/sdk/contracts/SCHEMA_VERSIONING.md` |

The schema is **JSON Schema 2020-12**.

---

## 2. Top-Level Shape

```jsonc
{
  "schemaVersion": "1.0.0",          // semver, required, governed by ADR-0074
  "context":            { ... },     // who is the principal, in what tenant/branch/role
  "authentication":     { ... },     // how was identity verified
  "actions":            [ ... ],     // full action catalog of the SystemSuite
  "menuAccess":         [ ... ],     // UI navigation tree with resolved permissions
  "domainPermissions":  [ ... ],     // domain resources (Aggregate/Entity) with resolved permissions
  "featureFlags":       [ ... ],     // flags evaluated at auth-time
  "effectiveConfig":    { ... },     // tenant-resolved parameters
  "scopes":             [ ... ],     // OAuth2-style "RESOURCE.ACTION" strings
  "generatedAt":        "...",       // ISO-8601 UTC
  "validUntil":         "..."        // ISO-8601 UTC; = generatedAt + sessionTimeoutMinutes
}
```

All fields above are **required** at v1.0.0. Adding optional fields in the future is a MINOR bump per [ADR-0074](../../architecture/adrs/0074-auth-graph-schema-versioning.md).

---

## 3. Field-by-Field Reference

### 3.1 `schemaVersion` (string, required)

- Format: semver `MAJOR.MINOR.PATCH`
- Pattern: `^\d+\.\d+\.\d+$`
- Initial value: `"1.0.0"`
- Governs SDK acceptance — see [versioning.md](./versioning.md).

### 3.2 `context` (object, required)

```jsonc
"context": {
  "user":        { "id": "uuid", "email": "string", "username": "string",
                   "displayName": "string", "status": "ACTIVE|PENDING|BLOCKED" },
  "tenant":      { "id": "uuid", "code": "string", "name": "string",
                   "status": "ACTIVE|SUSPENDED|ARCHIVED" },
  "systemSuite": { "id": "uuid", "code": "string", "name": "string",
                   "status": "DRAFT|PUBLISHED|RETIRED" },
  "role":        { "id": "uuid", "code": "string", "name": "string",
                   "hierarchyLevel": 0, "parentRoleId": "uuid|null" },
  "profile":     { "id": "uuid", "scope": "OrgWide|BranchScoped", "isActive": true },
  "branch":      { "id": "uuid", "code": "string", "name": "string" } | null
}
```

Notes:
- `branch` is `null` when the profile scope is `OrgWide`.
- All enum values are closed at v1.0.0 — adding new ones is a MINOR bump but SDKs treat unknown enum values as opaque strings (and log a warning).

### 3.3 `authentication` (object, required)

```jsonc
"authentication": {
  "method":           "Local" | "IDP",
  "provider":         { "name": "string", "code": "string", "strategy": "string" } | null,
  "mfaRequired":      true|false,
  "issuedAt":         "ISO-8601",
  "sessionExpiresAt": "ISO-8601"
}
```

`provider` is `null` when `method = "Local"`. When `method = "IDP"`, `provider` is required and `strategy` matches a value of `IdpStrategyHint` from BC-A (e.g., `AZURE_AD`, `OKTA`, `SAML2`, `GENERIC_OIDC`).

The SDK does NOT receive IDP tokens — see [Security Notes in auth-graph.md §10](../../domain/identity/auth-graph.md#10-notas-de-seguridad).

### 3.4 `actions` (array, required, may be empty)

Catalog of all `Action` entities registered in the `SystemSuite`. Used by clients to resolve `actionCode` references in `menuAccess` and `domainPermissions`.

```jsonc
"actions": [
  { "id": "uuid", "code": "VIEW", "name": "View" }
]
```

### 3.5 `menuAccess` (array, required, may be empty)

UI navigation tree: `Module → Menu → SubMenu → Option`, with the resolved `AccessEffect` and `source` at the leaf level.

```jsonc
"menuAccess": [
  {
    "module": { "id": "uuid", "code": "string", "name": "string",
                "sortOrder": 0, "status": "PUBLISHED" },
    "menus": [
      {
        "id": "uuid", "code": "string", "label": "string", "sortOrder": 0,
        "subMenus": [
          {
            "id": "uuid", "code": "string", "label": "string", "sortOrder": 0,
            "options": [
              {
                "id": "uuid", "code": "string", "label": "string",
                "actionCode": "VIEW",
                "effect": "Allow" | "Deny" | "NotGranted",
                "source": "Template" | "Override"
              }
            ]
          }
        ]
      }
    ]
  }
]
```

### 3.6 `domainPermissions` (array, required, may be empty)

Domain resources (Aggregate or Entity) with resolved permissions per action.

```jsonc
"domainPermissions": [
  {
    "resource": { "id": "uuid", "type": "Aggregate" | "Entity",
                  "code": "string", "name": "string", "moduleId": "uuid|null" },
    "actions": [
      { "actionId": "uuid", "actionCode": "string", "actionName": "string",
        "effect": "Allow" | "Deny" | "NotGranted",
        "source": "Template" | "Override" }
    ]
  }
]
```

### 3.7 `featureFlags` (array, required, may be empty)

Feature flags **evaluated at auth-time** against the user's full context.

```jsonc
"featureFlags": [
  {
    "flagCode": "string",
    "systemSuiteId": "uuid",
    "isEnabled": true|false,
    "matchedCriteriaType": "TenantId|BranchId|UserProfileId|RoleCode|Environment|DateRange|PercentageHash|CustomRule" | null
  }
]
```

`isEnabled = false` and `matchedCriteriaType = null` means the flag exists but no criteria matched the context (false-on-missing-context per [ADR-0068](../../architecture/adrs/0068-feature-flag-system-scope.md)).

### 3.8 `effectiveConfig` (object, required)

Tenant-resolved configuration parameters from the `ParameterCatalog`.

```jsonc
"effectiveConfig": {
  "sessionTimeoutMinutes":  60,
  "maxLoginAttempts":       5,
  "minPasswordLength":      12,
  "mfaRequiredForAdmin":    true,
  "accessTokenDurationMs":  3600000,
  "authUseExternalIdp":     false
}
```

This is **not** the full `ParameterCatalog` — only the parameters relevant to client-side authentication, authorization and session management. Sensitive parameters (connection strings, secrets) are **never** included.

### 3.9 `scopes` (array of strings, required, may be empty)

OAuth2-style scopes derived from the **Allow** entries in `menuAccess` and `domainPermissions`. Format: `"{resourceCode}.{actionCode}"`.

```jsonc
"scopes": ["STOCK_VIEW.VIEW", "STOCK_ADJUST.UPDATE", "PURCHASE_ORDER.VIEW"]
```

`Deny` entries are **not** present in `scopes` — they are exposed only via the resolved trees so the SDK can apply deny-precedence. Some SDKs surface a derived `denies` list for ergonomic checks (see runtime guides).

### 3.10 `generatedAt` / `validUntil` (string, required)

ISO-8601 UTC timestamps. The SDK MUST check `validUntil > now` before applying any authorization decision; if `validUntil <= now`, the decision is `Expired` and the client should re-authenticate.

---

## 4. Open vs Closed Enumerations

The schema treats enumerations in two categories:

| Closed (rejection on unknown) | Open (preserved with warning) |
|---|---|
| `effect`: `Allow`/`Deny`/`NotGranted` | `tenant.status`, `user.status` (can add categories) |
| `source`: `Template`/`Override` | `authentication.method` (Local/IDP today, more future) |
| `profile.scope`: `OrgWide`/`BranchScoped` | `resource.type` (Aggregate/Entity today) |

Closed enumerations affect authorization decisions directly — an unknown value would be unsafe to interpret. Open enumerations are informational; unknown values are preserved and surfaced via `UnknownFieldsObservedEvent`.

---

## 5. Round-Trip Guarantee

Every fixture under `src/libs/sdk/contracts/fixtures/` must:
1. Validate against `auth-graph.schema.json`.
2. Be successfully deserialized by all three SDKs.
3. Be re-serialized by each SDK to produce a payload that re-validates against the schema.

CI enforces this on every PR — see [fixtures.md](./fixtures.md).

---

## 6. References

- [ADR-0071: Auth Graph Engine](../../architecture/adrs/0071-auth-graph-engine.md)
- [ADR-0073: UMS SDK Multi-Runtime](../../architecture/adrs/0073-ums-sdk-multi-runtime.md)
- [ADR-0074: Schema Versioning Policy](../../architecture/adrs/0074-auth-graph-schema-versioning.md)
- [Authorization Graph (domain doc)](../../domain/identity/auth-graph.md)
- [Error Codes Catalog](./error-codes.md)
- [Versioning Policy (developer summary)](./versioning.md)
- [Fixtures](./fixtures.md)
- [Compatibility Matrix](./compatibility-matrix.md)
