# ADR-0074: Auth Graph Schema Versioning Policy

**Status:** Accepted
**Date:** 2026-05-31
**Decision Owner:** Architecture
**Related:**
- [ADR-0071: Auth Graph Engine](./0071-auth-graph-engine.md)
- [ADR-0073: UMS SDK — Multi-Runtime Client Integration Surface](./0073-ums-sdk-multi-runtime.md)

---

## Context

ADR-0073 establishes the `AuthorizationGraph` JSON Schema as the canonical contract between the UMS server and three SDK runtimes (.NET, TypeScript, NestJS). The schema evolves: new fields are added when the graph carries new information, existing fields are deprecated, occasionally a field is removed or its semantics change.

A multi-runtime SDK with independent release cadence per package means the server may emit a graph at version N while a client is still running an SDK that knows version N-1. Without a documented compatibility policy, every schema change becomes a coordination problem that can break clients in production.

This ADR defines how the schema is versioned, how compatibility is decided per change category, and how SDKs and the server enforce it.

---

## Decision

### 1. The graph carries an explicit `schemaVersion` field

Every `AuthorizationGraph` payload emitted by `AuthorizationGraphBuilderService` includes a top-level `schemaVersion` string conforming to semantic versioning (`MAJOR.MINOR.PATCH`).

```json
{
  "schemaVersion": "1.2.0",
  "context": { ... },
  ...
}
```

The field is required and validated against `auth-graph.schema.json`'s own `schemaVersion` constant. Absence of the field is interpreted by SDKs as "v0.x — unsupported, refuse to deserialize".

### 2. Semantic versioning rules — what counts as MAJOR / MINOR / PATCH

| Bump | Trigger |
|---|---|
| **MAJOR** | A change that an SDK targeting the previous major **cannot** deserialize or interpret safely. Examples: removing a required field, renaming a field, narrowing a field's type, removing an enum value that may appear in older payloads, changing the resolution semantics (e.g., flipping Allow/Deny precedence). |
| **MINOR** | A change that is **backward-compatible** for readers of the previous major: adding an optional field, adding a new enum value to an open enumeration, adding a new section that is independent of existing sections, adding a new `featureFlags[]` criteria type. |
| **PATCH** | Documentation-only changes to the schema, tightening of validation patterns that do not reject previously-valid payloads, clarifying field descriptions. No structural changes. |

When in doubt, **prefer the higher bump**. False-MAJOR is annoying; false-MINOR is a production incident.

### 3. SDKs declare a target compatibility range

Each SDK package declares its supported schema range using semver range syntax in its package metadata:

- **.NET (csproj):** custom property `<UmsSchemaCompatibility>>=1.0.0 &lt;2.0.0</UmsSchemaCompatibility>`
- **npm (package.json):** custom field `"umsSchemaCompatibility": ">=1.0.0 <2.0.0"`

The SDK reads this at startup, exposes it via `ISdkMetadata.SchemaCompatibility` (or equivalent in each runtime), and uses it to validate incoming graphs.

### 4. Strict on MAJOR, permissive on MINOR — the runtime contract

| Server emits | SDK supports | SDK behavior |
|---|---|---|
| `1.0.0` | `>=1.0.0 <2.0.0` | **Accept** — exact match |
| `1.2.0` | `>=1.0.0 <2.0.0` | **Accept with warning** — server is newer, unknown fields preserved but unused. Log structured event `SchemaMinorMismatchEvent`. |
| `1.0.0` | `>=1.2.0 <2.0.0` | **Accept with warning** — SDK is newer, server emits subset. Log structured event `SchemaServerOlderEvent`. |
| `2.0.0` | `>=1.0.0 <2.0.0` | **Reject** — error `AUTH_GRAPH_SCHEMA_UNSUPPORTED`. SDK refuses to deserialize. |
| `0.x` or missing | any | **Reject** — error `AUTH_GRAPH_SCHEMA_MISSING`. |

The rationale: a MINOR bump means readers of the previous version can still extract everything they need; warning is appropriate. A MAJOR bump means semantics changed; only hard rejection is safe.

### 5. Unknown-field policy: preserve, warn, do not error

When deserializing a graph at a MINOR ahead of the SDK's compatibility ceiling, unknown fields are:

- **Preserved** in the in-memory representation (an `extensions: Map<string, unknown>` bag), so re-serialization (for caching, logging) round-trips faithfully.
- **Ignored** for authorization decisions — the SDK cannot apply rules it doesn't understand.
- **Logged once per session** as a structured `UnknownFieldsObservedEvent` listing the field paths, so operators can detect SDK staleness.

This explicitly rejects two alternatives: strict-only (reject any unknown field) which makes MINOR bumps breaking, and silent-ignore which hides staleness from operations.

### 6. Deprecation lifecycle for removing or changing fields

Removing or changing a field requires the following sequence:

