# Entity-Relationship (E/R) Model - SQL Server 2022

**Document Type:** Database Design  
**Status:** Refactored (Role-Scoped & Strict Hierarchy)  
**Architecture:** Hierarchical Master Framework (5-Level Control)  
**Engine:** SQL Server 2022

## 1. Introduction
This document details the **Role-Scoped** authorization model, strictly enforcing the hierarchical chain: **System → Module → Menu → SubMenu → Option**.

> [!NOTE]
> **Ubiquitous Language Mapping:** The schema entity names align with the [Glossary](../../governance/requirements/glossary.md) as follows:
> `SYSTEM_SUITE` = **System** · `FUNCTIONAL_MODULE` = **Module** · `FUNCTIONAL_MENU` = **Menu** · `FUNCTIONAL_SUBMENU` = **SubMenu** · `FUNCTIONAL_OPTION` = **Option**

> [!TIP]
> **Visualization Issues?**  
> If Mermaid diagrams do not render correctly in your IDE, please use the **[ Alternative Export Formats (dbdiagram.io, DDL, D2)](./er-export-formats.md)**. These formats are compatible with professional tools like DBeaver, SSMS, and dbdiagram.io.

---

## 2. Standard Corporate Audit & Traceability
All entities implement the standard 10-column audit schema.

---

## 3. Modular Domain Views

### 3.1 Global High-Level Map
Full Resolution Path: `Tenant -> System -> Role -> Template -> ProfilePermission`.

```mermaid
erDiagram
    TENANT ||--o{ SYSTEM_SUITE : "owns"
    TENANT ||--o{ BRANCH : "operates"
    TENANT ||--o{ USER_ACCOUNT : "owns"
    TENANT ||--o{ IDENTITY_PROVIDER : "registers"
    TENANT ||--o| BRANDING : "configures"
    TENANT ||--o{ IDP_CONFIGURATION : "resolves_oidc"
    TENANT ||--o{ FEATURE_FLAG : "controls"
    TENANT ||--o{ AUDIT_RECORD : "traces"
    SYSTEM_SUITE ||--o{ ROLE : "defines"
    SYSTEM_SUITE ||--o{ FUNCTIONAL_MODULE : "contains"
    
    ROLE ||--o{ ROLE : "parent_of"
    ROLE ||--o{ ROLE_MATURITY_STATUS : "defines eligibility for"
    
    TENANT ||--o{ APP_CONFIGURATION : "settings"
    SYSTEM_SUITE ||--o{ APP_CONFIGURATION : "overrides"
    
    ROLE ||--o{ PERMISSION_TEMPLATE : "governs"
    PERMISSION_TEMPLATE ||--o{ PROFILE_PERMISSION : "materialized"
    
    USER_ACCOUNT ||--o{ PROFILE : "acts_as"
    ROLE ||--o{ PROFILE : "assigned_to"
    BRANCH ||--o{ PROFILE : "scopes"
    PROFILE ||--o{ PROFILE_PERMISSION : "customizes"
    USER_ACCOUNT ||--o{ USER_MANAGEMENT_DELEGATION : "administers"
    USER_ACCOUNT ||--o{ USER_MANAGEMENT_DELEGATION : "is_managed"
    USER_ACCOUNT ||--o{ APPROVAL_REQUEST : "onboards_or_approves"
    
    SYSTEM_SUITE ||--o{ ACTION : "defines global"
    FUNCTIONAL_MODULE ||--o{ ACTION : "defines local"
    
    FUNCTIONAL_MODULE ||--o{ FUNCTIONAL_MENU : "contains"
    FUNCTIONAL_MENU ||--o{ FUNCTIONAL_SUBMENU : "contains"
    FUNCTIONAL_SUBMENU ||--o{ FUNCTIONAL_OPTION : "contains"
    
    ACTION ||--o{ PERMISSION_TEMPLATE : "authorized_action"
```

---

### 3.2 Domain: Role-Centric Authority & Strict Hierarchy
This domain ensures every permission is scoped to a Role and maps exactly to the 5-level functional hierarchy.

