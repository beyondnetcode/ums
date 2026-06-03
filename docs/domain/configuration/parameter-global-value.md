# ParameterGlobalValue — Aggregate Architecture

**Bounded Context:** Configuration  
**Aggregate Root:** `ParameterGlobalValue`  
**Module:** `Ums.Domain.Configuration.Parameter`  
**Status:** Production

---

## 1. Aggregate Overview

### Purpose
`ParameterGlobalValue` stores the system-wide default value for a parameter defined by `ParameterDefinition`. It is the fallback when no tenant-specific override exists.

### State Machine

```
Active ──Archive()──► Archived (terminal)
```

### Invariants
1. Value must conform to the `DataType` specified in the referenced `ParameterDefinition`.
2. Only one active global value per `ParameterDefinition`.
3. A global value cannot be archived while tenant overrides are still active.

---

## 2. Related Aggregates

| Aggregate | Relationship |
|-----------|-------------|
| `ParameterDefinition` | Defines the schema and constraints for the value. |
| `ParameterTenantValue` | Can override this value for a specific tenant. |

---

**[Back to Configuration BC Index](./index.md)** | **[Consistency Rules](../consistency-rules/configuration-bc.md)**
