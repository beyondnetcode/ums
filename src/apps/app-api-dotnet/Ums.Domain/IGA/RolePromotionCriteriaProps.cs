namespace Ums.Domain.IGA;

public class RolePromotionCriteriaProps : ParametricCatalogProps
{
    public global::Ums.Domain.Authorization.ValueObjects.RoleId SourceRoleId { get; set; } = default!;
    public global::Ums.Domain.Authorization.ValueObjects.RoleId TargetRoleId { get; set; } = default!;
    public StringValueObject EvaluationExpression { get; set; } = default!;

    public RolePromotionCriteriaProps()
    {
        Id = IdValueObject.Create();
        Audit = AuditValueObject.Create("system");
    }
}
