namespace Ums.Domain.Authorization;

public sealed class TemplateAssignmentRule : ParametricCatalogEntity<TemplateAssignmentRule, TemplateAssignmentRuleProps>
{
    private TemplateAssignmentRule(TemplateAssignmentRuleProps props) : base(props) { }

    public Guid TemplateId => Props.TemplateId.GetValue();
    public string PredicateExpression => Props.PredicateExpression.GetValue();
    public int Priority => Props.Priority;

    public static Result<TemplateAssignmentRule> Create(
        Guid tenantId,
        Guid templateId,
        string code,
        string value,
        string description,
        string predicateExpression,
        int priority,
        string version = "1.0.0")
    {
        if (templateId == Guid.Empty)
            return Result<TemplateAssignmentRule>.Failure(DomainErrors.TemplateAssignmentRule.TemplateIdRequired);

        if (string.IsNullOrWhiteSpace(predicateExpression))
            return Result<TemplateAssignmentRule>.Failure(DomainErrors.TemplateAssignmentRule.PredicateRequired);

        var props = new TemplateAssignmentRuleProps
        {
            TemplateId = global::Ums.Domain.Authorization.ValueObjects.TemplateId.Load(templateId),
            PredicateExpression = global::Ums.Domain.Authorization.ValueObjects.PredicateExpression.Create(predicateExpression.Trim()),
            Priority = priority
        };

        var rule = new TemplateAssignmentRule(props);
        var result = rule.SetCatalogFields(tenantId, code, value, description, version);
        
        return result.IsFailure ? Result<TemplateAssignmentRule>.Failure(result.Error) : Result<TemplateAssignmentRule>.Success(rule);
    }
}
