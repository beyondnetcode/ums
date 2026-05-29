namespace Ums.Domain.Authorization.SystemSuite;

using Ums.Domain.Authorization.SystemSuite.AppSetting;
using Ums.Domain.Authorization.SystemSuite.Module;
using Ums.Domain.Authorization.SystemSuite.Action;
using Ums.Domain.Authorization.SystemSuite.Events;
using Ums.Domain.Authorization.SystemSuite.DomainResource;
using ModuleEntity = Ums.Domain.Authorization.SystemSuite.Module.Module;
using AppSettingEntity = Ums.Domain.Authorization.SystemSuite.AppSetting.AppSetting;
using ActionEntity = Ums.Domain.Authorization.SystemSuite.Action.Action;
using DomainResourceEntity = Ums.Domain.Authorization.SystemSuite.DomainResource.DomainResource;

public sealed class SystemSuite : AggregateRoot<SystemSuite, SystemSuiteProps>
{
    private readonly List<ModuleEntity> _modules = new();
    private readonly List<AppSettingEntity> _appSettings = new();
    private readonly List<ActionEntity> _actions = new();
    private readonly List<DomainResourceEntity> _domainResources = new();

    public new SystemSuiteDomainEventsManager DomainEvents { get; }

    private SystemSuite(SystemSuiteProps props) : base(props)
    {
        DomainEvents = new SystemSuiteDomainEventsManager(this);

        if (TrackingState.IsNew)
        {
            DomainEvents.RaiseEvent(new SystemSuiteRegisteredEvent(props.Id.GetValue(), props.TenantId.GetValue(), props.Code.GetValue()));
        }
    }

    public TenantId TenantId => Props.TenantId;
    public Code Code => Props.Code;
    public Name Name => Props.Name;
    public Description Description => Props.Description;
    public SystemStatus Status => Props.Status;

    public IReadOnlyCollection<ModuleEntity> Modules => _modules.AsReadOnly();
    public IReadOnlyCollection<AppSettingEntity> AppSettings => _appSettings.AsReadOnly();
    public IReadOnlyCollection<ActionEntity> Actions => _actions.AsReadOnly();
    public IReadOnlyCollection<DomainResourceEntity> DomainResources => _domainResources.AsReadOnly();

    public SystemSuiteId GetId() => SystemSuiteId.Load(Props.Id.GetValue());