1. **MINOR N**: introduce the replacement field. Both old and new coexist. Mark old as `deprecated: true` in the JSON Schema with `description` pointing to the replacement and a target removal version.
2. **MINOR N+1 or later**: SDKs emit `DeprecatedFieldUsageEvent` whenever consumer code reads the deprecated field.
3. **MAJOR N+1**: remove the deprecated field. Servers stop emitting it. Documentation and migration guide updated.

Minimum window between steps 1 and 3: **two MINOR releases** or **six months**, whichever is longer.

### 7. Compatibility matrix is a maintained artifact

`docs/sdk/contracts/compatibility-matrix.md` (bilingual) lists every published SDK version against every emitted schema version, marked `Accept`, `Accept-with-warning`, or `Reject`. CI updates this file when SDKs are released. The matrix is the source of truth for support questions ("does Ums.Sdk.Authorization 1.4.2 work with UMS server emitting schema 2.0?").

### 8. Schema version is independent of SDK package version

- `auth-graph.schema.json` carries `schemaVersion: "1.0.0"`.
- `Ums.Sdk.Contracts` may be `1.0.0`, `1.5.3`, etc. — its version reflects API changes in the DTO surface, which may bump independently (e.g., for naming refactors that do not affect the wire schema).
- A single schema version can be supported by many SDK package versions during its lifetime.

The two version axes are **never** conflated. The graph carries `schemaVersion`. The SDK declares `umsSchemaCompatibility`. Neither says anything about the other's package version.

---

## Rationale

### Why semver applied to the schema and not just to packages

The schema is a public contract consumed by code we don't control (third-party client systems running deployed SDKs). Semver is the universally understood vocabulary for compatibility promises. Inventing a custom scheme means every consumer has to learn it.

### Why strict-on-MAJOR / permissive-on-MINOR

This matches the semver promise: MAJOR bumps explicitly signal "you must update to keep working", MINOR bumps explicitly signal "your code keeps working". Hard-rejecting MAJOR mismatches forces upgrades that would otherwise be unsafe; accepting MINOR with logging surfaces staleness without breaking production.

### Why a deprecation window of two MINORs / six months

A single MINOR window is too short — consumers may not even notice the deprecation before it's gone. Indefinite windows clutter the schema. Two MINORs / six months matches the cadence at which client systems typically refresh their SDK dependencies in regulated environments.

### Why preserve unknown fields instead of stripping them

Stripping unknown fields breaks round-trip serialization — if the SDK caches the graph and later re-emits it (for diagnostic dumps, debugging), the cached graph silently differs from what the server sent. Preserving with `extensions` keeps round-trip integrity without granting the unknown fields authorization weight.

### Why the compatibility matrix is a maintained doc and not just code

Support engineers, integration partners, and security reviewers ask "what works with what?" via documentation channels, not by reading test fixtures. A maintained matrix is the lowest-friction answer. CI maintenance prevents drift from reality.

---

## Consequences

### Positive

- Schema evolution is governable: every change has a clear compatibility implication communicated by the version bump.
- Clients running older SDKs continue working through MINOR bumps with structured warnings instead of silent breakage.
- The compatibility matrix gives operations and support a single source of truth.
- Deprecation is an explicit lifecycle, not a surprise — clients have time to migrate before a field disappears.

### Trade-offs

- Adding a field to the graph is no longer a free action — every server-side change requires a schema bump decision, even when ergonomically the change is trivial.
- The `extensions` bag for unknown fields adds memory overhead per graph instance (negligible in practice but present).
- Maintaining the compatibility matrix is documentation work that grows linearly with releases. CI automation mitigates but does not eliminate this.

### Neutral

- ADR-0071 (Auth Graph Engine) does not change — it described the structure; this ADR governs its evolution.
- `schemaVersion` becomes a new required field but is a single string addition — no structural impact on existing consumers (after a MAJOR bump to 1.0.0).

---

## Implementation

| Component | Location | Owner |
|---|---|---|
| `auth-graph.schema.json` with `schemaVersion` constant | `src/libs/sdk/contracts/` | Architecture |
| `SCHEMA_VERSIONING.md` developer summary | `src/libs/sdk/contracts/` | Architecture |
| `compatibility-matrix.md` (EN + ES) | `docs/sdk/contracts/` | Architecture + Release |
| Server emits `schemaVersion` in graph | `Ums.Application/Authorization/Graph/AuthorizationGraphBuilderService.cs` | Backend |
| SDK metadata declares `umsSchemaCompatibility` | Each SDK package | SDK team |
| SDK rejects MAJOR mismatch, warns on MINOR | `Ums.Sdk.Authorization`, `@ums/sdk-authorization` | SDK team |
| `UnknownFieldsObservedEvent`, `SchemaMinorMismatchEvent`, `DeprecatedFieldUsageEvent` logged | All SDKs | SDK team |
| CI job: round-trip every fixture through every SDK | `.github/workflows/sdk-contract-validation.yml` | Platform |

Initial release: schema `1.0.0`. The current graph structure documented in [`auth-graph.md` §8](../../domain/identity/auth-graph.md#8-ejemplo-json-de-respuesta) is the canonical 1.0.0 schema.

---

**[ADR Registry](./index.md)**
