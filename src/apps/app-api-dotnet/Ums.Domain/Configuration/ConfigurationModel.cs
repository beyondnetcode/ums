namespace Ums.Domain.Configuration;

using Ums.Domain.Common;
using Ums.Domain.Enums;
using Ums.Domain.Events;
using Ums.Shell.Ddd;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Shell.Ddd.Interfaces;
using Ums.Shell.Ddd.ValueObjects.Common;

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
        var result = configuration.SetCatalogFields(tenantId, code, value, description, version, systemSuiteId);
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

public sealed class AppConfiguration : ParametricCatalogEntity<AppConfiguration, AppConfigurationProps>
{
    private AppConfiguration(AppConfigurationProps props) : base(props) { }

    public LifecycleStatus Status => Props.Status;
    public DateTimeOffset? PublishedAt => Props.PublishedAt;
    public Guid? PublishedBy => Props.PublishedBy?.GetValue();

    public static Result<AppConfiguration> Create(Guid tenantId, Guid systemSuiteId, string code, string value, string description, string version = "1.0.0")
    {
        if (systemSuiteId == Guid.Empty)
            return Result<AppConfiguration>.Failure("System suite identifier is required.");

        var props = new AppConfigurationProps();
        var configuration = new AppConfiguration(props);
        
        var result = configuration.SetCatalogFields(tenantId, code, value, description, version, systemSuiteId);
        if (result.IsFailure)
            return Result<AppConfiguration>.Failure(result.Error);

        configuration.DomainEvents.ApplyChange(new ConfigurationChangedEvent(tenantId, configuration.GetId(), configuration.Code, configuration.Version), true);
        return Result<AppConfiguration>.Success(configuration);
    }

    public Result Publish(Guid publishedBy)
    {
        if (publishedBy == Guid.Empty)
            return Result.Failure("Publisher identifier is required.");

        Props.Status = LifecycleStatus.Published;
        Props.PublishedAt = DateTimeOffset.UtcNow;
        Props.PublishedBy = IdValueObject.Load(publishedBy);
        Props.Audit.Update("system");
        
        DomainEvents.ApplyChange(new ConfigurationChangedEvent(TenantId, GetId(), Code, Version), true);
        return Result.Success();
    }
}

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

public sealed class FeatureFlag : ParametricCatalogEntity<FeatureFlag, FeatureFlagProps>
{
    private FeatureFlag(FeatureFlagProps props) : base(props) { }

    public FeatureFlagType Type => Props.Type;
    public string Targets => Props.Targets.GetValue();
    public LifecycleStatus Status => Props.Status;
    public string? LinkedResourceType => Props.LinkedResourceType?.GetValue();
    public Guid? LinkedResourceId => Props.LinkedResourceId?.GetValue();
    public Guid CreatedBy => Props.CreatedBy.GetValue();

    public static Result<FeatureFlag> Create(
        Guid tenantId,
        string code,
        string value,
        string description,
        FeatureFlagType type,
        string targets,
        Guid createdBy,
        string version = "1.0.0",
        Guid? systemSuiteId = null)
    {
        if (createdBy == Guid.Empty)
            return Result<FeatureFlag>.Failure("Creator identifier is required.");

        var props = new FeatureFlagProps
        {
            Type = type,
            Targets = global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(string.IsNullOrWhiteSpace(targets) ? "{}" : targets.Trim()),
            CreatedBy = IdValueObject.Load(createdBy)
        };

        var flag = new FeatureFlag(props);
        var result = flag.SetCatalogFields(tenantId, code, value, description, version, systemSuiteId);
        if (result.IsFailure)
            return Result<FeatureFlag>.Failure(result.Error);

        flag.DomainEvents.ApplyChange(new FeatureFlagChangedEvent(tenantId, flag.GetId(), flag.Code, flag.Version), true);
        return Result<FeatureFlag>.Success(flag);
    }

    public void Activate()
    {
        Props.Status = LifecycleStatus.Active;
        Props.Audit.Update("system");
        DomainEvents.ApplyChange(new FeatureFlagChangedEvent(TenantId, GetId(), Code, Version), true);
    }
}

