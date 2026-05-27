IF SCHEMA_ID(N'ums_authorization') IS NULL
    EXEC('CREATE SCHEMA [ums_authorization]');
GO

IF OBJECT_ID(N'[ums_authorization].[Roles]', N'U') IS NULL
BEGIN
    CREATE TABLE [ums_authorization].[Roles]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SystemSuiteId] UNIQUEIDENTIFIER NOT NULL,
        [ParentRoleId] UNIQUEIDENTIFIER NULL,
        [Code] NVARCHAR(50) NOT NULL,
        [Value] NVARCHAR(150) NOT NULL,
        [Description] NVARCHAR(500) NOT NULL,
        [HierarchyLevel] INT NOT NULL,
        [PromotionOrder] INT NOT NULL,
        [IsActive] BIT NOT NULL,
        [CreatedBy] NVARCHAR(100) NOT NULL,
        [CreatedAtUtc] DATETIME2 NOT NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        [UpdatedAtUtc] DATETIME2 NULL,
        [AuditTimeSpan] NVARCHAR(100) NOT NULL,
        [RowVersion] ROWVERSION NOT NULL,
        CONSTRAINT [FK_Roles_SystemSuites]
            FOREIGN KEY ([SystemSuiteId]) REFERENCES [ums_authorization].[SystemSuites]([Id]),
        CONSTRAINT [FK_Roles_ParentRole]
            FOREIGN KEY ([ParentRoleId]) REFERENCES [ums_authorization].[Roles]([Id]),
        CONSTRAINT [CK_Roles_HierarchyLevel] CHECK ([HierarchyLevel] >= 0),
        CONSTRAINT [CK_Roles_PromotionOrder] CHECK ([PromotionOrder] >= 0)
    );

    CREATE UNIQUE INDEX [UX_Roles_SystemSuite_Code]
        ON [ums_authorization].[Roles]([SystemSuiteId], [Code]);
    CREATE INDEX [IX_Roles_TenantId]
        ON [ums_authorization].[Roles]([TenantId]);
    CREATE INDEX [IX_Roles_ParentRoleId]
        ON [ums_authorization].[Roles]([ParentRoleId]);
END
GO
