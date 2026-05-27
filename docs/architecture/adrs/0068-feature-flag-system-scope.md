# ADR-0068: Feature Flag System Scope — SystemSuite Ownership and Dynamic Criteria Model

**Status:** Accepted
**Date:** 2026-05-27
**Decision Owner:** Architecture
**Related:**
- [ADR-0054: Shell Library Isolation](./0054-shell-library-isolation.md)
- [ADR-0061: Execution Context Accessor](./0061-execution-context-accessor.md)
- [FeatureFlag Domain](../../domain/configuration/feature-flag.md)
- [FeatureFlagCriteria Model](../../domain/configuration/feature-flag-criteria.md)
- [BC Configuration Context](../../governance/construction/ddd-design/05-configuration-context.md)

---

## Context

The original `FeatureFlag` aggregate was designed as a global toggle: `FlagCode` was unique across the entire platform without any ownership scope. This created two operational problems:

1. **No system isolation.** A flag activated globally could affect tenants or system suites that should not be included in a specific rollout. There was no mechanism to constrain a flag to a particular system boundary.

2. **Rigid targeting model.** The original `FlagTargets` property was a free-form JSON string that described targeting rules. This made it impossible to query, validate, or evolve targeting conditions without parsing opaque payloads. Adding or removing a single targeting condition required replacing the entire JSON blob.

The Authorization bounded context (BC-B) already models `SystemSuite` as the authoritative unit of feature composition — a system suite groups modules, roles, and permissions for a specific product or system. Feature flags naturally belong to the same boundary: a flag for "enable new export module" is only meaningful within the suite that contains the export module.

A refactoring was needed to introduce `SystemSuite` as the mandatory scope of every feature flag and to replace the opaque targeting JSON with a structured, queryable criteria model.

---

## Decision

### 1. FeatureFlag remains an independent Aggregate Root in BC-C

`FeatureFlag` remains an Aggregate Root in the Configuration bounded context (`ums_configuration` schema). It is not converted into a child entity of the `SystemSuite` aggregate in BC-B. The link between the two aggregates is expressed through a foreign key: `FeatureFlag.SystemSuiteId → ums_authorization.SystemSuites.Id`.

### 2. SystemSuiteId is the mandatory, immutable scope of every flag

Every `FeatureFlag` must be created with a valid `SystemSuiteId`. This value is validated against BC-B at creation time and is immutable thereafter. A flag cannot be transferred from one system suite to another; if a different scope is needed, a new flag must be created.

### 3. FlagCode uniqueness changes from global to (SystemSuiteId, FlagCode)

The previous global unique constraint on `FlagCode` is replaced by a composite unique constraint on `(SystemSuiteId, FlagCode)`. The same code string may exist in different system suites without conflict.

### 4. Criteria replace the opaque FlagTargets JSON

A new owned entity `FeatureFlagCriteria` replaces the free-form `FlagTargets` JSON for targeting purposes. Each criterion carries:
- `CriteriaType` — the dimension being evaluated (`TenantId`, `BranchId`, `UserProfileId`, `RoleCode`, `Environment`, `DateRange`, `PercentageHash`, `CustomRule`)
- `Operator` — the comparison to apply (`Equals`, `NotEquals`, `In`, `Between`, `LessThanOrEqual`, `Matches`)
- `Value` — the target value as a JSON-compatible string

The criteria collection is **optional and dynamic**:
- An empty collection means the flag is active for all callers in the system.
- Criteria can be added or removed independently without modifying the aggregate root properties.
- Each change emits a discrete domain event.

### 5. Evaluation semantics: OR within type, AND across types

The `IFeatureFlagEvaluator` port evaluates the criteria collection using the following rules:
- **Within the same CriteriaType:** criteria are combined with **OR** logic.
- **Across different CriteriaType groups:** groups are combined with **AND** logic.

If the evaluation context does not provide the data required by a criterion, the evaluation returns **`false`** (safe posture). This prevents inadvertent feature activation when context is partially populated.

---

## Rationale

### Why FeatureFlag is not a child of SystemSuite

Converting `FeatureFlag` into a child entity of the `SystemSuite` aggregate was considered but rejected for four reasons:

1. **Aggregate size.** `SystemSuite` already owns modules, roles, and permission templates. Adding an unbounded collection of feature flags would grow the aggregate to an unmanageable size, harming load times and increasing the risk of concurrency conflicts.

2. **Independent lifecycle.** Feature flags transition through their own states (`Inactive → Active → Archived`) on a schedule driven by release management, not by the system suite lifecycle. Archiving a flag does not affect the system suite; disabling a system suite does not archive its flags.

