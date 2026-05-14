# 🗄️ Modelo Entidad-Relación (E/R) - SQL Server 2022

**Tipo de Documento:** Diseño de Base de Datos  
**Estatus:** Refactorizado (Modelo Perfil Enterprise)  
**Arquitectura:** Multi-tenancy Jerárquico (Nexo de Perfiles)  
**Motor:** SQL Server 2022

## 1. Introducción
Este documento detalla el modelo de datos **Centrado en el Perfil Enterprise** para el **User Management System (UMS)**. El modelo impone una propiedad jerárquica estricta y posiciona al `Profile` como la intersección contextual de la identidad y la autorización.

---

## 2. Estándares Corporativos de Auditoría y Trazabilidad
Cada entidad en este esquema DEBE implementar las siguientes columnas.

| Columna | Tipo | Descripción |
| :--- | :--- | :--- |
| `CreatedAt` | `datetimeoffset` | Marca de tiempo de creación. |
| `CreatedBy` | `uniqueidentifier` | ID del creador. |
| `UpdatedAt` | `datetimeoffset` | Marca de tiempo de actualización. |
| `UpdatedBy` | `uniqueidentifier` | ID del último actualizador. |
| `DeletedAt` | `datetimeoffset` | Marca de tiempo de eliminación lógica. |
| `DeletedBy` | `uniqueidentifier` | ID del eliminador. |
| `Version` | `int` | Bloqueo optimista (Predeterminado: 1). |
| `IsActive` | `bit` | Indicador de estado activo. |
| `TenantId` | `uniqueidentifier` | Aislamiento contextual (cuando aplique). |
| `CorrelationId`| `uniqueidentifier` | Trazabilidad en operaciones distribuidas. |

---

## 3. Diagrama E/R (Mermaid)

```mermaid
erDiagram
    TENANT ||--o{ SYSTEM_SUITE : "posee"
    TENANT ||--o{ BRANCH : "posee"
    TENANT ||--o{ USER : "posee"
    TENANT ||--o{ PROFILE : "gobierna"
    TENANT ||--o{ AUDIT_LOG : "monitorea"

    SYSTEM_SUITE ||--o{ ROLE : "define"
    SYSTEM_SUITE ||--o{ PERMISSION : "categoriza"
    SYSTEM_SUITE ||--o{ MENU_ITEM : "topología"
    SYSTEM_SUITE ||--o{ PERMISSION_TEMPLATE : "provee_esquemas"
    SYSTEM_SUITE ||--o{ PROFILE : "contexto_funcional"

    BRANCH ||--o{ PROFILE : "ubicación_operativa"

    USER ||--o{ PROFILE : "identidad_activa"
    USER ||--|| USER_CREDENTIAL : "credenciales"

    ROLE ||--o{ ROLE_PERMISSION : "autorizaciones_base"
    ROLE ||--o{ PROFILE : "esquema_de_autoridad"

    PERMISSION ||--o{ ROLE_PERMISSION : "vincula"
    PERMISSION ||--o{ PROFILE_PERMISSION : "vínculos_efectivos"
    PERMISSION ||--o{ TEMPLATE_PERMISSION : "vínculos_de_plantilla"
    PERMISSION ||--o{ MENU_ITEM : "guarda_de_acceso"

    PERMISSION_TEMPLATE ||--o{ TEMPLATE_PERMISSION : "contiene"
    PERMISSION_TEMPLATE ||--|| ROLE : "representa_rol_de_sistema"

    PROFILE ||--o{ PROFILE_PERMISSION : "consolida"

    MENU_ITEM ||--o{ MENU_ITEM : "jerarquía"

    TENANT {
        uniqueidentifier TenantId PK
        nvarchar Name "NOT NULL"
        nvarchar Code "UK"
    }

    SYSTEM_SUITE {
        uniqueidentifier SuiteId PK
        uniqueidentifier TenantId FK
        nvarchar Name
        nvarchar Code
    }

    BRANCH {
        uniqueidentifier BranchId PK
        uniqueidentifier TenantId FK
        nvarchar Name
        nvarchar Code
    }

    ROLE {
        uniqueidentifier RoleId PK
        uniqueidentifier SuiteId FK
        uniqueidentifier TenantId FK
        nvarchar Name
    }

    PERMISSION_TEMPLATE {
        uniqueidentifier TemplateId PK
        uniqueidentifier SuiteId FK
        uniqueidentifier RoleId FK
        nvarchar Name
    }

    PROFILE {
        uniqueidentifier ProfileId PK
        uniqueidentifier TenantId FK
        uniqueidentifier UserId FK
        uniqueidentifier SystemId FK
        uniqueidentifier BranchId FK
        uniqueidentifier RoleId FK
        nvarchar DisplayName
    }

    PROFILE_PERMISSION {
        uniqueidentifier ProfileId PK, FK
        uniqueidentifier PermissionId PK, FK
        bit IsDenied "Anulación de Permiso"
        nvarchar GrantReason
    }

    PERMISSION {
        uniqueidentifier PermissionId PK
        uniqueidentifier SuiteId FK
        nvarchar Code "UK"
        nvarchar Name
    }

    AUDIT_LOG {
        bigint LogId PK
        uniqueidentifier TenantId
        uniqueidentifier UserId
        uniqueidentifier ProfileId
        uniqueidentifier CorrelationId
        uniqueidentifier TransactionId
        datetimeoffset Timestamp
    }
```

---

## 4. Reglas de Negocio y Normalización
1.  **Aislamiento Estricto**: Un Rol no puede existir fuera de un contexto de Sistema.
2.  **Integridad Contextual**: Un Perfil solo puede crearse si el Rol seleccionado pertenece al Sistema seleccionado, y ambos pertenecen al mismo Tenant.
3.  **Hub de Plantillas**: Las plantillas de permisos vinculan a un Rol de Sistema específico, permitiendo la inicialización de perfiles de forma agnóstica a la sucursal.
4.  **Eliminación Lógica (Soft Delete)**: Los datos nunca se eliminan físicamente; se completa `DeletedAt` para mantener el historial.
5.  **Persistencia Efectiva**: `PROFILE_PERMISSION` actúa como la fuente de verdad para el `Motor de Autorización`, combinando los valores por defecto del Rol con las anulaciones específicas del Perfil.
