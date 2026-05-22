IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ums_identity')
BEGIN
    EXEC('CREATE SCHEMA [ums_identity]');
END
GO

IF OBJECT_ID('[ums_identity].[Tenants]', 'U') IS NULL
BEGIN
    CREATE TABLE [ums_identity].[Tenants]
    (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(100) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [OrganizationTypeId] int NOT NULL,
        [IdpStrategyId] int NOT NULL,
        [CompanyReference] nvarchar(100) NULL,
        [ParentTenantId] uniqueidentifier NULL,
        [StatusId] int NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [AuditTimeSpan] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Tenants] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [IX_Tenants_Code] ON [ums_identity].[Tenants]([Code]);
    CREATE INDEX [IX_Tenants_ParentTenantId] ON [ums_identity].[Tenants]([ParentTenantId]);
END
GO

IF OBJECT_ID('[ums_identity].[TenantBranches]', 'U') IS NULL
BEGIN
    CREATE TABLE [ums_identity].[TenantBranches]
    (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NOT NULL,
        [Code] nvarchar(100) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [GeofencingMetadata] nvarchar(4000) NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [AuditTimeSpan] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_TenantBranches] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TenantBranches_Tenants] FOREIGN KEY ([TenantId]) REFERENCES [ums_identity].[Tenants]([Id]) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX [IX_TenantBranches_TenantId_Code] ON [ums_identity].[TenantBranches]([TenantId], [Code]);
END
GO

IF OBJECT_ID('[ums_identity].[TenantIdentityProviders]', 'U') IS NULL
BEGIN
    CREATE TABLE [ums_identity].[TenantIdentityProviders]
    (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NOT NULL,
        [Code] nvarchar(100) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NOT NULL,
        [StrategyId] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [AuditTimeSpan] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_TenantIdentityProviders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TenantIdentityProviders_Tenants] FOREIGN KEY ([TenantId]) REFERENCES [ums_identity].[Tenants]([Id]) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX [IX_TenantIdentityProviders_TenantId_Code] ON [ums_identity].[TenantIdentityProviders]([TenantId], [Code]);
END
GO

IF OBJECT_ID('[ums_identity].[TenantBrandings]', 'U') IS NULL
BEGIN
    CREATE TABLE [ums_identity].[TenantBrandings]
    (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NOT NULL,
        [Logo] nvarchar(4000) NOT NULL,
        [LogoFormatId] int NOT NULL,
        [PrimaryColor] nvarchar(20) NOT NULL,
        [BackgroundStyleId] int NOT NULL,
        [HeadlineText] nvarchar(200) NOT NULL,
        [SecondaryText] nvarchar(500) NOT NULL,
        [PrimaryButtonLabel] nvarchar(100) NOT NULL,
        [FooterText] nvarchar(200) NOT NULL,
        [CustomDomain] nvarchar(255) NULL,
        [DnsVerificationStatusId] int NOT NULL,
        [DnsCnameTarget] nvarchar(255) NOT NULL,
        [MagicLinkFallbackEnabled] bit NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [AuditTimeSpan] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_TenantBrandings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TenantBrandings_Tenants] FOREIGN KEY ([TenantId]) REFERENCES [ums_identity].[Tenants]([Id]) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX [IX_TenantBrandings_TenantId] ON [ums_identity].[TenantBrandings]([TenantId]);
    CREATE UNIQUE INDEX [IX_TenantBrandings_CustomDomain] ON [ums_identity].[TenantBrandings]([CustomDomain]) WHERE [CustomDomain] IS NOT NULL;
END
GO

IF OBJECT_ID('[ums_identity].[UserAccounts]', 'U') IS NULL
BEGIN
    CREATE TABLE [ums_identity].[UserAccounts]
    (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NOT NULL,
        [BranchId] uniqueidentifier NULL,
        [Email] nvarchar(255) NOT NULL,
        [CategoryId] int NOT NULL,
        [StatusId] int NOT NULL,
        [IdentityReference] nvarchar(255) NULL,
        [IdentityReferenceTypeId] int NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [AuditTimeSpan] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_UserAccounts] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [IX_UserAccounts_TenantId_Email] ON [ums_identity].[UserAccounts]([TenantId], [Email]);
    CREATE INDEX [IX_UserAccounts_Email] ON [ums_identity].[UserAccounts]([Email]);
    CREATE INDEX [IX_UserAccounts_TenantId] ON [ums_identity].[UserAccounts]([TenantId]);
END
GO

IF OBJECT_ID('[ums_identity].[UserAccountMfaEnrollments]', 'U') IS NULL
BEGIN
    CREATE TABLE [ums_identity].[UserAccountMfaEnrollments]
    (
        [Id] uniqueidentifier NOT NULL,
        [UserAccountId] uniqueidentifier NOT NULL,
        [MethodId] int NOT NULL,
        [StatusId] int NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [AuditTimeSpan] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_UserAccountMfaEnrollments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserAccountMfaEnrollments_UserAccounts] FOREIGN KEY ([UserAccountId]) REFERENCES [ums_identity].[UserAccounts]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_UserAccountMfaEnrollments_UserAccountId_MethodId] ON [ums_identity].[UserAccountMfaEnrollments]([UserAccountId], [MethodId]);
END
GO

IF OBJECT_ID('[ums_identity].[UserAccountPasswordCredentials]', 'U') IS NULL
BEGIN
    CREATE TABLE [ums_identity].[UserAccountPasswordCredentials]
    (
        [Id] uniqueidentifier NOT NULL,
        [UserAccountId] uniqueidentifier NOT NULL,
        [PasswordHash] nvarchar(500) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [AuditTimeSpan] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_UserAccountPasswordCredentials] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserAccountPasswordCredentials_UserAccounts] FOREIGN KEY ([UserAccountId]) REFERENCES [ums_identity].[UserAccounts]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_UserAccountPasswordCredentials_UserAccountId] ON [ums_identity].[UserAccountPasswordCredentials]([UserAccountId]);
END
GO
