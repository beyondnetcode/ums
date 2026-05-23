using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ums.Infrastructure.Persistence.Outbox.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

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
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.LastError)
            .HasMaxLength(4000);

        builder.Property(x => x.RetryCount)
            .HasDefaultValue(0);

        // HARDENING-01: lease columns for distributed multi-instance dispatch
        builder.Property(x => x.LockedBy).HasMaxLength(200);

        builder.HasIndex(x => new { x.ProcessedOnUtc, x.OccurredOnUtc })
            .HasDatabaseName("IX_OutboxMessages_Dispatch");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("IX_OutboxMessages_TenantId");

        builder.HasIndex(x => new { x.AggregateName, x.AggregateId })
            .HasDatabaseName("IX_OutboxMessages_Aggregate");
    }
}
