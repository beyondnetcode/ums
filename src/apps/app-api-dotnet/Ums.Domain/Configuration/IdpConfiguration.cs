namespace Ums.Domain.Configuration;

public sealed class IdpConfiguration : ParametricCatalogEntity<IdpConfiguration, IdpConfigurationProps>
{
    private IdpConfiguration(IdpConfigurationProps props) : base(props) { }

    public IdpStrategy ProviderType => Props.ProviderType;
    public int Priority => Props.Priority;
    public Guid? FallbackToId => Props.FallbackToId?.GetValue();
    public string? SecretReference => Props.SecretReference?.GetValue();
    public string[] DomainHints => Props.DomainHints;
    public bool MfaEnforced => Props.MfaEnforced;
    public LifecycleStatus Status => Props.Status;

    public static Result<IdpConfiguration> Create(
        Guid tenantId,
        string code,
        string value,
        string description,
        IdpStrategy providerType,
        int priority,
        string version = "1.0.0",
        Guid? systemSuiteId = null,
        string? secretReference = null,
        IEnumerable<string>? domainHints = null,
        bool mfaEnforced = false)
    {
        var props = new IdpConfigurationProps
        {
            ProviderType = providerType,
            Priority = priority,
            SecretReference = secretReference != null ? global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(secretReference) : null,
            DomainHints = domainHints?.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim().ToLowerInvariant()).Distinct().ToArray() ?? [],
            MfaEnforced = mfaEnforced
        };

        var configuration = new IdpConfiguration(props);
        var result = configuration.SetCatalogFields(tenantId, code, value, description, "system", version, systemSuiteId);
        if (result.IsFailure)
            return Result<IdpConfiguration>.Failure(result.Error);

        configuration.DomainEvents.ApplyChange(new ConfigurationChangedEvent(tenantId, configuration.GetId(), configuration.Code, configuration.Version), true);
        return Result<IdpConfiguration>.Success(configuration);
    }

    public void Activate()
    {
        Props.Status = LifecycleStatus.Active;
        Props.Audit.Update("system");
    }
}
