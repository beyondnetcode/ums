# Golden Fixtures — Canonical Set v1.0.0

This directory contains representative `AuthorizationGraph` payloads used as the **executable contract** for the UMS SDK. Every SDK runtime and the UMS server must agree on how to interpret these fixtures.

See [`docs/sdk/contracts/fixtures.md`](../../../../../docs/sdk/contracts/fixtures.md) for the policy, conventions and CI integration details.

## Inventory

### Valid fixtures (must pass JSON Schema validation)

| File | Scenario |
|---|---|
| `local-auth-success.json` | Happy path Local (BCrypt) auth, OrgWide profile, mix of Allow / NotGranted |
| `idp-auth-success.json` | Happy path IDP (Azure AD) auth, BranchScoped profile, MFA required, Override Allow |
| `deny-wins.json` | Same action denied — exercises Axiom A3 (Deny precedence) |
| `override-allow.json` | Profile override flips effect to Allow |
| `empty-permissions.json` | User with Profile but zero permissions granted |
| `expired-graph.json` | `validUntil` deliberately in the past — exercises `AUTH_201` |
| `feature-flag-matched.json` | Two flags enabled via different criteria types |
| `feature-flag-missed-context.json` | Flag exists but context does not match any criteria (`isEnabled = false`) |
| `multi-tenant-rejection.json` | Graph for `ACME_RETAIL` tenant — used to probe `AUTH_109` |

### Invalid fixtures (must FAIL JSON Schema validation / trigger specific SDK rejection)

| File | Triggers |
|---|---|
| `schema-unsupported-major.json` | `schemaVersion: "2.0.0"` — `AUTH_205` from SDKs with `<2.0.0` compatibility |
| `schema-minor-ahead.json` | `schemaVersion: "1.99.0"` with unknown optional fields — accept-with-warning, preserve in `extensions` |
| `schema-missing.json` | No `schemaVersion` field at all — `AUTH_204` |

## CI Behavior

Phase B CI workflow (`.github/workflows/sdk-contract-validation.yml`, to be added):

1. **Valid fixtures** are loaded by every SDK, run through deserialize → validator-probe → re-serialize, and the result must validate against the schema again.
2. **Invalid fixtures** are loaded by every SDK and must produce the expected error code.
3. Server's `AuthorizationGraphBuilderService` integration tests round-trip the valid fixtures as builder regression suites.
