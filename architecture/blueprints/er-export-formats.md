# UMS E/R Model - Export Formats & Alternatives

If Mermaid visualization is failing or insufficient, use these industry-standard formats to visualize the **Scoped Action Governance Framework**.

## 1. dbdiagram.io (DBML - Recommended) 🚀
1.  Go to [dbdiagram.io](https://dbdiagram.io/d).
2.  Paste the code below.

```dbml
// UMS Scoped Action Governance Model
// Engine: SQL Server 2022

Table TENANT {
  TenantId uniqueidentifier [pk]
  Name nvarchar
  Code nvarchar [unique]
}

Table USER {
  UserId uniqueidentifier [pk]
  TenantId uniqueidentifier
  Username nvarchar
}

Table SYSTEM_SUITE {
  SuiteId uniqueidentifier [pk]
  TenantId uniqueidentifier
  Name nvarchar
}

Table FUNCTIONAL_MODULE {
  ModuleId uniqueidentifier [pk]
  SuiteId uniqueidentifier
  Name nvarchar
}

Table ACTION {
  ActionId uniqueidentifier [pk]
  SuiteId uniqueidentifier [note: 'Nullable']
  ModuleId uniqueidentifier [note: 'Nullable']
  Code nvarchar
}

Table PERMISSION_TEMPLATE {
  TemplateId uniqueidentifier [pk]
  ActionId uniqueidentifier
  ResourceType nvarchar
  ResourceId nvarchar
}

Table PROFILE {
  ProfileId uniqueidentifier [pk]
  TenantId uniqueidentifier
  UserId uniqueidentifier
  RoleId uniqueidentifier
}

Table PROFILE_PERMISSION {
  ProfileId uniqueidentifier [pk]
  TemplateId uniqueidentifier [pk]
  IsAllowed bit
  IsDenied bit
  IsActive bit
}

Table ROLE {
  RoleId uniqueidentifier [pk]
  SuiteId uniqueidentifier
  Name nvarchar
}

// Relationships
Ref: USER.TenantId > TENANT.TenantId
Ref: SYSTEM_SUITE.TenantId > TENANT.TenantId
Ref: FUNCTIONAL_MODULE.SuiteId > SYSTEM_SUITE.SuiteId
Ref: ACTION.SuiteId > SYSTEM_SUITE.SuiteId
Ref: ACTION.ModuleId > FUNCTIONAL_MODULE.ModuleId
Ref: PERMISSION_TEMPLATE.ActionId > ACTION.ActionId
Ref: PROFILE.TenantId > TENANT.TenantId
Ref: PROFILE.UserId > USER.UserId
Ref: PROFILE.RoleId > ROLE.RoleId
Ref: PROFILE_PERMISSION.ProfileId > PROFILE.ProfileId
Ref: PROFILE_PERMISSION.TemplateId > PERMISSION_TEMPLATE.TemplateId
```

---

## 2. SQL DDL (SQL Server 2022) 🛠️
```sql
-- Scoped Action Schema

CREATE TABLE ACTION (
    ActionId UNIQUEIDENTIFIER PRIMARY KEY,
    SuiteId UNIQUEIDENTIFIER REFERENCES SYSTEM_SUITE(SuiteId),
    ModuleId UNIQUEIDENTIFIER REFERENCES FUNCTIONAL_MODULE(ModuleId),
    Code NVARCHAR(50) NOT NULL,
    CONSTRAINT CHK_ActionOwnership CHECK (SuiteId IS NOT NULL OR ModuleId IS NOT NULL)
);

CREATE TABLE PERMISSION_TEMPLATE (
    TemplateId UNIQUEIDENTIFIER PRIMARY KEY,
    ActionId UNIQUEIDENTIFIER REFERENCES ACTION(ActionId),
    ResourceType NVARCHAR(100),
    ResourceId NVARCHAR(255),
    AuditFields... -- Add standard 10 columns
);

CREATE TABLE PROFILE_PERMISSION (
    ProfileId UNIQUEIDENTIFIER REFERENCES PROFILE(ProfileId),
    TemplateId UNIQUEIDENTIFIER REFERENCES PERMISSION_TEMPLATE(TemplateId),
    IsAllowed BIT DEFAULT 1,
    IsDenied BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    PRIMARY KEY (ProfileId, TemplateId)
);
```
