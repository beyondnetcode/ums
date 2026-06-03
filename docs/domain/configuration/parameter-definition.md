# ParameterDefinition — Aggregate Architecture

**Bounded Context:** Configuration  
**Aggregate Root:** `ParameterDefinition`  
**Module:** `Ums.Domain.Configuration.Parameter`  
**Status:** Production

---

## 1. Aggregate Overview

### Purpose
`ParameterDefinition` defines the schema of a configurable system parameter: its code, data type, scope (Global or Tenant), allowed values, and whether tenant-level overrides are permitted. It does not store parameter values — it only defines what parameters exist and their constraints.

### Aggregate Root
`ParameterDefinition` is the schema authority. Parameter values are stored in separate aggregates: `ParameterGlobalValue` (system-wide default) and `ParameterTenantValue` (tenant-specific override).

### State Machine

```
Active ──Archive()──► Archived (terminal)
```

### Invariants
1. Parameter code must be unique per scope.
2. `DataType` determines which values are valid in `ParameterGlobalValue` and `ParameterTenantValue`.
3. Tenant-level overrides are only valid when the scope supports tenant values.
4. A definition cannot be archived while active global or tenant values exist.

---

## 2. Related Aggregates

| Aggregate | Relationship |
|-----------|-------------|
| `ParameterGlobalValue` | References this definition by `DefinitionId`. One global value per definition. |
| `ParameterTenantValue` | References this definition by `DefinitionId`. One per (definition, tenant) pair. |

---

## 3. Public Operations

| Method | Description |
|--------|-------------|
| `Create(code, name, description, dataType, defaultValue, scope, isActive, isMandatory, displayOrder, createdBy, existingDefinitionCount)` | Creates a new parameter definition when the code is still unique within scope. |
| `Archive(updatedBy, globalValueCount, tenantValueCount)` | Archives the definition if no active values reference it. |

---

**[Back to Configuration BC Index](./index.md)** | **[Consistency Rules](../consistency-rules/configuration-bc.md)**
