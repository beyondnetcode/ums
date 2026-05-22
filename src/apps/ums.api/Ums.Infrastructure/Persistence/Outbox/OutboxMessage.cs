namespace Ums.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; set; }

    public Guid AggregateId { get; set; }

    public string AggregateName { get; set; } = string.Empty;

    public string EventName { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public Guid? TenantId { get; set; }

    public DateTime OccurredOnUtc { get; set; }

    public DateTime? ProcessedOnUtc { get; set; }

    public int RetryCount { get; set; }

    public string? LastError { get; set; }
}
