using Microsoft.EntityFrameworkCore;
using Ums.Application.Common.Interfaces;
using Ums.Infrastructure.Persistence.Authorization.Configurations;
using Ums.Infrastructure.Persistence.Authorization.Entities;
using Ums.Infrastructure.Persistence.Configuration.Configurations;
using Ums.Infrastructure.Persistence.Configuration.Entities;
using Ums.Infrastructure.Persistence.Identity.Configurations;
using Ums.Infrastructure.Persistence.Identity.Entities;
using Ums.Infrastructure.Persistence.Outbox;
using Ums.Infrastructure.Persistence.Outbox.Configurations;

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
    // TODO(api-aggregate-tracker): Add SQL-backed DbSets and mappings for SystemSuite, PermissionTemplate, Approval aggregates, IGA aggregates, and AuditRecord.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DefaultSchema);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
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

        // -------------------------------------------------------------------------
        // FIX-05: Global query filters — primary tenant isolation mechanism.
        //
        // When OrganizationId is null (system/background context) the short-circuit
        // !HasValue condition makes EF emit no WHERE clause, so all rows are visible.
        // When OrganizationId is set, only the current tenant's rows are returned.
        // -------------------------------------------------------------------------
        modelBuilder.Entity<TenantBranchRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId.Value);

        modelBuilder.Entity<TenantBrandingRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId.Value);

        modelBuilder.Entity<TenantIdentityProviderRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId.Value);

        modelBuilder.Entity<UserAccountRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId.Value);

        modelBuilder.Entity<ProfileRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId.Value);

        modelBuilder.Entity<UserManagementDelegationRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId.Value);

        modelBuilder.Entity<IdpConfigurationRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == tenantContext.OrganizationId.Value);

        // AppConfiguration TenantId is nullable: global records (TenantId IS NULL) are
        // always visible regardless of tenant context; tenant-scoped records only for the
        // current tenant.
        modelBuilder.Entity<AppConfigurationRecord>()
            .HasQueryFilter(x =>
                !tenantContext.OrganizationId.HasValue ||
                x.TenantId == null ||
                x.TenantId == tenantContext.OrganizationId.Value);

        base.OnModelCreating(modelBuilder);
    }
}
