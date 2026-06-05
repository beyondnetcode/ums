# Versioning Policy — Developer Summary

> **Language:** English | [Español](../../sdk-es/contracts/versioning.md)

This is a **developer-facing summary** of the schema versioning policy. The full architectural decision is in [ADR-0074](../../architecture/adrs/0074-auth-graph-schema-versioning.md).

The semantic-first client contract is documented in [Semantic Auth Graph Client Contract](./semantic-client-contract.md). It is the current path for external consumers that should no longer depend on GUIDs in the default payload.

---

## TL;DR

- The `AuthorizationGraph` carries `"schemaVersion": "MAJOR.MINOR.PATCH"`.
- SDKs declare a supported range (e.g., `>=1.0.0 <2.0.0`) in package metadata.
- **Server emits MAJOR your SDK doesn't support → SDK rejects** (`AUTH_205`).
- **Server emits MINOR ahead of your SDK → SDK accepts with warning**, unknown fields preserved but unused.
- **Server emits MINOR behind your SDK → SDK accepts with warning**, you may see absent fields.

You almost never need to think about the version. You bump your SDK on a normal cadence; the rules above keep production safe in between.

---

## What changes count as what

### MAJOR bump (breaking)

- Removing a required field
- Renaming a field
- Narrowing a field's type
- Removing an enum value that may appear in older payloads
- Changing resolution semantics (e.g., flipping Allow/Deny precedence)
- Removing GUID fields from the default client payload while external clients still depend on them

### MINOR bump (additive)

- Adding an optional field
- Adding a new enum value to an open enumeration
- Adding a new top-level section
- Adding a new `featureFlags[]` criteria type
- Adding a support-only diagnostic mode that exposes IDs without changing the default surface

### PATCH bump (cosmetic)

- Documentation-only updates
- Tightening validation patterns that do not reject previously-valid payloads

When in doubt, the schema owner bumps higher. False-MAJOR is annoying; false-MINOR is an incident.

---

## What you do as an SDK consumer

1. **Pin a MAJOR range.** Your package.json or csproj already does this through `umsSchemaCompatibility`. Don't widen the range unless you've tested against a new MAJOR.
2. **Watch logs for these events:**
   - `SchemaMinorMismatchEvent` — server emits a newer MINOR. Consider updating soon.
   - `SchemaServerOlderEvent` — your SDK is newer than the server. Usually safe but track UMS server upgrades.
   - `UnknownFieldsObservedEvent` — server is sending fields you can't use. Update at your next opportunity.
   - `DeprecatedFieldUsageEvent` — your code reads a field marked for removal. Migrate before next MAJOR.
3. **Re-authenticate on `AUTH_205`** — your SDK can't safely interpret what the server sent. Either the server upgraded (you should too) or the server downgraded (talk to your UMS operator).

---

## What you do as a UMS server contributor

When you propose a graph change, document the bump category in the PR description:

> Schema impact: MINOR — adds optional `context.user.locale` field.

CI validates that the schema's declared `schemaVersion` matches the change category by running golden fixtures from the previous version against the new schema.

For MAJOR changes:
1. Open a separate ADR documenting the breakage and the migration path.
2. Update `compatibility-matrix.md`.
3. Communicate via release notes at least one MINOR release ahead.

For deprecations:
1. Mark the field in JSON Schema with `"deprecated": true` and `"x-deprecation-removed-in": "2.0.0"`.
2. Wait at least two MINOR releases or six months.
3. Remove in the next MAJOR.

---

## Deprecation lifecycle example

| Release | Action |
|---|---|
| Schema 1.4.0 (MINOR) | Add `context.tenant.legalName` (new). Mark `context.tenant.name` as `deprecated: true`, replacement is `legalName`. |
| Schema 1.5.0 (MINOR) | SDKs reading `name` emit `DeprecatedFieldUsageEvent`. |
| Schema 1.6.0 (MINOR) | Same — last grace period. |
| Schema 2.0.0 (MAJOR) | `context.tenant.name` removed. Old SDKs (`<2.0.0` compat) hit `AUTH_205`. Migration guide published. |

Minimum window: two MINORs or six months, whichever is longer.

---

## References

- [ADR-0074: Auth Graph Schema Versioning Policy](../../architecture/adrs/0074-auth-graph-schema-versioning.md) — full decision
- [Schema Overview](./schema-overview.md) — current contract shape
- [Semantic Auth Graph Client Contract](./semantic-client-contract.md) — current semantic contract
- [Compatibility Matrix](./compatibility-matrix.md) — what works with what
- `src/libs/sdk/contracts/SCHEMA_VERSIONING.md` — operational summary inside the source tree
