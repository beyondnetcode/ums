namespace Ums.Domain.Audit;

public class AuditRecordProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public StringValueObject EventType { get; private set; }
    public StringValueObject Actor { get; private set; }
    public global::Ums.Domain.Authorization.ValueObjects.Payload Payload { get; private set; }
    public StringValueObject CorrelationId { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }

    public AuditRecordProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, StringValueObject eventType, StringValueObject actor, global::Ums.Domain.Authorization.ValueObjects.Payload payload, StringValueObject correlationId)
    {
        Id = id;
        TenantId = tenantId;
        EventType = eventType;
        Actor = actor;
        Payload = payload;
        CorrelationId = correlationId;
        RecordedAt = DateTimeOffset.UtcNow;
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
