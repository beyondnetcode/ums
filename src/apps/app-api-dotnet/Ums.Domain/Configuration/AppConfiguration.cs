namespace Ums.Domain.Configuration;

public sealed class AppConfiguration : ParametricCatalogEntity<AppConfiguration, AppConfigurationProps>
{
    private AppConfiguration(AppConfigurationProps props) : base(props) { }

    public LifecycleStatus Status => Props.Status;
    public DateTimeOffset? PublishedAt => Props.PublishedAt;
    public Guid? PublishedBy => Props.PublishedBy?.GetValue();

    public static Result<AppConfiguration> Create(Guid tenantId, Guid systemSuiteId, string code, string value, string description, string version = "1.0.0")
    {
        if (systemSuiteId == Guid.Empty)
            return Result<AppConfiguration>.Failure(DomainErrors.Configuration.SystemSuiteIdRequired);

        var props = new AppConfigurationProps();
        var configuration = new AppConfiguration(props);
        
        var result = configuration.SetCatalogFields(tenantId, code, value, description, "system", version, systemSuiteId);
        if (result.IsFailure)
            return Result<AppConfiguration>.Failure(result.Error);

        configuration.DomainEvents.ApplyChange(new ConfigurationChangedEvent(tenantId, configuration.GetId(), configuration.Code, configuration.Version), true);
        return Result<AppConfiguration>.Success(configuration);
    }

    public Result Publish(Guid publishedBy)
    {
        if (publishedBy == Guid.Empty)
            return Result.Failure(DomainErrors.Configuration.PublisherRequired);

        Props.Status = LifecycleStatus.Published;
        Props.PublishedAt = DateTimeOffset.UtcNow;
        Props.PublishedBy = IdValueObject.Load(publishedBy);
        Props.Audit.Update("system");
        
        DomainEvents.ApplyChange(new ConfigurationChangedEvent(TenantId, GetId(), Code, Version), true);
        return Result.Success();
    }
}
