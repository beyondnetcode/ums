using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ums.Infrastructure.Persistence.Identity.Entities;

namespace Ums.Infrastructure.Persistence.Identity.Configurations;

public sealed class TenantBrandingRecordConfiguration : IEntityTypeConfiguration<TenantBrandingRecord>
{
    public void Configure(EntityTypeBuilder<TenantBrandingRecord> builder)
    {
        builder.ToTable("TenantBrandings", IdentityPersistenceConstants.Schema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Logo).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.PrimaryColor).HasMaxLength(20).IsRequired();
        builder.Property(x => x.HeadlineText).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SecondaryText).HasMaxLength(500).IsRequired();
        builder.Property(x => x.PrimaryButtonLabel).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FooterText).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CustomDomain).HasMaxLength(255);
        builder.Property(x => x.DnsCnameTarget).HasMaxLength(255).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.AuditTimeSpan).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => x.TenantId).IsUnique();
        builder.HasIndex(x => x.CustomDomain).IsUnique().HasFilter("[CustomDomain] IS NOT NULL");
    }
}
