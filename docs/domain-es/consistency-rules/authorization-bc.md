# Reglas de Consistencia - BC Authorization

> **Contexto Delimitado:** `Ums.Domain.Authorization`

## PermissionTemplate

| Operación | Regla | Broken Rule |
|---|---|---|
| `Publish()` | Debe tener al menos un item | `authorization.template_items_required` |
| `Delete()` | Solo `Draft` o `Deprecated` | `authorization.template_not_deletable` |
| `Delete(activeProfileCount > 0)` | No puede haber profiles activos dependientes | `TEMPLATE_HAS_ACTIVE_PROFILES` |

## Role

| Operación | Regla | Broken Rule |
|---|---|---|
| `Deactivate(activeProfileCount > 0)` | No puede dejar profiles activos | `ROLE_HAS_ACTIVE_PROFILES` |
| `Deactivate(activeChildRoleCount > 0)` | No puede dejar roles hijos activos | `ROLE_HAS_ACTIVE_CHILD_ROLES` |

## SystemSuite

| Operación | Regla | Broken Rule |
|---|---|---|
| `RemoveModule(activeMenuCount > 0)` | No puede eliminar módulos con menús activos | `MODULE_HAS_ACTIVE_MENUS` |
| `RemoveDomainResource(templateItemCount > 0)` | No puede eliminar recursos con items de template | `DOMAIN_RESOURCE_HAS_TEMPLATE_ITEMS` |

