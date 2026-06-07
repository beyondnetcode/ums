using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ums.Infrastructure.Persistence.Identity.Entities;

namespace Ums.Infrastructure.Persistence.Identity.Configurations;

public sealed class TenantParameterRecordConfiguration : IEntityTypeConfiguration<TenantParameterRecord>
{
    public void Configure(EntityTypeBuilder<TenantParameterRecord> builder)
    {
        builder.ToTable("TenantParameters", IdentityPersistenceConstants.Schema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.ValueTypeId).IsRequired();
        builder.Property(x => x.CategoryId).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.IsSensitive).IsRequired();
        builder.Property(x => x.DefaultValue).HasMaxLength(4000);
        builder.Property(x => x.AllowedValues).HasMaxLength(2000);
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.AuditTimeSpan).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.Code, x.IsActive })
            .HasFilter("\"IsActive\" = true")
            .IsUnique()
            .HasDatabaseName("IX_TenantParameters_TenantId_Code_IsActive");
    }
}