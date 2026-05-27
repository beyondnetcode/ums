using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ums.Infrastructure.Persistence.IGA.Entities;

namespace Ums.Infrastructure.Persistence.IGA.Configurations;

public sealed class PromotionImpactAnalysisRecordConfiguration : IEntityTypeConfiguration<PromotionImpactAnalysisRecord>
{


    public void Configure(EntityTypeBuilder<PromotionImpactAnalysisRecord> builder)
    {
        builder.ToTable("PromotionImpactAnalyses", IgaPersistenceConstants.Schema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RiskLevel).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ConflictingPermissions).HasMaxLength(2000);
        builder.Property(x => x.RiskFactors).HasMaxLength(2000);
        builder.Property(x => x.SuggestedMitigations).HasMaxLength(2000);
        builder.Property(x => x.AnalyzedBy).HasMaxLength(100);
        builder.Property(x => x.RiskScore).HasPrecision(5, 2);
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.AuditTimeSpan).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => x.PromotionRequestId);
    }
}
