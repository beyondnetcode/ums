using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ums.Infrastructure.Persistence.Authorization.Entities;

namespace Ums.Infrastructure.Persistence.Authorization.Configurations;

public sealed class SystemSuiteAppSettingRecordConfiguration : IEntityTypeConfiguration<SystemSuiteAppSettingRecord>
{
    public void Configure(EntityTypeBuilder<SystemSuiteAppSettingRecord> builder)
    {
        builder.ToTable("SystemSuiteAppSettings", AuthorizationPersistenceConstants.Schema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ConfigKey).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ConfigValue).HasMaxLength(4000).IsRequired();

        builder.HasIndex(x => new { x.SystemSuiteId, x.ConfigKey, x.ScopeId }).IsUnique();
    }
}
