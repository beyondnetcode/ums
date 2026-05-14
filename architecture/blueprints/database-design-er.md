# 🗄️ Entity-Relationship (E/R) Model - SQL Server 2022

**Document Type:** Database Design  
**Status:** Refactored (Enterprise Profile-Centric)  
**Architecture:** Hierarchical Multi-tenancy (Profile Nexus)  
**Engine:** SQL Server 2022

## 1. Introduction
This document details the **Enterprise Profile-Centric** data model for the **User Management System (UMS)**. The model enforces strict hierarchical ownership and positions the `Profile` as the contextual intersection of identity and authorization.

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
| `TenantId` | `uniqueidentifier` | Contextual isolation (where applicable). |
| `CorrelationId`| `uniqueidentifier` | Traceability across distributed operations. |

---

## 3. E/R Diagram (Mermaid)

```mermaid
erDiagram
    TENANT ||--o{ SYSTEM_SUITE : "owns"
    TENANT ||--o{ BRANCH : "owns"
    TENANT ||--o{ USER : "owns"
    TENANT ||--o{ PROFILE : "governs"
    TENANT ||--o{ AUDIT_LOG : "monitors"

    SYSTEM_SUITE ||--o{ ROLE : "defines"
    SYSTEM_SUITE ||--o{ PERMISSION : "categorizes"
    SYSTEM_SUITE ||--o{ MENU_ITEM : "topology"
    SYSTEM_SUITE ||--o{ PERMISSION_TEMPLATE : "provides_blueprints"
    SYSTEM_SUITE ||--o{ PROFILE : "functional_context"

    BRANCH ||--o{ PROFILE : "operational_location"

    USER ||--o{ PROFILE : "active_identity"
    USER ||--|| USER_CREDENTIAL : "credentials"

    ROLE ||--o{ ROLE_PERMISSION : "base_authorizations"
    ROLE ||--o{ PROFILE : "authority_blueprint"

    PERMISSION ||--o{ ROLE_PERMISSION : "links"
    PERMISSION ||--o{ PROFILE_PERMISSION : "effective_links"
    PERMISSION ||--o{ TEMPLATE_PERMISSION : "links"
    PERMISSION ||--o{ MENU_ITEM : "access_guard"

    PERMISSION_TEMPLATE ||--o{ TEMPLATE_PERMISSION : "blueprints"
    PERMISSION_TEMPLATE ||--|| ROLE : "represents_system_role"

    PROFILE ||--o{ PROFILE_PERMISSION : "consolidates"

    MENU_ITEM ||--o{ MENU_ITEM : "hierarchy"

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
        bit IsDenied "Override Grant"
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

## 4. Business Rules & Normalization
1.  **Strict Isolation**: A Role cannot exist outside a System context.
2.  **Contextual Integrity**: A Profile can only be created if the selected Role belongs to the selected System, and both belong to the same Tenant.
3.  **Template Hub**: Permission Templates link to a specific System Role, allowing for branch-agnostic profile initialization.
4.  **Soft Delete**: Data is never physically removed; `DeletedAt` is populated to maintain history.
5.  **Effective Persistance**: `PROFILE_PERMISSION` acts as the source of truth for the `Authorization Engine`, combining Role-based defaults with Profile-specific overrides.
