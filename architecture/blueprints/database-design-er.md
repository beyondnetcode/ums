# 🗄️ Entity-Relationship (E/R) Model - SQL Server 2022

**Document Type:** Database Design  
**Status:** Refactored (Profile-Centric)  
**Architecture:** Contextual Multi-tenancy (Profile Hub)  
**Engine:** SQL Server 2022

## 1. Introduction
This document details the **Profile-Centric** data model for the **User Management System (UMS)**. The model is centered around the `Profile` entity, which serves as the contextual consolidation of identity and authority across Systems, Roles, and Branches.

---

## 2. Standard Corporate Audit & Traceability
Every table in this schema MUST implement the following audit columns to comply with corporate governance standards.

| Column | Type | Description |
| :--- | :--- | :--- |
| `CreatedAt` | `datetimeoffset` | Creation timestamp. |
| `CreatedBy` | `uniqueidentifier` | ID of the user/system that created the record. |
| `UpdatedAt` | `datetimeoffset` | Last update timestamp (Nullable). |
| `UpdatedBy` | `uniqueidentifier` | ID of the user/system that last updated the record. |
| `DeletedAt` | `datetimeoffset` | Soft delete timestamp (Nullable). |
| `DeletedBy` | `uniqueidentifier` | ID of the user/system that deleted the record. |
| `Version` | `int` | Row version for optimistic concurrency (Default: 1). |
| `Status` | `int` | Record status (1: Active, 0: Inactive, 99: Deleted). |

---

## 3. E/R Diagram (Mermaid)

```mermaid
erDiagram
    TENANT ||--o{ USER : "hosts"
    TENANT ||--o{ SYSTEM_SUITE : "subscribes_to"
    TENANT ||--o{ BRANCH : "contains"
    TENANT ||--o{ ROLE : "manages"
    TENANT ||--o{ PROFILE : "governs"
    TENANT ||--o{ AUDIT_LOG : "logs"

    USER ||--o{ PROFILE : "acts_as"
    USER ||--|| USER_CREDENTIAL : "has"

    SYSTEM_SUITE ||--o{ PERMISSION : "defines"
    SYSTEM_SUITE ||--o{ PERMISSION_TEMPLATE : "templates"
    SYSTEM_SUITE ||--o{ PROFILE : "context_for"
    SYSTEM_SUITE ||--o{ MENU_ITEM : "contains_topology"

    MENU_ITEM ||--o{ MENU_ITEM : "parent_of"
    MENU_ITEM ||--o{ PERMISSION : "requires"

    ROLE ||--o{ PROFILE : "blueprint_for"
    ROLE ||--o{ ROLE_PERMISSION : "base_perms"

    BRANCH ||--o{ PROFILE : "location_for"

    PROFILE ||--o{ PROFILE_PERMISSION : "consolidates"

    PERMISSION ||--o{ ROLE_PERMISSION : "links"
    PERMISSION ||--o{ PROFILE_PERMISSION : "effective_link"
    PERMISSION ||--o{ TEMPLATE_PERMISSION : "links"

    PERMISSION_TEMPLATE ||--o{ TEMPLATE_PERMISSION : "blueprints"

    PROFILE {
        uniqueidentifier ProfileId PK
        uniqueidentifier TenantId FK
        uniqueidentifier UserId FK
        uniqueidentifier SystemId FK
        uniqueidentifier RoleId FK
        uniqueidentifier BranchId FK
        nvarchar DisplayName
        nvarchar AuditId "Traceability Link"
    }

    PROFILE_PERMISSION {
        uniqueidentifier ProfileId PK, FK
        uniqueidentifier PermissionId PK, FK
        bit IsDenied "Override: Force Deny"
        nvarchar GrantReason
    }

    USER {
        uniqueidentifier UserId PK
        uniqueidentifier TenantId FK
        nvarchar Username "UK"
        nvarchar Email "UK"
    }

    ROLE {
        uniqueidentifier RoleId PK
        uniqueidentifier TenantId FK
        uniqueidentifier SourceTemplateId FK
        nvarchar Name
    }

    SYSTEM_SUITE {
        uniqueidentifier SuiteId PK
        nvarchar Name
        nvarchar Code "UK"
    }

    BRANCH {
        uniqueidentifier BranchId PK
        uniqueidentifier TenantId FK
        nvarchar Name
        nvarchar Code
    }

    MENU_ITEM {
        uniqueidentifier MenuItemId PK
        uniqueidentifier SuiteId FK
        uniqueidentifier ParentItemId FK "NULL for Root"
        uniqueidentifier PermissionId FK "Access Grant"
        nvarchar Name
        nvarchar Route
        nvarchar Icon
        int SortOrder
    }

    AUDIT_LOG {
        bigint LogId PK
        uniqueidentifier TenantId
        uniqueidentifier UserId
        uniqueidentifier ProfileId
        uniqueidentifier CorrelationId
        uniqueidentifier TransactionId
        nvarchar Action
        nvarchar EntityName
        nvarchar OldValue
        nvarchar NewValue
        datetimeoffset Timestamp
    }
```

---

## 4. Multi-tenancy & Isolation
The isolation is enforced via **SQL Server Row-Level Security (RLS)**.

*   **Global Entities** (`SystemSuites`, `Permissions`, `GlobalTemplates`): Accessible by all tenants.
*   **Tenant Entities** (`Users`, `Roles`, `Profiles`, `Branches`): Isolated by `TenantId` using `SESSION_CONTEXT(N'TenantId')`.
*   **Effective Resolution**: The `Authorization Engine` resolves permissions primarily from `ProfilePermissions` for the `ActiveProfileId`.

---

## 5. Persistence of Effective Permissions
*   When a **Profile** is created, permissions from the selected **Role** (and its Template) are projected into `ProfilePermissions`.
*   Any **Override** performed by an administrator is stored directly in `ProfilePermissions` for that specific `ProfileId`.
*   This ensures that the `Authorization Engine` performs a single, highly-indexed join to retrieve the full permission set for the user's current context.
