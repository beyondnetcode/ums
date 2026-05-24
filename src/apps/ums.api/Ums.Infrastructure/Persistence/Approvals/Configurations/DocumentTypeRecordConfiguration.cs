using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ums.Infrastructure.Persistence.Approvals.Entities;

namespace Ums.Infrastructure.Persistence.Approvals.Configurations;

public sealed class DocumentTypeRecordConfiguration : IEntityTypeConfiguration<DocumentTypeRecord>
{
    public const string Schema = "approvals";

    public void Configure(EntityTypeBuilder<DocumentTypeRecord> builder)
    {
        builder.ToTable("DocumentTypes", Schema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.AuditTimeSpan).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}
