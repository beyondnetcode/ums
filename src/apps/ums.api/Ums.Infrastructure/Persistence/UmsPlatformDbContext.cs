using Microsoft.EntityFrameworkCore;
using Ums.Infrastructure.Persistence.Authorization.Configurations;
using Ums.Infrastructure.Persistence.Authorization.Entities;
using Ums.Infrastructure.Persistence.Identity.Configurations;
using Ums.Infrastructure.Persistence.Identity.Entities;
using Ums.Infrastructure.Persistence.Outbox;
using Ums.Infrastructure.Persistence.Outbox.Configurations;

namespace Ums.Infrastructure.Persistence;

public sealed class UmsPlatformDbContext(DbContextOptions<UmsPlatformDbContext> options) : DbContext(options)
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
    // TODO(api-aggregate-tracker): Add SQL-backed DbSets and mappings for SystemSuite, PermissionTemplate, Approval aggregates, IGA aggregates, AuditRecord, and Configuration aggregates.

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

        base.OnModelCreating(modelBuilder);
    }
}