3. **Existing pattern.** `AppConfiguration` and `IdpConfiguration` in BC-C demonstrate that configuration aggregates reference BC-B and BC-A identifiers as FKs without becoming children of those aggregates. `FeatureFlag` follows the same established pattern.

4. **Bounded context separation.** Feature flag management is a configuration responsibility (BC-C). Moving it into BC-B would merge two distinct subdomains and violate the single-responsibility principle at the bounded context level.

### Why a structured criteria model rather than free-form JSON

The opaque `FlagTargets` JSON made it impossible to:
- Query flags by targeting condition (e.g., "which flags target tenant T1?")
- Validate targeting rules at the domain boundary
- Emit meaningful domain events when a targeting rule changes
- Add or remove a single condition without replacing the entire payload

A structured `FeatureFlagCriteria` entity collection addresses all four concerns at the cost of a slightly more complex schema.

### Why the safe posture is false on missing context

An absent context value most commonly indicates an anonymous caller, a service request without tenant context, or a client that has not yet migrated to provide the required context field. In all cases, activating a targeted feature for an unidentified caller would be incorrect. The false-on-missing-context rule ensures that new criteria types can be added to a live flag without risk of unintended broad activation.

---

## Consequences

### Positive

- Every feature flag has an explicit, queryable ownership scope aligned with the system boundary it controls.
- Targeting conditions are individually addressable: they can be queried, added, removed, and audited without touching the aggregate root.
- Domain events for criteria changes provide a detailed audit trail for compliance.
- The `IFeatureFlagEvaluator` port keeps evaluation logic extensible and testable in isolation.
- The same flag code can be reused across different system suites without naming conflicts.

### Trade-offs

- The cross-schema FK (`ums_configuration → ums_authorization`) introduces a database-level coupling between two bounded contexts. This is an intentional constraint enforcing referential integrity at the persistence layer.
- Creating a flag now requires a valid `SystemSuiteId`, which means the caller must resolve the suite identity before issuing the command.
- The criteria collection introduces a new table (`FeatureFlagCriteria`) and requires JOIN operations for full flag reads. A read-model projection is recommended for high-frequency evaluation paths.

---

## Alternatives Considered

### Alternative A — FeatureFlag as a child entity of SystemSuite

`FeatureFlag` would become an entity inside the `SystemSuite` aggregate, managed through `SystemSuite` commands.

**Rejected.** As argued in the Rationale section, this bloats the `SystemSuite` aggregate, couples two different lifecycle boundaries, and violates bounded context separation. The independent aggregate pattern is already established for other configuration entities in BC-C.

### Alternative B — External feature flag service (LaunchDarkly / Unleash)

Replace the internal domain model with an external SaaS feature flag provider and expose it through the `IFeatureFlagPort` anti-corruption layer already defined in the bounded context map.

**Out of scope.** The existing `IFeatureFlagPort` ACL entry in the bounded context map accounts for this possibility. The decision to keep feature flags internal is a product scope decision, not an architectural one. This ADR governs the internal model design; the external integration path remains available through the ACL port without requiring changes to this decision.

---

## Implementation Mapping

| Concern | Location |
|---|---|
| Aggregate root | `Ums.Domain.Configuration.FeatureFlag` |
| Criteria entity | `Ums.Domain.Configuration.FeatureFlagCriteria` |
| Evaluator port | `Ums.Domain.Configuration.Ports.IFeatureFlagEvaluator` |
| Evaluator implementation | `Ums.Infrastructure.Configuration.FeatureFlagEvaluator` |
| `FeatureFlags` table | `ums_configuration.FeatureFlags` — ADD `SystemSuiteId` FK, `TenantId`; CHANGE UK |
| `FeatureFlagCriteria` table | `ums_configuration.FeatureFlagCriteria` (new table) |
| New commands | `AddFeatureFlagCriteriaCommand`, `RemoveFeatureFlagCriteriaCommand`, `UpdateFeatureFlagCommand` |
| Modified commands | `CreateFeatureFlagCommand` (+ `SystemSuiteId`, `TenantId`), `EvaluateFeatureFlagCommand` (typed `EvaluationContext`) |
| New queries | `GetFeatureFlagsBySystemSuiteQuery`, `GetFeatureFlagCriteriaQuery` |

---

**[ADR Registry](./index.md)** | **[FeatureFlag Domain](../../domain/configuration/feature-flag.md)** | **[FeatureFlagCriteria](../../domain/configuration/feature-flag-criteria.md)**
