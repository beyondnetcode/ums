IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ums_authorization')
BEGIN
    EXEC('CREATE SCHEMA [ums_authorization]');
END
GO

IF OBJECT_ID('[ums_authorization].[Profiles]', 'U') IS NULL
BEGIN
    CREATE TABLE [ums_authorization].[Profiles]
    (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        [BranchId] uniqueidentifier NULL,
        [ScopeId] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [AuditTimeSpan] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Profiles] PRIMARY KEY ([Id])
    );
    CREATE INDEX [IX_Profiles_TenantId] ON [ums_authorization].[Profiles]([TenantId]);
    CREATE INDEX [IX_Profiles_UserId] ON [ums_authorization].[Profiles]([UserId]);
    CREATE INDEX [IX_Profiles_TenantId_UserId_RoleId_BranchId] ON [ums_authorization].[Profiles]([TenantId], [UserId], [RoleId], [BranchId]);
END
GO

IF OBJECT_ID('[ums_authorization].[ProfilePermissions]', 'U') IS NULL
BEGIN
    CREATE TABLE [ums_authorization].[ProfilePermissions]
    (
        [Id] uniqueidentifier NOT NULL,
        [ProfileId] uniqueidentifier NOT NULL,
        [TemplateId] uniqueidentifier NOT NULL,
        [TargetTypeId] int NOT NULL,
        [TargetId] uniqueidentifier NOT NULL,
        [ActionId] uniqueidentifier NOT NULL,
        [IsAllowed] bit NOT NULL,
        [IsDenied] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [IsOverride] bit NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [AuditTimeSpan] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_ProfilePermissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProfilePermissions_Profiles] FOREIGN KEY ([ProfileId]) REFERENCES [ums_authorization].[Profiles]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_ProfilePermissions_ProfileId] ON [ums_authorization].[ProfilePermissions]([ProfileId]);
    CREATE INDEX [IX_ProfilePermissions_ProfileId_TemplateId_ActionId_TargetId] ON [ums_authorization].[ProfilePermissions]([ProfileId], [TemplateId], [ActionId], [TargetId]);
END
GO
