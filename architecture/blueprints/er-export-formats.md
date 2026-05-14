# UMS E/R Model - Export Formats & Alternatives

If Mermaid visualization is failing or insufficient, use these industry-standard formats to visualize the **Role-Scoped Master Template Framework**.

## 1. dbdiagram.io (DBML - Recommended) 🚀
1.  Go to [dbdiagram.io](https://dbdiagram.io/d).
2.  Paste the code below.

```dbml
// UMS Role-Scoped Master Template Model
// Engine: SQL Server 2022

Table TENANT {
  TenantId uniqueidentifier [pk]
  Name nvarchar
}

Table USER {
  UserId uniqueidentifier [pk]
  TenantId uniqueidentifier
}

Table SYSTEM_SUITE {
  SuiteId uniqueidentifier [pk]
  TenantId uniqueidentifier
  Name nvarchar
}

Table ROLE {
  RoleId uniqueidentifier [pk]
  SuiteId uniqueidentifier
  Name nvarchar
}

Table PERMISSION_TEMPLATE {
  TemplateId uniqueidentifier [pk]
  RoleId uniqueidentifier
  ActionId uniqueidentifier
  ResourceLevel nvarchar
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

Table FUNCTIONAL_MODULE {
  ModuleId uniqueidentifier [pk]
  SuiteId uniqueidentifier
  Name nvarchar
}

Table ACTION {
  ActionId uniqueidentifier [pk]
  SuiteId uniqueidentifier [note: 'Global']
  ModuleId uniqueidentifier [note: 'Module-Specific']
  Code nvarchar
}

// Relationships
Ref: USER.TenantId > TENANT.TenantId
Ref: SYSTEM_SUITE.TenantId > TENANT.TenantId
Ref: ROLE.SuiteId > SYSTEM_SUITE.SuiteId
Ref: PERMISSION_TEMPLATE.RoleId > ROLE.RoleId
Ref: PERMISSION_TEMPLATE.ActionId > ACTION.ActionId
Ref: PROFILE.TenantId > TENANT.TenantId
Ref: PROFILE.UserId > USER.UserId
Ref: PROFILE.RoleId > ROLE.RoleId
Ref: PROFILE_PERMISSION.ProfileId > PROFILE.ProfileId
Ref: PROFILE_PERMISSION.TemplateId > PERMISSION_TEMPLATE.TemplateId
Ref: ACTION.SuiteId > SYSTEM_SUITE.SuiteId
Ref: ACTION.ModuleId > FUNCTIONAL_MODULE.ModuleId
```

---

## 2. SQL DDL (SQL Server 2022) 🛠️
```sql
-- Role-Scoped Master Schema

CREATE TABLE ROLE (
    RoleId UNIQUEIDENTIFIER PRIMARY KEY,
    SuiteId UNIQUEIDENTIFIER REFERENCES SYSTEM_SUITE(SuiteId),
    Name NVARCHAR(255)
);

CREATE TABLE PERMISSION_TEMPLATE (
    TemplateId UNIQUEIDENTIFIER PRIMARY KEY,
    RoleId UNIQUEIDENTIFIER REFERENCES ROLE(RoleId),
    ActionId UNIQUEIDENTIFIER REFERENCES ACTION(ActionId),
    ResourceLevel NVARCHAR(50), -- SYSTEM, MODULE, MENU, etc.
    ResourceId NVARCHAR(255)
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
