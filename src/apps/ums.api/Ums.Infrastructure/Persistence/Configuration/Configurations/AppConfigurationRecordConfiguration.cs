using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ums.Infrastructure.Persistence.Configuration.Entities;

namespace Ums.Infrastructure.Persistence.Configuration.Configurations;

public sealed class AppConfigurationRecordConfiguration : IEntityTypeConfiguration<AppConfigurationRecord>
{
    public void Configure(EntityTypeBuilder<AppConfigurationRecord> builder)
    {
        builder.ToTable("AppConfigurations", ConfigurationPersistenceConstants.Schema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Version).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.AuditTimeSpan).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion(); // FIX-03: optimistic concurrency

        builder.HasIndex(x => new { x.TenantId, x.SystemSuiteId, x.ModuleId, x.Code }).IsUnique();
        builder.HasIndex(x => x.ScopeId);
        builder.HasIndex(x => x.StatusId);
    }
}
