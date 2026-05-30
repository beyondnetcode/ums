using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ums.Infrastructure.Persistence.Configuration.Entities;

namespace Ums.Infrastructure.Persistence.Configuration.Configurations;

public sealed class ParameterDefinitionRecordConfiguration : IEntityTypeConfiguration<ParameterDefinitionRecord>
{
    public void Configure(EntityTypeBuilder<ParameterDefinitionRecord> builder)
    {
        builder.ToTable("ParameterDefinitions", ConfigurationPersistenceConstants.Schema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.DefaultValue).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Version).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.AuditTimeSpan).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => x.ScopeId);
        builder.HasIndex(x => x.IsActive);
    }
}