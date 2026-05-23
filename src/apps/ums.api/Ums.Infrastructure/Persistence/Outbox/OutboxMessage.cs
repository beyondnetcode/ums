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

    // HARDENING-01: Distributed dispatch lease — prevents two pods from processing the
    // same message concurrently. Set atomically by ExecuteUpdateAsync before reading.
    // Rows with LockedUntil > UtcNow are invisible to other instances.
    public DateTime? LockedUntil { get; set; }

    /// <summary>Identity of the pod/process that holds the current lease (for diagnostics).</summary>
    public string? LockedBy { get; set; }
}
