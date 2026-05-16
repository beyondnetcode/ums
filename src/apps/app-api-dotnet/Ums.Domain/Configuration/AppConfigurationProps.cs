namespace Ums.Domain.Configuration;

public class AppConfigurationProps : ParametricCatalogProps
{
    public LifecycleStatus Status { get; set; } = LifecycleStatus.Draft;
    public DateTimeOffset? PublishedAt { get; set; }
    public IdValueObject? PublishedBy { get; set; }

    public AppConfigurationProps()
    {
        Id = IdValueObject.Create();
        Audit = AuditValueObject.Create("system");
    }
}
