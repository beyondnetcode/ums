using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ums.Infrastructure.Persistence.Configuration.Entities;

namespace Ums.Infrastructure.Persistence.Configuration.Configurations;

public sealed class FeatureFlagRecordConfiguration : IEntityTypeConfiguration<FeatureFlagRecord>
{
    public void Configure(EntityTypeBuilder<FeatureFlagRecord> builder)
    {
        builder.ToTable("FeatureFlags", ConfigurationPersistenceConstants.Schema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FlagCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FlagTargets).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.AuditTimeSpan).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion(); // FIX-03: optimistic concurrency

        builder.HasIndex(x => x.FlagCode).IsUnique();
        builder.HasIndex(x => x.StatusId);
        builder.HasIndex(x => x.FlagTypeId);

        builder.HasMany(x => x.EvaluationLogs)
            .WithOne(x => x.FeatureFlag)
            .HasForeignKey(x => x.FeatureFlagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
