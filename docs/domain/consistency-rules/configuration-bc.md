# Configuration BC — Consistency Rules

> **Bounded Context:** `Ums.Domain.Configuration`  
> **Aggregates:** `AppConfiguration`, `IdpConfiguration`, `FeatureFlag`, `ParameterDefinition`, `ParameterGlobalValue`, `ParameterTenantValue`

---

## AppConfiguration

### State Machine

```
Draft ──Publish()──► Published ──Archive()──► Archived (terminal)
```

### Intra-aggregate Guards

| Operation | Guard | Broken Rule |
|-----------|-------|-------------|
| Any mutation | Status must be `Draft` | `configuration.app_config_not_draft` |
| `Archive()` | Status must not already be `Archived` | `configuration.app_config_already_archived` |

---

## IdpConfiguration

### State Machine

```
Draft ──Activate()──► Active ──Deactivate()──► Inactive
Inactive ──Activate()──► Active
```

### Intra-aggregate Guards

| Operation | Guard | Broken Rule |
|-----------|-------|-------------|
| Mutations | Status must be `Draft` | `configuration.idp_config_not_draft` |
| `Activate()` | Status must not be `Active` | `configuration.idp_config_already_active` |
| `Deactivate()` | Status must not be `Inactive` | `configuration.idp_config_already_inactive` |

---

## FeatureFlag

### State Machine

```
Active ──Deactivate()──► Inactive
Inactive ──Activate()──► Active
Active/Inactive ──Archive()──► Archived (terminal)
```

**Archived is terminal — no transitions out.**

### Intra-aggregate Guards

| Operation | Guard | Broken Rule |
|-----------|-------|-------------|
| Any mutation | Status must not be `Archived` | `configuration.flag_archived_cannot_change` |
| `Activate()` | Flag must not already be `Active` | `configuration.flag_already_active` |
| `Deactivate()` | Flag must not already be `Inactive` | `configuration.flag_already_inactive` |
| `Create()/UpdateTargeting()` | Rollout percentage must be 0–100 | `configuration.flag_percentage_out_of_range` |
| `AddCriteria()` | Criteria (type, operator, value) must be unique | `configuration.duplicate_criteria` |
| `RemoveCriteria()` | Criteria must exist | `configuration.criteria_not_found` |

### Child Entity Cascade Rules

| Event | Cascade |
|-------|---------|
| `Archive()` | `FlagEvaluationLog` is append-only — no cascade needed. |
| `RemoveCriteria()` | Criteria removed from collection. No orphan risk. |

---

## ParameterDefinition

### State Machine

```
Active ──Archive()──► Archived (terminal)
```

### Intra-aggregate Guards

| Operation | Guard | Broken Rule |
|-----------|-------|-------------|
| `Create()` | Code must be unique per scope | `configuration.parameter_code_not_unique` |

### Cross-Aggregate Dependency Guards

| Operation | Guard (count parameter) | Broken Rule |
|-----------|------------------------|-------------|
| `Archive(globalValueCount, tenantValueCount)` | `globalValueCount > 0 \|\| tenantValueCount > 0` | `configuration.parameter_has_active_values` |

### Notes

`ParameterDefinition` defines the **schema** of a configurable parameter (code, data type, scope). It does not store values itself. Values live in `ParameterGlobalValue` and `ParameterTenantValue`.

---

## ParameterGlobalValue

### State Machine

```
Active ──Archive()──► Archived (terminal)
```

### Intra-aggregate Guards

| Operation | Guard | Broken Rule |
|-----------|-------|-------------|
| `Create()` / `Update()` | Value must conform to ParameterDefinition DataType | `configuration.parameter_value_invalid_type` |

### Cross-Aggregate Dependency Guards

| Operation | Guard (count parameter) | Broken Rule |
|-----------|------------------------|-------------|
| `Archive(activeTenantValueCount)` | Must not be the active effective value for any tenant | `configuration.parameter_global_value_in_use` |

---

## ParameterTenantValue

### State Machine

```
Active ──Archive()──► Archived (terminal)
```

### Intra-aggregate Guards

| Operation | Guard | Broken Rule |
|-----------|-------|-------------|
| `Create()` / `Update()` | Value must conform to ParameterDefinition DataType | `configuration.parameter_value_invalid_type` |
| `Create()` | ParameterDefinition must allow tenant override | `configuration.parameter_override_not_allowed` |

### Notes

`ParameterTenantValue` overrides a global value for a specific tenant. If no tenant value exists, the system falls back to `ParameterGlobalValue`.

---

*Part of [consistency-rules/index.md](./index.md)*
