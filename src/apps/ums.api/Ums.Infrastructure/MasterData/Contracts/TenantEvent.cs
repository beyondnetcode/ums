namespace Evolith.Contracts.MasterData;

/// <summary>
/// Wire contract for master-Tenant events published by MMS (ADR-0106 / ADR-0050). Duplicated
/// verbatim from the producer's canonical <c>Evolith.Contracts.MasterData</c> namespace so
/// MassTransit routes the message to this consumer (until a shared Evolith.Messaging.Contracts
/// package exists). Do not change field names/namespace independently of the producer.
/// </summary>
public sealed record TenantEvent
{
    public string SpecVersion { get; init; } = "1.0";
    public Guid Id { get; init; }
    public string Type { get; init; } = default!;
    public string Source { get; init; } = "mms";
    public Guid Subject { get; init; }
    public DateTimeOffset Time { get; init; }
    public string SchemaVersion { get; init; } = "1.0";
    public long Sequence { get; init; }
    public Guid CorrelationId { get; init; }
    public Guid CausationId { get; init; }
    public string Actor { get; init; } = "system";
    public DateTimeOffset ProducedAt { get; init; }
    public TenantData Data { get; init; } = default!;
}

public sealed record TenantData(Guid TenantId, string Code, string Name, string Status, long Version);
