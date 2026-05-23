namespace Ums.Infrastructure.Persistence.Outbox;

/// <summary>
/// REC-13: Outbox dead-letter store.
///
/// An <see cref="OutboxMessage"/> is moved here after it exhausts all retries
/// (<see cref="OutboxMessage.RetryCount"/> reaches <c>MaxRetries</c>).  Moving
/// the failed message out of the hot outbox table ensures:
///
///   1. The outbox dispatcher's batch query stays small and fast (no "stuck" rows).
///   2. Failed messages are preserved for manual inspection / replay.
///   3. The readiness health check <see cref="HealthChecks.OutboxBacklogHealthCheck"/>
///      can distinguish live failures from known-dead messages.
///
/// Admin can query dead-letter messages via <c>GET /api/v1/outbox/dead-letters</c>
/// and trigger a replay via <c>POST /api/v1/outbox/dead-letters/{id}/replay</c>
/// (Presentation layer endpoints to be added separately).
/// </summary>
public sealed class DeadLetterMessage
{
    public Guid Id { get; set; }

    /// <summary>Original outbox message id for cross-reference.</summary>
    public Guid OriginalMessageId { get; set; }

    public Guid AggregateId { get; set; }

    public string AggregateName { get; set; } = string.Empty;

    public string EventName { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public Guid? TenantId { get; set; }

    public DateTime OccurredOnUtc { get; set; }

    /// <summary>UTC timestamp when the message was moved to dead-letter storage.</summary>
    public DateTime DeadLetteredOnUtc { get; set; }

    /// <summary>Number of dispatch attempts before the message was dead-lettered.</summary>
    public int RetryCount { get; set; }

    /// <summary>Last error that caused the final failure.</summary>
    public string? LastError { get; set; }

    /// <summary>True after a manual replay successfully re-dispatched the event.</summary>
    public bool ReplayedSuccessfully { get; set; }

    /// <summary>UTC timestamp of a successful replay (null if not replayed).</summary>
    public DateTime? ReplayedOnUtc { get; set; }

    // ─── Factory ─────────────────────────────────────────────────────────────

    public static DeadLetterMessage FromOutboxMessage(OutboxMessage source)
        => new()
        {
            Id                  = Guid.NewGuid(),
            OriginalMessageId   = source.Id,
            AggregateId         = source.AggregateId,
            AggregateName       = source.AggregateName,
            EventName           = source.EventName,
            EventType           = source.EventType,
            Payload             = source.Payload,
            TenantId            = source.TenantId,
            OccurredOnUtc       = source.OccurredOnUtc,
            DeadLetteredOnUtc   = DateTime.UtcNow,
            RetryCount          = source.RetryCount,
            LastError           = source.LastError,
            ReplayedSuccessfully = false,
        };
}
