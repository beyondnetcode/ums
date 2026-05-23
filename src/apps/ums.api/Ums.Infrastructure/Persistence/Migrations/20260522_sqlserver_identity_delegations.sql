-- ============================================================
-- Migration: UserManagementDelegations table
-- Date:      2026-05-22
-- Context:   Identity BC — FS-14 Delegated Administration
-- Schema:    ums_identity
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ums_identity')
BEGIN
    EXEC('CREATE SCHEMA [ums_identity]');
END
GO

IF OBJECT_ID('[ums_identity].[UserManagementDelegations]', 'U') IS NULL
BEGIN
    CREATE TABLE [ums_identity].[UserManagementDelegations]
    (
        -- Identity
        [Id]                 uniqueidentifier NOT NULL,
        [TenantId]           uniqueidentifier NOT NULL,

        -- Participants
        [DelegatingAdminId]  uniqueidentifier NOT NULL,
        [DelegatedAdminId]   uniqueidentifier NOT NULL,

        -- Scope
        [ScopeTypeId]        int              NOT NULL,
        [ScopeId]            uniqueidentifier NULL,

        -- Permissions
        [AllowedActionsJson] nvarchar(500)    NOT NULL,

        -- Validity window
        [ValidFrom]          datetimeoffset   NOT NULL,
        [ValidUntil]         datetimeoffset   NOT NULL,
        [MaxDurationDays]    int              NULL,

        -- Approval
        [RequiresApproval]   bit              NOT NULL DEFAULT 0,
        [ApprovalRequestId]  uniqueidentifier NULL,

        -- Lifecycle
        [StatusId]           int              NOT NULL,
        [RevokedAt]          datetimeoffset   NULL,
        [RevokedBy]          uniqueidentifier NULL,
        [RevocationReason]   nvarchar(500)    NULL,

        -- Audit
        [CreatedBy]          nvarchar(100)    NOT NULL,
        [CreatedAtUtc]       datetime2        NOT NULL,
        [UpdatedBy]          nvarchar(100)    NULL,
        [UpdatedAtUtc]       datetime2        NULL,
        [AuditTimeSpan]      nvarchar(100)    NOT NULL,

        CONSTRAINT [PK_UserManagementDelegations] PRIMARY KEY ([Id]),

        -- INV-DEL2: delegating admin cannot delegate to themselves
        CONSTRAINT [CK_UserManagementDelegations_NoSelfDelegation]
            CHECK ([DelegatingAdminId] <> [DelegatedAdminId]),

        -- INV-DEL3: validity window must be positive
        CONSTRAINT [CK_UserManagementDelegations_ValidWindow]
            CHECK ([ValidUntil] > [ValidFrom])
    );

    -- Hot-path: find active delegations for a given delegated admin (scope validation)
    CREATE INDEX [IX_UserManagementDelegations_DelegatedAdminId_Status]
        ON [ums_identity].[UserManagementDelegations] ([DelegatedAdminId], [StatusId])
        INCLUDE ([TenantId], [AllowedActionsJson], [ScopeTypeId], [ScopeId], [ValidFrom], [ValidUntil]);

    -- Dashboard: load all delegations a given admin has granted
    CREATE INDEX [IX_UserManagementDelegations_DelegatingAdminId]
        ON [ums_identity].[UserManagementDelegations] ([DelegatingAdminId]);

    -- Tenant-scoped listing (admin portal)
    CREATE INDEX [IX_UserManagementDelegations_TenantId_StatusId]
        ON [ums_identity].[UserManagementDelegations] ([TenantId], [StatusId]);

    -- Background expiry worker: find active delegations past their ValidUntil
    CREATE INDEX [IX_UserManagementDelegations_Active_ValidUntil]
        ON [ums_identity].[UserManagementDelegations] ([StatusId], [ValidUntil])
        WHERE [StatusId] = 1; -- 1 = Active
END
GO
