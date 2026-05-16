namespace Ums.Domain.Audit;

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
