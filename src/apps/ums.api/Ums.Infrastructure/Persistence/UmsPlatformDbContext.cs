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
    public DbSet<UserAccountRecord> UserAccounts => Set<UserAccountRecord>();
    public DbSet<UserAccountMfaEnrollmentRecord> UserAccountMfaEnrollments => Set<UserAccountMfaEnrollmentRecord>();
    public DbSet<UserAccountPasswordCredentialRecord> UserAccountPasswordCredentials => Set<UserAccountPasswordCredentialRecord>();
    public DbSet<ProfileRecord> Profiles => Set<ProfileRecord>();
    public DbSet<ProfilePermissionRecord> ProfilePermissions => Set<ProfilePermissionRecord>();
    public DbSet<UserManagementDelegationRecord> UserManagementDelegations => Set<UserManagementDelegationRecord>();
    public DbSet<AppConfigurationRecord> AppConfigurations => Set<AppConfigurationRecord>();
    public DbSet<FeatureFlagRecord> FeatureFlags => Set<FeatureFlagRecord>();
    public DbSet<FeatureFlagEvaluationLogRecord> FeatureFlagEvaluationLogs => Set<FeatureFlagEvaluationLogRecord>();
    public DbSet<IdpConfigurationRecord> IdpConfigurations => Set<IdpConfigurationRecord>();
    public DbSet<AuditRecordRecord> AuditRecords => Set<AuditRecordRecord>();
    public DbSet<ApprovalWorkflowRecord> ApprovalWorkflows => Set<ApprovalWorkflowRecord>();
    public DbSet<ApprovalRequiredDocumentRecord> ApprovalRequiredDocuments => Set<ApprovalRequiredDocumentRecord>();
    public DbSet<ApprovalRequestRecord> ApprovalRequests => Set<ApprovalRequestRecord>();
    public DbSet<NotificationRuleRecord> NotificationRules => Set<NotificationRuleRecord>();
    // TODO(api-aggregate-tracker): Add SQL-backed DbSets and mappings for SystemSuite, PermissionTemplate, and IGA aggregates.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DefaultSchema);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new DeadLetterMessageConfiguration());
        modelBuilder.ApplyConfiguration(new TenantRecordConfiguration());
        modelBuilder.ApplyConfiguration(new TenantBranchRecordConfiguration());
        modelBuilder.ApplyConfiguration(new TenantIdentityProviderRecordConfiguration());
        modelBuilder.ApplyConfiguration(new TenantBrandingRecordConfiguration());
        modelBuilder.ApplyConfiguration(new UserAccountRecordConfiguration());
        modelBuilder.ApplyConfiguration(new UserAccountMfaEnrollmentRecordConfiguration());
        modelBuilder.ApplyConfiguration(new UserAccountPasswordCredentialRecordConfiguration());
        modelBuilder.ApplyConfiguration(new ProfileRecordConfiguration());
        modelBuilder.ApplyConfiguration(new ProfilePermissionRecordConfiguration());
        modelBuilder.ApplyConfiguration(new UserManagementDelegationRecordConfiguration());
        modelBuilder.ApplyConfiguration(new AppConfigurationRecordConfiguration());
        modelBuilder.ApplyConfiguration(new FeatureFlagRecordConfiguration());
        modelBuilder.ApplyConfiguration(new FeatureFlagEvaluationLogRecordConfiguration());
        modelBuilder.ApplyConfiguration(new IdpConfigurationRecordConfiguration());
        modelBuilder.ApplyConfiguration(new AuditRecordRecordConfiguration());
        modelBuilder.ApplyConfiguration(new ApprovalWorkflowRecordConfiguration());
        modelBuilder.ApplyConfiguration(new ApprovalRequiredDocumentRecordConfiguration());
        modelBuilder.ApplyConfiguration(new ApprovalRequestRecordConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationRuleRecordConfiguration());

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

        modelBuilder.Entity<ApprovalWorkflowRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId);

        modelBuilder.Entity<NotificationRuleRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId);

        base.OnModelCreating(modelBuilder);
    }
}
