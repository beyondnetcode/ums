using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ums.Infrastructure.Persistence.Configuration.Entities;

namespace Ums.Infrastructure.Persistence.Configuration.Configurations;

public sealed class ParameterGlobalValueRecordConfiguration : IEntityTypeConfiguration<ParameterGlobalValueRecord>
{
    public void Configure(EntityTypeBuilder<ParameterGlobalValueRecord> builder)
    {
        builder.ToTable("ParameterGlobalValues", ConfigurationPersistenceConstants.Schema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EffectiveValue).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Version).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.AuditTimeSpan).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => x.ParameterDefinitionId).IsUnique();
        builder.HasIndex(x => x.StatusId);
    }
}