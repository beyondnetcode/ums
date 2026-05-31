# Golden Fixtures

> **Language:** English | [Español](../../sdk-es/contracts/fixtures.md)

Golden fixtures are JSON files in `src/libs/sdk/contracts/fixtures/` that capture **representative `AuthorizationGraph` payloads** covering the full spectrum of behaviors. They are the executable contract: every SDK and the UMS server must agree on how to interpret them.

---

## 1. What fixtures guarantee

Each fixture is round-tripped through every artifact that touches the schema:

1. **JSON Schema validation** — the fixture conforms to `auth-graph.schema.json`.
2. **Server builder regression** — UMS's `AuthorizationGraphBuilderService` can produce this exact shape given the equivalent input.
3. **SDK deserialization** — `Ums.Sdk.Contracts`, `@ums/sdk-contracts` parse it without loss.
4. **SDK validator behavior** — given this fixture and a probe (e.g., "check `RequiresScope('STOCK_VIEW.VIEW')`"), every runtime returns the same `AuthorizationDecision`.
5. **Re-serialization** — every SDK can serialize the deserialized graph back into JSON that still validates against the schema.

CI runs all five steps on every PR. A failure in any step blocks the PR.

---

## 2. Canonical fixture set (v1.0.0)

| File | Scenario | Used to verify |
|---|---|---|
| `local-auth-success.json` | Happy path Local (BCrypt) auth, OrgWide profile, mix of Allow / NotGranted / Deny | Baseline shape, scope generation |
| `idp-auth-success.json` | Happy path IDP (Azure AD) auth, BranchScoped profile, MFA required | IDP `authentication` block, `branch` not null |
| `deny-wins.json` | Same action has Allow in one place and Deny in another | Deny-precedence rule (Axiom A3) |
| `override-allow.json` | Profile override flips a Template Deny to Allow | Override-takes-precedence rule |
| `empty-permissions.json` | User has Profile but no permissions granted | Empty `scopes[]`, `domainPermissions[]` |
| `expired-graph.json` | `validUntil < generatedAt + 1ms` (deliberately expired) | `AUTH_201` rejection by SDKs |
| `feature-flag-matched.json` | Flag matches via `BranchId` criteria | `isEnabled = true`, `matchedCriteriaType` set |
| `feature-flag-missed-context.json` | Flag exists, but context doesn't provide required field | `isEnabled = false`, `matchedCriteriaType = null` |
| `multi-tenant-rejection.json` | Graph is for tenant A, used to probe a tenant-B-expected client | `AUTH_109` tenant mismatch |
| `schema-unsupported-major.json` | `schemaVersion: "2.0.0"` (future MAJOR) | SDK with `<2.0.0` compat rejects with `AUTH_205` |
| `schema-minor-ahead.json` | `schemaVersion: "1.99.0"`, contains unknown optional field | Accept-with-warning, preserve in `extensions` |
| `schema-missing.json` | Payload without `schemaVersion` | `AUTH_204` rejection |

---

## 3. Conventions for writing fixtures

- **Use the placeholder UUID pattern** introduced in `auth-graph.md` §8 (`11111111-1111-...` for tenants, `22222222-...` for system suites, etc.). Predictable IDs make diffs readable.
- **Use fictional but realistic tenant codes**: `LOGISTICS_CORE`, `ACME_RETAIL`, `EXAMPLE_BANK`. Never real customer names.
- **No PII**: emails follow `firstname.lastname@<fictional-domain>.example`. Use `.example` per RFC 2606.
- **No secrets, no tokens, no hashes** of real values. JWT values in fixtures are literal `"PLACEHOLDER"`.
- **One fixture, one scenario**: don't pile multiple semantic concerns into one fixture. If you need to test two things, write two fixtures.
- **Fixtures are immutable once shipped**: changing an existing fixture is a coordinated change across all SDK test suites. Adding a new fixture is the default.

---

## 4. Adding a new fixture

1. Create `src/libs/sdk/contracts/fixtures/<scenario-name>.json`.
2. Add a row to this file's table above.
3. Add corresponding test cases in each SDK's test suite, referencing the fixture by relative path.
4. Run the CI contract-validation job locally to confirm round-trip passes.
5. PR includes: new fixture + 3 SDK test updates + docs row.

---

## 5. Removing a fixture

A fixture can only be removed if the scenario it represents is no longer reachable in any supported schema version. Removal requires:

1. Confirmation in the PR description that no supported schema produces a payload matching this fixture.
2. Removal of corresponding SDK test cases.
3. Row deleted from this file.

If a fixture becomes outdated due to a MAJOR schema change, prefer to **migrate it** (update to match the new schema) rather than delete. Historic fixtures are useful for regression analysis.

---

## 6. References

- [Schema Overview](./schema-overview.md)
- [Versioning Policy](./versioning.md)
- [Authorization Graph (domain doc)](../../domain/identity/auth-graph.md)
- Fixtures source directory: `src/libs/sdk/contracts/fixtures/`
