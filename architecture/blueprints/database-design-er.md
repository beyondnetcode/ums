# 🗄️ Entity-Relationship (E/R) Model - SQL Server 2022

**Document Type:** Database Design  
**Status:** Refactored (Master-Template Driven)  
**Architecture:** Hierarchical Framework (Materialized Authority)  
**Engine:** SQL Server 2022

## 1. Introduction
This document details the **Master-Template Driven** authorization model. Every effective permission in the system must be a materialized instance of a controlled `PermissionTemplate`, ensuring 100% governance over the authority catalog.

---

## 2. Standard Corporate Audit & Traceability
Every entity in this schema MUST implement the following columns.

| Column | Type | Description |
| :--- | :--- | :--- |
| `CreatedAt` | `datetimeoffset` | Creation timestamp. |
| `CreatedBy` | `uniqueidentifier` | Creator ID. |
| `UpdatedAt` | `datetimeoffset` | Update timestamp. |
| `UpdatedBy` | `uniqueidentifier` | Last updater ID. |
| `DeletedAt` | `datetimeoffset` | Soft delete timestamp. |
| `DeletedBy` | `uniqueidentifier` | Deletor ID. |
| `Version` | `int` | Optimistic locking (Default: 1). |
| `IsActive` | `bit` | Status flag. |
| `TenantId` | `uniqueidentifier` | Contextual isolation. |
| `CorrelationId`| `uniqueidentifier` | Distributed traceability. |

---

## 3. Modular Domain Views

### 🗺️ 3.1 Global High-Level Map
Comprehensive view of core module relationships.

```mermaid
erDiagram
    TENANT ||--o{ SYSTEM_SUITE : "owns"
    TENANT ||--o{ USER : "owns"
    TENANT ||--o{ BRANCH : "owns"
    
    SYSTEM_SUITE ||--o{ FUNCTIONAL_MODULE : "contains"
    FUNCTIONAL_MODULE ||--o{ PERMISSION_TEMPLATE : "defines_catalog"
    
    PERMISSION_TEMPLATE ||--o{ PROFILE_PERMISSION : "materialized_into"
    
    USER ||--o{ PROFILE : "acts_as"
    PROFILE ||--o{ PROFILE_PERMISSION : "holds_authority"
    
    ROLE ||--o{ PROFILE : "authority_blueprint"
    ROLE ||--o{ ROLE_PERMISSION : "base_blueprint"
    PERMISSION_TEMPLATE ||--o{ ROLE_PERMISSION : "defines_role_base"
```

---

### 🔐 3.2 Domain: Master Authorization Framework (The Core)
This domain manages the immutable permission catalog and its materialization into profiles.

```mermaid
erDiagram
    SYSTEM_SUITE ||--o{ FUNCTIONAL_MODULE : "contains"
    FUNCTIONAL_MODULE ||--o{ PERMISSION_TEMPLATE : "master_definitions"
    
    PERMISSION_TEMPLATE ||--o{ ROLE_PERMISSION : "standard_blueprint"
    PERMISSION_TEMPLATE ||--o{ PROFILE_PERMISSION : "materialized_authority"
    
    PROFILE ||--o{ PROFILE_PERMISSION : "effective_context"
    ROLE ||--o{ PROFILE : "authority_source"
    
    PERMISSION_TEMPLATE {
        uniqueidentifier TemplateId PK
        uniqueidentifier ModuleId FK
        nvarchar ResourceName "Menu/Option/SubMenu"
        nvarchar ActionCode "View/Create/Export/etc"
        nvarchar Name "Editable Name"
    }
    
    PROFILE_PERMISSION {
        uniqueidentifier ProfileId PK, FK
        uniqueidentifier TemplateId PK, FK
        bit IsAllowed "Explicit Allow"
        bit IsDenied "Explicit Deny (Override)"
        bit IsActive "Operational Status"
    }
```

---

### 📍 3.3 Domain: Functional Topology & Navigation
Hierarchical structure of systems and menus.

```mermaid
erDiagram
    SYSTEM_SUITE ||--o{ FUNCTIONAL_MODULE : "contains"
    FUNCTIONAL_MODULE ||--o{ MENU_ITEM : "topology"
    MENU_ITEM ||--o{ MENU_ITEM : "hierarchy"
    MENU_ITEM ||--o{ PERMISSION_TEMPLATE : "requires_access"
    
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

### 📝 3.4 Domain: Audit & Identity
Management of identities and global traceability.

```mermaid
erDiagram
    TENANT ||--o{ USER : "hosts"
    TENANT ||--o{ AUDIT_LOG : "monitors"
    PROFILE ||--o{ AUDIT_LOG : "context"
    
    AUDIT_LOG {
        bigint LogId PK
        uniqueidentifier CorrelationId
        uniqueidentifier TransactionId
        nvarchar Action
        datetimeoffset Timestamp
    }
```

---

## 4. Business Rules & Normalization
1.  **Template Primacy**: `PermissionTemplate` is the absolute master source. No ad-hoc permissions are allowed.
2.  **Triple-State Authority**: `ProfilePermission` uses `IsAllowed`, `IsDenied`, and `IsActive` to resolve final authority.
3.  **Hierarchy**: `System > Module > Menu > Action`.
4.  **Action Matrix**: Templates support granular actions: `view`, `create`, `edit`, `delete`, `approve`, `export`, `import`, `print`, `copy`, `download`, `execute`, `manage`, `assign`, `audit`.
5.  **Soft Delete**: Mandatory for all entities to maintain audit integrity.
