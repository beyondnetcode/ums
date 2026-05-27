using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ums.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPermissionTemplateRoleFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "approvals");

            migrationBuilder.EnsureSchema(
                name: "ums_configuration");

            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.EnsureSchema(
                name: "ums_platform");

            migrationBuilder.EnsureSchema(
                name: "ums_authorization");

            migrationBuilder.EnsureSchema(
                name: "iga");

            migrationBuilder.EnsureSchema(
                name: "ums_identity");

            migrationBuilder.CreateTable(
                name: "AccessEnforcementPolicies",
                schema: "approvals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProfileId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EnforcementActionId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessEnforcementPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppConfigurations",
                schema: "ums_configuration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SystemSuiteId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ModuleId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ScopeId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsInheritable = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEncrypted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Version = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalRequests",
                schema: "approvals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetProfileId = table.Column<Guid>(type: "TEXT", nullable: true),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalWorkflows",
                schema: "approvals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SystemSuiteId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TargetUserCategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalWorkflows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditRecords",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WhoActed = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubjectTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    WhenOccurred = table.Column<DateTime>(type: "TEXT", nullable: false),
                    WhatChanged = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    EventType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    AuditResultId = table.Column<int>(type: "INTEGER", nullable: false),
                    AffectedEntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AffectedEntityType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    RootTenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Metadata = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                schema: "approvals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CriticityId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeatureFlags",
                schema: "ums_configuration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FlagCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FlagTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    FlagTargets = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    LinkedResourceTypeId = table.Column<int>(type: "INTEGER", nullable: true),
                    LinkedResourceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RolloutPercentage = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureFlags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IdpConfigurations",
                schema: "ums_configuration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SystemSuiteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProviderTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    DomainHintsJson = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    ConfigPayload = table.Column<string>(type: "TEXT", maxLength: 20000, nullable: false),
                    SecretRef = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    ResolutionPriority = table.Column<int>(type: "INTEGER", nullable: false),
                    FallbackToId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdpConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationRules",
                schema: "approvals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChannelId = table.Column<int>(type: "INTEGER", nullable: false),
                    Recipient = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxDeadLetters",
                schema: "ums_platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OriginalMessageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AggregateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AggregateName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    EventName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    EventType = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Payload = table.Column<string>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OccurredOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DeadLetteredOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    LastError = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    ReplayedSuccessfully = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    ReplayedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxDeadLetters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "ums_platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AggregateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AggregateName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    EventName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    EventType = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Payload = table.Column<string>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OccurredOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    LastError = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    LockedUntil = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LockedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                schema: "ums_authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BranchId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ScopeId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromotionRequests",
                schema: "iga",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CurrentRoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetRoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RequestedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RequestReason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ManagerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ManagerApprovalStatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    ManagerDecisionAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SecurityApprovalStatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    SecurityDecisionAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExecutedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleMaturityStatuses",
                schema: "iga",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CurrentMaturityLevelId = table.Column<int>(type: "INTEGER", nullable: false),
                    NextEligibleMaturityLevelId = table.Column<int>(type: "INTEGER", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CurrentLevelSince = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EligibleForPromotionAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedCertificationsCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletedTrainingsCount = table.Column<int>(type: "INTEGER", nullable: false),
                    PerformanceScore = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    HasNoComplianceIssues = table.Column<bool>(type: "INTEGER", nullable: false),
                    BlockingFactor = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    LastReviewedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMaturityStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSuites",
                schema: "ums_authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSuites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                schema: "ums_identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    OrganizationTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    IdpStrategyId = table.Column<int>(type: "INTEGER", nullable: false),
                    CompanyReference = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ParentTenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAccounts",
                schema: "ums_identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BranchId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    IdentityReference = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    IdentityReferenceTypeId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    AnonymizedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserDocuments",
                schema: "approvals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    CriticityId = table.Column<int>(type: "INTEGER", nullable: false),
                    FileStoragePath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    FileChecksum = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    NotificationStep = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserManagementDelegations",
                schema: "ums_identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DelegatingAdminId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DelegatedAdminId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ScopeTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    ScopeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AllowedActionsJson = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ValidFrom = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    ValidUntil = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    MaxDurationDays = table.Column<int>(type: "INTEGER", nullable: true),
                    RequiresApproval = table.Column<bool>(type: "INTEGER", nullable: false),
                    ApprovalRequestId = table.Column<Guid>(type: "TEXT", nullable: true),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    RevokedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    RevocationReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserManagementDelegations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalRequiredDocuments",
                schema: "approvals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsMandatory = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalRequiredDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalRequiredDocuments_ApprovalWorkflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalSchema: "approvals",
                        principalTable: "ApprovalWorkflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeatureFlagEvaluationLogs",
                schema: "ums_configuration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FeatureFlagId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EvaluatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    Result = table.Column<bool>(type: "INTEGER", nullable: false),
                    Context = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    EvaluatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureFlagEvaluationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeatureFlagEvaluationLogs_FeatureFlags_FeatureFlagId",
                        column: x => x.FeatureFlagId,
                        principalSchema: "ums_configuration",
                        principalTable: "FeatureFlags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfilePermissions",
                schema: "ums_authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TemplateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsAllowed = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDenied = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsOverride = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfilePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfilePermissions_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalSchema: "ums_authorization",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromotionImpactAnalyses",
                schema: "iga",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PromotionRequestId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RiskScore = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    RiskLevel = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NewPermissionsCount = table.Column<int>(type: "INTEGER", nullable: false),
                    RemovedPermissionsCount = table.Column<int>(type: "INTEGER", nullable: false),
                    AffectedSystemsCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ConflictingPermissions = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    RiskFactors = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    SuggestedMitigations = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    AnalyzedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AnalyzedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionImpactAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionImpactAnalyses_PromotionRequests_PromotionRequestId",
                        column: x => x.PromotionRequestId,
                        principalSchema: "iga",
                        principalTable: "PromotionRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "ums_authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SystemSuiteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParentRoleId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    HierarchyLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    PromotionOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Roles_ParentRoleId",
                        column: x => x.ParentRoleId,
                        principalSchema: "ums_authorization",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Roles_SystemSuites_SystemSuiteId",
                        column: x => x.SystemSuiteId,
                        principalSchema: "ums_authorization",
                        principalTable: "SystemSuites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemSuiteActions",
                schema: "ums_authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SystemSuiteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModuleId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSuiteActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemSuiteActions_SystemSuites_SystemSuiteId",
                        column: x => x.SystemSuiteId,
                        principalSchema: "ums_authorization",
                        principalTable: "SystemSuites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemSuiteAppSettings",
                schema: "ums_authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SystemSuiteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConfigKey = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ConfigValue = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    ScopeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSuiteAppSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemSuiteAppSettings_SystemSuites_SystemSuiteId",
                        column: x => x.SystemSuiteId,
                        principalSchema: "ums_authorization",
                        principalTable: "SystemSuites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemSuiteModules",
                schema: "ums_authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SystemSuiteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSuiteModules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemSuiteModules_SystemSuites_SystemSuiteId",
                        column: x => x.SystemSuiteId,
                        principalSchema: "ums_authorization",
                        principalTable: "SystemSuites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantBranches",
                schema: "ums_identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    GeofencingMetadata = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantBranches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantBranches_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "ums_identity",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantBrandings",
                schema: "ums_identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Logo = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    LogoFormatId = table.Column<int>(type: "INTEGER", nullable: false),
                    PrimaryColor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    BackgroundStyleId = table.Column<int>(type: "INTEGER", nullable: false),
                    HeadlineText = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SecondaryText = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    PrimaryButtonLabel = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FooterText = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CustomDomain = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DnsVerificationStatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    DnsCnameTarget = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    MagicLinkFallbackEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantBrandings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantBrandings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "ums_identity",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantIdentityProviders",
                schema: "ums_identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    StrategyId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantIdentityProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantIdentityProviders_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "ums_identity",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAccountMfaEnrollments",
                schema: "ums_identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserAccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MethodId = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccountMfaEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAccountMfaEnrollments_UserAccounts_UserAccountId",
                        column: x => x.UserAccountId,
                        principalSchema: "ums_identity",
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAccountPasswordCredentials",
                schema: "ums_identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserAccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccountPasswordCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAccountPasswordCredentials_UserAccounts_UserAccountId",
                        column: x => x.UserAccountId,
                        principalSchema: "ums_identity",
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDocumentNotifications",
                schema: "approvals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserDocumentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Step = table.Column<int>(type: "INTEGER", nullable: false),
                    ChannelId = table.Column<int>(type: "INTEGER", nullable: false),
                    DaysRemaining = table.Column<int>(type: "INTEGER", nullable: false),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDocumentNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDocumentNotifications_UserDocuments_UserDocumentId",
                        column: x => x.UserDocumentId,
                        principalSchema: "approvals",
                        principalTable: "UserDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermissionTemplates",
                schema: "ums_authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SystemSuiteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissionTemplates_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "ums_authorization",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PermissionTemplates_SystemSuites_SystemSuiteId",
                        column: x => x.SystemSuiteId,
                        principalSchema: "ums_authorization",
                        principalTable: "SystemSuites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemSuiteMenus",
                schema: "ums_authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModuleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSuiteMenus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemSuiteMenus_SystemSuiteModules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "ums_authorization",
                        principalTable: "SystemSuiteModules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermissionTemplateItems",
                schema: "ums_authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TemplateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsAllowed = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDenied = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionTemplateItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissionTemplateItems_PermissionTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "ums_authorization",
                        principalTable: "PermissionTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemSuiteSubMenus",
                schema: "ums_authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MenuId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSuiteSubMenus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemSuiteSubMenus_SystemSuiteMenus_MenuId",
                        column: x => x.MenuId,
                        principalSchema: "ums_authorization",
                        principalTable: "SystemSuiteMenus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemSuiteOptions",
                schema: "ums_authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubMenuId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ActionCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSuiteOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemSuiteOptions_SystemSuiteSubMenus_SubMenuId",
                        column: x => x.SubMenuId,
                        principalSchema: "ums_authorization",
                        principalTable: "SystemSuiteSubMenus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessEnforcementPolicies_TenantId",
                schema: "approvals",
                table: "AccessEnforcementPolicies",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessEnforcementPolicies_TenantId_ProfileId",
                schema: "approvals",
                table: "AccessEnforcementPolicies",
                columns: new[] { "TenantId", "ProfileId" },
                filter: "[ProfileId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AccessEnforcementPolicies_TenantId_RoleId",
                schema: "approvals",
                table: "AccessEnforcementPolicies",
                columns: new[] { "TenantId", "RoleId" },
                filter: "[RoleId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigurations_ScopeId",
                schema: "ums_configuration",
                table: "AppConfigurations",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigurations_StatusId",
                schema: "ums_configuration",
                table: "AppConfigurations",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigurations_TenantId_SystemSuiteId_ModuleId_Code",
                schema: "ums_configuration",
                table: "AppConfigurations",
                columns: new[] { "TenantId", "SystemSuiteId", "ModuleId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequests_TargetProfileId",
                schema: "approvals",
                table: "ApprovalRequests",
                column: "TargetProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequests_TargetUserId",
                schema: "approvals",
                table: "ApprovalRequests",
                column: "TargetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequests_WorkflowId",
                schema: "approvals",
                table: "ApprovalRequests",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequiredDocuments_DocumentTypeId",
                schema: "approvals",
                table: "ApprovalRequiredDocuments",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequiredDocuments_WorkflowId",
                schema: "approvals",
                table: "ApprovalRequiredDocuments",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequiredDocuments_WorkflowId_DocumentTypeId",
                schema: "approvals",
                table: "ApprovalRequiredDocuments",
                columns: new[] { "WorkflowId", "DocumentTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalWorkflows_Code",
                schema: "approvals",
                table: "ApprovalWorkflows",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalWorkflows_TenantId",
                schema: "approvals",
                table: "ApprovalWorkflows",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalWorkflows_TenantId_Code",
                schema: "approvals",
                table: "ApprovalWorkflows",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_AffectedEntityId",
                schema: "audit",
                table: "AuditRecords",
                column: "AffectedEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_AffectedEntityId_AffectedEntityType",
                schema: "audit",
                table: "AuditRecords",
                columns: new[] { "AffectedEntityId", "AffectedEntityType" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_EventType",
                schema: "audit",
                table: "AuditRecords",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_RootTenantId",
                schema: "audit",
                table: "AuditRecords",
                column: "RootTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_WhoActed",
                schema: "audit",
                table: "AuditRecords",
                column: "WhoActed");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_TenantId",
                schema: "approvals",
                table: "DocumentTypes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_TenantId_Code",
                schema: "approvals",
                table: "DocumentTypes",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlagEvaluationLogs_EvaluatedAtUtc",
                schema: "ums_configuration",
                table: "FeatureFlagEvaluationLogs",
                column: "EvaluatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlagEvaluationLogs_FeatureFlagId",
                schema: "ums_configuration",
                table: "FeatureFlagEvaluationLogs",
                column: "FeatureFlagId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlags_FlagCode",
                schema: "ums_configuration",
                table: "FeatureFlags",
                column: "FlagCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlags_FlagTypeId",
                schema: "ums_configuration",
                table: "FeatureFlags",
                column: "FlagTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlags_StatusId",
                schema: "ums_configuration",
                table: "FeatureFlags",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_IdpConfigurations_ProviderTypeId",
                schema: "ums_configuration",
                table: "IdpConfigurations",
                column: "ProviderTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_IdpConfigurations_SystemSuiteId",
                schema: "ums_configuration",
                table: "IdpConfigurations",
                column: "SystemSuiteId");

            migrationBuilder.CreateIndex(
                name: "IX_IdpConfigurations_TenantId",
                schema: "ums_configuration",
                table: "IdpConfigurations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_IdpConfigurations_TenantId_SystemSuiteId_ResolutionPriority",
                schema: "ums_configuration",
                table: "IdpConfigurations",
                columns: new[] { "TenantId", "SystemSuiteId", "ResolutionPriority" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRules_TenantId",
                schema: "approvals",
                table: "NotificationRules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxDeadLetters_DeadLetteredOnUtc",
                schema: "ums_platform",
                table: "OutboxDeadLetters",
                column: "DeadLetteredOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxDeadLetters_OriginalMessageId",
                schema: "ums_platform",
                table: "OutboxDeadLetters",
                column: "OriginalMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxDeadLetters_PendingReplay",
                schema: "ums_platform",
                table: "OutboxDeadLetters",
                columns: new[] { "ReplayedSuccessfully", "DeadLetteredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxDeadLetters_TenantId",
                schema: "ums_platform",
                table: "OutboxDeadLetters",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Aggregate",
                schema: "ums_platform",
                table: "OutboxMessages",
                columns: new[] { "AggregateName", "AggregateId" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Dispatch",
                schema: "ums_platform",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOnUtc", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_TenantId",
                schema: "ums_platform",
                table: "OutboxMessages",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionTemplateItems_TemplateId_TargetTypeId_TargetId_ActionId",
                schema: "ums_authorization",
                table: "PermissionTemplateItems",
                columns: new[] { "TemplateId", "TargetTypeId", "TargetId", "ActionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionTemplates_RoleId",
                schema: "ums_authorization",
                table: "PermissionTemplates",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionTemplates_SystemSuiteId",
                schema: "ums_authorization",
                table: "PermissionTemplates",
                column: "SystemSuiteId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionTemplates_TenantId",
                schema: "ums_authorization",
                table: "PermissionTemplates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionTemplates_TenantId_RoleId_SystemSuiteId_Version",
                schema: "ums_authorization",
                table: "PermissionTemplates",
                columns: new[] { "TenantId", "RoleId", "SystemSuiteId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfilePermissions_ProfileId",
                schema: "ums_authorization",
                table: "ProfilePermissions",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfilePermissions_ProfileId_TemplateId_ActionId_TargetId",
                schema: "ums_authorization",
                table: "ProfilePermissions",
                columns: new[] { "ProfileId", "TemplateId", "ActionId", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_TenantId",
                schema: "ums_authorization",
                table: "Profiles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_TenantId_UserId_RoleId_BranchId",
                schema: "ums_authorization",
                table: "Profiles",
                columns: new[] { "TenantId", "UserId", "RoleId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_UserId",
                schema: "ums_authorization",
                table: "Profiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionImpactAnalyses_PromotionRequestId",
                schema: "iga",
                table: "PromotionImpactAnalyses",
                column: "PromotionRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionRequests_ManagerId",
                schema: "iga",
                table: "PromotionRequests",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionRequests_TenantId",
                schema: "iga",
                table: "PromotionRequests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionRequests_TenantId_StatusId",
                schema: "iga",
                table: "PromotionRequests",
                columns: new[] { "TenantId", "StatusId" });

            migrationBuilder.CreateIndex(
                name: "IX_PromotionRequests_UserId",
                schema: "iga",
                table: "PromotionRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMaturityStatuses_RoleId",
                schema: "iga",
                table: "RoleMaturityStatuses",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMaturityStatuses_TenantId",
                schema: "iga",
                table: "RoleMaturityStatuses",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMaturityStatuses_UserId",
                schema: "iga",
                table: "RoleMaturityStatuses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMaturityStatuses_UserId_RoleId",
                schema: "iga",
                table: "RoleMaturityStatuses",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_ParentRoleId",
                schema: "ums_authorization",
                table: "Roles",
                column: "ParentRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_SystemSuiteId_Code",
                schema: "ums_authorization",
                table: "Roles",
                columns: new[] { "SystemSuiteId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId",
                schema: "ums_authorization",
                table: "Roles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSuiteActions_SystemSuiteId_Code",
                schema: "ums_authorization",
                table: "SystemSuiteActions",
                columns: new[] { "SystemSuiteId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemSuiteActions_TenantId",
                schema: "ums_authorization",
                table: "SystemSuiteActions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSuiteAppSettings_SystemSuiteId_ConfigKey_ScopeId",
                schema: "ums_authorization",
                table: "SystemSuiteAppSettings",
                columns: new[] { "SystemSuiteId", "ConfigKey", "ScopeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemSuiteMenus_ModuleId_Code",
                schema: "ums_authorization",
                table: "SystemSuiteMenus",
                columns: new[] { "ModuleId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemSuiteModules_SystemSuiteId_Code",
                schema: "ums_authorization",
                table: "SystemSuiteModules",
                columns: new[] { "SystemSuiteId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemSuiteOptions_SubMenuId_Code",
                schema: "ums_authorization",
                table: "SystemSuiteOptions",
                columns: new[] { "SubMenuId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemSuites_TenantId",
                schema: "ums_authorization",
                table: "SystemSuites",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSuites_TenantId_Code",
                schema: "ums_authorization",
                table: "SystemSuites",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemSuiteSubMenus_MenuId_Code",
                schema: "ums_authorization",
                table: "SystemSuiteSubMenus",
                columns: new[] { "MenuId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantBranches_TenantId_Code",
                schema: "ums_identity",
                table: "TenantBranches",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantBrandings_CustomDomain",
                schema: "ums_identity",
                table: "TenantBrandings",
                column: "CustomDomain",
                unique: true,
                filter: "[CustomDomain] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TenantBrandings_TenantId",
                schema: "ums_identity",
                table: "TenantBrandings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantIdentityProviders_TenantId_Code",
                schema: "ums_identity",
                table: "TenantIdentityProviders",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Code",
                schema: "ums_identity",
                table: "Tenants",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_IsDeleted",
                schema: "ums_identity",
                table: "Tenants",
                column: "IsDeleted",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ParentTenantId",
                schema: "ums_identity",
                table: "Tenants",
                column: "ParentTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccountMfaEnrollments_UserAccountId_MethodId",
                schema: "ums_identity",
                table: "UserAccountMfaEnrollments",
                columns: new[] { "UserAccountId", "MethodId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAccountPasswordCredentials_UserAccountId",
                schema: "ums_identity",
                table: "UserAccountPasswordCredentials",
                column: "UserAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_Email",
                schema: "ums_identity",
                table: "UserAccounts",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_IsDeleted",
                schema: "ums_identity",
                table: "UserAccounts",
                column: "IsDeleted",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_TenantId",
                schema: "ums_identity",
                table: "UserAccounts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_TenantId_Email",
                schema: "ums_identity",
                table: "UserAccounts",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDocumentNotifications_UserDocumentId",
                schema: "approvals",
                table: "UserDocumentNotifications",
                column: "UserDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDocumentNotifications_UserDocumentId_Step",
                schema: "approvals",
                table: "UserDocumentNotifications",
                columns: new[] { "UserDocumentId", "Step" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDocuments_ExpirationDate",
                schema: "approvals",
                table: "UserDocuments",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserDocuments_StatusId",
                schema: "approvals",
                table: "UserDocuments",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDocuments_UserId",
                schema: "approvals",
                table: "UserDocuments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDocuments_UserId_DocumentTypeId",
                schema: "approvals",
                table: "UserDocuments",
                columns: new[] { "UserId", "DocumentTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserManagementDelegations_DelegatedAdminId",
                schema: "ums_identity",
                table: "UserManagementDelegations",
                column: "DelegatedAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_UserManagementDelegations_DelegatingAdminId",
                schema: "ums_identity",
                table: "UserManagementDelegations",
                column: "DelegatingAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_UserManagementDelegations_TenantId",
                schema: "ums_identity",
                table: "UserManagementDelegations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserManagementDelegations_TenantId_StatusId",
                schema: "ums_identity",
                table: "UserManagementDelegations",
                columns: new[] { "TenantId", "StatusId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessEnforcementPolicies",
                schema: "approvals");

            migrationBuilder.DropTable(
                name: "AppConfigurations",
                schema: "ums_configuration");

            migrationBuilder.DropTable(
                name: "ApprovalRequests",
                schema: "approvals");

            migrationBuilder.DropTable(
                name: "ApprovalRequiredDocuments",
                schema: "approvals");

            migrationBuilder.DropTable(
                name: "AuditRecords",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "DocumentTypes",
                schema: "approvals");

            migrationBuilder.DropTable(
                name: "FeatureFlagEvaluationLogs",
                schema: "ums_configuration");

            migrationBuilder.DropTable(
                name: "IdpConfigurations",
                schema: "ums_configuration");

            migrationBuilder.DropTable(
                name: "NotificationRules",
                schema: "approvals");

            migrationBuilder.DropTable(
                name: "OutboxDeadLetters",
                schema: "ums_platform");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "ums_platform");

            migrationBuilder.DropTable(
                name: "PermissionTemplateItems",
                schema: "ums_authorization");

            migrationBuilder.DropTable(
                name: "ProfilePermissions",
                schema: "ums_authorization");

            migrationBuilder.DropTable(
                name: "PromotionImpactAnalyses",
                schema: "iga");

            migrationBuilder.DropTable(
                name: "RoleMaturityStatuses",
                schema: "iga");

            migrationBuilder.DropTable(
                name: "SystemSuiteActions",
                schema: "ums_authorization");

            migrationBuilder.DropTable(
                name: "SystemSuiteAppSettings",
                schema: "ums_authorization");

            migrationBuilder.DropTable(
                name: "SystemSuiteOptions",
                schema: "ums_authorization");

            migrationBuilder.DropTable(
                name: "TenantBranches",
                schema: "ums_identity");

            migrationBuilder.DropTable(
                name: "TenantBrandings",
                schema: "ums_identity");

            migrationBuilder.DropTable(
                name: "TenantIdentityProviders",
                schema: "ums_identity");

            migrationBuilder.DropTable(
                name: "UserAccountMfaEnrollments",
                schema: "ums_identity");

            migrationBuilder.DropTable(
                name: "UserAccountPasswordCredentials",
                schema: "ums_identity");

            migrationBuilder.DropTable(
                name: "UserDocumentNotifications",
                schema: "approvals");

            migrationBuilder.DropTable(
                name: "UserManagementDelegations",
                schema: "ums_identity");

            migrationBuilder.DropTable(
                name: "ApprovalWorkflows",
                schema: "approvals");

            migrationBuilder.DropTable(
                name: "FeatureFlags",
                schema: "ums_configuration");

            migrationBuilder.DropTable(
                name: "PermissionTemplates",
                schema: "ums_authorization");

            migrationBuilder.DropTable(
                name: "Profiles",
                schema: "ums_authorization");

            migrationBuilder.DropTable(
                name: "PromotionRequests",
                schema: "iga");

            migrationBuilder.DropTable(
                name: "SystemSuiteSubMenus",
                schema: "ums_authorization");

            migrationBuilder.DropTable(
                name: "Tenants",
                schema: "ums_identity");

            migrationBuilder.DropTable(
                name: "UserAccounts",
                schema: "ums_identity");

            migrationBuilder.DropTable(
                name: "UserDocuments",
                schema: "approvals");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "ums_authorization");

            migrationBuilder.DropTable(
                name: "SystemSuiteMenus",
                schema: "ums_authorization");

            migrationBuilder.DropTable(
                name: "SystemSuiteModules",
                schema: "ums_authorization");

            migrationBuilder.DropTable(
                name: "SystemSuites",
                schema: "ums_authorization");
        }
    }
}
