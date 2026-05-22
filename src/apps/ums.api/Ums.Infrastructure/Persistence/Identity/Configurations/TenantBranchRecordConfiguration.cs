using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ums.Infrastructure.Persistence.Identity.Entities;

namespace Ums.Infrastructure.Persistence.Identity.Configurations;

public sealed class TenantBranchRecordConfiguration : IEntityTypeConfiguration<TenantBranchRecord>
{
    public void Configure(EntityTypeBuilder<TenantBranchRecord> builder)
    {
        builder.ToTable("TenantBranches", IdentityPersistenceConstants.Schema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.GeofencingMetadata).HasMaxLength(4000);
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.AuditTimeSpan).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}
