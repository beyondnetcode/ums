using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ums.Infrastructure.Persistence.Authorization.Entities;

namespace Ums.Infrastructure.Persistence.Authorization.Configurations;

public sealed class SystemSuiteSubMenuRecordConfiguration : IEntityTypeConfiguration<SystemSuiteSubMenuRecord>
{
    public void Configure(EntityTypeBuilder<SystemSuiteSubMenuRecord> builder)
    {
        builder.ToTable("SystemSuiteSubMenus", AuthorizationPersistenceConstants.Schema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Label).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.AuditTimeSpan).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => new { x.MenuId, x.Code }).IsUnique();

        builder.HasMany(x => x.Options)
            .WithOne(x => x.SubMenu)
            .HasForeignKey(x => x.SubMenuId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
