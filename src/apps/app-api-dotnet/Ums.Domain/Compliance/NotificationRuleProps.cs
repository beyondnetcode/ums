namespace Ums.Domain.Compliance;

public class NotificationRuleProps : ParametricCatalogProps
{
    public StringValueObject TriggerEvent { get; set; } = default!;
    public StringValueObject Channel { get; set; } = default!;

    public NotificationRuleProps()
    {
        Id = IdValueObject.Create();
        Audit = AuditValueObject.Create("system");
    }
}
