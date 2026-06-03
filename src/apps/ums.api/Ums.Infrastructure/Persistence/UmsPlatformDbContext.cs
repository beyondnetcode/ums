using Microsoft.EntityFrameworkCore;
using Ums.Infrastructure.Persistence.Audit.Configurations;
using Ums.Infrastructure.Persistence.Audit.Entities;
using Ums.Infrastructure.Persistence.Authorization.Configurations;
using Ums.Infrastructure.Persistence.Authorization.Entities;
using Ums.Infrastructure.Persistence.Configuration.Configurations;
using Ums.Infrastructure.Persistence.Configuration.Entities;
using Ums.Infrastructure.Persistence.Identity.Configurations;
using Ums.Infrastructure.Persistence.Identity.Entities;
using Ums.Infrastructure.Persistence.Outbox;
using Ums.Infrastructure.Persistence.Outbox.Configurations;
using Ums.Infrastructure.Persistence.Approvals.Configurations;
using Ums.Infrastructure.Persistence.Approvals.Entities;
using Ums.Infrastructure.Persistence.IGA.Configurations;
using Ums.Infrastructure.Persistence.IGA.Entities;

namespace Ums.Infrastructure.Persistence;

/// <summary>
/// Central EF Core DbContext for the UMS platform.
///
/// Tenant isolation (FIX-05):
/// All tenant-scoped entities carry a <c>TenantId</c> column. A global query filter is
/// applied to each of them so that EF never emits a SQL query without a WHERE clause
/// restricting to the caller's <see cref="ITenantContext.OrganizationId"/>.
///
/// Filter semantics:
/// - <c>OrganizationId == null</c> (system / background context) → no filter applied,
///   all rows visible (needed for the outbox dispatcher, dev seeding, admin ops).
/// - <c>OrganizationId.HasValue</c>                              → strict per-tenant rows only.
/// - <see cref="AppConfigurationRecord"/> is nullable-TenantId    → also includes global
///   records (<c>TenantId IS NULL</c>) so system-level config is always visible.
/// 
/// The SQL Server RLS predicates set via <see cref="Interceptors.OrganizationDbContextInterceptor"/>
/// remain as the database-level failsafe.
/// </summary>
public sealed class UmsPlatformDbContext(
    DbContextOptions<UmsPlatformDbContext> options,
    ITenantContext tenantContext) : DbContext(options)
{
    public const string DefaultSchema = "ums_platform";

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    // REC-13: Dead-letter store for outbox messages that exhausted all retries
    public DbSet<DeadLetterMessage> OutboxDeadLetters => Set<DeadLetterMessage>();
    public DbSet<TenantRecord> Tenants => Set<TenantRecord>();
    public DbSet<TenantBranchRecord> TenantBranches => Set<TenantBranchRecord>();
    public DbSet<TenantIdentityProviderRecord> TenantIdentityProviders => Set<TenantIdentityProviderRecord>();
    public DbSet<TenantBrandingRecord> TenantBrandings => Set<TenantBrandingRecord>();
    public DbSet<TenantParameterRecord> TenantParameters => Set<TenantParameterRecord>();
    public DbSet<TenantSignupRequestRecord> TenantSignupRequests => Set<TenantSignupRequestRecord>();
    public DbSet<UserAccountRecord> UserAccounts => Set<UserAccountRecord>();
    public DbSet<UserAccountMfaEnrollmentRecord> UserAccountMfaEnrollments => Set<UserAccountMfaEnrollmentRecord>();
    public DbSet<UserAccountPasswordCredentialRecord> UserAccountPasswordCredentials => Set<UserAccountPasswordCredentialRecord>();
    public DbSet<ProfileRecord> Profiles => Set<ProfileRecord>();
    public DbSet<ProfilePermissionRecord> ProfilePermissions => Set<ProfilePermissionRecord>();
    public DbSet<SystemSuiteRecord> SystemSuites => Set<SystemSuiteRecord>();
    public DbSet<SystemSuiteModuleRecord> SystemSuiteModules => Set<SystemSuiteModuleRecord>();
    public DbSet<SystemSuiteMenuRecord> SystemSuiteMenus => Set<SystemSuiteMenuRecord>();
    public DbSet<SystemSuiteSubMenuRecord> SystemSuiteSubMenus => Set<SystemSuiteSubMenuRecord>();
    public DbSet<SystemSuiteOptionRecord> SystemSuiteOptions => Set<SystemSuiteOptionRecord>();
    public DbSet<SystemSuiteAppSettingRecord> SystemSuiteAppSettings => Set<SystemSuiteAppSettingRecord>();
    public DbSet<SystemSuiteActionRecord> SystemSuiteActions => Set<SystemSuiteActionRecord>();
    public DbSet<SystemSuiteDomainResourceRecord> SystemSuiteDomainResources => Set<SystemSuiteDomainResourceRecord>();
    public DbSet<PermissionTemplateRecord> PermissionTemplates => Set<PermissionTemplateRecord>();
    public DbSet<PermissionTemplateItemRecord> PermissionTemplateItems => Set<PermissionTemplateItemRecord>();
    public DbSet<TemplateAssignmentRuleRecord> TemplateAssignmentRules => Set<TemplateAssignmentRuleRecord>();
    public DbSet<RoleRecord> Roles => Set<RoleRecord>();
    public DbSet<UserManagementDelegationRecord> UserManagementDelegations => Set<UserManagementDelegationRecord>();
    public DbSet<AppConfigurationRecord> AppConfigurations => Set<AppConfigurationRecord>();
    public DbSet<ParameterDefinitionRecord> ParameterDefinitions => Set<ParameterDefinitionRecord>();
    public DbSet<ParameterGlobalValueRecord> ParameterGlobalValues => Set<ParameterGlobalValueRecord>();
    public DbSet<ParameterTenantValueRecord> ParameterTenantValues => Set<ParameterTenantValueRecord>();
    public DbSet<FeatureFlagRecord> FeatureFlags => Set<FeatureFlagRecord>();
    public DbSet<FeatureFlagEvaluationLogRecord> FeatureFlagEvaluationLogs => Set<FeatureFlagEvaluationLogRecord>();
    public DbSet<FeatureFlagCriteriaRecord> FeatureFlagCriteria => Set<FeatureFlagCriteriaRecord>();
    public DbSet<IdpConfigurationRecord> IdpConfigurations => Set<IdpConfigurationRecord>();
    public DbSet<AuditRecordRecord> AuditRecords => Set<AuditRecordRecord>();
    public DbSet<ApprovalWorkflowRecord> ApprovalWorkflows => Set<ApprovalWorkflowRecord>();
    public DbSet<ApprovalRequiredDocumentRecord> ApprovalRequiredDocuments => Set<ApprovalRequiredDocumentRecord>();
    public DbSet<ApprovalRequestRecord> ApprovalRequests => Set<ApprovalRequestRecord>();
    public DbSet<NotificationRuleRecord> NotificationRules => Set<NotificationRuleRecord>();
    public DbSet<DocumentTypeRecord> DocumentTypes => Set<DocumentTypeRecord>();
    public DbSet<UserDocumentRecord> UserDocuments => Set<UserDocumentRecord>();
    public DbSet<AccessNotificationRecord> UserDocumentNotifications => Set<AccessNotificationRecord>();
    public DbSet<AccessEnforcementPolicyRecord> AccessEnforcementPolicies => Set<AccessEnforcementPolicyRecord>();
    public DbSet<PromotionRequestRecord> PromotionRequests => Set<PromotionRequestRecord>();
    public DbSet<PromotionImpactAnalysisRecord> PromotionImpactAnalyses => Set<PromotionImpactAnalysisRecord>();
    public DbSet<RoleMaturityStatusRecord> RoleMaturityStatuses => Set<RoleMaturityStatusRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DefaultSchema);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new DeadLetterMessageConfiguration());
        modelBuilder.ApplyConfiguration(new TenantRecordConfiguration());
        modelBuilder.ApplyConfiguration(new TenantBranchRecordConfiguration());
        modelBuilder.ApplyConfiguration(new TenantIdentityProviderRecordConfiguration());
        modelBuilder.ApplyConfiguration(new TenantBrandingRecordConfiguration());
        modelBuilder.ApplyConfiguration(new TenantParameterRecordConfiguration());
        modelBuilder.ApplyConfiguration(new TenantSignupRequestRecordConfiguration());
        modelBuilder.ApplyConfiguration(new UserAccountRecordConfiguration());
        modelBuilder.ApplyConfiguration(new UserAccountMfaEnrollmentRecordConfiguration());
        modelBuilder.ApplyConfiguration(new UserAccountPasswordCredentialRecordConfiguration());
        modelBuilder.ApplyConfiguration(new ProfileRecordConfiguration());
        modelBuilder.ApplyConfiguration(new ProfilePermissionRecordConfiguration());
        modelBuilder.ApplyConfiguration(new SystemSuiteRecordConfiguration());
        modelBuilder.ApplyConfiguration(new SystemSuiteModuleRecordConfiguration());
        modelBuilder.ApplyConfiguration(new SystemSuiteMenuRecordConfiguration());
        modelBuilder.ApplyConfiguration(new SystemSuiteSubMenuRecordConfiguration());
        modelBuilder.ApplyConfiguration(new SystemSuiteOptionRecordConfiguration());
        modelBuilder.ApplyConfiguration(new SystemSuiteAppSettingRecordConfiguration());
        modelBuilder.ApplyConfiguration(new SystemSuiteActionRecordConfiguration());
        modelBuilder.ApplyConfiguration(new SystemSuiteDomainResourceRecordConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionTemplateRecordConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionTemplateItemRecordConfiguration());
        modelBuilder.ApplyConfiguration(new RoleRecordConfiguration());
        modelBuilder.ApplyConfiguration(new UserManagementDelegationRecordConfiguration());
        modelBuilder.ApplyConfiguration(new AppConfigurationRecordConfiguration());
        modelBuilder.ApplyConfiguration(new ParameterDefinitionRecordConfiguration());
        modelBuilder.ApplyConfiguration(new ParameterGlobalValueRecordConfiguration());
        modelBuilder.ApplyConfiguration(new ParameterTenantValueRecordConfiguration());
        modelBuilder.ApplyConfiguration(new FeatureFlagRecordConfiguration());
        modelBuilder.ApplyConfiguration(new FeatureFlagEvaluationLogRecordConfiguration());
        modelBuilder.ApplyConfiguration(new FeatureFlagCriteriaRecordConfiguration());
        modelBuilder.ApplyConfiguration(new IdpConfigurationRecordConfiguration());
        modelBuilder.ApplyConfiguration(new AuditRecordRecordConfiguration());
        modelBuilder.ApplyConfiguration(new ApprovalWorkflowRecordConfiguration());
        modelBuilder.ApplyConfiguration(new ApprovalRequiredDocumentRecordConfiguration());
        modelBuilder.ApplyConfiguration(new ApprovalRequestRecordConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationRuleRecordConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentTypeRecordConfiguration());
        modelBuilder.ApplyConfiguration(new UserDocumentRecordConfiguration());
        modelBuilder.ApplyConfiguration(new AccessNotificationRecordConfiguration());
        modelBuilder.ApplyConfiguration(new AccessEnforcementPolicyRecordConfiguration());
        modelBuilder.ApplyConfiguration(new PromotionRequestRecordConfiguration());
        modelBuilder.ApplyConfiguration(new PromotionImpactAnalysisRecordConfiguration());
        modelBuilder.ApplyConfiguration(new RoleMaturityStatusRecordConfiguration());

        // -------------------------------------------------------------------------
        // FIX-05: Global query filters — primary tenant isolation mechanism.
        //
        // When OrganizationId is null (system/background context) the short-circuit
        // !HasValue condition makes EF emit no WHERE clause, so all rows are visible.
        // When OrganizationId is set, only the current tenant's rows are returned.
        //
        // REC-16: Soft-delete filters are combined here so no deleted rows ever leak.
        // -------------------------------------------------------------------------

        // TenantRecord is not tenant-scoped (it IS the tenant) but must be soft-delete filtered.
        modelBuilder.Entity<TenantRecord>()
            .HasQueryFilter(x => !x.IsDeleted);

        modelBuilder.Entity<TenantBranchRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId);

        modelBuilder.Entity<TenantBrandingRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId);

        modelBuilder.Entity<TenantParameterRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId);

        modelBuilder.Entity<TenantIdentityProviderRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId);

        modelBuilder.Entity<UserAccountRecord>()
            .HasQueryFilter(x =>
                !x.IsDeleted &&
                (!tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId));

        modelBuilder.Entity<ProfileRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId);

        modelBuilder.Entity<RoleRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId);

        modelBuilder.Entity<SystemSuiteRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId);

        modelBuilder.Entity<PermissionTemplateRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId);

        modelBuilder.Entity<UserManagementDelegationRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId);

        modelBuilder.Entity<IdpConfigurationRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId);

        // AppConfiguration TenantId is nullable: global records (TenantId IS NULL) are
        // always visible regardless of tenant context; tenant-scoped records only for the
        // current tenant.
        modelBuilder.Entity<AppConfigurationRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == null ||
                x.TenantId == tenantContext.OrganizationId);

        // ParameterTenantValue: tenant-specific parameter overrides
        modelBuilder.Entity<ParameterTenantValueRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId);

        modelBuilder.Entity<ApprovalWorkflowRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId);

        modelBuilder.Entity<NotificationRuleRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId);

        modelBuilder.Entity<DocumentTypeRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId);

        // UserDocuments have UserId but no TenantId — filtered via join in queries; no global filter here.

        modelBuilder.Entity<AccessEnforcementPolicyRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId);

        if (Database.IsSqlite())
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var rowVersionProperty = entityType.FindProperty("RowVersion");
                if (rowVersionProperty != null && rowVersionProperty.ClrType == typeof(byte[]))
                {
                    rowVersionProperty.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never;
                    rowVersionProperty.IsConcurrencyToken = false;
                    rowVersionProperty.SetBeforeSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Save);
                    rowVersionProperty.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Save);
                }
            }
        }

        // ADR-0076 D1: Force DateTimeKind.Utc on all DateTime properties read from the
        // database. EF Core / SQLite does not preserve timezone info, so values are stored
        // as ISO-8601 strings. Without this converter, DateTime.Kind == Unspecified after
        // materialisation, which breaks serialisation and comparisons with DateTime.UtcNow.
        var utcConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableUtcConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue
                ? (v.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v.Value.ToUniversalTime())
                : (DateTime?)null,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                    property.SetValueConverter(utcConverter);
                else if (property.ClrType == typeof(DateTime?))
                    property.SetValueConverter(nullableUtcConverter);
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}
