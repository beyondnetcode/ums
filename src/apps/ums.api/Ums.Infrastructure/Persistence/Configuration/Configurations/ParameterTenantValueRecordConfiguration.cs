using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ums.Infrastructure.Persistence.Configuration.Entities;

namespace Ums.Infrastructure.Persistence.Configuration.Configurations;

public sealed class ParameterTenantValueRecordConfiguration : IEntityTypeConfiguration<ParameterTenantValueRecord>
{
    public void Configure(EntityTypeBuilder<ParameterTenantValueRecord> builder)
    {
        builder.ToTable("ParameterTenantValues", ConfigurationPersistenceConstants.Schema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OverrideValue).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Version).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.AuditTimeSpan).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.ParameterDefinitionId }).IsUnique();
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.StatusId);
    }
}