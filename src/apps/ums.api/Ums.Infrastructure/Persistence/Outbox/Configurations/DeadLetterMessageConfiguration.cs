using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ums.Infrastructure.Persistence.Outbox.Configurations;

public sealed class DeadLetterMessageConfiguration : IEntityTypeConfiguration<DeadLetterMessage>
{
    public void Configure(EntityTypeBuilder<DeadLetterMessage> builder)
    {
        builder.ToTable("OutboxDeadLetters", UmsPlatformDbContext.DefaultSchema);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AggregateName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.EventName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.EventType)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Payload)
            .IsRequired();

        builder.Property(x => x.LastError)
            .HasMaxLength(4000);

        builder.Property(x => x.RetryCount)
            .HasDefaultValue(0);

        builder.Property(x => x.ReplayedSuccessfully)
            .HasDefaultValue(false);

        // Index for tenant-scoped queries
        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("IX_OutboxDeadLetters_TenantId");

        // Index for looking up by original message
        builder.HasIndex(x => x.OriginalMessageId)
            .HasDatabaseName("IX_OutboxDeadLetters_OriginalMessageId");

        // Index for admin dashboard — most recent dead-letters first
        builder.HasIndex(x => x.DeadLetteredOnUtc)
            .HasDatabaseName("IX_OutboxDeadLetters_DeadLetteredOnUtc");

        builder.HasIndex(x => new { x.ReplayedSuccessfully, x.DeadLetteredOnUtc })
            .HasDatabaseName("IX_OutboxDeadLetters_PendingReplay");
    }
}
