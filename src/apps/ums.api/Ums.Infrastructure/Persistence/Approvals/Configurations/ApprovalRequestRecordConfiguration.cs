using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ums.Infrastructure.Persistence.Approvals.Entities;

namespace Ums.Infrastructure.Persistence.Approvals.Configurations;

public sealed class ApprovalRequestRecordConfiguration : IEntityTypeConfiguration<ApprovalRequestRecord>
{


    public void Configure(EntityTypeBuilder<ApprovalRequestRecord> builder)
    {
        builder.ToTable("ApprovalRequests", ApprovalsPersistenceConstants.Schema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.AuditTimeSpan).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => x.WorkflowId);
        builder.HasIndex(x => x.TargetUserId);
        builder.HasIndex(x => x.TargetProfileId);
    }
}
