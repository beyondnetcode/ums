# 🗄️ Entity-Relationship (E/R) Model - SQL Server 2022

**Document Type:** Database Design  
**Status:** Refactored (Role-Scoped & Strict Hierarchy)  
**Architecture:** Hierarchical Master Framework (5-Level Control)  
**Engine:** SQL Server 2022

## 1. Introduction
This document details the **Role-Scoped** authorization model, strictly enforcing the hierarchical chain: **System → Module → Menu → Option → Action**.

> [!NOTE]
> **Ubiquitous Language Mapping:** The schema entity names align with the [Glossary](../../governance/requirements/glossary.md) as follows:
> `SYSTEM_SUITE` = **System** · `FUNCTIONAL_MODULE` = **Module** · `FUNCTIONAL_SUBMODULE` = **Menu** · `FUNCTIONAL_OPTION` = **Option** · `ACTION` = **Action**

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
    
    ROLE ||--o{ ROLE : "parent_of"
    ROLE ||--o{ ROLE_PROMOTION_CRITERIA : "source"
    ROLE ||--o{ ROLE_PROMOTION_CRITERIA : "target"
    
    TENANT ||--o{ APP_CONFIGURATION : "settings"
    SYSTEM_SUITE ||--o{ APP_CONFIGURATION : "overrides"
    
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
    APPROVAL_WORKFLOW ||--o{ APPROVAL_REQUIRED_DOCUMENT : "mandates"
    APPROVAL_REQUEST ||--o{ USER_DOCUMENT : "evidenced_by"
    APPROVAL_REQUIRED_DOCUMENT ||--o{ DOCUMENT_TYPE : "typed_as"
    
    USER_ACCOUNT ||--o{ USER_DOCUMENT : "holds"
    DOCUMENT_TYPE ||--o{ USER_DOCUMENT : "classifies"
    DOCUMENT_TYPE ||--o{ NOTIFICATION_RULE : "alerts_for"
    DOCUMENT_TYPE ||--o{ ACCESS_ENFORCEMENT_POLICY : "governs_access"
    
    USER_ACCOUNT ||--o{ USER_PROMOTION_PROCESS : "candidate"
    ROLE ||--o{ USER_PROMOTION_PROCESS : "target"
    APPROVAL_REQUEST ||--o{ USER_PROMOTION_PROCESS : "authorized_by"
    
    USER_ACCOUNT {
        uniqueidentifier UserId PK
        uniqueidentifier TenantId FK
        nvarchar UserCategory "INTERNAL/EXTERNAL/B2B/PARTNER"
        nvarchar Status "ACTIVE/BLOCKED/PENDING"
    }

    ROLE {
        uniqueidentifier RoleId PK
        uniqueidentifier ParentRoleId FK "Self-Ref"
        int HierarchyLevel
        int PromotionOrder
    }

    ROLE_PROMOTION_CRITERIA {
        uniqueidentifier CriteriaId PK
        uniqueidentifier SourceRoleId FK
        uniqueidentifier TargetRoleId FK
        bit FlagSeniority
        bit FlagCompliance
        bit FlagManualApproval
    }

    USER_PROMOTION_PROCESS {
        uniqueidentifier ProcessId PK
        uniqueidentifier UserId FK
        uniqueidentifier TargetRoleId FK
        nvarchar Status "EVALUATING/CRITERIA_MET/PENDING_APPROVAL/PROMOTED"
    }

    APP_CONFIGURATION {
        uniqueidentifier SettingId PK
        uniqueidentifier TenantId FK "Nullable"
        uniqueidentifier SuiteId FK "Nullable"
        uniqueidentifier ModuleId FK "Nullable"
        nvarchar Code "Feature Flag / Parameter"
        nvarchar Value "Operational Value"
        nvarchar Description "Purpose + impact + expected behavior + scope"
        bit IsInheritable
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
        nvarchar Code
        nvarchar Value
        nvarchar Description
        nvarchar TargetUserCategory
        bit RequiresApproval
    }

    APPROVAL_REQUIRED_DOCUMENT {
        uniqueidentifier DocumentTypeId PK, FK
        uniqueidentifier WorkflowId FK
        bit IsMandatory
    }
    
    APPROVAL_REQUEST {
        uniqueidentifier RequestId PK
        uniqueidentifier WorkflowId FK
        uniqueidentifier TargetUserId FK
        uniqueidentifier TargetProfileId FK "Nullable"
        nvarchar RequestStatus "PENDING/APPROVED/REJECTED"
    }

    USER_DOCUMENT {
        uniqueidentifier DocumentId PK
        uniqueidentifier UserId FK
        uniqueidentifier DocumentTypeId FK
        datetime2 IssueDate
        datetime2 ExpirationDate
        nvarchar Status "VALID/EXPIRED/PENDING_RENEWAL"
        nvarchar Criticity "LOW/MEDIUM/HIGH/CRITICAL"
        nvarchar FileStoragePath "URI/Path to File Server"
    }

    NOTIFICATION_RULE {
        uniqueidentifier RuleId PK
        uniqueidentifier TenantId FK
        uniqueidentifier DocumentTypeId FK
        nvarchar Code
        nvarchar Value
        nvarchar Description
        int DaysBefore
        nvarchar Channel "EMAIL/IN_APP/SMS"
    }

    ACCESS_ENFORCEMENT_POLICY {
        uniqueidentifier PolicyId PK
        uniqueidentifier TenantId FK "Nullable (Global if NULL)"
        nvarchar Code
        nvarchar Value
        nvarchar Description
        uniqueidentifier DocumentTypeId FK
        nvarchar ActionOnExpiration "BLOCK_USER/RESTRICT_PROFILE/LOG_ONLY"
    }
```

---

## 4. Business Rules & Technical Constraints
1.  **Row-Level Security (RLS)**: `TenantId` is denormalized across all functional entities (Module, Option, Template, Action, Role) to allow O(1) isolation checks via SQL Server RLS.
2.  **Exclusive Arc (Template Integrity)**: `PermissionTemplate` implements 4 nullable FKs pointing to the resource hierarchy. A `CHECK` constraint guarantees exactly ONE is populated, enforcing strict database referential integrity over polymorphism.
3.  **Strict XOR Action Ownership**: An Action must belong to a System OR a Module, but never both: `CHECK ((SuiteId IS NOT NULL AND ModuleId IS NULL) OR (SuiteId IS NULL AND ModuleId IS NOT NULL))`.
4.  **Hierarchy Integrity**: Access must be traced through `System > Module > Menu > Option > Action` (schema: `SYSTEM_SUITE → FUNCTIONAL_MODULE → FUNCTIONAL_SUBMODULE → FUNCTIONAL_OPTION → ACTION`).
5.  **Delegated Administration (Many-to-Many)**: A user's scope of administration is defined via the `USER_MANAGEMENT_DELEGATION` table. This allows multiple administrators to manage the same user pool, optionally restricted by `SuiteId`.
6.  **Approval Mandates**: External/B2B users MUST pass through an `APPROVAL_WORKFLOW` before reaching an `ACTIVE` status or being assigned high-risk profiles. Supporting documents defined in `APPROVAL_REQUIRED_DOCUMENT` must be uploaded to `USER_DOCUMENT` before workflow advancement.
7.  **Automated Compliance Enforcement**: Background workers scan `USER_DOCUMENT`. Upon expiration, the `ACCESS_ENFORCEMENT_POLICY` is triggered. Critical documents will automatically transition the `USER_ACCOUNT` to a `BLOCKED` status or restrict specific `PROFILE` context.
8.  **Parametric Notifications**: `NOTIFICATION_RULE` allows configuring N-step alerts (e.g., 30, 15, 5 days before expiration) per Tenant and Document Type.
9.  **Mandatory Parametric Catalog Standard**: Every parameter/configuration/catalog entity MUST include `Code`, `Value`, and `Description`. `Description` must document purpose, functional impact, expected behavior, and applicable scope. All such entities must additionally define uniqueness by scope, versioning lineage, auditing metadata, traceability events, cache invalidation strategy, and forward extensibility.
