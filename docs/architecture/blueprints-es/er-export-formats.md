# UMS E/R Model - Export Formats & Alternatives

If Mermaid visualization is failing or insufficient, use these industry-standard formats to visualize the **Advanced IGA, Role Evolution & Hierarchical Configuration Framework**.

## 1. dbdiagram.io (DBML - Recommended) 
1.  Go to [dbdiagram.io](https://dbdiagram.io/d).
2. Paste the code below.

```dbml
// UMS Framework: Advanced IGA & Hierarchical Governance
// Engine: SQL Server 2022

Table TENANT {
  TenantId uniqueidentifier [pk]
  Name nvarchar
}

Table BRANCH {
  BranchId uniqueidentifier [pk]
  TenantId uniqueidentifier
  Name nvarchar
}

Table IDENTITY_PROVIDER {
  IdpId uniqueidentifier [pk]
  TenantId uniqueidentifier
  Code nvarchar
  Name nvarchar
  Description nvarchar
  Strategy nvarchar
  IsActive bit
}

Table BRANDING {
  BrandingId uniqueidentifier [pk]
  TenantId uniqueidentifier
  Logo nvarchar
  LogoFormat nvarchar
  PrimaryColor nvarchar
  BackgroundStyle nvarchar
  HeadlineText nvarchar
  SecondaryText nvarchar
  PrimaryButtonLabel nvarchar
  FooterText nvarchar
  CustomDomain nvarchar
  DnsVerificationStatus nvarchar
  DnsCnameTarget nvarchar
  MagicLinkFallbackEnabled bit
}

Table USER_ACCOUNT {
  UserId uniqueidentifier [pk]
  TenantId uniqueidentifier
  UserCategory nvarchar
  Status nvarchar [note: 'ACTIVE/BLOCKED/PENDING']
}

Table USER_MANAGEMENT_DELEGATION {
  DelegationId uniqueidentifier [pk]
  TenantId uniqueidentifier
  ParentAdminUserId uniqueidentifier
  ManagedUserId uniqueidentifier
  SuiteId uniqueidentifier
}

Table ROLE {
  RoleId uniqueidentifier [pk]
  SuiteId uniqueidentifier
  TenantId uniqueidentifier
  ParentRoleId uniqueidentifier [note: 'Recursive Hierarchy']
  Name nvarchar
  HierarchyLevel int
  PromotionOrder int
}

Table ROLE_MATURITY_STATUS {
  MaturityStatusId uniqueidentifier [pk]
  TenantId uniqueidentifier
  UserId uniqueidentifier
  CurrentLevel nvarchar
  CompletedCertificationsCount int
  CompletedTrainingsCount int
  PerformanceScore double
  HasComplianceIssues bit
  LastLevelChangeDate datetime2
}

Table PROMOTION_REQUEST {
  PromotionRequestId uniqueidentifier [pk]
  TenantId uniqueidentifier
  TargetRoleId uniqueidentifier
  Status nvarchar [note: 'DRAFT/SUBMITTED/UNDER_REVIEW/APPROVED/EXECUTED/VERIFIED']
  InitiatedByUserId uniqueidentifier
}

Table PROMOTION_IMPACT_ANALYSIS {
  ImpactAnalysisId uniqueidentifier [pk]
  PromotionRequestId uniqueidentifier
  RiskLevel nvarchar [note: 'LOW/MEDIUM/HIGH']
  AnalysisDetails nvarchar
  ViolatesSoD bit
}

Table APP_CONFIGURATION {
  SettingId uniqueidentifier [pk]
  TenantId uniqueidentifier [note: 'Nullable: Global if null']
  SuiteId uniqueidentifier [note: 'Nullable']
  ModuleId uniqueidentifier [note: 'Nullable']
  Code nvarchar [note: 'e.g. ENABLE_ROLE_EVOLUTION']
  Value nvarchar
  Description nvarchar [note: 'Technical/Business purpose of the setting']
  IsInheritable bit [default: 1]
  IsEncrypted bit [default: 0]
}

Table DOCUMENT_TYPE {
  DocumentTypeId uniqueidentifier [pk]
  TenantId uniqueidentifier
  Name nvarchar
  IsAccessCritical bit
}

Table USER_DOCUMENT {
  DocumentId uniqueidentifier [pk]
  UserId uniqueidentifier
  DocumentTypeId uniqueidentifier
  RequestId uniqueidentifier
  FileName nvarchar
  FileStoragePath nvarchar
  Checksum nvarchar
  IssueDate datetime2
  ExpirationDate datetime2
  Status nvarchar
  Criticity nvarchar
}

Table NOTIFICATION_RULE {
  RuleId uniqueidentifier [pk]
  TenantId uniqueidentifier
  DocumentTypeId uniqueidentifier
  Code nvarchar [note: 'ej. DOC_EXPIRY_NOTICE_30D_EMAIL']
  Value nvarchar [note: 'ej. 30|EMAIL o payload JSON']
  Description nvarchar [note: 'Propósito, impacto funcional, comportamiento esperado y alcance']
  DaysBefore int
  Channel nvarchar
}

Table ACCESS_ENFORCEMENT_POLICY {
  PolicyId uniqueidentifier [pk]
  TenantId uniqueidentifier [note: 'Anulable si es política global base']
  Code nvarchar [note: 'ej. DOC_EXPIRY_BLOCK_USER']
  Value nvarchar [note: 'ej. BLOCK_USER/RESTRICT_PROFILE/LOG_ONLY']
  Description nvarchar [note: 'Propósito, impacto, comportamiento esperado y alcance']
  DocumentTypeId uniqueidentifier
  ActionOnExpiration nvarchar
}

Table APPROVAL_WORKFLOW {
  WorkflowId uniqueidentifier [pk]
  TenantId uniqueidentifier
  SuiteId uniqueidentifier
  Code nvarchar [note: 'ej. B2B_EXTERNAL_ACCESS_DEFAULT']
  Value nvarchar [note: 'ej. JSON con etapas/aprobadores']
  Description nvarchar [note: 'Propósito, impacto, comportamiento esperado y alcance']
  TargetUserCategory nvarchar
  RequiresApproval bit
}

Table APPROVAL_REQUIRED_DOCUMENT {
  DocumentTypeId uniqueidentifier [pk]
  WorkflowId uniqueidentifier
  IsMandatory bit
}

Table APPROVAL_REQUEST {
  RequestId uniqueidentifier [pk]
  TenantId uniqueidentifier
  WorkflowId uniqueidentifier
  TargetUserId uniqueidentifier
  TargetProfileId uniqueidentifier
  RequestStatus nvarchar
}

Table APPROVAL_LOG {
  LogId bigint [pk]
  RequestId uniqueidentifier
  ApproverUserId uniqueidentifier
  ActionTaken nvarchar
}

Table SYSTEM_SUITE {
  SuiteId uniqueidentifier [pk]
  TenantId uniqueidentifier
  Name nvarchar
}

Table PROFILE {
  ProfileId uniqueidentifier [pk]
  TenantId uniqueidentifier
  UserId uniqueidentifier
  RoleId uniqueidentifier
  BranchId uniqueidentifier
}

Table PROFILE_PERMISSION {
  ProfileId uniqueidentifier [pk]
  TemplateId uniqueidentifier [pk]
  IsAllowed bit
  IsDenied bit
  IsActive bit
}

Table PERMISSION_TEMPLATE {
  TemplateId uniqueidentifier [pk]
  RoleId uniqueidentifier
  ActionId uniqueidentifier
  TenantId uniqueidentifier
  SuiteId uniqueidentifier
  ModuleId uniqueidentifier
  SubModuleId uniqueidentifier
  OptionId uniqueidentifier
}

Table FUNCTIONAL_MODULE {
  ModuleId uniqueidentifier [pk]
  SuiteId uniqueidentifier
  TenantId uniqueidentifier
  Name nvarchar
}

Table FUNCTIONAL_SUBMODULE {
  SubModuleId uniqueidentifier [pk]
  ModuleId uniqueidentifier
  TenantId uniqueidentifier
}

Table FUNCTIONAL_OPTION {
  OptionId uniqueidentifier [pk]
  SubModuleId uniqueidentifier
  TenantId uniqueidentifier
  Code nvarchar
}

Table ACTION {
  ActionId uniqueidentifier [pk]
  SuiteId uniqueidentifier
  ModuleId uniqueidentifier
  TenantId uniqueidentifier
  Code nvarchar
}

// Relationships
Ref: USER_ACCOUNT.TenantId > TENANT.TenantId
Ref: USER_MANAGEMENT_DELEGATION.TenantId > TENANT.TenantId
Ref: IDENTITY_PROVIDER.TenantId > TENANT.TenantId
Ref: BRANDING.TenantId - TENANT.TenantId
Ref: USER_MANAGEMENT_DELEGATION.ParentAdminUserId > USER_ACCOUNT.UserId
Ref: USER_MANAGEMENT_DELEGATION.ManagedUserId > USER_ACCOUNT.UserId
Ref: USER_MANAGEMENT_DELEGATION.SuiteId > SYSTEM_SUITE.SuiteId

Ref: ROLE.SuiteId > SYSTEM_SUITE.SuiteId
Ref: ROLE.TenantId > TENANT.TenantId
Ref: ROLE_MATURITY_STATUS.UserId > USER_ACCOUNT.UserId
Ref: ROLE_MATURITY_STATUS.TenantId > TENANT.TenantId
Ref: PROMOTION_REQUEST.TenantId > TENANT.TenantId
Ref: PROMOTION_REQUEST.TargetRoleId > ROLE.RoleId
Ref: PROMOTION_REQUEST.InitiatedByUserId > USER_ACCOUNT.UserId
Ref: PROMOTION_IMPACT_ANALYSIS.PromotionRequestId > PROMOTION_REQUEST.PromotionRequestId

Ref: APP_CONFIGURATION.TenantId > TENANT.TenantId
Ref: APP_CONFIGURATION.SuiteId > SYSTEM_SUITE.SuiteId
Ref: APP_CONFIGURATION.ModuleId > FUNCTIONAL_MODULE.ModuleId

Ref: DOCUMENT_TYPE.TenantId > TENANT.TenantId
Ref: USER_DOCUMENT.UserId > USER_ACCOUNT.UserId
Ref: USER_DOCUMENT.DocumentTypeId > DOCUMENT_TYPE.DocumentTypeId
Ref: USER_DOCUMENT.RequestId > APPROVAL_REQUEST.RequestId
Ref: NOTIFICATION_RULE.TenantId > TENANT.TenantId
Ref: NOTIFICATION_RULE.DocumentTypeId > DOCUMENT_TYPE.DocumentTypeId
Ref: ACCESS_ENFORCEMENT_POLICY.DocumentTypeId > DOCUMENT_TYPE.DocumentTypeId

Ref: APPROVAL_WORKFLOW.TenantId > TENANT.TenantId
Ref: APPROVAL_WORKFLOW.SuiteId > SYSTEM_SUITE.SuiteId
Ref: APPROVAL_REQUIRED_DOCUMENT.WorkflowId > APPROVAL_WORKFLOW.WorkflowId
Ref: APPROVAL_REQUIRED_DOCUMENT.DocumentTypeId > DOCUMENT_TYPE.DocumentTypeId
Ref: APPROVAL_REQUEST.TenantId > TENANT.TenantId
Ref: APPROVAL_REQUEST.WorkflowId > APPROVAL_WORKFLOW.WorkflowId
Ref: APPROVAL_REQUEST.TargetUserId > USER_ACCOUNT.UserId
Ref: APPROVAL_REQUEST.TargetProfileId > PROFILE.ProfileId
Ref: APPROVAL_LOG.RequestId > APPROVAL_REQUEST.RequestId
Ref: APPROVAL_LOG.ApproverUserId > USER_ACCOUNT.UserId

Ref: SYSTEM_SUITE.TenantId > TENANT.TenantId
Ref: PROFILE.UserId > USER_ACCOUNT.UserId
Ref: PROFILE.RoleId > ROLE.RoleId
Ref: PROFILE.BranchId > BRANCH.BranchId
Ref: PROFILE_PERMISSION.ProfileId > PROFILE.ProfileId
Ref: PROFILE_PERMISSION.TemplateId > PERMISSION_TEMPLATE.TemplateId
Ref: PERMISSION_TEMPLATE.RoleId > ROLE.RoleId
Ref: PERMISSION_TEMPLATE.ActionId > ACTION.ActionId
Ref: PERMISSION_TEMPLATE.SuiteId > SYSTEM_SUITE.SuiteId
Ref: PERMISSION_TEMPLATE.ModuleId > FUNCTIONAL_MODULE.ModuleId
Ref: PERMISSION_TEMPLATE.SubModuleId > FUNCTIONAL_SUBMODULE.SubModuleId
Ref: PERMISSION_TEMPLATE.OptionId > FUNCTIONAL_OPTION.OptionId
Ref: FUNCTIONAL_MODULE.SuiteId > SYSTEM_SUITE.SuiteId
Ref: FUNCTIONAL_SUBMODULE.ModuleId > FUNCTIONAL_MODULE.ModuleId
Ref: FUNCTIONAL_OPTION.SubModuleId > FUNCTIONAL_SUBMODULE.SubModuleId
Ref: ACTION.SuiteId > SYSTEM_SUITE.SuiteId
Ref: ACTION.ModuleId > FUNCTIONAL_MODULE.ModuleId
```

---

## 2. SQL DDL (SQL Server 2022) 
```sql
-- Advanced IGA: Role Evolution & Hierarchical Configuration

CREATE TABLE TENANT (
    TenantId UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(255)
);

CREATE TABLE BRANCH (
    BranchId UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    Name NVARCHAR(255)
);

CREATE TABLE IDENTITY_PROVIDER (
    IdpId UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    Code NVARCHAR(100) NOT NULL,
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    Strategy NVARCHAR(100) NOT NULL,
    IsActive BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
);

CREATE TABLE BRANDING (
    BrandingId UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER UNIQUE REFERENCES TENANT(TenantId),
    Logo NVARCHAR(MAX),
    LogoFormat NVARCHAR(50),
    PrimaryColor NVARCHAR(50),
    BackgroundStyle NVARCHAR(100),
    HeadlineText NVARCHAR(MAX),
    SecondaryText NVARCHAR(MAX),
    PrimaryButtonLabel NVARCHAR(255),
    FooterText NVARCHAR(MAX),
    CustomDomain NVARCHAR(255) NULL,
    DnsVerificationStatus NVARCHAR(50) DEFAULT 'PENDING',
    DnsCnameTarget NVARCHAR(255),
    MagicLinkFallbackEnabled BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
);

CREATE TABLE USER_ACCOUNT (
    UserId UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    UserCategory NVARCHAR(50),
    Status NVARCHAR(50) DEFAULT 'PENDING'
);

CREATE TABLE SYSTEM_SUITE (
    SuiteId UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    Name NVARCHAR(255)
);

CREATE TABLE ROLE (
    RoleId UNIQUEIDENTIFIER PRIMARY KEY,
    SuiteId UNIQUEIDENTIFIER REFERENCES SYSTEM_SUITE(SuiteId),
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    ParentRoleId UNIQUEIDENTIFIER NULL REFERENCES ROLE(RoleId),
    Name NVARCHAR(255),
    HierarchyLevel INT DEFAULT 0,
    PromotionOrder INT DEFAULT 0
);

CREATE TABLE ROLE_MATURITY_STATUS (
    MaturityStatusId UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    UserId UNIQUEIDENTIFIER REFERENCES USER_ACCOUNT(UserId),
    CurrentLevel NVARCHAR(100),
    CompletedCertificationsCount INT DEFAULT 0,
    CompletedTrainingsCount INT DEFAULT 0,
    PerformanceScore FLOAT DEFAULT 0.0,
    HasComplianceIssues BIT DEFAULT 0,
    LastLevelChangeDate DATETIME2
);

CREATE TABLE APP_CONFIGURATION (
    SettingId UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NULL REFERENCES TENANT(TenantId),
    SuiteId UNIQUEIDENTIFIER NULL REFERENCES SYSTEM_SUITE(SuiteId),
    ModuleId UNIQUEIDENTIFIER NULL, -- References Functional Module later
    Code NVARCHAR(100) NOT NULL,
    Value NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    IsInheritable BIT DEFAULT 1,
    IsEncrypted BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
);

CREATE TABLE DOCUMENT_TYPE (
    DocumentTypeId UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    Name NVARCHAR(255),
    IsAccessCritical BIT DEFAULT 0
);

CREATE TABLE APPROVAL_WORKFLOW (
    WorkflowId UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    SuiteId UNIQUEIDENTIFIER NULL REFERENCES SYSTEM_SUITE(SuiteId),
    Code NVARCHAR(100) NOT NULL,
    Value NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    TargetUserCategory NVARCHAR(50),
    RequiresApproval BIT DEFAULT 1
);

CREATE TABLE APPROVAL_REQUEST (
    RequestId UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    WorkflowId UNIQUEIDENTIFIER REFERENCES APPROVAL_WORKFLOW(WorkflowId),
    TargetUserId UNIQUEIDENTIFIER REFERENCES USER_ACCOUNT(UserId),
    TargetProfileId UNIQUEIDENTIFIER NULL,
    RequestStatus NVARCHAR(50)
);

CREATE TABLE PROMOTION_REQUEST (
    PromotionRequestId UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    TargetRoleId UNIQUEIDENTIFIER REFERENCES ROLE(RoleId),
    Status NVARCHAR(50) DEFAULT 'DRAFT', -- DRAFT, SUBMITTED, UNDER_REVIEW, APPROVED, EXECUTED, VERIFIED
    InitiatedByUserId UNIQUEIDENTIFIER REFERENCES USER_ACCOUNT(UserId),
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
);

CREATE TABLE PROMOTION_IMPACT_ANALYSIS (
    ImpactAnalysisId UNIQUEIDENTIFIER PRIMARY KEY,
    PromotionRequestId UNIQUEIDENTIFIER REFERENCES PROMOTION_REQUEST(PromotionRequestId),
    RiskLevel NVARCHAR(50),
    AnalysisDetails NVARCHAR(MAX),
    ViolatesSoD BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
);

CREATE TABLE USER_DOCUMENT (
    DocumentId UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER REFERENCES USER_ACCOUNT(UserId),
    DocumentTypeId UNIQUEIDENTIFIER REFERENCES DOCUMENT_TYPE(DocumentTypeId),
    RequestId UNIQUEIDENTIFIER NULL REFERENCES APPROVAL_REQUEST(RequestId),
    FileName NVARCHAR(255),
    FileStoragePath NVARCHAR(MAX),
    Checksum NVARCHAR(255),
    IssueDate DATETIME2,
    ExpirationDate DATETIME2,
    Status NVARCHAR(50) DEFAULT 'VALID',
    Criticity NVARCHAR(50) DEFAULT 'MEDIUM',
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
);

CREATE TABLE NOTIFICATION_RULE (
    RuleId UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    DocumentTypeId UNIQUEIDENTIFIER REFERENCES DOCUMENT_TYPE(DocumentTypeId),
    Code NVARCHAR(100) NOT NULL,
    Value NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    DaysBefore INT,
    Channel NVARCHAR(50)
);

CREATE TABLE ACCESS_ENFORCEMENT_POLICY (
    PolicyId UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NULL REFERENCES TENANT(TenantId),
    Code NVARCHAR(100) NOT NULL,
    Value NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    DocumentTypeId UNIQUEIDENTIFIER REFERENCES DOCUMENT_TYPE(DocumentTypeId),
    ActionOnExpiration NVARCHAR(50)
);

CREATE TABLE PROFILE (
    ProfileId UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    UserId UNIQUEIDENTIFIER REFERENCES USER_ACCOUNT(UserId),
    RoleId UNIQUEIDENTIFIER REFERENCES ROLE(RoleId),
    BranchId UNIQUEIDENTIFIER REFERENCES BRANCH(BranchId)
);

CREATE TABLE FUNCTIONAL_MODULE (
    ModuleId UNIQUEIDENTIFIER PRIMARY KEY,
    SuiteId UNIQUEIDENTIFIER REFERENCES SYSTEM_SUITE(SuiteId),
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    Name NVARCHAR(255)
);

-- Finish App Configuration reference
ALTER TABLE APP_CONFIGURATION ADD CONSTRAINT FK_Config_Module FOREIGN KEY (ModuleId) REFERENCES FUNCTIONAL_MODULE(ModuleId);
CREATE UNIQUE INDEX UX_APP_CONFIGURATION_CODE_SCOPE
ON APP_CONFIGURATION (Code, ISNULL(TenantId, '00000000-0000-0000-0000-000000000000'), ISNULL(SuiteId, '00000000-0000-0000-0000-000000000000'), ISNULL(ModuleId, '00000000-0000-0000-0000-000000000000'));

CREATE UNIQUE INDEX UX_NOTIFICATION_RULE_CODE_SCOPE
ON NOTIFICATION_RULE (Code, TenantId, DocumentTypeId);

CREATE UNIQUE INDEX UX_ACCESS_ENFORCEMENT_POLICY_CODE_SCOPE
ON ACCESS_ENFORCEMENT_POLICY (Code, ISNULL(TenantId, '00000000-0000-0000-0000-000000000000'), DocumentTypeId);

CREATE UNIQUE INDEX UX_APPROVAL_WORKFLOW_CODE_SCOPE
ON APPROVAL_WORKFLOW (Code, TenantId, ISNULL(SuiteId, '00000000-0000-0000-0000-000000000000'));

CREATE TABLE FUNCTIONAL_SUBMODULE (
    SubModuleId UNIQUEIDENTIFIER PRIMARY KEY,
    ModuleId UNIQUEIDENTIFIER REFERENCES FUNCTIONAL_MODULE(ModuleId),
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId)
);

CREATE TABLE FUNCTIONAL_OPTION (
    OptionId UNIQUEIDENTIFIER PRIMARY KEY,
    SubModuleId UNIQUEIDENTIFIER REFERENCES FUNCTIONAL_SUBMODULE(SubModuleId),
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    Code NVARCHAR(50)
);

CREATE TABLE ACTION (
    ActionId UNIQUEIDENTIFIER PRIMARY KEY,
    SuiteId UNIQUEIDENTIFIER REFERENCES SYSTEM_SUITE(SuiteId),
    ModuleId UNIQUEIDENTIFIER REFERENCES FUNCTIONAL_MODULE(ModuleId),
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    Code NVARCHAR(50)
);

CREATE TABLE PERMISSION_TEMPLATE (
    TemplateId UNIQUEIDENTIFIER PRIMARY KEY,
    RoleId UNIQUEIDENTIFIER REFERENCES ROLE(RoleId),
    ActionId UNIQUEIDENTIFIER REFERENCES ACTION(ActionId),
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    SuiteId UNIQUEIDENTIFIER NULL REFERENCES SYSTEM_SUITE(SuiteId),
    ModuleId UNIQUEIDENTIFIER NULL REFERENCES FUNCTIONAL_MODULE(ModuleId),
    SubModuleId UNIQUEIDENTIFIER NULL REFERENCES FUNCTIONAL_SUBMODULE(SubModuleId),
    OptionId UNIQUEIDENTIFIER NULL REFERENCES FUNCTIONAL_OPTION(OptionId)
);

CREATE TABLE PROFILE_PERMISSION (
    ProfileId UNIQUEIDENTIFIER REFERENCES PROFILE(ProfileId),
    TemplateId UNIQUEIDENTIFIER REFERENCES PERMISSION_TEMPLATE(TemplateId),
    IsAllowed BIT,
    IsDenied BIT,
    IsActive BIT,
    PRIMARY KEY (ProfileId, TemplateId)
);

CREATE TABLE APPROVAL_REQUIRED_DOCUMENT (
    DocumentTypeId UNIQUEIDENTIFIER REFERENCES DOCUMENT_TYPE(DocumentTypeId),
    WorkflowId UNIQUEIDENTIFIER REFERENCES APPROVAL_WORKFLOW(WorkflowId),
    IsMandatory BIT DEFAULT 1,
    PRIMARY KEY (DocumentTypeId, WorkflowId)
);

CREATE TABLE APPROVAL_LOG (
    LogId BIGINT IDENTITY(1,1) PRIMARY KEY,
    RequestId UNIQUEIDENTIFIER REFERENCES APPROVAL_REQUEST(RequestId),
    ApproverUserId UNIQUEIDENTIFIER REFERENCES USER_ACCOUNT(UserId),
    ActionTaken NVARCHAR(50),
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
);

CREATE TABLE USER_MANAGEMENT_DELEGATION (
    DelegationId UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    ParentAdminUserId UNIQUEIDENTIFIER REFERENCES USER_ACCOUNT(UserId),
    ManagedUserId UNIQUEIDENTIFIER REFERENCES USER_ACCOUNT(UserId),
    SuiteId UNIQUEIDENTIFIER NULL REFERENCES SYSTEM_SUITE(SuiteId)
);
```
