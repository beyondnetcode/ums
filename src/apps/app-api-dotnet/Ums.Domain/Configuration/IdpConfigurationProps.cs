namespace Ums.Domain.Configuration;

public class IdpConfigurationProps : ParametricCatalogProps
{
    public IdpStrategy ProviderType { get; set; } = default!;
    public int Priority { get; set; }
    public IdValueObject? FallbackToId { get; set; }
    public StringValueObject? SecretReference { get; set; }
    public string[] DomainHints { get; set; } = [];
    public bool MfaEnforced { get; set; }
    public LifecycleStatus Status { get; set; } = LifecycleStatus.Draft;

    public IdpConfigurationProps()
    {
        Id = IdValueObject.Create();
        Audit = AuditValueObject.Create("system");
    }
}
