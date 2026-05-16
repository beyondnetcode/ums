namespace Ums.Domain.Audit;

public sealed class AuditRecord : AggregateRoot<AuditRecord, AuditRecordProps>
{
    private AuditRecord(AuditRecordProps props) : base(props)
    {
        if (TrackingState.IsNew)
        {
            DomainEvents.ApplyChange(new AuditRecordAppendedEvent(Props.TenantId.GetValue(), Props.Id.GetValue(), Props.EventType.GetValue()), true);
        }
    }

    public Guid TenantId => Props.TenantId.GetValue();
    public string EventType => Props.EventType.GetValue();
    public string Actor => Props.Actor.GetValue();
    public string Payload => Props.Payload.GetValue();
    public string CorrelationId => Props.CorrelationId.GetValue();
    public DateTimeOffset RecordedAt => Props.RecordedAt;

    public static Result<AuditRecord> Append(Guid tenantId, string eventType, string actor, string payload, string correlationId)
    {
        if (tenantId == Guid.Empty)
            return Result<AuditRecord>.Failure(DomainErrors.Tenant.Required);

        if (string.IsNullOrWhiteSpace(eventType) || string.IsNullOrWhiteSpace(actor) || string.IsNullOrWhiteSpace(correlationId))
            return Result<AuditRecord>.Failure(DomainErrors.Audit.RecordIdentifiersRequired);

        var props = new AuditRecordProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(eventType.Trim()),
            global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(actor.Trim()),
            global::Ums.Domain.Authorization.ValueObjects.Payload.Create(string.IsNullOrWhiteSpace(payload) ? "{}" : payload.Trim()),
            global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(correlationId.Trim()));

        var record = new AuditRecord(props);
        return Result<AuditRecord>.Success(record);
    }
}
