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
    TENANT ||--o{ BRANCH : "operates"
    TENANT ||--o{ USER_ACCOUNT : "owns"
    SYSTEM_SUITE ||--o{ ROLE : "defines"
    SYSTEM_SUITE ||--o{ FUNCTIONAL_MODULE : "contains"
    
    ROLE ||--o{ PERMISSION_TEMPLATE : "governs"
    PERMISSION_TEMPLATE ||--o{ PROFILE_PERMISSION : "materialized"
    
    USER_ACCOUNT ||--o{ PROFILE : "acts_as"
    USER_ACCOUNT ||--o{ USER_MANAGEMENT_DELEGATION : "administers"
    USER_ACCOUNT ||--o{ USER_MANAGEMENT_DELEGATION : "is_managed"
    USER_ACCOUNT ||--o{ APPROVAL_REQUEST : "onboards/approves"
    
    BRANCH ||--o{ PROFILE : "context_of"
    PROFILE ||--o{ PROFILE_PERMISSION : "effective_authority"
    
    FUNCTIONAL_MODULE ||--o{ FUNCTIONAL_SUBMODULE : "contains"
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
        uniqueidentifier TenantId FK "RLS"
        uniqueidentifier SuiteId FK "Exclusive Arc"
        uniqueidentifier ModuleId FK "Exclusive Arc"
        uniqueidentifier SubModuleId FK "Exclusive Arc"
        uniqueidentifier OptionId FK "Exclusive Arc"
        bit IsAllowed "Default State"
        bit IsDenied "Default State"
        bit IsActive "Default State"
    }
    
    PROFILE {
        uniqueidentifier ProfileId PK, FK
        uniqueidentifier TenantId FK "RLS"
        uniqueidentifier UserId FK
        uniqueidentifier RoleId FK
        uniqueidentifier BranchId FK "Location Context"
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
        uniqueidentifier TenantId FK "RLS"
        nvarchar Name
    }
    
    FUNCTIONAL_SUBMODULE {
        uniqueidentifier SubModuleId PK
        uniqueidentifier ModuleId FK
        uniqueidentifier TenantId FK "RLS"
        nvarchar Name
    }
    
    FUNCTIONAL_OPTION {
        uniqueidentifier OptionId PK
        uniqueidentifier SubModuleId FK
        uniqueidentifier TenantId FK "RLS"
        nvarchar Name
        nvarchar Code
    }
```

---

### 🛂 3.4 Domain: Identity Governance & Approvals
Management of user lifecycle, delegated administration, and onboarding workflows for high-risk or external identities.

```mermaid
erDiagram
    USER_ACCOUNT ||--o{ USER_MANAGEMENT_DELEGATION : "admin"
    USER_ACCOUNT ||--o{ USER_MANAGEMENT_DELEGATION : "managed"
    APPROVAL_WORKFLOW ||--o{ APPROVAL_REQUEST : "defines_rules_for"
    
    USER_ACCOUNT {
        uniqueidentifier UserId PK
        uniqueidentifier TenantId FK
        nvarchar UserCategory "INTERNAL/EXTERNAL/B2B/PARTNER"
        nvarchar Status "PENDING/ACTIVE/SUSPENDED"
    }

    USER_MANAGEMENT_DELEGATION {
        uniqueidentifier DelegationId PK
        uniqueidentifier ParentAdminUserId FK
        uniqueidentifier ManagedUserId FK
        uniqueidentifier SuiteId FK "Optional Scope"
    }
    
    APPROVAL_WORKFLOW {
        uniqueidentifier WorkflowId PK
        uniqueidentifier TenantId FK
        uniqueidentifier SuiteId FK "Nullable"
        nvarchar TargetUserCategory
        bit RequiresApproval
    }
    
    APPROVAL_REQUEST {
        uniqueidentifier RequestId PK
        uniqueidentifier WorkflowId FK
        uniqueidentifier TargetUserId FK
        uniqueidentifier TargetProfileId FK "Nullable"
        nvarchar RequestStatus "PENDING/APPROVED/REJECTED"
    }
```

---

## 4. Business Rules & Technical Constraints
1.  **Row-Level Security (RLS)**: `TenantId` is denormalized across all functional entities (Module, Option, Template, Action, Role) to allow O(1) isolation checks via SQL Server RLS.
2.  **Exclusive Arc (Template Integrity)**: `PermissionTemplate` implements 4 nullable FKs pointing to the resource hierarchy. A `CHECK` constraint guarantees exactly ONE is populated, enforcing strict database referential integrity over polymorphism.
3.  **Strict XOR Action Ownership**: An Action must belong to a System OR a Module, but never both: `CHECK ((SuiteId IS NOT NULL AND ModuleId IS NULL) OR (SuiteId IS NULL AND ModuleId IS NOT NULL))`.
4.  **Hierarchy Integrity**: Access must be traced through `System > Module > Sub-module > Option > Action`.
5.  **Delegated Administration (Many-to-Many)**: A user's scope of administration is defined via the `USER_MANAGEMENT_DELEGATION` table. This allows multiple administrators to manage the same user pool, optionally restricted by `SuiteId`.
6.  **Approval Mandates**: External/B2B users MUST pass through an `APPROVAL_WORKFLOW` before reaching an `ACTIVE` status or being assigned high-risk profiles.