```mermaid
erDiagram
    ROLE ||--o{ PERMISSION_TEMPLATE : "owns"
    ROLE ||--o{ PROFILE : "assigned_to"
    PROFILE ||--o{ PROFILE_PERMISSION : "customizes"
    PERMISSION_TEMPLATE ||--o{ PROFILE_PERMISSION : "defines"
    ACTION ||--o{ PERMISSION_TEMPLATE : "authorized"

    ROLE {
        uniqueidentifier RoleId PK
        uniqueidentifier SuiteId FK
        uniqueidentifier TenantId FK "RLS"
        uniqueidentifier ParentRoleId FK "Self-Ref"
        nvarchar Name
        nvarchar Code
        int HierarchyLevel
        int PromotionOrder
        bit IsActive
    }

    ACTION {
        uniqueidentifier ActionId PK
        uniqueidentifier SuiteId FK "Nullable"
        uniqueidentifier ModuleId FK "Nullable"
        uniqueidentifier TenantId FK "RLS"
        nvarchar Name
        nvarchar Code
        bit IsActive
    }

    PERMISSION_TEMPLATE {
        uniqueidentifier TemplateId PK
        uniqueidentifier RoleId FK
        uniqueidentifier ActionId FK
        uniqueidentifier TenantId FK "RLS"
        uniqueidentifier SuiteId FK "Exclusive Arc"
        uniqueidentifier ModuleId FK "Exclusive Arc"
        uniqueidentifier MenuId FK "Exclusive Arc"
        uniqueidentifier SubMenuId FK "Exclusive Arc"
        uniqueidentifier OptionId FK "Exclusive Arc"
        bit IsAllowed "Default State"
        bit IsDenied "Default State"
        bit IsActive "Default State"
    }

    PROFILE {
        uniqueidentifier ProfileId PK
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

### 3.3 Domain: Functional Topology (The 5 Levels)
Organizational structure of resources.

```mermaid
erDiagram
    SYSTEM_SUITE ||--o{ FUNCTIONAL_MODULE : "L1: System-Module"
    FUNCTIONAL_MODULE ||--o{ FUNCTIONAL_MENU : "L2: Module-Menu"
    FUNCTIONAL_MENU ||--o{ FUNCTIONAL_SUBMENU : "L3: Menu-SubMenu"
    FUNCTIONAL_SUBMENU ||--o{ FUNCTIONAL_OPTION : "L4: SubMenu-Option"

    SYSTEM_SUITE {
        uniqueidentifier SuiteId PK
        uniqueidentifier TenantId FK "RLS"
        nvarchar Name
        nvarchar Code
        bit IsActive
    }

    FUNCTIONAL_MODULE {
        uniqueidentifier ModuleId PK
        uniqueidentifier SuiteId FK
        uniqueidentifier TenantId FK "RLS"
        nvarchar Name
    }
    
    FUNCTIONAL_MENU {
        uniqueidentifier MenuId PK
        uniqueidentifier ModuleId FK
        uniqueidentifier TenantId FK "RLS"
        nvarchar Name
    }

    FUNCTIONAL_SUBMENU {
        uniqueidentifier SubMenuId PK
        uniqueidentifier MenuId FK
        uniqueidentifier TenantId FK "RLS"
        nvarchar Name
    }
    
    FUNCTIONAL_OPTION {
        uniqueidentifier OptionId PK
        uniqueidentifier SubMenuId FK
        uniqueidentifier TenantId FK "RLS"
        nvarchar Name
        nvarchar Code
    }
    
    ACTION {
        uniqueidentifier ActionId PK
        uniqueidentifier SuiteId FK "Nullable"
        uniqueidentifier ModuleId FK "Nullable"
        uniqueidentifier TenantId FK "RLS"
        nvarchar Name
        nvarchar Code
    }
```

---

### 3.4 Domain: Identity Governance & Approvals
Management of user lifecycle, delegated administration, and onboarding workflows for high-risk or external identities.

```mermaid
erDiagram
    TENANT ||--o{ USER_ACCOUNT : "owns"
    TENANT ||--o{ IDENTITY_PROVIDER : "registers"
    TENANT ||--o| BRANDING : "configures"
    USER_ACCOUNT ||--o{ USER_MANAGEMENT_DELEGATION : "admin"
    USER_ACCOUNT ||--o{ USER_MANAGEMENT_DELEGATION : "managed"
    USER_ACCOUNT ||--o{ ROLE_MATURITY_STATUS : "has"
    ROLE ||--o{ ROLE_MATURITY_STATUS : "defines_eligibility"
    APPROVAL_WORKFLOW ||--o{ APPROVAL_REQUEST : "defines_rules_for"
    APPROVAL_WORKFLOW ||--o{ APPROVAL_REQUIRED_DOCUMENT : "mandates"
    APPROVAL_REQUEST ||--o{ USER_DOCUMENT : "evidenced_by"
    APPROVAL_REQUIRED_DOCUMENT ||--o{ DOCUMENT_TYPE : "typed_as"

    USER_ACCOUNT ||--o{ USER_DOCUMENT : "holds"
    DOCUMENT_TYPE ||--o{ USER_DOCUMENT : "classifies"
    DOCUMENT_TYPE ||--o{ NOTIFICATION_RULE : "alerts_for"
    DOCUMENT_TYPE ||--o{ ACCESS_ENFORCEMENT_POLICY : "governs_access"

    USER_ACCOUNT ||--o{ PROMOTION_REQUEST : "initiates"
    ROLE ||--o{ PROMOTION_REQUEST : "target"
    APPROVAL_REQUEST ||--o{ PROMOTION_REQUEST : "authorized_by"
    PROMOTION_REQUEST ||--o{ PROMOTION_IMPACT_ANALYSIS : "evaluates_risk"

    TENANT {
        uniqueidentifier TenantId PK
        nvarchar Name
        nvarchar Code
        nvarchar Status "ACTIVE-SUSPENDED-INACTIVE"
        bit IsActive
    }

    USER_ACCOUNT {
        uniqueidentifier UserId PK
        uniqueidentifier TenantId FK "RLS"
        nvarchar UserCategory "INTERNAL-EXTERNAL-B2B-PARTNER"
        nvarchar Status "ACTIVE-BLOCKED-PENDING"
    }

    IDENTITY_PROVIDER {
        uniqueidentifier IdpId PK
        uniqueidentifier TenantId FK
        nvarchar Code "SAML-OIDC-AZURE_AD"
        nvarchar Name
        nvarchar Description
        nvarchar Strategy "OIDC-SAML2-WS_FED"
        bit IsActive
    }

    BRANDING {
        uniqueidentifier BrandingId PK
        uniqueidentifier TenantId FK "One-to-One RLS"
        nvarchar Logo "URI Storage Path"
        nvarchar LogoFormat "PNG-SVG-JPEG"
        nvarchar PrimaryColor "Hex Code"
        nvarchar BackgroundStyle "Glassmorphism-SleekDark"
        nvarchar HeadlineText
        nvarchar SecondaryText
        nvarchar PrimaryButtonLabel
        nvarchar FooterText
        nvarchar CustomDomain "Nullable"
        nvarchar DnsVerificationStatus "PENDING-VERIFIED-FAILED"
        nvarchar DnsCnameTarget
        bit MagicLinkFallbackEnabled
    }

    ROLE {
        uniqueidentifier RoleId PK
        uniqueidentifier SuiteId FK
        uniqueidentifier TenantId FK "RLS"
        uniqueidentifier ParentRoleId FK "Self-Ref"
        nvarchar Name
        nvarchar Code
        int HierarchyLevel
        int PromotionOrder
        bit IsActive
    }

    ROLE_MATURITY_STATUS {
        uniqueidentifier MaturityStatusId PK
        uniqueidentifier TenantId FK "RLS"
        uniqueidentifier UserId FK
        uniqueidentifier RoleId FK
        nvarchar CurrentLevel "Junior-Intermediate-Senior-Lead-Principal"
        int CompletedCertificationsCount
        int CompletedTrainingsCount
        double PerformanceScore
        bit HasComplianceIssues
        datetime2 LastLevelChangeDate
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
        nvarchar RequestStatus "PENDING-APPROVED-REJECTED"
    }

    DOCUMENT_TYPE {
        uniqueidentifier DocumentTypeId PK
        uniqueidentifier TenantId FK
        nvarchar Name
        nvarchar Code
        nvarchar Description
        int ExpirationDays
        nvarchar Criticity "LOW-MEDIUM-HIGH-CRITICAL"
        bit IsActive
    }

    USER_DOCUMENT {
        uniqueidentifier DocumentId PK
        uniqueidentifier UserId FK
        uniqueidentifier DocumentTypeId FK
        datetime2 IssueDate
        datetime2 ExpirationDate
        nvarchar Status "VALID-EXPIRED-PENDING_RENEWAL"
        nvarchar Criticity "LOW-MEDIUM-HIGH-CRITICAL"
        nvarchar FileStoragePath "URI Path to File Server"
    }

    NOTIFICATION_RULE {
        uniqueidentifier RuleId PK
        uniqueidentifier TenantId FK
        uniqueidentifier DocumentTypeId FK
        nvarchar Code
        nvarchar Value
        nvarchar Description
        int DaysBefore
        nvarchar Channel "EMAIL-IN_APP-SMS"
    }

    ACCESS_ENFORCEMENT_POLICY {
        uniqueidentifier PolicyId PK
        uniqueidentifier TenantId FK "Nullable (Global if NULL)"
        nvarchar Code
        nvarchar Value
        nvarchar Description
        uniqueidentifier DocumentTypeId FK
        nvarchar ActionOnExpiration "BLOCK_USER-RESTRICT_PROFILE-LOG_ONLY"
    }

    PROMOTION_REQUEST {
        uniqueidentifier PromotionRequestId PK
        uniqueidentifier TenantId FK
        uniqueidentifier TargetRoleId FK
        uniqueidentifier InitiatedByUserId FK
        nvarchar Status "DRAFT-SUBMITTED-APPROVED-EXECUTED-VERIFIED"
    }

    PROMOTION_IMPACT_ANALYSIS {
        uniqueidentifier ImpactAnalysisId PK
        uniqueidentifier PromotionRequestId FK
        nvarchar RiskLevel "LOW-MEDIUM-HIGH"
        nvarchar AnalysisDetails "JSON"
        bit ViolatesSoD
    }
```

---

### 3.5 Domain: Platform Configuration & System Auditing
This domain covers system-wide configuration, OIDC Identity Provider integrations, multi-dimensional Feature Flag controls, and the immutable append-only ledger for all system actions.

```mermaid
erDiagram
    TENANT ||--o{ IDP_CONFIGURATION : "configures_auth"
    TENANT ||--o{ FEATURE_FLAG : "defines_toggles"
    TENANT ||--o{ AUDIT_RECORD : "records_actions"
    TENANT ||--o{ APP_CONFIGURATION : "parameterizes"
    SYSTEM_SUITE ||--o{ APP_CONFIGURATION : "overrides"
    FEATURE_FLAG ||--o{ FLAG_EVALUATION_LOG : "evaluates"
    USER_ACCOUNT ||--o{ FLAG_EVALUATION_LOG : "triggers"
    USER_ACCOUNT ||--o{ AUDIT_RECORD : "initiates"

    TENANT {
        uniqueidentifier TenantId PK
        nvarchar Name
        nvarchar Code
        nvarchar Status "ACTIVE-SUSPENDED-INACTIVE"
        bit IsActive
    }

    SYSTEM_SUITE {
        uniqueidentifier SuiteId PK
        uniqueidentifier TenantId FK "RLS"
        nvarchar Name
        nvarchar Code
        bit IsActive
    }

    USER_ACCOUNT {
        uniqueidentifier UserId PK
        uniqueidentifier TenantId FK "RLS"
        nvarchar UserCategory "INTERNAL-EXTERNAL-B2B-PARTNER"
        nvarchar Status "ACTIVE-BLOCKED-PENDING"
    }

    APP_CONFIGURATION {
        uniqueidentifier SettingId PK
        uniqueidentifier TenantId FK "Nullable"
        uniqueidentifier SuiteId FK "Nullable"
        uniqueidentifier ModuleId FK "Nullable"
        nvarchar Code "Feature Flag-Parameter"
        nvarchar Value "Operational Value"
        nvarchar Description "Purpose + impact + behavior + scope"
        bit IsInheritable
    }

    IDP_CONFIGURATION {
        uniqueidentifier IdpConfigId PK
        uniqueidentifier TenantId FK
        nvarchar ProviderType "INTERNAL_BCRYPT-ZITADEL-AZURE_AD-OKTA-KEYCLOAK"
        nvarchar DomainHints "OIDC Domain Routing"
        nvarchar ConfigPayload "Encrypted Authorizations"
        nvarchar SecretRef "Vault Path"
        nvarchar IdpConfigStatus "DRAFT-ACTIVE-INACTIVE"
        int ResolutionPriority
        nvarchar Version
    }

    FEATURE_FLAG {
        uniqueidentifier FlagId PK
        uniqueidentifier TenantId FK
        nvarchar FlagCode "Unique Code"
        nvarchar FlagType "BOOLEAN-VARIANT-PERCENTAGE"
        nvarchar FlagTargets "JSON Rules"
        nvarchar FlagStatus "ACTIVE-INACTIVE-ARCHIVED"
        nvarchar LinkedResourceType "Nullable: MENU-MODULE-ENDPOINT-WORKFLOW"
        nvarchar Description
        bit IsActive
    }

    FLAG_EVALUATION_LOG {
        uniqueidentifier LogId PK
        uniqueidentifier FlagId FK
        uniqueidentifier UserId FK
        uniqueidentifier TenantId FK
        datetime2 EvaluatedAt
        nvarchar Result
        nvarchar ContextPayload "JSON Context"
    }

    AUDIT_RECORD {
        uniqueidentifier AuditRecordId PK
        uniqueidentifier TenantId FK "RLS"
        nvarchar AuditEventType
        nvarchar SubjectType "USER-ADMIN-SYSTEM-BACKGROUND_WORKER"
        uniqueidentifier ActorId FK
        datetime2 EvaluatedAt
        nvarchar AuditResult "SUCCESS-FAILURE-PARTIAL"
        nvarchar AffectedEntityType
        uniqueidentifier AffectedEntityId
        nvarchar AuditMetadata "JSON Metadata"
    }
```

---

## 4. Business Rules & Technical Constraints
1.  **Row-Level Security (RLS)**: `TenantId` is denormalized across all functional entities (Module, Option, Template, Action, Role) to allow O(1) isolation checks via SQL Server RLS.
2.  **Exclusive Arc (Template Integrity)**: `PermissionTemplate` implements 5 nullable FKs pointing to the resource hierarchy. A `CHECK` constraint guarantees exactly ONE is populated, enforcing strict database referential integrity over polymorphism.
3.  **Strict XOR Action Ownership**: An Action must belong to a System OR a Module, but never both: `CHECK ((SuiteId IS NOT NULL AND ModuleId IS NULL) OR (SuiteId IS NULL AND ModuleId IS NOT NULL))`.
4.  **Hierarchy Integrity**: Access must be traced through `System > Module > Menu > SubMenu > Option` (schema: `SYSTEM_SUITE → FUNCTIONAL_MODULE → FUNCTIONAL_MENU → FUNCTIONAL_SUBMENU → FUNCTIONAL_OPTION`).
5.  **Delegated Administration (Many-to-Many)**: A user's scope of administration is defined via the `USER_MANAGEMENT_DELEGATION` table. This allows multiple administrators to manage the same user pool, optionally restricted by `SuiteId`.
6.  **Approval Mandates**: External/B2B users MUST pass through an `APPROVAL_WORKFLOW` before reaching an `ACTIVE` status or being assigned high-risk profiles. Supporting documents defined in `APPROVAL_REQUIRED_DOCUMENT` must be uploaded to `USER_DOCUMENT` before workflow advancement.
7.  **Automated Compliance Enforcement**: Background workers scan `USER_DOCUMENT`. Upon expiration, the `ACCESS_ENFORCEMENT_POLICY` is triggered. Critical documents will automatically transition the `USER_ACCOUNT` to a `BLOCKED` status or restrict specific `PROFILE` context.
8.  **Parametric Notifications**: `NOTIFICATION_RULE` allows configuring N-step alerts (e.g., 30, 15, 5 days before expiration) per Tenant and Document Type.
9.  **Mandatory Parametric Catalog Standard**: Every parameter/configuration/catalog entity MUST include `Code`, `Value`, and `Description`. `Description` must document purpose, functional impact, expected behavior, and applicable scope. All such entities must additionally define uniqueness by scope, versioning lineage, auditing metadata, traceability events, cache invalidation strategy, and forward extensibility.
