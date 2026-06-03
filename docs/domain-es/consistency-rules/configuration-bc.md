# Reglas de Consistencia - BC Configuration

> **Contexto Delimitado:** `Ums.Domain.Configuration`

## Parameter Domain Model

| Aggregate Root | Rol |
|---|---|
| `ParameterDefinition` | Esquema canónico del parámetro |
| `ParameterGlobalValue` | Valor global por defecto |
| `ParameterTenantValue` | Override por tenant |

## ParameterDefinition

| Operación | Regla | Broken Rule |
|---|---|---|
| `Create()` | El código debe ser único dentro del scope | `configuration.parameter_code_not_unique` |
| `Archive(globalValueCount, tenantValueCount)` | No puede archivarse si existen valores activos | `configuration.parameter_has_active_values` |

## ParameterGlobalValue

| Operación | Regla | Broken Rule |
|---|---|---|
| `Create()` / `UpdateValue()` | El valor debe coincidir con el `DataType` | `configuration.parameter_value_invalid_type` |
| `Archive(activeTenantValueCount)` | No puede archivarse si sigue en uso por tenants | `configuration.parameter_global_value_in_use` |

## ParameterTenantValue

| Operación | Regla | Broken Rule |
|---|---|---|
| `Create()` / `UpdateValue()` | El valor debe coincidir con el `DataType` | `configuration.parameter_value_invalid_type` |
| `Create()` | El scope debe permitir overrides por tenant | `configuration.parameter_override_not_allowed` |

## FeatureFlag

| Operación | Regla | Broken Rule |
|---|---|---|
| `AddCriteria()` | La tupla `(CriteriaType, Operator, Value)` debe ser única | `configuration.duplicate_criteria` |
| `UpdateTargeting()` | Si el tipo es Percentage, el porcentaje debe estar entre 0 y 100 | `configuration.flag_percentage_out_of_range` |
