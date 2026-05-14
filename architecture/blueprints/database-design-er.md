# 🗄️ Entity-Relationship (E/R) Model - SQL Server 2022

**Document Type:** Database Design  
**Status:** Refactored (Scoped Action Governance)  
**Architecture:** Hierarchical Master Framework  
**Engine:** SQL Server 2022

## 1. Introduction
This document details the **Scoped Action** authorization model. Every authority in the system must belong to a functional container (System or Module), eliminating orphaned permissions and ensuring strict architectural governance.

> [!TIP]
> **Visualization Issues?**  
> If Mermaid diagrams do not render correctly in your IDE, please use the **[🚀 Alternative Export Formats (dbdiagram.io, DDL, D2)](./er-export-formats.md)**. These formats are compatible with professional tools like DBeaver, SSMS, and dbdiagram.io.

---

## 2. Standard Corporate Audit & Traceability
Every entity in this schema MUST implement the standard 10 columns (`CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`, `DeletedAt`, `DeletedBy`, `Version`, `IsActive`, `TenantId`, `CorrelationId`).

---

## 3. Modular Domain Views

### 🗺️ 3.1 Global High-Level Map
Full resolution path: `Tenant -> System -> Module -> Resource -> Action -> Template -> ProfilePermission`.

```mermaid
erDiagram
    TENANT ||--o{ SYSTEM_SUITE : "owns"
    SYSTEM_SUITE ||--o{ FUNCTIONAL_MODULE : "contains"
    SYSTEM_SUITE ||--o{ ACTION : "global_actions"
    FUNCTIONAL_MODULE ||--o{ ACTION : "module_actions"
    
    ACTION ||--o{ PERMISSION_TEMPLATE : "scoped_to"
    PERMISSION_TEMPLATE ||--o{ PROFILE_PERMISSION : "materialized"
    
    USER ||--o{ PROFILE : "acts_as"
    PROFILE ||--o{ PROFILE_PERMISSION : "authority"
    
    ROLE ||--o{ PROFILE : "blueprint"
    PERMISSION_TEMPLATE ||--o{ ROLE_PERMISSION : "role_base"
```

---

### 🔐 3.2 Domain: Scoped Authorization Framework
Management of scoped actions and their materialization.

```mermaid
erDiagram
    SYSTEM_SUITE ||--o{ ACTION : "global"
    FUNCTIONAL_MODULE ||--o{ ACTION : "specific"
    
    ACTION ||--o{ PERMISSION_TEMPLATE : "defines"
    PERMISSION_TEMPLATE ||--o{ PROFILE_PERMISSION : "effective"
    
    ACTION {
        uniqueidentifier ActionId PK
        uniqueidentifier SuiteId FK "Nullable"
        uniqueidentifier ModuleId FK "Nullable"
        nvarchar Code "VIEW/CREATE/EDIT"
    }
    
    PERMISSION_TEMPLATE {
        uniqueidentifier TemplateId PK
        uniqueidentifier ActionId FK
        nvarchar ResourceType "SYSTEM/MODULE/MENU/OPTION"
        nvarchar ResourceId "Target ID"
    }
    
    PROFILE_PERMISSION {
        uniqueidentifier ProfileId PK, FK
        uniqueidentifier TemplateId PK, FK
        bit IsAllowed
        bit IsDenied
        bit IsActive
    }
```

---

### 📍 3.3 Domain: Functional Topology
Hierarchy of organizational structure and navigation.

```mermaid
erDiagram
    SYSTEM_SUITE ||--o{ FUNCTIONAL_MODULE : "contains"
    FUNCTIONAL_MODULE ||--o{ MENU_ITEM : "topology"
    MENU_ITEM ||--o{ MENU_ITEM : "hierarchy"
    
    MENU_ITEM {
        uniqueidentifier MenuItemId PK
        uniqueidentifier ModuleId FK
        uniqueidentifier ParentItemId FK
        nvarchar Name
    }
```

---

## 4. Business Rules & Constraints
1.  **Action Ownership**: Every Action MUST have either a `SuiteId` or a `ModuleId`.
2.  **Constraint**: `CHECK (SuiteId IS NOT NULL OR ModuleId IS NOT NULL)`.
3.  **No Orphans**: All permissions must trace back to a template.
4.  **Immutability**: Permission templates are the source of truth; ProfilePermissions are overrides/materializations.
