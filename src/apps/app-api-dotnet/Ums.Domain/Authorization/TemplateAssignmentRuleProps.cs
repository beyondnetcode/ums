namespace Ums.Domain.Authorization;

public class TemplateAssignmentRuleProps : ParametricCatalogProps
{
    public global::Ums.Domain.Authorization.ValueObjects.TemplateId TemplateId { get; set; } = default!;
    public global::Ums.Domain.Authorization.ValueObjects.PredicateExpression PredicateExpression { get; set; } = default!;
    public int Priority { get; set; }

    public TemplateAssignmentRuleProps()
    {
        Id = IdValueObject.Create();
        Audit = AuditValueObject.Create("system");
    }
}
