# 🗄️ Entity-Relationship (E/R) Model - SQL Server 2022

**Document Type:** Database Design  
**Status:** Refactored (Role-Scoped & Strict Hierarchy)  
**Architecture:** Hierarchical Master Framework (5-Level Control)  
**Engine:** SQL Server 2022

## 1. Introduction
This document details the **Role-Scoped** authorization model, strictly enforcing the hierarchical chain: **System -> Module -> Sub-module -> Option -> Action**.

> [!TIP]
> **Visualization Issues?**  
> If Mermaid diagrams do not render correctly in your IDE, please use the **[🚀 Alternative Export Formats (dbdiagram.io, DDL, D2)](./er-export-formats.md)**. These formats are compatible with professional tools like DBeaver, SSMS, and dbdiagram.io.

---

## 2. Standard Corporate Audit & Traceability
All entities implement the standard 10-column audit schema.

---

## 3. Modular Domain Views

### 🗺️ 3.1 Global High-Level Map
Full Resolution Path: `Tenant -> System -> Role -> Template -> ProfilePermission`.

```mermaid
erDiagram
    TENANT ||--o{ SYSTEM_SUITE : "owns"
    SYSTEM_SUITE ||--o{ ROLE : "defines"
    SYSTEM_SUITE ||--o{ FUNCTIONAL_MODULE : "contains"
    
    ROLE ||--o{ PERMISSION_TEMPLATE : "governs"
    PERMISSION_TEMPLATE ||--o{ PROFILE_PERMISSION : "materialized"
    
    FUNCTIONAL_MODULE ||--o{ FUNCTIONAL_SUBMODULE : "contains"
    FUNCTIONAL_SUBMODULE ||--o{ FUNCTIONAL_OPTION : "provides"
    FUNCTIONAL_OPTION ||--o{ ACTION : "executes"
    
    ACTION ||--o{ PERMISSION_TEMPLATE : "authorized_action"
```

---

### 🔐 3.2 Domain: Role-Centric Authority & Strict Hierarchy
This domain ensures every permission is scoped to a Role and maps exactly to the 5-level functional hierarchy.

```mermaid
erDiagram
    ROLE ||--o{ PERMISSION_TEMPLATE : "owns"
    PERMISSION_TEMPLATE ||--o{ PROFILE_PERMISSION : "defines"
    ACTION ||--o{ PERMISSION_TEMPLATE : "authorized"
    
    PERMISSION_TEMPLATE {
        uniqueidentifier TemplateId PK
        uniqueidentifier RoleId FK
        uniqueidentifier ActionId FK
        nvarchar ResourceLevel "SYSTEM/MODULE/SUBMODULE/OPTION"
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

### 📍 3.3 Domain: Functional Topology (The 5 Levels)
Organizational structure of resources.

```mermaid
erDiagram
    SYSTEM_SUITE ||--o{ FUNCTIONAL_MODULE : "L1: System -> Module"
    FUNCTIONAL_MODULE ||--o{ FUNCTIONAL_SUBMODULE : "L2: Module -> SubModule"
    FUNCTIONAL_SUBMODULE ||--o{ FUNCTIONAL_OPTION : "L3: SubModule -> Option"
    
    FUNCTIONAL_MODULE {
        uniqueidentifier ModuleId PK
        uniqueidentifier SuiteId FK
        nvarchar Name
    }
    
    FUNCTIONAL_SUBMODULE {
        uniqueidentifier SubModuleId PK
        uniqueidentifier ModuleId FK
        nvarchar Name
    }
    
    FUNCTIONAL_OPTION {
        uniqueidentifier OptionId PK
        uniqueidentifier SubModuleId FK
        nvarchar Name
        nvarchar Code
    }
```

---

## 4. Business Rules & Constraints
1.  **Hierarchy Integrity**: Access must be traced through `System > Module > Sub-module > Option > Action`.
2.  **Role Ownership**: A `PermissionTemplate` MUST belong to a `Role`.
3.  **No Orphan Actions**: Actions must be owned by a System or Module (Global vs Local context).
4.  **Materialization**: Effective permissions always reference a valid Role-scoped template.
