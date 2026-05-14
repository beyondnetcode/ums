# UMS E/R Model - Export Formats & Alternatives

If Mermaid visualization is failing or insufficient, use these industry-standard formats to visualize the **Role-Scoped & 5-Level Hierarchy Framework**.

## 1. dbdiagram.io (DBML - Recommended) 🚀
1.  Go to [dbdiagram.io](https://dbdiagram.io/d).
2.  Paste the code below.

```dbml
// UMS Role-Scoped & Strict 5-Level Hierarchy Model
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

Table FUNCTIONAL_SUBMODULE {
  SubModuleId uniqueidentifier [pk]
  ModuleId uniqueidentifier
  Name nvarchar
}

Table FUNCTIONAL_OPTION {
  OptionId uniqueidentifier [pk]
  SubModuleId uniqueidentifier
  Name nvarchar
  Code nvarchar
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
Ref: FUNCTIONAL_MODULE.SuiteId > SYSTEM_SUITE.SuiteId
Ref: FUNCTIONAL_SUBMODULE.ModuleId > FUNCTIONAL_MODULE.ModuleId
Ref: FUNCTIONAL_OPTION.SubModuleId > FUNCTIONAL_SUBMODULE.SubModuleId
Ref: ACTION.SuiteId > SYSTEM_SUITE.SuiteId
Ref: ACTION.ModuleId > FUNCTIONAL_MODULE.ModuleId
```

---

## 2. SQL DDL (SQL Server 2022) 🛠️
```sql
-- 5-Level Hierarchy Master Schema

CREATE TABLE FUNCTIONAL_MODULE (
    ModuleId UNIQUEIDENTIFIER PRIMARY KEY,
    SuiteId UNIQUEIDENTIFIER REFERENCES SYSTEM_SUITE(SuiteId),
    Name NVARCHAR(255)
);

CREATE TABLE FUNCTIONAL_SUBMODULE (
    SubModuleId UNIQUEIDENTIFIER PRIMARY KEY,
    ModuleId UNIQUEIDENTIFIER REFERENCES FUNCTIONAL_MODULE(ModuleId),
    Name NVARCHAR(255)
);

CREATE TABLE FUNCTIONAL_OPTION (
    OptionId UNIQUEIDENTIFIER PRIMARY KEY,
    SubModuleId UNIQUEIDENTIFIER REFERENCES FUNCTIONAL_SUBMODULE(SubModuleId),
    Name NVARCHAR(255),
    Code NVARCHAR(50)
);

CREATE TABLE ACTION (
    ActionId UNIQUEIDENTIFIER PRIMARY KEY,
    SuiteId UNIQUEIDENTIFIER REFERENCES SYSTEM_SUITE(SuiteId),
    ModuleId UNIQUEIDENTIFIER REFERENCES FUNCTIONAL_MODULE(ModuleId),
    Code NVARCHAR(50),
    CONSTRAINT CHK_ActionOwnership CHECK (SuiteId IS NOT NULL OR ModuleId IS NOT NULL)
);
```
