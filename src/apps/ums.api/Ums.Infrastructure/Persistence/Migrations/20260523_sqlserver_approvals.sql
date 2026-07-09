-- =========================================================================
-- Migration: UMS Approvals Bounded Context Tables
-- Date: 2026-05-23
-- Bounded Context: Approvals Bounded Context
-- Schema: approvals
-- =========================================================================

IF SCHEMA_ID('approvals') IS NULL 
BEGIN
    EXEC('CREATE SCHEMA approvals;');
END
GO

-- 1. approvals.ApprovalWorkflows Table
IF OBJECT_ID('approvals.ApprovalWorkflows', 'U') IS NULL
BEGIN
    CREATE TABLE approvals.ApprovalWorkflows (
        Id UNIQUEIDENTIFIER NOT NULL,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SystemSuiteId UNIQUEIDENTIFIER NULL,
        Code NVARCHAR(50) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(255) NOT NULL,
        TargetUserCategoryId INT NOT NULL,
        RequiresApproval BIT NOT NULL,
        CreatedBy NVARCHAR(100) NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL,
        UpdatedBy NVARCHAR(100) NULL,
        UpdatedAtUtc DATETIME2 NULL,
        AuditTimeSpan NVARCHAR(100) NOT NULL,
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT PK_ApprovalWorkflows PRIMARY KEY (Id)
    );

    CREATE INDEX IX_ApprovalWorkflows_TenantId ON approvals.ApprovalWorkflows (TenantId);
    CREATE INDEX IX_ApprovalWorkflows_Code ON approvals.ApprovalWorkflows (Code);
    CREATE UNIQUE INDEX UK_ApprovalWorkflows_TenantId_Code ON approvals.ApprovalWorkflows (TenantId, Code);
END
GO

-- 2. approvals.ApprovalRequiredDocuments Table
IF OBJECT_ID('approvals.ApprovalRequiredDocuments', 'U') IS NULL
BEGIN
    CREATE TABLE approvals.ApprovalRequiredDocuments (
        Id UNIQUEIDENTIFIER NOT NULL,
        WorkflowId UNIQUEIDENTIFIER NOT NULL,
        DocumentTypeId UNIQUEIDENTIFIER NOT NULL,
        IsMandatory BIT NOT NULL,
        CreatedBy NVARCHAR(100) NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL,
        UpdatedBy NVARCHAR(100) NULL,
        UpdatedAtUtc DATETIME2 NULL,
        AuditTimeSpan NVARCHAR(100) NOT NULL,
        CONSTRAINT PK_ApprovalRequiredDocuments PRIMARY KEY (Id),
        CONSTRAINT FK_ApprovalRequiredDocuments_ApprovalWorkflows FOREIGN KEY (WorkflowId) 
            REFERENCES approvals.ApprovalWorkflows (Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_ApprovalRequiredDocuments_WorkflowId ON approvals.ApprovalRequiredDocuments (WorkflowId);
    CREATE INDEX IX_ApprovalRequiredDocuments_DocumentTypeId ON approvals.ApprovalRequiredDocuments (DocumentTypeId);
    CREATE UNIQUE INDEX UK_ApprovalRequiredDocuments_WorkflowId_DocumentTypeId ON approvals.ApprovalRequiredDocuments (WorkflowId, DocumentTypeId);
END
GO

-- 3. approvals.ApprovalRequests Table
IF OBJECT_ID('approvals.ApprovalRequests', 'U') IS NULL
BEGIN
    CREATE TABLE approvals.ApprovalRequests (
        Id UNIQUEIDENTIFIER NOT NULL,
        WorkflowId UNIQUEIDENTIFIER NOT NULL,
        TargetUserId UNIQUEIDENTIFIER NOT NULL,
        TargetProfileId UNIQUEIDENTIFIER NULL,
        StatusId INT NOT NULL,
        CreatedBy NVARCHAR(100) NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL,
        UpdatedBy NVARCHAR(100) NULL,
        UpdatedAtUtc DATETIME2 NULL,
        AuditTimeSpan NVARCHAR(100) NOT NULL,
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT PK_ApprovalRequests PRIMARY KEY (Id),
        CONSTRAINT FK_ApprovalRequests_ApprovalWorkflows FOREIGN KEY (WorkflowId) 
            REFERENCES approvals.ApprovalWorkflows (Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_ApprovalRequests_WorkflowId ON approvals.ApprovalRequests (WorkflowId);
    CREATE INDEX IX_ApprovalRequests_TargetUserId ON approvals.ApprovalRequests (TargetUserId);
    CREATE INDEX IX_ApprovalRequests_TargetProfileId ON approvals.ApprovalRequests (TargetProfileId);
END
GO

-- 4. approvals.NotificationRules Table
IF OBJECT_ID('approvals.NotificationRules', 'U') IS NULL
BEGIN
    CREATE TABLE approvals.NotificationRules (
        Id UNIQUEIDENTIFIER NOT NULL,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        ChannelId INT NOT NULL,
        Recipient NVARCHAR(255) NOT NULL,
        IsActive BIT NOT NULL,
        CreatedBy NVARCHAR(100) NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL,
        UpdatedBy NVARCHAR(100) NULL,
        UpdatedAtUtc DATETIME2 NULL,
        AuditTimeSpan NVARCHAR(100) NOT NULL,
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT PK_NotificationRules PRIMARY KEY (Id)
    );

    CREATE INDEX IX_NotificationRules_TenantId ON approvals.NotificationRules (TenantId);
END
GO
