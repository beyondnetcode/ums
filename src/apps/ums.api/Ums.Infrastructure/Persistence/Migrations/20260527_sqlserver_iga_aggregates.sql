-- =========================================================================
-- Migration: UMS IGA Bounded Context Tables
-- Date: 2026-05-27
-- Bounded Context: IGA (Identity Governance & Administration)
-- Schema: iga
-- =========================================================================

IF SCHEMA_ID('iga') IS NULL
BEGIN
    EXEC('CREATE SCHEMA iga;');
END
GO

-- 1. iga.PromotionRequests Table
IF OBJECT_ID('iga.PromotionRequests', 'U') IS NULL
BEGIN
    CREATE TABLE iga.PromotionRequests (
        Id                      UNIQUEIDENTIFIER NOT NULL,
        TenantId                UNIQUEIDENTIFIER NOT NULL,
        UserId                  UNIQUEIDENTIFIER NOT NULL,
        CurrentRoleId           UNIQUEIDENTIFIER NOT NULL,
        TargetRoleId            UNIQUEIDENTIFIER NOT NULL,
        RequestedAt             DATETIME2        NOT NULL,
        RequestedBy             NVARCHAR(100)    NOT NULL,
        RequestReason           NVARCHAR(1000)   NULL,
        ManagerId               UNIQUEIDENTIFIER NOT NULL,
        ManagerApprovalStatusId INT              NOT NULL,
        ManagerDecisionAt       DATETIME2        NULL,
        SecurityApprovalStatusId INT             NOT NULL,
        SecurityDecisionAt      DATETIME2        NULL,
        StatusId                INT              NOT NULL,
        ExecutedAt              DATETIME2        NULL,
        ExecutedBy              NVARCHAR(100)    NULL,
        VerifiedAt              DATETIME2        NULL,
        CreatedBy               NVARCHAR(100)    NOT NULL,
        CreatedAtUtc            DATETIME2        NOT NULL,
        UpdatedBy               NVARCHAR(100)    NULL,
        UpdatedAtUtc            DATETIME2        NULL,
        AuditTimeSpan           NVARCHAR(100)    NOT NULL,
        RowVersion              ROWVERSION       NOT NULL,
        CONSTRAINT PK_PromotionRequests PRIMARY KEY (Id)
    );

    CREATE INDEX IX_PromotionRequests_TenantId   ON iga.PromotionRequests (TenantId);
    CREATE INDEX IX_PromotionRequests_UserId      ON iga.PromotionRequests (UserId);
    CREATE INDEX IX_PromotionRequests_ManagerId   ON iga.PromotionRequests (ManagerId);
    CREATE INDEX IX_PromotionRequests_TenantId_StatusId ON iga.PromotionRequests (TenantId, StatusId);
END
GO

-- 2. iga.PromotionImpactAnalyses Table
IF OBJECT_ID('iga.PromotionImpactAnalyses', 'U') IS NULL
BEGIN
    CREATE TABLE iga.PromotionImpactAnalyses (
        Id                      UNIQUEIDENTIFIER NOT NULL,
        PromotionRequestId      UNIQUEIDENTIFIER NOT NULL,
        RiskScore               DECIMAL(5, 2)    NOT NULL,
        RiskLevel               NVARCHAR(50)     NOT NULL,
        NewPermissionsCount     INT              NOT NULL,
        RemovedPermissionsCount INT              NOT NULL,
        AffectedSystemsCount    INT              NOT NULL,
        ConflictingPermissions  NVARCHAR(2000)   NULL,
        RiskFactors             NVARCHAR(2000)   NULL,
        SuggestedMitigations    NVARCHAR(2000)   NULL,
        AnalyzedAt              DATETIME2        NOT NULL,
        AnalyzedBy              NVARCHAR(100)    NULL,
        CreatedBy               NVARCHAR(100)    NOT NULL,
        CreatedAtUtc            DATETIME2        NOT NULL,
        UpdatedBy               NVARCHAR(100)    NULL,
        UpdatedAtUtc            DATETIME2        NULL,
        AuditTimeSpan           NVARCHAR(100)    NOT NULL,
        CONSTRAINT PK_PromotionImpactAnalyses PRIMARY KEY (Id),
        CONSTRAINT FK_PromotionImpactAnalyses_PromotionRequests
            FOREIGN KEY (PromotionRequestId)
            REFERENCES iga.PromotionRequests (Id)
            ON DELETE CASCADE
    );

    CREATE INDEX IX_PromotionImpactAnalyses_PromotionRequestId
        ON iga.PromotionImpactAnalyses (PromotionRequestId);
END
GO

-- 3. iga.RoleMaturityStatuses Table
IF OBJECT_ID('iga.RoleMaturityStatuses', 'U') IS NULL
BEGIN
    CREATE TABLE iga.RoleMaturityStatuses (
        Id                          UNIQUEIDENTIFIER NOT NULL,
        TenantId                    UNIQUEIDENTIFIER NOT NULL,
        UserId                      UNIQUEIDENTIFIER NOT NULL,
        RoleId                      UNIQUEIDENTIFIER NOT NULL,
        CurrentMaturityLevelId      INT              NOT NULL,
        NextEligibleMaturityLevelId INT              NULL,
        AssignedAt                  DATETIME2        NOT NULL,
        CurrentLevelSince           DATETIME2        NOT NULL,
        EligibleForPromotionAt      DATETIME2        NULL,
        CompletedCertificationsCount INT             NOT NULL DEFAULT 0,
        CompletedTrainingsCount     INT              NOT NULL DEFAULT 0,
        PerformanceScore            DECIMAL(5, 2)    NOT NULL DEFAULT 0,
        HasNoComplianceIssues       BIT              NOT NULL DEFAULT 1,
        BlockingFactor              NVARCHAR(500)    NULL,
        LastReviewedAt              DATETIME2        NULL,
        CreatedBy                   NVARCHAR(100)    NOT NULL,
        CreatedAtUtc                DATETIME2        NOT NULL,
        UpdatedBy                   NVARCHAR(100)    NULL,
        UpdatedAtUtc                DATETIME2        NULL,
        AuditTimeSpan               NVARCHAR(100)    NOT NULL,
        RowVersion                  ROWVERSION       NOT NULL,
        CONSTRAINT PK_RoleMaturityStatuses PRIMARY KEY (Id)
    );

    CREATE INDEX IX_RoleMaturityStatuses_TenantId ON iga.RoleMaturityStatuses (TenantId);
    CREATE INDEX IX_RoleMaturityStatuses_UserId   ON iga.RoleMaturityStatuses (UserId);
    CREATE UNIQUE INDEX UK_RoleMaturityStatuses_UserId_RoleId
        ON iga.RoleMaturityStatuses (UserId, RoleId);
END
GO
