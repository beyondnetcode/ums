using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ums.Infrastructure.Persistence.IGA.Entities;

namespace Ums.Infrastructure.Persistence.IGA.Configurations;

public sealed class RoleMaturityStatusRecordConfiguration : IEntityTypeConfiguration<RoleMaturityStatusRecord>
{
    public const string Schema = "iga";

    public void Configure(EntityTypeBuilder<RoleMaturityStatusRecord> builder)
    {
        builder.ToTable("RoleMaturityStatuses", Schema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BlockingFactor).HasMaxLength(500);
        builder.Property(x => x.PerformanceScore).HasPrecision(5, 2);
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.AuditTimeSpan).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.RoleId);
        builder.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
    }
}
