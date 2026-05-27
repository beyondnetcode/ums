-- UMS - Standardize schema names to ums_<context> convention
-- Date: 2026-05-27
-- Scope: SQL Server 2022

-- Rename approvals schema to ums_approvals
IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'approvals')
BEGIN
    ALTER SCHEMA [ums_approvals] TRANSFER [approvals].[ApprovalRequests];
    ALTER SCHEMA [ums_approvals] TRANSFER [approvals].[ApprovalActions];
    ALTER SCHEMA [ums_approvals] TRANSFER [approvals].[ApprovalComments];
    ALTER SCHEMA [ums_approvals] TRANSFER [approvals].[ApprovalWorkflows];
    DROP SCHEMA [approvals];
END

-- Rename iga schema to ums_iga
IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'iga')
BEGIN
    ALTER SCHEMA [ums_iga] TRANSFER [iga].[PromotionRequests];
    ALTER SCHEMA [ums_iga] TRANSFER [iga].[PromotionImpactAnalyses];
    ALTER SCHEMA [ums_iga] TRANSFER [iga].[RolePromotionRecords];
    ALTER SCHEMA [ums_iga] TRANSFER [iga].[CertificationCampaigns];
    ALTER SCHEMA [ums_iga] TRANSFER [iga].[CertificationReviews];
    ALTER SCHEMA [ums_iga] TRANSFER [iga].[AccessReviews];
    DROP SCHEMA [iga];
END

-- Create new schemas if they don't exist
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'ums_approvals')
    EXEC('CREATE SCHEMA [ums_approvals]');

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'ums_iga')
    EXEC('CREATE SCHEMA [ums_iga]');
