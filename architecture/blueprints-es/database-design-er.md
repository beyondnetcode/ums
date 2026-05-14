# 🗄️ Modelo Entidad-Relación (E/R) - SQL Server 2022

**Tipo de Documento:** Diseño de Base de Datos  
**Estatus:** Refactorizado (Impulsado por Plantillas Maestras)  
**Arquitectura:** Framework Jerárquico (Autoridad Materializada)  
**Motor:** SQL Server 2022

> [!TIP]
> **¿Problemas de Visualización?**  
> Si los diagramas Mermaid no se renderizan correctamente, utiliza los **[🚀 Formatos de Exportación Alternativos (dbdiagram.io, DDL, D2)](./er-export-formats.md)**. Estos formatos son compatibles con herramientas profesionales como DBeaver, SSMS y dbdiagram.io.

## 1. Introducción
Este documento detalla el modelo de datos **Impulsado por Plantillas Maestras**. Cada permiso efectivo en el sistema debe ser una instancia materializada de una `PermissionTemplate` controlada, garantizando una gobernanza del 100% sobre el catálogo de autoridad.

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
| `IsActive` | `bit` | Indicador de estado. |
| `TenantId` | `uniqueidentifier` | Aislamiento contextual. |
| `CorrelationId`| `uniqueidentifier` | Trazabilidad distribuida. |

---

## 3. Vistas Modulares por Dominio

### 🗺️ 3.1 Mapa Global de Alto Nivel
Vista completa de las relaciones entre módulos núcleo.

```mermaid
erDiagram
    TENANT ||--o{ SYSTEM_SUITE : "posee"
    TENANT ||--o{ USER : "posee"
    TENANT ||--o{ BRANCH : "posee"
    
    SYSTEM_SUITE ||--o{ FUNCTIONAL_MODULE : "contiene"
    FUNCTIONAL_MODULE ||--o{ PERMISSION_TEMPLATE : "define_catálogo"
    
    PERMISSION_TEMPLATE ||--o{ PROFILE_PERMISSION : "materializado_en"
    
    USER ||--o{ PROFILE : "actúa_como"
    PROFILE ||--o{ PROFILE_PERMISSION : "posee_autoridad"
    
    ROLE ||--o{ PROFILE : "esquema_de_autoridad"
    ROLE ||--o{ ROLE_PERMISSION : "blueprint_base"
    PERMISSION_TEMPLATE ||--o{ ROLE_PERMISSION : "define_base_de_rol"
```

---

### 🔐 3.2 Dominio: Framework de Autorización Maestro (El Núcleo)
Este dominio gestiona el catálogo de permisos inmutables y su materialización en perfiles.

```mermaid
erDiagram
    SYSTEM_SUITE ||--o{ FUNCTIONAL_MODULE : "contiene"
    FUNCTIONAL_MODULE ||--o{ PERMISSION_TEMPLATE : "definiciones_maestras"
    
    PERMISSION_TEMPLATE ||--o{ ROLE_PERMISSION : "esquema_estándar"
    PERMISSION_TEMPLATE ||--o{ PROFILE_PERMISSION : "autoridad_materializada"
    
    PROFILE ||--o{ PROFILE_PERMISSION : "contexto_efectivo"
    ROLE ||--o{ PROFILE : "fuente_de_autoridad"
    
    PERMISSION_TEMPLATE {
        uniqueidentifier TemplateId PK
        uniqueidentifier ModuleId FK
        nvarchar ResourceName "Menú/Opción/SubMenú"
        nvarchar ActionCode "Ver/Crear/Exportar/etc"
        nvarchar Name "Nombre Editable"
    }
    
    PROFILE_PERMISSION {
        uniqueidentifier ProfileId PK, FK
        uniqueidentifier TemplateId PK, FK
        bit IsAllowed "Permitir Explícito"
        bit IsDenied "Denegar Explícito (Anulación)"
        bit IsActive "Estado Operativo"
    }
```

---

### 📍 3.3 Dominio: Topología Funcional y Navegación
Estructura jerárquica de sistemas y menús.

```mermaid
erDiagram
    SYSTEM_SUITE ||--o{ FUNCTIONAL_MODULE : "contiene"
    FUNCTIONAL_MODULE ||--o{ MENU_ITEM : "topología"
    MENU_ITEM ||--o{ MENU_ITEM : "jerarquía"
    MENU_ITEM ||--o{ PERMISSION_TEMPLATE : "requiere_acceso"
    
    FUNCTIONAL_MODULE {
        uniqueidentifier ModuleId PK
        uniqueidentifier SuiteId FK
        nvarchar Name
        nvarchar Code
    }
    
    MENU_ITEM {
        uniqueidentifier MenuItemId PK
        uniqueidentifier ModuleId FK
        uniqueidentifier ParentItemId FK
        nvarchar Name
        nvarchar Route
    }
```

---

### 📝 3.4 Dominio: Auditoría e Identidad
Gestión de identidades y trazabilidad global.

```mermaid
erDiagram
    TENANT ||--o{ USER : "aloja"
    TENANT ||--o{ AUDIT_LOG : "monitorea"
    PROFILE ||--o{ AUDIT_LOG : "contexto"
    
    AUDIT_LOG {
        bigint LogId PK
        uniqueidentifier CorrelationId
        uniqueidentifier TransactionId
        nvarchar Action
        datetimeoffset Timestamp
    }
```

---

## 4. Reglas de Negocio y Normalización
1.  **Primacía de la Plantilla**: `PermissionTemplate` es la fuente maestra absoluta. No se permiten permisos ad-hoc.
2.  **Autoridad de Triple Estado**: `ProfilePermission` utiliza `IsAllowed`, `IsDenied` e `IsActive` para resolver la autoridad final.
3.  **Jerarquía**: `Sistema > Módulo > Menú > Acción`.
4.  **Matriz de Acciones**: Las plantillas admiten acciones granulares: `view`, `create`, `edit`, `delete`, `approve`, `export`, `import`, `print`, `copy`, `download`, `execute`, `manage`, `assign`, `audit`.
5.  **Eliminación Lógica (Soft Delete)**: Obligatoria para todas las entidades para mantener la integridad de la auditoría.
