-- =========================================================================
-- Migration: UMS AuditRecords Table and Immutability Trigger
-- Date: 2026-05-23
-- Bounded Context: Audit Bounded Context
-- Schema: audit
-- =========================================================================

IF SCHEMA_ID('audit') IS NULL 
BEGIN
    EXEC('CREATE SCHEMA audit;');
END
GO

IF OBJECT_ID('audit.AuditRecords', 'U') IS NULL
BEGIN
    CREATE TABLE audit.AuditRecords (
        Id UNIQUEIDENTIFIER NOT NULL,
        WhoActed UNIQUEIDENTIFIER NOT NULL,
        SubjectTypeId INT NOT NULL,
        WhenOccurred DATETIME2 NOT NULL,
        WhatChanged NVARCHAR(4000) NOT NULL,
        EventType NVARCHAR(255) NOT NULL,
        AuditResultId INT NOT NULL,
        AffectedEntityId UNIQUEIDENTIFIER NOT NULL,
        AffectedEntityType NVARCHAR(255) NOT NULL,
        RootTenantId UNIQUEIDENTIFIER NOT NULL,
        Metadata NVARCHAR(4000) NULL,
        CreatedBy NVARCHAR(100) NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL,
        UpdatedBy NVARCHAR(100) NULL,
        UpdatedAtUtc DATETIME2 NULL,
        AuditTimeSpan NVARCHAR(100) NOT NULL,
        CONSTRAINT PK_AuditRecords PRIMARY KEY (Id)
    );

    CREATE INDEX IX_AuditRecords_WhoActed ON audit.AuditRecords (WhoActed);
    CREATE INDEX IX_AuditRecords_AffectedEntityId ON audit.AuditRecords (AffectedEntityId);
    CREATE INDEX IX_AuditRecords_RootTenantId ON audit.AuditRecords (RootTenantId);
    CREATE INDEX IX_AuditRecords_EventType ON audit.AuditRecords (EventType);
    CREATE INDEX IX_AuditRecords_Affected ON audit.AuditRecords (AffectedEntityId, AffectedEntityType);
END
GO

-- Enforce Audit Immutability at the SQL Engine Layer
IF OBJECT_ID('audit.trg_audit_records_immutable', 'TR') IS NOT NULL
BEGIN
    DROP TRIGGER audit.trg_audit_records_immutable;
END
GO

CREATE TRIGGER trg_audit_records_immutable
ON audit.AuditRecords
AFTER UPDATE, DELETE
AS
BEGIN
    RAISERROR ('Audit log is immutable. UPDATE and DELETE are prohibited on audit.AuditRecords.', 16, 1);
    ROLLBACK TRANSACTION;
END;
GO
