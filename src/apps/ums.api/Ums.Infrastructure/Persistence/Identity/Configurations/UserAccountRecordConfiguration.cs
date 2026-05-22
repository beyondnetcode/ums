using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ums.Infrastructure.Persistence.Identity.Entities;

namespace Ums.Infrastructure.Persistence.Identity.Configurations;

public sealed class UserAccountRecordConfiguration : IEntityTypeConfiguration<UserAccountRecord>
{
    public void Configure(EntityTypeBuilder<UserAccountRecord> builder)
    {
        builder.ToTable("UserAccounts", IdentityPersistenceConstants.Schema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).HasMaxLength(255).IsRequired();
        builder.Property(x => x.IdentityReference).HasMaxLength(255);
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.AuditTimeSpan).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
        builder.HasIndex(x => x.TenantId);

        builder.HasMany(x => x.MfaEnrollments)
            .WithOne(x => x.UserAccount)
            .HasForeignKey(x => x.UserAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.PasswordCredentials)
            .WithOne(x => x.UserAccount)
            .HasForeignKey(x => x.UserAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
