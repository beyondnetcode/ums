# UMS E/R Model - Export Formats & Alternatives

If Mermaid visualization is failing or insufficient, use these industry-standard formats to visualize the **Role-Scoped & 5-Level Hierarchy Framework**.

## 1. dbdiagram.io (DBML - Recommended) 🚀
1.  Go to [dbdiagram.io](https://dbdiagram.io/d).
2.  Paste the code below.

```dbml
// UMS Role-Scoped & Strict 5-Level Hierarchy Model
// Engine: SQL Server 2022
// Optimizations: RLS Denormalization & Exclusive Arc

Table TENANT {
  TenantId uniqueidentifier [pk]
  Name nvarchar
}

Table BRANCH {
  BranchId uniqueidentifier [pk]
  TenantId uniqueidentifier [note: 'RLS']
  Name nvarchar
  Code nvarchar
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
  TenantId uniqueidentifier [note: 'RLS']
  Name nvarchar
}

Table PERMISSION_TEMPLATE {
  TemplateId uniqueidentifier [pk]
  RoleId uniqueidentifier
  ActionId uniqueidentifier
  TenantId uniqueidentifier [note: 'RLS']
  SuiteId uniqueidentifier [note: 'Exclusive Arc']
  ModuleId uniqueidentifier [note: 'Exclusive Arc']
  SubModuleId uniqueidentifier [note: 'Exclusive Arc']
  OptionId uniqueidentifier [note: 'Exclusive Arc']
}

Table PROFILE {
  ProfileId uniqueidentifier [pk]
  TenantId uniqueidentifier
  UserId uniqueidentifier
  RoleId uniqueidentifier
  BranchId uniqueidentifier [note: 'Branch Context']
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
  TenantId uniqueidentifier [note: 'RLS']
  Name nvarchar
}

Table FUNCTIONAL_SUBMODULE {
  SubModuleId uniqueidentifier [pk]
  ModuleId uniqueidentifier
  TenantId uniqueidentifier [note: 'RLS']
  Name nvarchar
}

Table FUNCTIONAL_OPTION {
  OptionId uniqueidentifier [pk]
  SubModuleId uniqueidentifier
  TenantId uniqueidentifier [note: 'RLS']
  Name nvarchar
  Code nvarchar
}

Table ACTION {
  ActionId uniqueidentifier [pk]
  SuiteId uniqueidentifier [note: 'XOR Global']
  ModuleId uniqueidentifier [note: 'XOR Module-Specific']
  TenantId uniqueidentifier [note: 'RLS']
  Code nvarchar
}

// Relationships
Ref: USER.TenantId > TENANT.TenantId
Ref: BRANCH.TenantId > TENANT.TenantId
Ref: SYSTEM_SUITE.TenantId > TENANT.TenantId
Ref: ROLE.SuiteId > SYSTEM_SUITE.SuiteId
Ref: ROLE.TenantId > TENANT.TenantId
Ref: PERMISSION_TEMPLATE.RoleId > ROLE.RoleId
Ref: PERMISSION_TEMPLATE.ActionId > ACTION.ActionId
Ref: PERMISSION_TEMPLATE.TenantId > TENANT.TenantId
Ref: PERMISSION_TEMPLATE.SuiteId > SYSTEM_SUITE.SuiteId
Ref: PERMISSION_TEMPLATE.ModuleId > FUNCTIONAL_MODULE.ModuleId
Ref: PERMISSION_TEMPLATE.SubModuleId > FUNCTIONAL_SUBMODULE.SubModuleId
Ref: PERMISSION_TEMPLATE.OptionId > FUNCTIONAL_OPTION.OptionId
Ref: PROFILE.TenantId > TENANT.TenantId
Ref: PROFILE.UserId > USER.UserId
Ref: PROFILE.RoleId > ROLE.RoleId
Ref: PROFILE.BranchId > BRANCH.BranchId
Ref: PROFILE_PERMISSION.ProfileId > PROFILE.ProfileId
Ref: PROFILE_PERMISSION.TemplateId > PERMISSION_TEMPLATE.TemplateId
Ref: FUNCTIONAL_MODULE.SuiteId > SYSTEM_SUITE.SuiteId
Ref: FUNCTIONAL_MODULE.TenantId > TENANT.TenantId
Ref: FUNCTIONAL_SUBMODULE.ModuleId > FUNCTIONAL_MODULE.ModuleId
Ref: FUNCTIONAL_SUBMODULE.TenantId > TENANT.TenantId
Ref: FUNCTIONAL_OPTION.SubModuleId > FUNCTIONAL_SUBMODULE.SubModuleId
Ref: FUNCTIONAL_OPTION.TenantId > TENANT.TenantId
Ref: ACTION.SuiteId > SYSTEM_SUITE.SuiteId
Ref: ACTION.ModuleId > FUNCTIONAL_MODULE.ModuleId
Ref: ACTION.TenantId > TENANT.TenantId
```

---

## 2. SQL DDL (SQL Server 2022) 🛠️
```sql
-- 5-Level Hierarchy Master Schema with Technical Optimizations

CREATE TABLE ACTION (
    ActionId UNIQUEIDENTIFIER PRIMARY KEY,
    SuiteId UNIQUEIDENTIFIER REFERENCES SYSTEM_SUITE(SuiteId),
    ModuleId UNIQUEIDENTIFIER REFERENCES FUNCTIONAL_MODULE(ModuleId),
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    Code NVARCHAR(50),
    CONSTRAINT CHK_ActionOwnership_XOR CHECK (
        (SuiteId IS NOT NULL AND ModuleId IS NULL) OR 
        (SuiteId IS NULL AND ModuleId IS NOT NULL)
    )
);

CREATE TABLE ROLE (
    RoleId UNIQUEIDENTIFIER PRIMARY KEY,
    SuiteId UNIQUEIDENTIFIER REFERENCES SYSTEM_SUITE(SuiteId),
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    Name NVARCHAR(255)
);

CREATE TABLE BRANCH (
    BranchId UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    Name NVARCHAR(255),
    Code NVARCHAR(50)
);

CREATE TABLE PROFILE (
    ProfileId UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    UserId UNIQUEIDENTIFIER REFERENCES USER(UserId),
    RoleId UNIQUEIDENTIFIER REFERENCES ROLE(RoleId),
    BranchId UNIQUEIDENTIFIER REFERENCES BRANCH(BranchId)
);

CREATE TABLE PERMISSION_TEMPLATE (
    TemplateId UNIQUEIDENTIFIER PRIMARY KEY,
    RoleId UNIQUEIDENTIFIER REFERENCES ROLE(RoleId),
    ActionId UNIQUEIDENTIFIER REFERENCES ACTION(ActionId),
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    SuiteId UNIQUEIDENTIFIER NULL REFERENCES SYSTEM_SUITE(SuiteId),
    ModuleId UNIQUEIDENTIFIER NULL REFERENCES FUNCTIONAL_MODULE(ModuleId),
    SubModuleId UNIQUEIDENTIFIER NULL REFERENCES FUNCTIONAL_SUBMODULE(SubModuleId),
    OptionId UNIQUEIDENTIFIER NULL REFERENCES FUNCTIONAL_OPTION(OptionId),
    CONSTRAINT CHK_Exclusive_Resource CHECK (
        (CASE WHEN SuiteId IS NULL THEN 0 ELSE 1 END +
         CASE WHEN ModuleId IS NULL THEN 0 ELSE 1 END +
         CASE WHEN SubModuleId IS NULL THEN 0 ELSE 1 END +
         CASE WHEN OptionId IS NULL THEN 0 ELSE 1 END) = 1
    )
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
