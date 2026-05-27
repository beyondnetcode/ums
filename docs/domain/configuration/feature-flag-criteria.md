# FeatureFlagCriteria — Evaluation Criteria Model

> **Language:** [English](./feature-flag-criteria.md) | [Español](../../domain-es/configuration/feature-flag-criteria.md)

**Bounded Context:** Configuration (`Ums.Domain.Configuration`)
**Owner Aggregate:** `FeatureFlag`
**Entity Type:** Owned Entity (no independent lifecycle)
**Status:** Production

---

## 1. Introduction

`FeatureFlagCriteria` are the building blocks that determine when a `FeatureFlag` is considered active for a specific evaluation context. Each criterion expresses a condition: a particular type of data (e.g., a tenant, a role, a date range), an operator, and a target value.

The criteria collection is **optional and dynamic**:

- A flag with an **empty** criteria collection is active for every caller in the system, regardless of context.
- A flag with **one or more** criteria is active only when the evaluation context satisfies the defined conditions.
- Criteria can be added or removed at any time while the flag is in `Inactive` or `Active` state.

Criteria do not have their own lifecycle states. They exist while attached to the flag and are removed by the `RemoveFeatureFlagCriteriaCommand`.

---

## 2. CriteriaTypes

| CriteriaType | Description | Value JSON type | Supported Operators | Example Value |
|---|---|---|---|---|
| `TenantId` | Matches by tenant identifier | `string` (GUID) | `Equals`, `NotEquals` | `"a3f2e1d0-..."` |
| `BranchId` | Matches by branch identifier | `string` (GUID) | `Equals`, `NotEquals` | `"b7c9a2e1-..."` |
| `UserProfileId` | Matches by user profile identifier | `string` (GUID) | `Equals`, `NotEquals` | `"c1d4f5a2-..."` |
| `RoleCode` | Matches by a role code string | `string` | `Equals`, `NotEquals`, `In` | `"ADMIN"` or `["ADMIN","MANAGER"]` |
| `Environment` | Matches by deployment environment | `string` | `Equals`, `NotEquals`, `In` | `"Production"` or `["Staging","Production"]` |
| `DateRange` | Active within a UTC date range | `{ "from": "ISO8601", "to": "ISO8601" }` | `Between` | `{"from":"2026-06-01T00:00:00Z","to":"2026-09-01T00:00:00Z"}` |
| `PercentageHash` | Activates for a deterministic percentage of users | `string` (integer 0–100) | `LessThanOrEqual` | `"30"` |
| `CustomRule` | Named rule resolved by a registered handler | `string` (rule identifier) | `Matches` | `"beta-tester-group"` |

**Value JSON notes:**

- For `In` operator, the Value must be a JSON array of strings: `["VALUE_A","VALUE_B"]`.
- For `DateRange`, start must be strictly before end (INV-FF5).
- For `PercentageHash`, the hash is computed from a stable user identity (e.g., `UserProfileId`) modulo 100; the criterion passes when `hash % 100 <= threshold`.
- For `CustomRule`, the infrastructure layer resolves the rule name to a concrete evaluator registered in the dependency container.

---

## 3. Combination Rules

The criteria collection follows a two-level Boolean model:

| Level | Rule |
|---|---|
| Within the same `CriteriaType` | **OR** — the condition passes if any single criterion of that type matches |
| Between different `CriteriaType` groups | **AND** — all type groups must pass for the overall result to be `true` |

**Example:** a flag has:
- `TenantId Equals T1`
- `TenantId Equals T2`
- `RoleCode Equals ADMIN`

Evaluation logic: `(TenantId == T1 OR TenantId == T2) AND (RoleCode == ADMIN)`

This means the flag is active only for tenants T1 or T2, **and** only when the caller has the ADMIN role.

---

## 4. Safe Posture — Missing Context Data

If the evaluation context does not provide a value for a `CriteriaType` that is required by a criterion, the entire evaluation returns **`false`**.

**Rationale:** It is safer to deny feature access than to grant it on incomplete information. This prevents inadvertent activation when context is partially populated (e.g., an anonymous caller or a service-to-service request without tenant context).

