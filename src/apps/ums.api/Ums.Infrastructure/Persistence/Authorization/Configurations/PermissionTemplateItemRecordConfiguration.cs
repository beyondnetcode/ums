using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ums.Infrastructure.Persistence.Authorization.Entities;

namespace Ums.Infrastructure.Persistence.Authorization.Configurations;

public sealed class PermissionTemplateItemRecordConfiguration : IEntityTypeConfiguration<PermissionTemplateItemRecord>
{
    public void Configure(EntityTypeBuilder<PermissionTemplateItemRecord> builder)
    {
        builder.ToTable("PermissionTemplateItems", AuthorizationPersistenceConstants.Schema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.AuditTimeSpan).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => new { x.TemplateId, x.TargetTypeId, x.TargetId, x.ActionId }).IsUnique();
    }
}
