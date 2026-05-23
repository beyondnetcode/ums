IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ums_configuration')
BEGIN
    EXEC('CREATE SCHEMA [ums_configuration]');
END
GO

IF OBJECT_ID('[ums_configuration].[AppConfigurations]', 'U') IS NULL
BEGIN
    CREATE TABLE [ums_configuration].[AppConfigurations]
    (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [SystemSuiteId] uniqueidentifier NULL,
        [ModuleId] uniqueidentifier NULL,
        [Code] nvarchar(100) NOT NULL,
        [Value] nvarchar(4000) NOT NULL,
        [Description] nvarchar(1000) NOT NULL,
        [ScopeId] int NOT NULL,
        [IsInheritable] bit NOT NULL,
        [IsEncrypted] bit NOT NULL,
        [Version] nvarchar(50) NOT NULL,
        [StatusId] int NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [AuditTimeSpan] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_AppConfigurations] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [IX_AppConfigurations_Scope_Code]
        ON [ums_configuration].[AppConfigurations]([TenantId], [SystemSuiteId], [ModuleId], [Code]);
    CREATE INDEX [IX_AppConfigurations_ScopeId] ON [ums_configuration].[AppConfigurations]([ScopeId]);
    CREATE INDEX [IX_AppConfigurations_StatusId] ON [ums_configuration].[AppConfigurations]([StatusId]);
END
GO

IF OBJECT_ID('[ums_configuration].[FeatureFlags]', 'U') IS NULL
BEGIN
    CREATE TABLE [ums_configuration].[FeatureFlags]
    (
        [Id] uniqueidentifier NOT NULL,
        [FlagCode] nvarchar(100) NOT NULL,
        [FlagTypeId] int NOT NULL,
        [FlagTargets] nvarchar(2000) NOT NULL,
        [StatusId] int NOT NULL,
        [LinkedResourceTypeId] int NULL,
        [LinkedResourceId] uniqueidentifier NULL,
        [RolloutPercentage] int NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [AuditTimeSpan] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_FeatureFlags] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [IX_FeatureFlags_FlagCode] ON [ums_configuration].[FeatureFlags]([FlagCode]);
    CREATE INDEX [IX_FeatureFlags_StatusId] ON [ums_configuration].[FeatureFlags]([StatusId]);
    CREATE INDEX [IX_FeatureFlags_FlagTypeId] ON [ums_configuration].[FeatureFlags]([FlagTypeId]);
END
GO

IF OBJECT_ID('[ums_configuration].[FeatureFlagEvaluationLogs]', 'U') IS NULL
BEGIN
    CREATE TABLE [ums_configuration].[FeatureFlagEvaluationLogs]
    (
        [Id] uniqueidentifier NOT NULL,
        [FeatureFlagId] uniqueidentifier NOT NULL,
        [EvaluatedBy] uniqueidentifier NOT NULL,
        [Result] bit NOT NULL,
        [Context] nvarchar(1000) NOT NULL,
        [EvaluatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_FeatureFlagEvaluationLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FeatureFlagEvaluationLogs_FeatureFlags] FOREIGN KEY ([FeatureFlagId]) REFERENCES [ums_configuration].[FeatureFlags]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_FeatureFlagEvaluationLogs_FeatureFlagId] ON [ums_configuration].[FeatureFlagEvaluationLogs]([FeatureFlagId]);
    CREATE INDEX [IX_FeatureFlagEvaluationLogs_EvaluatedAtUtc] ON [ums_configuration].[FeatureFlagEvaluationLogs]([EvaluatedAtUtc]);
END
GO
