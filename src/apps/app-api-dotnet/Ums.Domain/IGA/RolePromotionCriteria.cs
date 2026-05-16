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
        string createdBy,
        string version = "1.0.0")
    {
        var brokenRules = new BrokenRulesManager();
        if (sourceRoleId == Guid.Empty || targetRoleId == Guid.Empty)
            brokenRules.Add(new BrokenRule("Roles", "Source and target roles are required."));

        if (sourceRoleId == targetRoleId)
            brokenRules.Add(new BrokenRule("Roles", "Source and target roles must be different."));

        if (string.IsNullOrWhiteSpace(evaluationExpression))
            brokenRules.Add(new BrokenRule(nameof(evaluationExpression), "Evaluation expression is required."));

        if (brokenRules.GetBrokenRules().Any())
            return Result<RolePromotionCriteria>.Failure(brokenRules.GetBrokenRulesAsString());

        var props = new RolePromotionCriteriaProps
        {
            SourceRoleId = global::Ums.Domain.Authorization.ValueObjects.RoleId.Load(sourceRoleId),
            TargetRoleId = global::Ums.Domain.Authorization.ValueObjects.RoleId.Load(targetRoleId),
            EvaluationExpression = global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(evaluationExpression.Trim())
        };

        props.GetType().GetProperty(nameof(ParametricCatalogProps.Audit))?.SetValue(props, AuditValueObject.Create(createdBy));

        var criteria = new RolePromotionCriteria(props);
        
        if (!criteria.IsValid())
        {
            return Result<RolePromotionCriteria>.Failure(criteria.BrokenRules.GetBrokenRulesAsString());
        }

        var result = criteria.SetCatalogFields(tenantId, code, value, description, createdBy, version);
        
        return result.IsFailure ? Result<RolePromotionCriteria>.Failure(result.Error) : Result<RolePromotionCriteria>.Success(criteria);
    }
}
