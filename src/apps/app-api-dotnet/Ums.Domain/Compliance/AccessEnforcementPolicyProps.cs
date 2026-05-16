namespace Ums.Domain.Compliance;

public class AccessEnforcementPolicyProps : ParametricCatalogProps
{
    public EnforcementEffect Effect { get; set; } = default!;
    public StringValueObject ResourceScope { get; set; } = default!;

    public AccessEnforcementPolicyProps()
    {
        Id = IdValueObject.Create();
        Audit = AuditValueObject.Create("system");
    }
}