| Scenario | Result |
|---|---|
| Context provides all required fields | Evaluate normally |
| Context missing one required field | `false` (safe posture) |
| Criteria collection is empty | `true` (active for all) |
| Flag is Archived | Evaluation rejected by invariant |

---

## 5. Evaluation Algorithm (Pseudocode)

```
function Evaluate(flag: FeatureFlag, context: EvaluationContext) -> bool:

    if flag.Criteria is empty:
        return true

    groups = GroupBy(flag.Criteria, c => c.CriteriaType)

    for each group in groups:
        contextValue = context.GetValue(group.CriteriaType)

        if contextValue is null or missing:
            return false   // safe posture

        groupResult = false

        for each criterion in group:
            if ApplyOperator(criterion.Operator, contextValue, criterion.Value):
                groupResult = true
                break       // OR within group — short-circuit

        if groupResult is false:
            return false    // AND across groups — short-circuit

    return true
```

The `IFeatureFlagEvaluator` port exposes this algorithm. Its implementation resides in the infrastructure layer and is injected into the application handler.

```csharp
public interface IFeatureFlagEvaluator
{
    bool Evaluate(FeatureFlag flag, EvaluationContext context);
}
```

---

## 6. End-to-End Evaluation Scenarios

### Scenario 1 — Flag with no criteria (active for all)

**Setup:** Flag `NEW_DASHBOARD` has zero criteria.
**Context:** Any caller.
**Result:** `true` — the flag is active for everyone in the system.

---

### Scenario 2 — Single tenant criterion

**Setup:** Flag `BETA_EXPORT` has one criterion: `TenantId Equals acme-corp-id`.
**Context A:** `{ TenantId: "acme-corp-id" }` → `true`
**Context B:** `{ TenantId: "other-tenant-id" }` → `false`
**Context C:** `{ TenantId: null }` → `false` (safe posture)

---

### Scenario 3 — Multi-type AND combination

**Setup:** Flag `ADVANCED_REPORTS` has:
- `TenantId Equals acme-corp-id`
- `RoleCode In ["ADMIN", "MANAGER"]`

**Context A:** `{ TenantId: "acme-corp-id", RoleCode: "ADMIN" }` → `true`
**Context B:** `{ TenantId: "acme-corp-id", RoleCode: "VIEWER" }` → `false` (role not in list)
**Context C:** `{ TenantId: "other-tenant-id", RoleCode: "ADMIN" }` → `false` (wrong tenant)
**Context D:** `{ TenantId: "acme-corp-id" }` (RoleCode missing) → `false` (safe posture)

---

### Scenario 4 — Date range (temporary rollout)

**Setup:** Flag `SEASONAL_PROMOTION` has one criterion: `DateRange Between { from: "2026-06-01", to: "2026-09-01" }`.
**Context A (evaluated 2026-07-15):** `true` — within range.
**Context B (evaluated 2026-10-01):** `false` — past end date.
**Context C:** `{ CurrentDateUtc: null }` → `false` (safe posture).

---

### Scenario 5 — OR within the same type

**Setup:** Flag `MULTI_TENANT_PILOT` has:
- `TenantId Equals tenant-A`
- `TenantId Equals tenant-B`
- `Environment Equals Staging`

**Context A:** `{ TenantId: "tenant-A", Environment: "Staging" }` → `true` (tenant matches T-A OR T-B; env matches)
**Context B:** `{ TenantId: "tenant-C", Environment: "Staging" }` → `false` (no tenant match)
**Context C:** `{ TenantId: "tenant-B", Environment: "Production" }` → `false` (env does not match)

---

## 7. Reference

- See [`FeatureFlag`](./feature-flag.md) for the owning aggregate, lifecycle, and commands.
- See [ADR-0068](../../architecture/adrs/0068-feature-flag-system-scope.md) for the architectural decision that introduced the criteria model.
- `IFeatureFlagEvaluator` is registered in `Ums.Infrastructure.Configuration`.

---

**[Back to Configuration Index](./index.md)** | **[FeatureFlag](./feature-flag.md)**
