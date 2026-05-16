namespace Ums.Domain.Audit;

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
