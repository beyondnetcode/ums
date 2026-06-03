# ParameterTenantValue — Aggregate Architecture

**Bounded Context:** Configuration  
**Aggregate Root:** `ParameterTenantValue`  
**Module:** `Ums.Domain.Configuration.Parameter`  
**Status:** Production

---

## 1. Aggregate Overview

### Purpose
`ParameterTenantValue` stores a tenant-specific override for a parameter. When present and active, it takes precedence over `ParameterGlobalValue` for that tenant.

### State Machine

```
Active ──Archive()──► Archived (terminal)
```

### Invariants
1. The referenced `ParameterDefinition` must explicitly allow tenant overrides.
2. Value must conform to the `DataType` of the referenced `ParameterDefinition`.
3. Only one active tenant value per (ParameterDefinition, Tenant) pair.

---

## 2. Related Aggregates

| Aggregate | Relationship |
|-----------|-------------|
| `ParameterDefinition` | Defines schema and whether override is allowed. |
| `ParameterGlobalValue` | The value this overrides. |
| `Tenant` | The tenant this value applies to. |

---

**[Back to Configuration BC Index](./index.md)** | **[Consistency Rules](../consistency-rules/configuration-bc.md)**
