# Reglas de Consistencia - BC Identity

> **Contexto Delimitado:** `Ums.Domain.Identity`

## Tenant

| Operación | Regla | Broken Rule |
|---|---|---|
| `Archive(activeIdpCount > 0)` | No puede archivarse con IdP activos | `TENANT_HAS_ACTIVE_IDP` |
| `Suspend(activeUserCount > 0)` | No puede suspenderse con usuarios activos | `TENANT_HAS_ACTIVE_USERS` |
| `Suspend(activeBranchCount > 0)` | No puede suspenderse con sucursales activas | `TENANT_HAS_ACTIVE_BRANCHES` |

## UserAccount

| Operación | Regla | Broken Rule |
|---|---|---|
| `Delete(activeProfileCount > 0)` | No puede eliminarse con profiles activos | `USER_HAS_ACTIVE_PROFILES` |