    public static Result<SystemSuite> Create(
        TenantId tenantId,
        Code code,
        Name name,
        Description description,
        ActorId createdBy)
    {
        var props = new SystemSuiteProps(IdValueObject.Create(), tenantId, code, name, description, SystemStatus.Active, createdBy);
        var suite = new SystemSuite(props);

        if (!suite.IsValid())
        {
            return Result<SystemSuite>.Failure(suite.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<SystemSuite>.Success(suite);
    }

    public Result Update(Name name, Description description, ActorId updatedBy)
    {
        SetProps(Props.WithName(name).WithDescription(description));

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result SetStatus(SystemStatus status, ActorId updatedBy)
    {
        SetProps(Props.WithStatus(status));
        DomainEvents.RaiseEvent(new SystemSuiteStatusChangedEvent(Props.Id.GetValue(), status.Name));
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result AddModule(Code code, Name name, Description description, int sortOrder, ActorId createdBy)
    {
        BrokenRules.Clear();

        if (_modules.Any(m => m.Code == code))
        {
            BrokenRules.Add(new BrokenRule(nameof(Modules), DomainErrors.SystemSuite.ModuleCodeNotUnique));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var moduleResult = ModuleEntity.Create(SystemId.Load(Props.Id.GetValue()), code, name, description, sortOrder, createdBy);
        if (moduleResult.IsFailure)
        {
            return Result.Failure(moduleResult.Error);
        }

        _modules.Add(moduleResult.Value);
        DomainEvents.RaiseEvent(new SystemSuiteModuleAddedEvent(Props.Id.GetValue(), moduleResult.Value.GetId().GetValue(), code.GetValue()));
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
        DomainEvents.RaiseEvent(new SystemSuiteModuleRemovedEvent(Props.Id.GetValue(), module.Value.GetId().GetValue()));
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

        DomainEvents.RaiseEvent(new SystemSuiteModuleStatusChangedEvent(Props.Id.GetValue(), module.Value.GetId().GetValue(), "Active"));
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

        DomainEvents.RaiseEvent(new SystemSuiteModuleStatusChangedEvent(Props.Id.GetValue(), module.Value.GetId().GetValue(), "Inactive"));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result AddAppSetting(ConfigurationKey key, ConfigurationValue value, ConfigurationScope scope, ActorId createdBy)
    {
        if (_appSettings.Any(c => c.Key == key && c.Scope == scope))
        {
            BrokenRules.Add(new BrokenRule(nameof(AppSettings), DomainErrors.SystemSuite.ConfigurationKeyAlreadyExists));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var settingResult = AppSettingEntity.Create(key, value, scope);
        if (settingResult.IsFailure)
        {
            return Result.Failure(settingResult.Error);
        }

        _appSettings.Add(settingResult.Value);
        TrackingState.MarkAsDirty();
        Props.Audit.Update(createdBy.GetValue());
        return Result.Success();
    }

    public Result UpdateAppSetting(ConfigurationKey key, ConfigurationValue newValue, ActorId updatedBy)
    {
        var setting = FindAppSetting(key);
        if (setting.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(AppSettings), DomainErrors.SystemSuite.ConfigurationKeyNotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var index = _appSettings.IndexOf(setting.Value);
        _appSettings[index] = setting.Value.WithValue(newValue);
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result RemoveAppSetting(ConfigurationKey key, ActorId updatedBy)
    {
        var setting = FindAppSetting(key);
        if (setting.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(AppSettings), DomainErrors.SystemSuite.ConfigurationKeyNotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        _appSettings.Remove(setting.Value);
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    // ── Menu lifecycle ────────────────────────────────────────────────────────

    public Result AddMenu(IdValueObject moduleId, Code code, Name label, Description description, int sortOrder, ActorId createdBy)
    {
        var module = FindModule(moduleId);
        if (module.IsFailure)
            return Result.Failure(module.Error);

        var result = module.Value.AddMenu(code, label, description, sortOrder, createdBy);
        if (result.IsFailure) return result;

        TrackingState.MarkAsDirty();
        Props.Audit.Update(createdBy.GetValue());
        return Result.Success();
    }

    public Result UpdateMenu(IdValueObject moduleId, IdValueObject menuId, Name label, Description description, int sortOrder, ActorId updatedBy)
    {
        var module = FindModule(moduleId);
        if (module.IsFailure)
            return Result.Failure(module.Error);

        var result = module.Value.UpdateMenu(menuId, label, description, sortOrder, updatedBy);
        if (result.IsFailure) return result;

        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result RemoveMenu(IdValueObject moduleId, IdValueObject menuId, ActorId updatedBy)
    {
        var module = FindModule(moduleId);
        if (module.IsFailure)
            return Result.Failure(module.Error);

        var result = module.Value.RemoveMenu(menuId, updatedBy);
        if (result.IsFailure) return result;

        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    // ── SubMenu lifecycle ─────────────────────────────────────────────────────

    public Result AddSubMenu(IdValueObject moduleId, IdValueObject menuId, Code code, Name label, Description description, int sortOrder, ActorId createdBy)
    {
        var module = FindModule(moduleId);
        if (module.IsFailure)
            return Result.Failure(module.Error);

        var menu = module.Value.Menus.FirstOrDefault(m => m.Props.Id.GetValue() == menuId.GetValue());
        if (menu is null)
            return Result.Failure(DomainErrors.Common.NotFound);

        var result = menu.AddSubMenu(code, label, description, sortOrder, createdBy);
        if (result.IsFailure) return result;

        TrackingState.MarkAsDirty();
        Props.Audit.Update(createdBy.GetValue());
        return Result.Success();
    }

    public Result UpdateSubMenu(IdValueObject moduleId, IdValueObject menuId, IdValueObject subMenuId, Name label, Description description, int sortOrder, ActorId updatedBy)
    {
        var module = FindModule(moduleId);
        if (module.IsFailure)
            return Result.Failure(module.Error);

        var menu = module.Value.Menus.FirstOrDefault(m => m.Props.Id.GetValue() == menuId.GetValue());
        if (menu is null)
            return Result.Failure(DomainErrors.Common.NotFound);

        var result = menu.UpdateSubMenu(subMenuId, label, description, sortOrder, updatedBy);
        if (result.IsFailure) return result;

        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result RemoveSubMenu(IdValueObject moduleId, IdValueObject menuId, IdValueObject subMenuId, ActorId updatedBy)
    {
        var module = FindModule(moduleId);
        if (module.IsFailure)
            return Result.Failure(module.Error);

        var menu = module.Value.Menus.FirstOrDefault(m => m.Props.Id.GetValue() == menuId.GetValue());
        if (menu is null)
            return Result.Failure(DomainErrors.Common.NotFound);

        var result = menu.RemoveSubMenu(subMenuId, updatedBy);
        if (result.IsFailure) return result;

        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    // ── Option lifecycle ──────────────────────────────────────────────────────

    public Result AddOption(IdValueObject moduleId, IdValueObject menuId, IdValueObject subMenuId, Code code, Name label, Description description, ActionCode actionCode, int sortOrder, ActorId createdBy)
    {
        var module = FindModule(moduleId);
        if (module.IsFailure)
            return Result.Failure(module.Error);

        var menu = module.Value.Menus.FirstOrDefault(m => m.Props.Id.GetValue() == menuId.GetValue());
        if (menu is null)
            return Result.Failure(DomainErrors.Common.NotFound);

        var subMenu = menu.SubMenus.FirstOrDefault(sm => sm.Props.Id.GetValue() == subMenuId.GetValue());
        if (subMenu is null)
            return Result.Failure(DomainErrors.Common.NotFound);

        var result = subMenu.AddOption(code, label, description, actionCode, sortOrder, createdBy);
        if (result.IsFailure) return result;

        TrackingState.MarkAsDirty();
        Props.Audit.Update(createdBy.GetValue());
        return Result.Success();
    }

    public Result UpdateOption(IdValueObject moduleId, IdValueObject menuId, IdValueObject subMenuId, IdValueObject optionId, Name label, Description description, ActionCode actionCode, int sortOrder, ActorId updatedBy)
    {
        var module = FindModule(moduleId);
        if (module.IsFailure)
            return Result.Failure(module.Error);

        var menu = module.Value.Menus.FirstOrDefault(m => m.Props.Id.GetValue() == menuId.GetValue());
        if (menu is null)
            return Result.Failure(DomainErrors.Common.NotFound);

        var subMenu = menu.SubMenus.FirstOrDefault(sm => sm.Props.Id.GetValue() == subMenuId.GetValue());
        if (subMenu is null)
            return Result.Failure(DomainErrors.Common.NotFound);

        var result = subMenu.UpdateOption(optionId, label, description, actionCode, sortOrder, updatedBy);
        if (result.IsFailure) return result;

        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result RemoveOption(IdValueObject moduleId, IdValueObject menuId, IdValueObject subMenuId, IdValueObject optionId, ActorId updatedBy)
    {
        var module = FindModule(moduleId);
        if (module.IsFailure)
            return Result.Failure(module.Error);

        var menu = module.Value.Menus.FirstOrDefault(m => m.Props.Id.GetValue() == menuId.GetValue());
        if (menu is null)
            return Result.Failure(DomainErrors.Common.NotFound);

        var subMenu = menu.SubMenus.FirstOrDefault(sm => sm.Props.Id.GetValue() == subMenuId.GetValue());
        if (subMenu is null)
            return Result.Failure(DomainErrors.Common.NotFound);

        var result = subMenu.RemoveOption(optionId, updatedBy);
        if (result.IsFailure) return result;

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

        var actionResult = ActionEntity.Create(Props.TenantId, GetId(), null, code, name, createdBy);
        if (actionResult.IsFailure)
        {
            return Result.Failure(actionResult.Error);
        }

        _actions.Add(actionResult.Value);
        DomainEvents.RaiseEvent(new SystemSuiteActionRegisteredEvent(Props.Id.GetValue(), code.GetValue()));
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
        DomainEvents.RaiseEvent(new SystemSuiteActionRemovedEvent(Props.Id.GetValue(), code.GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    // ── Domain Resource lifecycle ─────────────────────────────────────────────

    public Result AddDomainResource(ModuleId? moduleId, DomainResourceType type, Code code, Name name, Description description, ActorId createdBy)
    {
        if (_domainResources.Any(dr => dr.Code == code))
        {
            BrokenRules.Add(new BrokenRule(nameof(DomainResources), DomainErrors.Common.Duplicate)); // Alternatively, a specific error
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var drResult = DomainResourceEntity.Create(GetId(), moduleId, type, code, name, description, createdBy);
        if (drResult.IsFailure)
        {
            return Result.Failure(drResult.Error);
        }

        _domainResources.Add(drResult.Value);
        TrackingState.MarkAsDirty();
        Props.Audit.Update(createdBy.GetValue());
        return Result.Success();
    }

    public Result UpdateDomainResource(IdValueObject resourceId, Name name, Description description, ActorId updatedBy)
    {
        var resource = FindDomainResource(resourceId);
        if (resource.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(DomainResources), DomainErrors.Common.NotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var updateResult = resource.Value.Update(name, description, updatedBy);
        if (updateResult.IsFailure)
        {
            return Result.Failure(updateResult.Error);
        }

        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result RemoveDomainResource(IdValueObject resourceId, ActorId updatedBy)
    {
        var resource = FindDomainResource(resourceId);
        if (resource.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(DomainResources), DomainErrors.Common.NotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        _domainResources.Remove(resource.Value);
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    private Result<ModuleEntity> FindModule(IdValueObject moduleId)
    {
        var module = _modules.FirstOrDefault(m =>
            m.Props.Id.GetValue() == moduleId.GetValue() ||
            m.Id.GetValue() == moduleId.GetValue());
        return module is null
            ? Result<ModuleEntity>.Failure(DomainErrors.Common.NotFound)
            : Result<ModuleEntity>.Success(module);
    }

    private Result<AppSettingEntity> FindAppSetting(ConfigurationKey key)
    {
        var setting = _appSettings.FirstOrDefault(c => c.Key == key);
        return setting is null
            ? Result<AppSettingEntity>.Failure(DomainErrors.SystemSuite.ConfigurationKeyNotFound)
            : Result<AppSettingEntity>.Success(setting);
    }

    private Result<ActionEntity> FindAction(ActionCode code)
    {
        var action = _actions.FirstOrDefault(a => a.Code == code);
        return action is null
            ? Result<ActionEntity>.Failure(DomainErrors.Common.NotFound)
            : Result<ActionEntity>.Success(action);
    }

    private Result<DomainResourceEntity> FindDomainResource(IdValueObject resourceId)
    {
        var resource = _domainResources.FirstOrDefault(r => r.Props.Id.GetValue() == resourceId.GetValue());
        return resource is null
            ? Result<DomainResourceEntity>.Failure(DomainErrors.Common.NotFound)
            : Result<DomainResourceEntity>.Success(resource);
    }
}
