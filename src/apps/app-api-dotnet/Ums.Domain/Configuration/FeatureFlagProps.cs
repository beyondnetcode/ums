namespace Ums.Domain.Configuration;

public class FeatureFlagProps : ParametricCatalogProps
{
    public FeatureFlagType Type { get; set; } = default!;
    public StringValueObject Targets { get; set; } = global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create("{}");
    public LifecycleStatus Status { get; set; } = LifecycleStatus.Draft;
    public StringValueObject? LinkedResourceType { get; set; }
    public IdValueObject? LinkedResourceId { get; set; }
    public IdValueObject CreatedBy { get; set; } = default!;

    public FeatureFlagProps()
    {
        Id = IdValueObject.Create();
        Audit = AuditValueObject.Create("system");
    }
}
