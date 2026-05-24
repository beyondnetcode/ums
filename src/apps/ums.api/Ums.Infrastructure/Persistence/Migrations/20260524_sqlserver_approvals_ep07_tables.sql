-- ============================================================
-- Migration: EP-07 Approvals compliance tables
-- Adds: approvals.DocumentTypes, approvals.UserDocuments,
--       approvals.UserDocumentNotifications,
--       approvals.AccessEnforcementPolicies
-- Idempotent: all CREATE statements wrapped in IF NOT EXISTS guards
-- ============================================================

IF SCHEMA_ID('approvals') IS NULL
BEGIN
    EXEC('CREATE SCHEMA approvals;');
END
GO

-- ── DocumentTypes ───────────────────────────────────────────
IF OBJECT_ID('approvals.DocumentTypes', 'U') IS NULL
BEGIN
    CREATE TABLE approvals.DocumentTypes (
        Id              UNIQUEIDENTIFIER    NOT NULL,
        TenantId        UNIQUEIDENTIFIER    NOT NULL,
        Code            NVARCHAR(50)        NOT NULL,
        Name            NVARCHAR(150)       NOT NULL,
        Description     NVARCHAR(500)       NOT NULL,
        CriticityId     INT                 NOT NULL,   -- 1=Low,2=Medium,3=High
        CreatedBy       NVARCHAR(100)       NOT NULL,
        CreatedAtUtc    DATETIME2(7)        NOT NULL,
        UpdatedBy       NVARCHAR(100)       NULL,
        UpdatedAtUtc    DATETIME2(7)        NULL,
        AuditTimeSpan   NVARCHAR(100)       NOT NULL,
        RowVersion      ROWVERSION          NOT NULL,
        CONSTRAINT PK_DocumentTypes PRIMARY KEY (Id)
    );

    CREATE INDEX IX_DocumentTypes_TenantId
        ON approvals.DocumentTypes (TenantId);

    CREATE UNIQUE INDEX UX_DocumentTypes_TenantId_Code
        ON approvals.DocumentTypes (TenantId, Code);
END
GO

-- ── UserDocuments ───────────────────────────────────────────
IF OBJECT_ID('approvals.UserDocuments', 'U') IS NULL
BEGIN
    CREATE TABLE approvals.UserDocuments (
        Id               UNIQUEIDENTIFIER    NOT NULL,
        UserId           UNIQUEIDENTIFIER    NOT NULL,
        DocumentTypeId   UNIQUEIDENTIFIER    NOT NULL,
        IssueDate        DATETIME2(7)        NOT NULL,
        ExpirationDate   DATETIME2(7)        NOT NULL,
        StatusId         INT                 NOT NULL,   -- 1=PendingReview,2=Valid,3=Expired,4=Rejected
        CriticityId      INT                 NOT NULL,   -- 1=Low,2=Medium,3=High
        FileStoragePath  NVARCHAR(1000)      NOT NULL,
        FileChecksum     NVARCHAR(128)       NOT NULL,
        NotificationStep INT                 NOT NULL    DEFAULT 0,
        CreatedBy        NVARCHAR(100)       NOT NULL,
        CreatedAtUtc     DATETIME2(7)        NOT NULL,
        UpdatedBy        NVARCHAR(100)       NULL,
        UpdatedAtUtc     DATETIME2(7)        NULL,
        AuditTimeSpan    NVARCHAR(100)       NOT NULL,
        RowVersion       ROWVERSION          NOT NULL,
        CONSTRAINT PK_UserDocuments PRIMARY KEY (Id)
    );

    CREATE INDEX IX_UserDocuments_UserId
        ON approvals.UserDocuments (UserId);

    CREATE INDEX IX_UserDocuments_UserId_DocumentTypeId
        ON approvals.UserDocuments (UserId, DocumentTypeId);

    CREATE INDEX IX_UserDocuments_ExpirationDate
        ON approvals.UserDocuments (ExpirationDate);

    CREATE INDEX IX_UserDocuments_StatusId
        ON approvals.UserDocuments (StatusId);
END
GO

-- ── UserDocumentNotifications ───────────────────────────────
IF OBJECT_ID('approvals.UserDocumentNotifications', 'U') IS NULL
BEGIN
    CREATE TABLE approvals.UserDocumentNotifications (
        Id               UNIQUEIDENTIFIER    NOT NULL,
        UserDocumentId   UNIQUEIDENTIFIER    NOT NULL,
        Step             INT                 NOT NULL,
        ChannelId        INT                 NOT NULL,
        DaysRemaining    INT                 NOT NULL,
        SentAt           DATETIME2(7)        NOT NULL,
        CONSTRAINT PK_UserDocumentNotifications PRIMARY KEY (Id),
        CONSTRAINT FK_UserDocumentNotifications_UserDocuments
            FOREIGN KEY (UserDocumentId)
            REFERENCES approvals.UserDocuments (Id)
            ON DELETE CASCADE
    );

    CREATE INDEX IX_UserDocumentNotifications_UserDocumentId
        ON approvals.UserDocumentNotifications (UserDocumentId);

    CREATE UNIQUE INDEX UX_UserDocumentNotifications_UserDocumentId_Step
        ON approvals.UserDocumentNotifications (UserDocumentId, Step);
END
GO

-- ── AccessEnforcementPolicies ───────────────────────────────
IF OBJECT_ID('approvals.AccessEnforcementPolicies', 'U') IS NULL
BEGIN
    CREATE TABLE approvals.AccessEnforcementPolicies (
        Id                   UNIQUEIDENTIFIER    NOT NULL,
        TenantId             UNIQUEIDENTIFIER    NOT NULL,
        ProfileId            UNIQUEIDENTIFIER    NULL,
        RoleId               UNIQUEIDENTIFIER    NULL,
        EnforcementActionId  INT                 NOT NULL,   -- 1=BlockUser,2=RestrictProfile,3=LogOnly
        IsActive             BIT                 NOT NULL    DEFAULT 1,
        CreatedBy            NVARCHAR(100)       NOT NULL,
        CreatedAtUtc         DATETIME2(7)        NOT NULL,
        UpdatedBy            NVARCHAR(100)       NULL,
        UpdatedAtUtc         DATETIME2(7)        NULL,
        AuditTimeSpan        NVARCHAR(100)       NOT NULL,
        RowVersion           ROWVERSION          NOT NULL,
        CONSTRAINT PK_AccessEnforcementPolicies PRIMARY KEY (Id),
        CONSTRAINT CK_AccessEnforcementPolicies_ProfileOrRole
            CHECK (ProfileId IS NOT NULL OR RoleId IS NOT NULL)
    );

    CREATE INDEX IX_AccessEnforcementPolicies_TenantId
        ON approvals.AccessEnforcementPolicies (TenantId);

    CREATE INDEX IX_AccessEnforcementPolicies_TenantId_ProfileId
        ON approvals.AccessEnforcementPolicies (TenantId, ProfileId)
        WHERE ProfileId IS NOT NULL;

    CREATE INDEX IX_AccessEnforcementPolicies_TenantId_RoleId
        ON approvals.AccessEnforcementPolicies (TenantId, RoleId)
        WHERE RoleId IS NOT NULL;
END
GO
