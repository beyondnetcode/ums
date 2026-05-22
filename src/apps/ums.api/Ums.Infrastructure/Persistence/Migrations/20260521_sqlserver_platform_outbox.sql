IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ums_platform')
BEGIN
    EXEC('CREATE SCHEMA [ums_platform]');
END
GO

IF OBJECT_ID('[ums_platform].[OutboxMessages]', 'U') IS NULL
BEGIN
    CREATE TABLE [ums_platform].[OutboxMessages]
    (
        [Id] uniqueidentifier NOT NULL,
        [AggregateId] uniqueidentifier NOT NULL,
        [AggregateName] nvarchar(200) NOT NULL,
        [EventName] nvarchar(200) NOT NULL,
        [EventType] nvarchar(500) NOT NULL,
        [Payload] nvarchar(max) NOT NULL,
        [TenantId] uniqueidentifier NULL,
        [OccurredOnUtc] datetime2 NOT NULL,
        [ProcessedOnUtc] datetime2 NULL,
        [RetryCount] int NOT NULL CONSTRAINT [DF_OutboxMessages_RetryCount] DEFAULT (0),
        [LastError] nvarchar(4000) NULL,
        CONSTRAINT [PK_OutboxMessages] PRIMARY KEY ([Id])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OutboxMessages_Dispatch' AND object_id = OBJECT_ID('[ums_platform].[OutboxMessages]'))
BEGIN
    CREATE INDEX [IX_OutboxMessages_Dispatch]
        ON [ums_platform].[OutboxMessages] ([ProcessedOnUtc], [OccurredOnUtc]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OutboxMessages_TenantId' AND object_id = OBJECT_ID('[ums_platform].[OutboxMessages]'))
BEGIN
    CREATE INDEX [IX_OutboxMessages_TenantId]
        ON [ums_platform].[OutboxMessages] ([TenantId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OutboxMessages_Aggregate' AND object_id = OBJECT_ID('[ums_platform].[OutboxMessages]'))
BEGIN
    CREATE INDEX [IX_OutboxMessages_Aggregate]
        ON [ums_platform].[OutboxMessages] ([AggregateName], [AggregateId]);
END
GO
