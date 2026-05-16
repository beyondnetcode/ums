namespace Ums.Domain.IGA;

public sealed class RolePromotionCriteria : ParametricCatalogEntity<RolePromotionCriteria, RolePromotionCriteriaProps>
{
    private RolePromotionCriteria(RolePromotionCriteriaProps props) : base(props) { }

    public Guid SourceRoleId => Props.SourceRoleId.GetValue();
    public Guid TargetRoleId => Props.TargetRoleId.GetValue();
    public string EvaluationExpression => Props.EvaluationExpression.GetValue();

    public static Result<RolePromotionCriteria> Create(
        Guid tenantId,
        string code,
        string value,
        string description,
        Guid sourceRoleId,
        Guid targetRoleId,
        string evaluationExpression,
        string version = "1.0.0")
    {
        if (sourceRoleId == Guid.Empty || targetRoleId == Guid.Empty)
            return Result<RolePromotionCriteria>.Failure("Source and target roles are required.");

        if (sourceRoleId == targetRoleId)
            return Result<RolePromotionCriteria>.Failure("Source and target roles must be different.");

        if (string.IsNullOrWhiteSpace(evaluationExpression))
            return Result<RolePromotionCriteria>.Failure("Evaluation expression is required.");

        var props = new RolePromotionCriteriaProps
        {
            SourceRoleId = global::Ums.Domain.Authorization.ValueObjects.RoleId.Load(sourceRoleId),
            TargetRoleId = global::Ums.Domain.Authorization.ValueObjects.RoleId.Load(targetRoleId),
            EvaluationExpression = global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(evaluationExpression.Trim())
        };

        var criteria = new RolePromotionCriteria(props);
        var result = criteria.SetCatalogFields(tenantId, code, value, description, version);
        
        return result.IsFailure ? Result<RolePromotionCriteria>.Failure(result.Error) : Result<RolePromotionCriteria>.Success(criteria);
    }
}
