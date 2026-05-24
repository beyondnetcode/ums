using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ums.Infrastructure.Persistence.Authorization.Entities;

namespace Ums.Infrastructure.Persistence.Authorization.Configurations;

public sealed class SystemSuiteMenuRecordConfiguration : IEntityTypeConfiguration<SystemSuiteMenuRecord>
{
    public void Configure(EntityTypeBuilder<SystemSuiteMenuRecord> builder)
    {
        builder.ToTable("SystemSuiteMenus", AuthorizationPersistenceConstants.Schema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Label).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.AuditTimeSpan).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => new { x.ModuleId, x.Code }).IsUnique();

        builder.HasMany(x => x.SubMenus)
            .WithOne(x => x.Menu)
            .HasForeignKey(x => x.MenuId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
