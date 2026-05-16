namespace Ums.Domain.Audit;

using Ums.Domain.Kernel;

using Ums.Domain.Common;
using Ums.Domain.Events;
using Ums.Shell.Ddd;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Shell.Ddd.Interfaces;
using Ums.Shell.Ddd.ValueObjects.Common;

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
            return Result<AuditRecord>.Failure(DomainErrors.TenantRequired);

        if (string.IsNullOrWhiteSpace(eventType) || string.IsNullOrWhiteSpace(actor) || string.IsNullOrWhiteSpace(correlationId))
            return Result<AuditRecord>.Failure("Event type, actor, and correlation id are required.");

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

public class FlagEvaluationLogProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public IdValueObject FlagId { get; private set; }
    public StringValueObject EvaluatedForType { get; private set; }
    public IdValueObject EvaluatedForId { get; private set; }
    public StringValueObject Result { get; private set; }
    public DateTimeOffset EvaluatedAt { get; private set; }

    public FlagEvaluationLogProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, IdValueObject flagId, StringValueObject evaluatedForType, IdValueObject evaluatedForId, StringValueObject result)
    {
        Id = id;
        TenantId = tenantId;
        FlagId = flagId;
        EvaluatedForType = evaluatedForType;
        EvaluatedForId = evaluatedForId;
        Result = result;
        EvaluatedAt = DateTimeOffset.UtcNow;
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public sealed class FlagEvaluationLog : Entity<FlagEvaluationLog, FlagEvaluationLogProps>
{
    private FlagEvaluationLog(FlagEvaluationLogProps props) : base(props) { }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid FlagId => Props.FlagId.GetValue();
    public string EvaluatedForType => Props.EvaluatedForType.GetValue();
    public Guid EvaluatedForId => Props.EvaluatedForId.GetValue();
    public string Result => Props.Result.GetValue();
    public DateTimeOffset EvaluatedAt => Props.EvaluatedAt;

    public static Result<FlagEvaluationLog> Create(Guid tenantId, Guid flagId, string evaluatedForType, Guid evaluatedForId, string result)
    {
        if (tenantId == Guid.Empty || flagId == Guid.Empty || evaluatedForId == Guid.Empty)
            return Result<FlagEvaluationLog>.Failure("Tenant, flag, and evaluated subject identifiers are required.");

        if (string.IsNullOrWhiteSpace(evaluatedForType) || string.IsNullOrWhiteSpace(result))
            return Result<FlagEvaluationLog>.Failure("Evaluated subject type and result are required.");

        var props = new FlagEvaluationLogProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            IdValueObject.Load(flagId),
            global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(evaluatedForType.Trim()),
            IdValueObject.Load(evaluatedForId),
            global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(result.Trim()));

        return Result<FlagEvaluationLog>.Success(new FlagEvaluationLog(props));
    }
}

