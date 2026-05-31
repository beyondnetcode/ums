# Auth Graph Schema Versioning — Operational Summary

> Operational summary inside the source tree. The full architectural decision is in
> [ADR-0074](../../../../docs/architecture/adrs/0074-auth-graph-schema-versioning.md).
> Developer-facing guide: [`docs/sdk/contracts/versioning.md`](../../../../docs/sdk/contracts/versioning.md).

## Current State

- **Current schema version:** `1.0.0` (initial release)
- **Schema file:** [`auth-graph.schema.json`](./auth-graph.schema.json)
- **Error codes catalog:** [`error-codes.yaml`](./error-codes.yaml)
- **Golden fixtures:** [`fixtures/`](./fixtures/)

## Bump Decision Cheatsheet

| Change | Bump | Example |
|---|---|---|
| Add optional field | MINOR | New `context.user.locale` optional string |
| Add new top-level section | MINOR | New `delegations` array |
| Add new enum value to open enumeration | MINOR | New `IdpStrategyHint` value |
| Remove required field | **MAJOR** | Drop `effectiveConfig.minPasswordLength` |
| Rename field | **MAJOR** | `tenant.name` → `tenant.legalName` |
| Narrow type | **MAJOR** | `string` → `enum` for an existing field |
| Change semantics | **MAJOR** | Flip Allow/Deny precedence rule |
| Documentation-only edit | PATCH | Clarify a `description` |

When in doubt, bump higher.

## Per-Change Workflow

1. Update `auth-graph.schema.json` — bump the `schemaVersion` `const` value to match the change category.
2. Update `error-codes.yaml` if new codes are introduced.
3. Add or update fixtures under `fixtures/` covering the new behavior.
4. Add an entry to `docs/sdk/contracts/compatibility-matrix.md`.
5. For MAJOR or removals: open an ADR amendment documenting the migration path.
6. Run CI locally: contract validation, round-trip through all SDKs.

## Deprecation Cycle

1. **MINOR N**: Add replacement field, mark old as `"deprecated": true` in schema, add target removal version via `"x-deprecation-removed-in"`.
2. **MINOR N+1**: SDKs emit `DeprecatedFieldUsageEvent` on read.
3. **MINOR N+2+**: Continue emitting events. Minimum window: 2 MINORs / 6 months.
4. **MAJOR N+1**: Remove. Servers stop emitting. SDK majors required to upgrade.
