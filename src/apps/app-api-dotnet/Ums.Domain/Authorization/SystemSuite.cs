namespace Ums.Domain.Authorization;

public sealed class SystemSuite : AggregateRoot<SystemSuite, SystemSuiteProps>
{
    private readonly List<FunctionalModule> _modules = new();
    private readonly List<FunctionalAction> _actions = new();

    private SystemSuite(SystemSuiteProps props) : base(props)
    {
        if (TrackingState.IsNew)
        {
            DomainEvents.ApplyChange(new SystemSuiteRegisteredEvent(Props.TenantId.GetValue(), Props.Id.GetValue(), Props.Code.GetValue()), true);
        }
    }

    public Guid TenantId => Props.TenantId.GetValue();
    public string Code => Props.Code.GetValue();
    public string Name => Props.Name.GetValue();
    public string BaseUrl => Props.BaseUrl.GetValue();
    public string? ApiCredentialHash => Props.ApiCredentialHash?.GetValue();
    public LifecycleStatus Status => Props.Status;
    public IReadOnlyCollection<FunctionalModule> Modules => _modules.AsReadOnly();
    public IReadOnlyCollection<FunctionalAction> Actions => _actions.AsReadOnly();

    public static Result<SystemSuite> Register(Guid tenantId, string code, string name, string baseUrl)
    {
        if (tenantId == Guid.Empty)
            return Result<SystemSuite>.Failure(DomainErrors.Tenant.Required);

        var codeValue = global::Ums.Domain.Kernel.ValueObjects.Code.Create(code);

        if (string.IsNullOrWhiteSpace(name))
            return Result<SystemSuite>.Failure(DomainErrors.Common.Required);

        if (string.IsNullOrWhiteSpace(baseUrl))
            return Result<SystemSuite>.Failure(DomainErrors.SystemSuite.BaseUrlRequired);

        var props = new SystemSuiteProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            codeValue,
            global::Ums.Domain.Kernel.ValueObjects.Name.Create(name.Trim()),
            global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(baseUrl.Trim()));

        var system = new SystemSuite(props);
        return Result<SystemSuite>.Success(system);
    }

    public Result AddModule(string code, string name, string description)
    {
        var moduleResult = FunctionalModule.Create(Props.TenantId.GetValue(), Props.Id.GetValue(), code, name, description);
        if (moduleResult.IsFailure)
            return Result.Failure(moduleResult.Error);

        if (_modules.Any(module => module.Code == moduleResult.Value.Code))
            return Result.Failure(DomainErrors.SystemSuite.ModuleCodeNotUnique);

        _modules.Add(moduleResult.Value);
        DomainEvents.ApplyChange(new FunctionalTopologyChangedEvent(Props.TenantId.GetValue(), Props.Id.GetValue(), moduleResult.Value.Code), true);
        Props.Audit.Update("system");
        return Result.Success();
    }

    public Result AddAction(string code, string name, FunctionalActionLevel level, Guid? levelId = null)
    {
        var actionResult = FunctionalAction.Create(Props.TenantId.GetValue(), Props.Id.GetValue(), code, name, level, levelId);
        if (actionResult.IsFailure)
            return Result.Failure(actionResult.Error);

        if (_actions.Any(action => action.Code == actionResult.Value.Code))
            return Result.Failure(DomainErrors.SystemSuite.ActionCodeNotUnique);

        _actions.Add(actionResult.Value);
        DomainEvents.ApplyChange(new FunctionalTopologyChangedEvent(Props.TenantId.GetValue(), Props.Id.GetValue(), actionResult.Value.Code), true);
        Props.Audit.Update("system");
        return Result.Success();
    }

    public void Publish()
    {
        Props.Status = LifecycleStatus.Published;
        Props.Audit.Update("system");
    }
}
