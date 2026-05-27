using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ums.Infrastructure.Persistence.IGA.Entities;

namespace Ums.Infrastructure.Persistence.IGA.Configurations;

public sealed class PromotionRequestRecordConfiguration : IEntityTypeConfiguration<PromotionRequestRecord>
{


    public void Configure(EntityTypeBuilder<PromotionRequestRecord> builder)
    {
        builder.ToTable("PromotionRequests", IgaPersistenceConstants.Schema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RequestedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RequestReason).HasMaxLength(1000);
        builder.Property(x => x.ExecutedBy).HasMaxLength(100);
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.AuditTimeSpan).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ManagerId);
        builder.HasIndex(x => new { x.TenantId, x.StatusId });

        builder.HasMany(x => x.ImpactAnalyses)
            .WithOne()
            .HasForeignKey(x => x.PromotionRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
