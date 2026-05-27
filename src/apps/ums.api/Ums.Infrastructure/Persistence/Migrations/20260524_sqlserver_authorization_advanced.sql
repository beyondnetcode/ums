IF SCHEMA_ID(N'ums_authorization') IS NULL
    EXEC('CREATE SCHEMA [ums_authorization]');
GO

IF OBJECT_ID(N'[ums_authorization].[SystemSuites]', N'U') IS NULL
BEGIN
    CREATE TABLE [ums_authorization].[SystemSuites]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [Code] NVARCHAR(100) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NOT NULL,
        [StatusId] INT NOT NULL,
        [CreatedBy] NVARCHAR(100) NOT NULL,
        [CreatedAtUtc] DATETIME2 NOT NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        [UpdatedAtUtc] DATETIME2 NULL,
        [AuditTimeSpan] NVARCHAR(100) NOT NULL,
        [RowVersion] ROWVERSION NOT NULL
    );

    CREATE UNIQUE INDEX [UX_SystemSuites_Tenant_Code]
        ON [ums_authorization].[SystemSuites]([TenantId], [Code]);
END
GO

IF OBJECT_ID(N'[ums_authorization].[SystemSuiteModules]', N'U') IS NULL
BEGIN
    CREATE TABLE [ums_authorization].[SystemSuiteModules]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [SystemSuiteId] UNIQUEIDENTIFIER NOT NULL,
        [Code] NVARCHAR(100) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NOT NULL,
        [StatusId] INT NOT NULL,
        [SortOrder] INT NOT NULL,
        [CreatedBy] NVARCHAR(100) NOT NULL,
        [CreatedAtUtc] DATETIME2 NOT NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        [UpdatedAtUtc] DATETIME2 NULL,
        [AuditTimeSpan] NVARCHAR(100) NOT NULL,
        CONSTRAINT [FK_SystemSuiteModules_SystemSuites]
            FOREIGN KEY ([SystemSuiteId]) REFERENCES [ums_authorization].[SystemSuites]([Id]) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX [UX_SystemSuiteModules_Suite_Code]
        ON [ums_authorization].[SystemSuiteModules]([SystemSuiteId], [Code]);
END
GO

IF OBJECT_ID(N'[ums_authorization].[SystemSuiteMenus]', N'U') IS NULL
BEGIN
    CREATE TABLE [ums_authorization].[SystemSuiteMenus]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [ModuleId] UNIQUEIDENTIFIER NOT NULL,
        [Code] NVARCHAR(100) NOT NULL,
        [Label] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NOT NULL,
        [SortOrder] INT NOT NULL,
        [CreatedBy] NVARCHAR(100) NOT NULL,
        [CreatedAtUtc] DATETIME2 NOT NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        [UpdatedAtUtc] DATETIME2 NULL,
        [AuditTimeSpan] NVARCHAR(100) NOT NULL,
        CONSTRAINT [FK_SystemSuiteMenus_Modules]
            FOREIGN KEY ([ModuleId]) REFERENCES [ums_authorization].[SystemSuiteModules]([Id]) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX [UX_SystemSuiteMenus_Module_Code]
        ON [ums_authorization].[SystemSuiteMenus]([ModuleId], [Code]);
END
GO

IF OBJECT_ID(N'[ums_authorization].[SystemSuiteSubMenus]', N'U') IS NULL
BEGIN
    CREATE TABLE [ums_authorization].[SystemSuiteSubMenus]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [MenuId] UNIQUEIDENTIFIER NOT NULL,
        [Code] NVARCHAR(100) NOT NULL,
        [Label] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NOT NULL,
        [SortOrder] INT NOT NULL,
        [CreatedBy] NVARCHAR(100) NOT NULL,
        [CreatedAtUtc] DATETIME2 NOT NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        [UpdatedAtUtc] DATETIME2 NULL,
        [AuditTimeSpan] NVARCHAR(100) NOT NULL,
        CONSTRAINT [FK_SystemSuiteSubMenus_Menus]
            FOREIGN KEY ([MenuId]) REFERENCES [ums_authorization].[SystemSuiteMenus]([Id]) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX [UX_SystemSuiteSubMenus_Menu_Code]
        ON [ums_authorization].[SystemSuiteSubMenus]([MenuId], [Code]);
END
GO

IF OBJECT_ID(N'[ums_authorization].[SystemSuiteOptions]', N'U') IS NULL
BEGIN
    CREATE TABLE [ums_authorization].[SystemSuiteOptions]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [SubMenuId] UNIQUEIDENTIFIER NOT NULL,
        [Code] NVARCHAR(100) NOT NULL,
        [Label] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NOT NULL,
        [ActionCode] NVARCHAR(100) NOT NULL,
        [SortOrder] INT NOT NULL,
        [CreatedBy] NVARCHAR(100) NOT NULL,
        [CreatedAtUtc] DATETIME2 NOT NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        [UpdatedAtUtc] DATETIME2 NULL,
        [AuditTimeSpan] NVARCHAR(100) NOT NULL,
        CONSTRAINT [FK_SystemSuiteOptions_SubMenus]
            FOREIGN KEY ([SubMenuId]) REFERENCES [ums_authorization].[SystemSuiteSubMenus]([Id]) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX [UX_SystemSuiteOptions_SubMenu_Code]
        ON [ums_authorization].[SystemSuiteOptions]([SubMenuId], [Code]);
