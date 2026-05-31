# Error Codes Catalog — `AUTH_xxx`

> **Language:** English | [Español](../../sdk-es/contracts/error-codes.md)

This is the **canonical catalog** of authorization-domain error codes used by UMS server, the three SDKs, and all client systems that integrate via the SDK.

The catalog is sourced from `src/libs/sdk/contracts/error-codes.yaml`. SDK packages generate constants from this file at build time, so consumer code references constants like `UmsErrorCodes.InvalidCredentials` instead of magic strings.

---

## 1. Authentication Errors (AUTH_001 – AUTH_010)

| Code | HTTP | Meaning | Emitted by |
|---|---|---|---|
| `AUTH_001` | 400 | Validation error — required fields missing or malformed | Server, SDKs |
| `AUTH_002` | 404 | Tenant not found | Server |
| `AUTH_003` | 403 | Tenant not active | Server |
| `AUTH_004` | 401 | IDP user has no matching UMS UserAccount | Server |
| `AUTH_005` | 403 | UserAccount status is not ACTIVE | Server |
| `AUTH_006` | 401 | Invalid credentials (Local BCrypt) | Server |
| `AUTH_007` | 423 | Account locked (max login attempts exceeded) | Server |
| `AUTH_008` | 401 | MFA challenge required but not provided | Server |
| `AUTH_009` | 401 | MFA challenge failed | Server |
| `AUTH_010` | 401 | Session expired | Server |

## 2. IDP Resolution Errors (AUTH_011 – AUTH_019)

| Code | HTTP | Meaning |
|---|---|---|
| `AUTH_011` | 503 | IDP mode configured but no active IDP provider |
| `AUTH_012` | 501 | No IDP adapter registered for the provider's strategy |
| `AUTH_013` | 502 | IDP authentication call failed (network / provider error) |
| `AUTH_014` | 401 | IDP token validation failed (signature, expiry, issuer) |

## 3. Authorization Errors (AUTH_100 – AUTH_199) — emitted by SDKs

| Code | Meaning | Trigger |
|---|---|---|
| `AUTH_101` | Scope not granted | `RequiresScope("X.Y")` and scope absent from `scopes[]` |
| `AUTH_102` | Scope explicitly denied | Scope present in resolved denies |
| `AUTH_103` | Menu option not granted | `RequiresMenuOption("CODE")` resolves to `NotGranted` |
| `AUTH_104` | Menu option denied | Resolves to `Deny` |
| `AUTH_105` | Domain access not granted | `RequiresDomainAccess` resolves to `NotGranted` |
| `AUTH_106` | Domain access denied | Resolves to `Deny` |
| `AUTH_107` | Feature flag disabled | `RequiresFeatureFlag` and `isEnabled = false` |
| `AUTH_108` | Feature flag not found | Flag code not present in `featureFlags[]` |
| `AUTH_109` | Tenant mismatch | Request expected tenant differs from graph tenant |

## 4. Graph Lifecycle Errors (AUTH_200 – AUTH_299) — emitted by SDKs

| Code | Meaning |
|---|---|
| `AUTH_201` | `AUTH_GRAPH_EXPIRED` — `validUntil` is in the past |
| `AUTH_202` | `AUTH_GRAPH_MISSING` — no graph available in the accessor |
| `AUTH_203` | `AUTH_GRAPH_MALFORMED` — graph failed JSON Schema validation |
| `AUTH_204` | `AUTH_GRAPH_SCHEMA_MISSING` — `schemaVersion` field absent |
| `AUTH_205` | `AUTH_GRAPH_SCHEMA_UNSUPPORTED` — MAJOR version outside SDK compatibility range |

## 5. Error Object Shape

All SDK-emitted authorization errors carry this shape (typed in each runtime, equivalent semantics):

```jsonc
{
  "code":        "AUTH_101",
  "message":     "Scope 'PURCHASE_ORDER.APPROVE' not granted",
  "primitive":   "RequiresScope",
  "target":      "PURCHASE_ORDER.APPROVE",
  "graphRequestId": "uuid",   // correlates to UMS audit trail
  "validUntil":  "ISO-8601",  // helps clients decide to re-auth
  "occurredAt":  "ISO-8601"
}
```

Server-emitted errors (AUTH_001–AUTH_099) follow the [Actionable User Error Contract](../../architecture/adrs/0066-actionable-user-error-contract.md) (ADR-0066).

## 6. Extension Policy

- New codes are added through a MINOR bump of the SDK contract.
- Codes are **never** reused — a retired code remains in the catalog marked `deprecated: true`.
- Codes are stable strings — they appear in logs, dashboards, and security audits. Renaming a code is a MAJOR bump.

---

## 7. References

- [`error-codes.yaml`](../../../src/libs/sdk/contracts/error-codes.yaml) (canonical source)
- [ADR-0066: Actionable User Error Contract](../../architecture/adrs/0066-actionable-user-error-contract.md)
- [ADR-0072: Dynamic Auth Method Resolution](../../architecture/adrs/0072-dynamic-auth-method-resolution.md)
- [Schema Overview](./schema-overview.md)
- [Versioning Policy](./versioning.md)
