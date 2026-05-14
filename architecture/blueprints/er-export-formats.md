# UMS E/R Model - Export Formats & Alternatives

If Mermaid visualization is failing or insufficient, use these industry-standard formats to visualize the **Master-Template Driven Authorization Framework**.

## 1. dbdiagram.io (DBML - Recommended)
The best tool for interactive, professional E/R visualization.
1.  Go to [dbdiagram.io](https://dbdiagram.io/d).
2.  Paste the following DBML code:

```dbml
// UMS Master-Template Driven ER Model
// Engine: SQL Server 2022

Table TENANT {
  TenantId uniqueidentifier [pk]
  Name nvarchar
  Code nvarchar [unique]
}

Table USER {
  UserId uniqueidentifier [pk]
  TenantId uniqueidentifier [ref: > TENANT.TenantId]
  Username nvarchar
  Email nvarchar
}

Table SYSTEM_SUITE {
  SuiteId uniqueidentifier [pk]
  TenantId uniqueidentifier [ref: > TENANT.TenantId]
  Name nvarchar
  Code nvarchar
}

Table FUNCTIONAL_MODULE {
  ModuleId uniqueidentifier [pk]
  SuiteId uniqueidentifier [ref: > SYSTEM_SUITE.SuiteId]
  Name nvarchar
  Code nvarchar
}

Table PERMISSION_TEMPLATE {
  TemplateId uniqueidentifier [pk]
  ModuleId uniqueidentifier [ref: > FUNCTIONAL_MODULE.ModuleId]
  ResourceName nvarchar
  ActionCode nvarchar
  Name nvarchar
}

Table PROFILE {
  ProfileId uniqueidentifier [pk]
  TenantId uniqueidentifier [ref: > TENANT.TenantId]
  UserId uniqueidentifier [ref: > USER.UserId]
  RoleId uniqueidentifier
  BranchId uniqueidentifier
  DisplayName nvarchar
}

Table PROFILE_PERMISSION {
  ProfileId uniqueidentifier [pk, ref: > PROFILE.ProfileId]
  TemplateId uniqueidentifier [pk, ref: > PERMISSION_TEMPLATE.TemplateId]
  IsAllowed bit
  IsDenied bit
  IsActive bit
}

Table ROLE {
  RoleId uniqueidentifier [pk]
  SuiteId uniqueidentifier [ref: > SYSTEM_SUITE.SuiteId]
  Name nvarchar
}

Table ROLE_PERMISSION {
  RoleId uniqueidentifier [ref: > ROLE.RoleId]
  TemplateId uniqueidentifier [ref: > PERMISSION_TEMPLATE.TemplateId]
}

Table BRANCH {
  BranchId uniqueidentifier [pk]
  TenantId uniqueidentifier [ref: > TENANT.TenantId]
  Name nvarchar
}

Table MENU_ITEM {
  MenuItemId uniqueidentifier [pk]
  ModuleId uniqueidentifier [ref: > FUNCTIONAL_MODULE.ModuleId]
  ParentItemId uniqueidentifier [ref: > MENU_ITEM.MenuItemId]
  Name nvarchar
}

Table AUDIT_LOG {
  LogId bigint [pk]
  TenantId uniqueidentifier
  CorrelationId uniqueidentifier
  TransactionId uniqueidentifier
  Action nvarchar
  Timestamp datetimeoffset
}
```

---

## 2. SQL DDL (SQL Server 2022)
Import this script into **DBeaver**, **SQL Server Management Studio (SSMS)**, or **DataGrip** to generate a native diagram.

```sql
-- UMS Master-Template Schema (Simplified for E/R Tools)

CREATE TABLE TENANT (
    TenantId UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Code NVARCHAR(50) UNIQUE
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
    UserId UNIQUEIDENTIFIER,
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

---

## 3. D2 (Declarative Diagramming)
Copy this to [play.d2lang.com](https://play.d2lang.com/) for a modern, high-quality SVG layout.

```d2
direction: right
TENANT: {
  shape: sql_table
  TenantId: uniqueidentifier {near: top}
  Name: nvarchar
}

SYSTEM_SUITE: {
  shape: sql_table
  SuiteId: uniqueidentifier
  TenantId: uniqueidentifier
}

FUNCTIONAL_MODULE: {
  shape: sql_table
  ModuleId: uniqueidentifier
  SuiteId: uniqueidentifier
}

PERMISSION_TEMPLATE: {
  shape: sql_table
  TemplateId: uniqueidentifier
  ModuleId: uniqueidentifier
  ActionCode: nvarchar
}

PROFILE_PERMISSION: {
  shape: sql_table
  ProfileId: uniqueidentifier
  TemplateId: uniqueidentifier
}

TENANT -> SYSTEM_SUITE: "owns"
SYSTEM_SUITE -> FUNCTIONAL_MODULE: "contains"
FUNCTIONAL_MODULE -> PERMISSION_TEMPLATE: "defines"
PERMISSION_TEMPLATE -> PROFILE_PERMISSION: "materializes"
```