END
GO

IF OBJECT_ID(N'[ums_authorization].[SystemSuiteAppSettings]', N'U') IS NULL
BEGIN
    CREATE TABLE [ums_authorization].[SystemSuiteAppSettings]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [SystemSuiteId] UNIQUEIDENTIFIER NOT NULL,
        [ConfigKey] NVARCHAR(100) NOT NULL,
        [ConfigValue] NVARCHAR(4000) NOT NULL,
        [ScopeId] INT NOT NULL,
        CONSTRAINT [FK_SystemSuiteAppSettings_SystemSuites]
            FOREIGN KEY ([SystemSuiteId]) REFERENCES [ums_authorization].[SystemSuites]([Id]) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX [UX_SystemSuiteAppSettings_Suite_Key_Scope]
        ON [ums_authorization].[SystemSuiteAppSettings]([SystemSuiteId], [ConfigKey], [ScopeId]);
END
GO

IF OBJECT_ID(N'[ums_authorization].[SystemSuiteActions]', N'U') IS NULL
BEGIN
    CREATE TABLE [ums_authorization].[SystemSuiteActions]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SystemSuiteId] UNIQUEIDENTIFIER NOT NULL,
        [ModuleId] UNIQUEIDENTIFIER NULL,
        [Code] NVARCHAR(100) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [CreatedBy] NVARCHAR(100) NOT NULL,
        [CreatedAtUtc] DATETIME2 NOT NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        [UpdatedAtUtc] DATETIME2 NULL,
        [AuditTimeSpan] NVARCHAR(100) NOT NULL,
        CONSTRAINT [FK_SystemSuiteActions_SystemSuites]
            FOREIGN KEY ([SystemSuiteId]) REFERENCES [ums_authorization].[SystemSuites]([Id]) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX [UX_SystemSuiteActions_Suite_Code]
        ON [ums_authorization].[SystemSuiteActions]([SystemSuiteId], [Code]);
END
GO

IF OBJECT_ID(N'[ums_authorization].[PermissionTemplates]', N'U') IS NULL
BEGIN
    CREATE TABLE [ums_authorization].[PermissionTemplates]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [RoleId] UNIQUEIDENTIFIER NOT NULL,
        [SystemSuiteId] UNIQUEIDENTIFIER NOT NULL,
        [Version] NVARCHAR(50) NOT NULL,
        [StatusId] INT NOT NULL,
        [CreatedBy] NVARCHAR(100) NOT NULL,
        [CreatedAtUtc] DATETIME2 NOT NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        [UpdatedAtUtc] DATETIME2 NULL,
        [AuditTimeSpan] NVARCHAR(100) NOT NULL,
        [RowVersion] ROWVERSION NOT NULL,
        CONSTRAINT [FK_PermissionTemplates_SystemSuites]
            FOREIGN KEY ([SystemSuiteId]) REFERENCES [ums_authorization].[SystemSuites]([Id])
    );

    CREATE UNIQUE INDEX [UX_PermissionTemplates_Tenant_Role_Suite_Version]
        ON [ums_authorization].[PermissionTemplates]([TenantId], [RoleId], [SystemSuiteId], [Version]);
END
GO

IF OBJECT_ID(N'[ums_authorization].[PermissionTemplateItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [ums_authorization].[PermissionTemplateItems]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TemplateId] UNIQUEIDENTIFIER NOT NULL,
        [TargetTypeId] INT NOT NULL,
        [TargetId] UNIQUEIDENTIFIER NOT NULL,
        [ActionId] UNIQUEIDENTIFIER NOT NULL,
        [IsAllowed] BIT NOT NULL,
        [IsDenied] BIT NOT NULL,
        [IsActive] BIT NOT NULL,
        [CreatedBy] NVARCHAR(100) NOT NULL,
        [CreatedAtUtc] DATETIME2 NOT NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        [UpdatedAtUtc] DATETIME2 NULL,
        [AuditTimeSpan] NVARCHAR(100) NOT NULL,
        CONSTRAINT [FK_PermissionTemplateItems_Templates]
            FOREIGN KEY ([TemplateId]) REFERENCES [ums_authorization].[PermissionTemplates]([Id]) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX [UX_PermissionTemplateItems_Target_Action]
        ON [ums_authorization].[PermissionTemplateItems]([TemplateId], [TargetTypeId], [TargetId], [ActionId]);
END
GO
