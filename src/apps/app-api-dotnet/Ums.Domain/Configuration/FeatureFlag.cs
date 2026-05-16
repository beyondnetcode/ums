namespace Ums.Domain.Configuration;

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
            return Result<FeatureFlag>.Failure(DomainErrors.Configuration.CreatorRequired);

        var props = new FeatureFlagProps
        {
            Type = type,
            Targets = global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(string.IsNullOrWhiteSpace(targets) ? "{}" : targets.Trim()),
            CreatedBy = IdValueObject.Load(createdBy)
        };

        var flag = new FeatureFlag(props);
        var result = flag.SetCatalogFields(tenantId, code, value, description, createdBy.ToString(), version, systemSuiteId);
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
