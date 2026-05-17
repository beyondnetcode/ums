namespace Ums.Domain.Identity.System;

using Ums.Domain.Identity.System.Configuration;
using Ums.Domain.Identity.System.Module;
using Ums.Domain.Identity.System.Action;
using ModuleEntity = Ums.Domain.Identity.System.Module.Module;
using ConfigurationEntity = Ums.Domain.Identity.System.Configuration.Configuration;
using ActionEntity = Ums.Domain.Identity.System.Action.Action;

public sealed class System : AggregateRoot<System, SystemProps>
{
    private readonly List<ModuleEntity> _modules = new();
    private readonly List<ConfigurationEntity> _configurations = new();
    private readonly List<ActionEntity> _actions = new();

    private System(SystemProps props) : base(props)
    {
    }

    public TenantId TenantId => Props.TenantId;
    public Code Code => Props.Code;
    public Name Name => Props.Name;
    public Description Description => Props.Description;
    public SystemStatus Status => Props.Status;

    public IReadOnlyCollection<ModuleEntity> Modules => _modules.AsReadOnly();
    public IReadOnlyCollection<ConfigurationEntity> Configurations => _configurations.AsReadOnly();
    public IReadOnlyCollection<ActionEntity> Actions => _actions.AsReadOnly();

    public SystemId GetId() => SystemId.Load(Props.Id.GetValue());

    public static Result<System> Create(
        TenantId tenantId,
        Code code,
        Name name,
        Description description,
        ActorId createdBy)
    {
        var props = new SystemProps(IdValueObject.Create(), tenantId, code, name, description, SystemStatus.Active, createdBy);
        var system = new System(props);

        if (!system.IsValid())
        {
            return Result<System>.Failure(system.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<System>.Success(system);
    }

    public Result Update(Name name, Description description, ActorId updatedBy)
    {
        Props.Name = name;
        Props.Description = description;

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result SetStatus(SystemStatus status, ActorId updatedBy)
    {
        Props.Status = status;
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result AddModule(Code code, Name name, Description description, int sortOrder, ActorId createdBy)
    {
        if (_modules.Any(m => m.Code == code))
        {
            BrokenRules.Add(new BrokenRule(nameof(Modules), DomainErrors.System.ModuleCodeNotUnique));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var moduleResult = ModuleEntity.Create(GetId(), code, name, description, sortOrder, createdBy);
        if (moduleResult.IsFailure)
        {
            return Result.Failure(moduleResult.Error);
        }

        _modules.Add(moduleResult.Value);
        TrackingState.MarkAsDirty();
        Props.Audit.Update(createdBy.GetValue());
        return Result.Success();
    }

    public Result RemoveModule(IdValueObject moduleId, ActorId updatedBy)
    {
        var module = FindModule(moduleId);
        if (module.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(Modules), DomainErrors.Common.NotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        _modules.Remove(module.Value);
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result UpdateModule(IdValueObject moduleId, Name name, Description description, int sortOrder, ActorId updatedBy)
    {
        var module = FindModule(moduleId);
        if (module.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(Modules), DomainErrors.Common.NotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var updateResult = module.Value.Update(name, description, sortOrder, updatedBy);
        if (updateResult.IsFailure)
        {
            return Result.Failure(updateResult.Error);
        }

        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result ActivateModule(IdValueObject moduleId, ActorId updatedBy)
    {
        var module = FindModule(moduleId);
        if (module.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(Modules), DomainErrors.Common.NotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var activateResult = module.Value.Activate(updatedBy);
        if (activateResult.IsFailure)
        {
            return Result.Failure(activateResult.Error);
        }

        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result DeactivateModule(IdValueObject moduleId, ActorId updatedBy)
    {
        var module = FindModule(moduleId);
        if (module.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(Modules), DomainErrors.Common.NotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var deactivateResult = module.Value.Deactivate(updatedBy);
        if (deactivateResult.IsFailure)
        {
            return Result.Failure(deactivateResult.Error);
        }

        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result AddConfiguration(ConfigurationKey key, ConfigurationValue value, ConfigurationScope scope, ActorId createdBy)
    {
        if (_configurations.Any(c => c.Key == key && c.Scope == scope))
        {
            BrokenRules.Add(new BrokenRule(nameof(Configurations), DomainErrors.System.ConfigurationKeyAlreadyExists));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var configResult = ConfigurationEntity.Create(key, value, scope);
        if (configResult.IsFailure)
        {
            return Result.Failure(configResult.Error);
        }

        _configurations.Add(configResult.Value);
        TrackingState.MarkAsDirty();
        Props.Audit.Update(createdBy.GetValue());
        return Result.Success();
    }

    public Result UpdateConfiguration(ConfigurationKey key, ConfigurationValue newValue, ActorId updatedBy)
    {
        var configuration = FindConfiguration(key);
        if (configuration.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(Configurations), DomainErrors.System.ConfigurationKeyNotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var index = _configurations.IndexOf(configuration.Value);
        _configurations[index] = configuration.Value.WithValue(newValue);
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result RemoveConfiguration(ConfigurationKey key, ActorId updatedBy)
    {
        var configuration = FindConfiguration(key);
        if (configuration.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(Configurations), DomainErrors.System.ConfigurationKeyNotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        _configurations.Remove(configuration.Value);
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result RegisterAction(ActionCode code, Name name, ActorId createdBy)
    {
        if (_actions.Any(a => a.Code == code))
        {
            BrokenRules.Add(new BrokenRule(nameof(Actions), DomainErrors.Common.Duplicate));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var systemSuiteId = SystemSuiteId.Load(Props.Id.GetValue());
        var actionResult = ActionEntity.Create(Props.TenantId, systemSuiteId, null, code, name, createdBy);
        if (actionResult.IsFailure)
        {
            return Result.Failure(actionResult.Error);
        }

        _actions.Add(actionResult.Value);
        TrackingState.MarkAsDirty();
        Props.Audit.Update(createdBy.GetValue());
        return Result.Success();
    }

    public Result RemoveAction(ActionCode code, ActorId updatedBy)
    {
        var action = FindAction(code);
        if (action.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(Actions), DomainErrors.Common.NotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        _actions.Remove(action.Value);
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    private Result<ModuleEntity> FindModule(IdValueObject moduleId)
    {
        var module = _modules.FirstOrDefault(m => m.Id.GetValue() == moduleId.GetValue());
        return module is null
            ? Result<ModuleEntity>.Failure(DomainErrors.Common.NotFound)
            : Result<ModuleEntity>.Success(module);
    }

    private Result<ConfigurationEntity> FindConfiguration(ConfigurationKey key)
    {
        var configuration = _configurations.FirstOrDefault(c => c.Key == key);
        return configuration is null
            ? Result<ConfigurationEntity>.Failure(DomainErrors.System.ConfigurationKeyNotFound)
            : Result<ConfigurationEntity>.Success(configuration);
    }

    private Result<ActionEntity> FindAction(ActionCode code)
    {
        var action = _actions.FirstOrDefault(a => a.Code == code);
        return action is null
            ? Result<ActionEntity>.Failure(DomainErrors.Common.NotFound)
            : Result<ActionEntity>.Success(action);
    }
}
