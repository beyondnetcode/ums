# UMS E/R Model - Export Formats & Alternatives

If Mermaid visualization is failing or insufficient, use these industry-standard formats to visualize the **Master-Template Driven Authorization Framework**.

## 1. dbdiagram.io (DBML - Recommended) 🚀
The most professional and interactive way to view the model.
1.  Go to [dbdiagram.io](https://dbdiagram.io/d).
2.  Clear the editor and paste the code below.

```dbml
// UMS Master-Template Driven ER Model
// Optimized for dbdiagram.io

Table TENANT {
  TenantId uniqueidentifier [pk]
  Name nvarchar
  Code nvarchar [unique]
}

Table USER {
  UserId uniqueidentifier [pk]
  TenantId uniqueidentifier
  Username nvarchar
  Email nvarchar
}

Table SYSTEM_SUITE {
  SuiteId uniqueidentifier [pk]
  TenantId uniqueidentifier
  Name nvarchar
  Code nvarchar
}

Table FUNCTIONAL_MODULE {
  ModuleId uniqueidentifier [pk]
  SuiteId uniqueidentifier
  Name nvarchar
  Code nvarchar
}

Table PERMISSION_TEMPLATE {
  TemplateId uniqueidentifier [pk]
  ModuleId uniqueidentifier
  ResourceName nvarchar
  ActionCode nvarchar
  Name nvarchar
}

Table ROLE {
  RoleId uniqueidentifier [pk]
  SuiteId uniqueidentifier
  Name nvarchar
}

Table PROFILE {
  ProfileId uniqueidentifier [pk]
  TenantId uniqueidentifier
  UserId uniqueidentifier
  RoleId uniqueidentifier
  BranchId uniqueidentifier
  DisplayName nvarchar
}

Table PROFILE_PERMISSION {
  ProfileId uniqueidentifier [pk]
  TemplateId uniqueidentifier [pk]
  IsAllowed bit
  IsDenied bit
  IsActive bit
}

Table ROLE_PERMISSION {
  RoleId uniqueidentifier [pk]
  TemplateId uniqueidentifier [pk]
}

Table BRANCH {
  BranchId uniqueidentifier [pk]
  TenantId uniqueidentifier
  Name nvarchar
}

Table MENU_ITEM {
  MenuItemId uniqueidentifier [pk]
  ModuleId uniqueidentifier
  ParentItemId uniqueidentifier
  Name nvarchar
  Route nvarchar
}

Table AUDIT_LOG {
  LogId bigint [pk]
  TenantId uniqueidentifier
  CorrelationId uniqueidentifier
  TransactionId uniqueidentifier
  Action nvarchar
  Timestamp datetimeoffset
}

// Relationships
Ref: USER.TenantId > TENANT.TenantId
Ref: SYSTEM_SUITE.TenantId > TENANT.TenantId
Ref: BRANCH.TenantId > TENANT.TenantId
Ref: FUNCTIONAL_MODULE.SuiteId > SYSTEM_SUITE.SuiteId
Ref: PERMISSION_TEMPLATE.ModuleId > FUNCTIONAL_MODULE.ModuleId
Ref: ROLE.SuiteId > SYSTEM_SUITE.SuiteId
Ref: ROLE_PERMISSION.RoleId > ROLE.RoleId
Ref: ROLE_PERMISSION.TemplateId > PERMISSION_TEMPLATE.TemplateId
Ref: PROFILE.TenantId > TENANT.TenantId
Ref: PROFILE.UserId > USER.UserId
Ref: PROFILE.RoleId > ROLE.RoleId
Ref: PROFILE.BranchId > BRANCH.BranchId
Ref: PROFILE_PERMISSION.ProfileId > PROFILE.ProfileId
Ref: PROFILE_PERMISSION.TemplateId > PERMISSION_TEMPLATE.TemplateId
Ref: MENU_ITEM.ModuleId > FUNCTIONAL_MODULE.ModuleId
Ref: MENU_ITEM.ParentItemId > MENU_ITEM.MenuItemId
```

---

## 2. SQL DDL (SQL Server 2022) 🛠️
Import into **DBeaver**, **SSMS**, or **DataGrip**.

```sql
CREATE TABLE TENANT (
    TenantId UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Code NVARCHAR(50) UNIQUE
);

CREATE TABLE USER (
    UserId UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    Username NVARCHAR(255),
    Email NVARCHAR(255)
);

CREATE TABLE SYSTEM_SUITE (
    SuiteId UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    Name NVARCHAR(255),
    Code NVARCHAR(50)
);

CREATE TABLE FUNCTIONAL_MODULE (
    ModuleId UNIQUEIDENTIFIER PRIMARY KEY,
    SuiteId UNIQUEIDENTIFIER REFERENCES SYSTEM_SUITE(SuiteId),
    Name NVARCHAR(255),
    Code NVARCHAR(50)
);

CREATE TABLE PERMISSION_TEMPLATE (
    TemplateId UNIQUEIDENTIFIER PRIMARY KEY,
    ModuleId UNIQUEIDENTIFIER REFERENCES FUNCTIONAL_MODULE(ModuleId),
    ResourceName NVARCHAR(255),
    ActionCode NVARCHAR(50),
    Name NVARCHAR(255)
);

CREATE TABLE ROLE (
    RoleId UNIQUEIDENTIFIER PRIMARY KEY,
    SuiteId UNIQUEIDENTIFIER REFERENCES SYSTEM_SUITE(SuiteId),
    Name NVARCHAR(255)
);

CREATE TABLE PROFILE (
    ProfileId UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER REFERENCES TENANT(TenantId),
    UserId UNIQUEIDENTIFIER REFERENCES USER(UserId),
    RoleId UNIQUEIDENTIFIER REFERENCES ROLE(RoleId),
    BranchId UNIQUEIDENTIFIER,
    DisplayName NVARCHAR(255)
);

CREATE TABLE PROFILE_PERMISSION (
    ProfileId UNIQUEIDENTIFIER REFERENCES PROFILE(ProfileId),
    TemplateId UNIQUEIDENTIFIER REFERENCES PERMISSION_TEMPLATE(TemplateId),
    IsAllowed BIT,
    IsDenied BIT,
    IsActive BIT,
    PRIMARY KEY (ProfileId, TemplateId)
);
```
