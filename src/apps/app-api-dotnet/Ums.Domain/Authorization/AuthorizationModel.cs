namespace Ums.Domain.Authorization;

using Ums.Domain.Kernel;

using Ums.Domain.Common;
using Ums.Domain.Enums;
using Ums.Domain.Events;
using Ums.Shell.Ddd;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Shell.Ddd.Interfaces;
using Ums.Shell.Ddd.ValueObjects.Common;

public class SystemSuiteProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public Code Code { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.Name Name { get; private set; }
    public StringValueObject BaseUrl { get; private set; }
    public StringValueObject? ApiCredentialHash { get; set; }
    public LifecycleStatus Status { get; set; }
    public AuditValueObject Audit { get; private set; }

    public SystemSuiteProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, Code code, global::Ums.Domain.Kernel.ValueObjects.Name name, StringValueObject baseUrl)
    {
        Id = id;
        TenantId = tenantId;
        Code = code;
        Name = name;
        BaseUrl = baseUrl;
        Status = LifecycleStatus.Draft;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

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
            return Result<SystemSuite>.Failure(DomainErrors.TenantRequired);

        var codeValue = global::Ums.Domain.Kernel.ValueObjects.Code.Create(code);

        if (string.IsNullOrWhiteSpace(name))
            return Result<SystemSuite>.Failure(DomainErrors.NameRequired);

        if (string.IsNullOrWhiteSpace(baseUrl))
            return Result<SystemSuite>.Failure("Base URL is required.");

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
            return Result.Failure("Module code must be unique inside the system.");

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
            return Result.Failure("Action code must be unique inside the system.");

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

public class FunctionalModuleProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId SystemSuiteId { get; private set; }
    public Code Code { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.Name Name { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.Description Description { get; private set; }
    public bool IsActive { get; set; }
    public AuditValueObject Audit { get; private set; }

    public FunctionalModuleProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId systemSuiteId, Code code, global::Ums.Domain.Kernel.ValueObjects.Name name, global::Ums.Domain.Kernel.ValueObjects.Description description)
    {
        Id = id;
        TenantId = tenantId;
        SystemSuiteId = systemSuiteId;
        Code = code;
        Name = name;
        Description = description;
        IsActive = true;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public sealed class FunctionalModule : Entity<FunctionalModule, FunctionalModuleProps>
{
    private readonly List<FunctionalSubmodule> _menus = new();

    private FunctionalModule(FunctionalModuleProps props) : base(props) { }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid SystemSuiteId => Props.SystemSuiteId.GetValue();
    public string Code => Props.Code.GetValue();
    public string Name => Props.Name.GetValue();
    public string Description => Props.Description.GetValue();
    public bool IsActive => Props.IsActive;
    public IReadOnlyCollection<FunctionalSubmodule> Menus => _menus.AsReadOnly();

    public static Result<FunctionalModule> Create(Guid tenantId, Guid systemSuiteId, string code, string name, string description)
    {
        if (tenantId == Guid.Empty || systemSuiteId == Guid.Empty)
            return Result<FunctionalModule>.Failure("Tenant and system identifiers are required.");

        var codeValue = global::Ums.Domain.Kernel.ValueObjects.Code.Create(code);

        if (string.IsNullOrWhiteSpace(name))
            return Result<FunctionalModule>.Failure(DomainErrors.NameRequired);

        if (string.IsNullOrWhiteSpace(description))
            return Result<FunctionalModule>.Failure(DomainErrors.DescriptionRequired);

        var props = new FunctionalModuleProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId.Load(systemSuiteId),
            codeValue,
            global::Ums.Domain.Kernel.ValueObjects.Name.Create(name.Trim()),
            global::Ums.Domain.Kernel.ValueObjects.Description.Create(description.Trim()));

        return Result<FunctionalModule>.Success(new FunctionalModule(props));
    }

    public Result AddMenu(string code, string label, int displayOrder, string? iconCode = null)
    {
        var menuResult = FunctionalSubmodule.Create(Props.TenantId.GetValue(), Props.SystemSuiteId.GetValue(), Props.Id.GetValue(), code, label, displayOrder, iconCode);
        if (menuResult.IsFailure)
            return Result.Failure(menuResult.Error);

        if (_menus.Any(menu => menu.Code == menuResult.Value.Code))
            return Result.Failure("Menu code must be unique inside the module.");

        _menus.Add(menuResult.Value);
        Props.Audit.Update("system");
        return Result.Success();
    }
}

public class FunctionalSubmoduleProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId SystemSuiteId { get; private set; }
    public IdValueObject ModuleId { get; private set; }
    public Code Code { get; private set; }
    public StringValueObject Label { get; private set; }
    public int DisplayOrder { get; private set; }
    public StringValueObject? IconCode { get; private set; }
    public bool IsActive { get; set; }
    public AuditValueObject Audit { get; private set; }

    public FunctionalSubmoduleProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId systemSuiteId, IdValueObject moduleId, Code code, StringValueObject label, int displayOrder, StringValueObject? iconCode)
    {
        Id = id;
        TenantId = tenantId;
        SystemSuiteId = systemSuiteId;
        ModuleId = moduleId;
        Code = code;
        Label = label;
        DisplayOrder = displayOrder;
        IconCode = iconCode;
        IsActive = true;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public sealed class FunctionalSubmodule : Entity<FunctionalSubmodule, FunctionalSubmoduleProps>
{
    private readonly List<FunctionalOption> _options = new();

    private FunctionalSubmodule(FunctionalSubmoduleProps props) : base(props) { }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid SystemSuiteId => Props.SystemSuiteId.GetValue();
    public Guid ModuleId => Props.ModuleId.GetValue();
    public string Code => Props.Code.GetValue();
    public string Label => Props.Label.GetValue();
    public int DisplayOrder => Props.DisplayOrder;
    public string? IconCode => Props.IconCode?.GetValue();
    public bool IsActive => Props.IsActive;
    public IReadOnlyCollection<FunctionalOption> Options => _options.AsReadOnly();

    public static Result<FunctionalSubmodule> Create(Guid tenantId, Guid systemSuiteId, Guid moduleId, string code, string label, int displayOrder, string? iconCode = null)
    {
        if (tenantId == Guid.Empty || systemSuiteId == Guid.Empty || moduleId == Guid.Empty)
            return Result<FunctionalSubmodule>.Failure("Tenant, system, and module identifiers are required.");

        var codeValue = global::Ums.Domain.Kernel.ValueObjects.Code.Create(code);

        if (string.IsNullOrWhiteSpace(label))
            return Result<FunctionalSubmodule>.Failure("Menu label is required.");

        var props = new FunctionalSubmoduleProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId.Load(systemSuiteId),
            IdValueObject.Load(moduleId),
            codeValue,
            global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(label.Trim()),
            displayOrder,
            iconCode != null ? global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(iconCode) : null);

        return Result<FunctionalSubmodule>.Success(new FunctionalSubmodule(props));
    }

    public Result AddOption(string code, string label, string routePath)
    {
        var optionResult = FunctionalOption.Create(Props.TenantId.GetValue(), Props.SystemSuiteId.GetValue(), Props.ModuleId.GetValue(), Props.Id.GetValue(), code, label, routePath);
        if (optionResult.IsFailure)
            return Result.Failure(optionResult.Error);

        if (_options.Any(option => option.Code == optionResult.Value.Code))
            return Result.Failure("Option code must be unique inside the menu.");

        _options.Add(optionResult.Value);
        Props.Audit.Update("system");
        return Result.Success();
    }
}

public class FunctionalOptionProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId SystemSuiteId { get; private set; }
    public IdValueObject ModuleId { get; private set; }
    public IdValueObject MenuId { get; private set; }
    public Code Code { get; private set; }
    public StringValueObject Label { get; private set; }
    public StringValueObject RoutePath { get; private set; }
    public bool IsActive { get; set; }
    public AuditValueObject Audit { get; private set; }

    public FunctionalOptionProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId systemSuiteId, IdValueObject moduleId, IdValueObject menuId, Code code, StringValueObject label, StringValueObject routePath)
    {
        Id = id;
        TenantId = tenantId;
        SystemSuiteId = systemSuiteId;
        ModuleId = moduleId;
        MenuId = menuId;
        Code = code;
        Label = label;
        RoutePath = routePath;
        IsActive = true;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public sealed class FunctionalOption : Entity<FunctionalOption, FunctionalOptionProps>
{
    private FunctionalOption(FunctionalOptionProps props) : base(props) { }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid SystemSuiteId => Props.SystemSuiteId.GetValue();
    public Guid ModuleId => Props.ModuleId.GetValue();
    public Guid MenuId => Props.MenuId.GetValue();
    public string Code => Props.Code.GetValue();
    public string Label => Props.Label.GetValue();
    public string RoutePath => Props.RoutePath.GetValue();
    public bool IsActive => Props.IsActive;

    public static Result<FunctionalOption> Create(Guid tenantId, Guid systemSuiteId, Guid moduleId, Guid menuId, string code, string label, string routePath)
    {
        if (tenantId == Guid.Empty || systemSuiteId == Guid.Empty || moduleId == Guid.Empty || menuId == Guid.Empty)
            return Result<FunctionalOption>.Failure("Tenant, system, module, and menu identifiers are required.");

        var codeValue = global::Ums.Domain.Kernel.ValueObjects.Code.Create(code);

        if (string.IsNullOrWhiteSpace(label))
            return Result<FunctionalOption>.Failure("Option label is required.");

        if (string.IsNullOrWhiteSpace(routePath))
            return Result<FunctionalOption>.Failure("Route path is required.");

        var props = new FunctionalOptionProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId.Load(systemSuiteId),
            IdValueObject.Load(moduleId),
            IdValueObject.Load(menuId),
            codeValue,
            global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(label.Trim()),
            global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(routePath.Trim()));

        return Result<FunctionalOption>.Success(new FunctionalOption(props));
    }
}

public class FunctionalActionProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId SystemSuiteId { get; private set; }
    public Code Code { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.Name Name { get; private set; }
    public FunctionalActionLevel Level { get; private set; }
    public IdValueObject? LevelId { get; private set; }
    public bool IsActive { get; set; }
    public AuditValueObject Audit { get; private set; }

    public FunctionalActionProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId systemSuiteId, Code code, global::Ums.Domain.Kernel.ValueObjects.Name name, FunctionalActionLevel level, IdValueObject? levelId)
    {
        Id = id;
        TenantId = tenantId;
        SystemSuiteId = systemSuiteId;
        Code = code;
        Name = name;
        Level = level;
        LevelId = levelId;
        IsActive = true;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public sealed class FunctionalAction : Entity<FunctionalAction, FunctionalActionProps>
{
    private FunctionalAction(FunctionalActionProps props) : base(props) { }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid SystemSuiteId => Props.SystemSuiteId.GetValue();
    public string Code => Props.Code.GetValue();
    public string Name => Props.Name.GetValue();
    public FunctionalActionLevel Level => Props.Level;
    public Guid? LevelId => Props.LevelId?.GetValue();
    public bool IsActive => Props.IsActive;

    public static Result<FunctionalAction> Create(Guid tenantId, Guid systemSuiteId, string code, string name, FunctionalActionLevel level, Guid? levelId = null)
    {
        if (tenantId == Guid.Empty || systemSuiteId == Guid.Empty)
            return Result<FunctionalAction>.Failure("Tenant and system identifiers are required.");

        if (level != FunctionalActionLevel.System && (!levelId.HasValue || levelId.Value == Guid.Empty))
            return Result<FunctionalAction>.Failure("Level identifier is required for non-system actions.");

        var codeValue = global::Ums.Domain.Kernel.ValueObjects.Code.Create(code);

        if (string.IsNullOrWhiteSpace(name))
            return Result<FunctionalAction>.Failure(DomainErrors.NameRequired);

        var props = new FunctionalActionProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId.Load(systemSuiteId),
            codeValue,
            global::Ums.Domain.Kernel.ValueObjects.Name.Create(name.Trim()),
            level,
            levelId.HasValue ? IdValueObject.Load(levelId.Value) : null);

        return Result<FunctionalAction>.Success(new FunctionalAction(props));
    }
}

public class RoleProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId SystemSuiteId { get; private set; }
    public Code Code { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.Name Name { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.Description Description { get; private set; }
    public IdValueObject? ParentRoleId { get; private set; }
    public bool IsActive { get; set; }
    public AuditValueObject Audit { get; private set; }

    public RoleProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId systemSuiteId, Code code, global::Ums.Domain.Kernel.ValueObjects.Name name, global::Ums.Domain.Kernel.ValueObjects.Description description, IdValueObject? parentRoleId)
    {
        Id = id;
        TenantId = tenantId;
        SystemSuiteId = systemSuiteId;
        Code = code;
        Name = name;
        Description = description;
        ParentRoleId = parentRoleId;
        IsActive = true;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public sealed class Role : AggregateRoot<Role, RoleProps>
{
    private Role(RoleProps props) : base(props)
    {
        if (TrackingState.IsNew)
        {
            DomainEvents.ApplyChange(new RoleCreatedEvent(Props.TenantId.GetValue(), Props.Id.GetValue(), Props.Code.GetValue()), true);
        }
    }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid SystemSuiteId => Props.SystemSuiteId.GetValue();
    public Guid? ParentRoleId => Props.ParentRoleId?.GetValue();
    public string Code => Props.Code.GetValue();
    public string Name => Props.Name.GetValue();
    public string Description => Props.Description.GetValue();
    public bool IsActive => Props.IsActive;

    public static Result<Role> Create(Guid tenantId, Guid systemSuiteId, string code, string name, string description, Guid? parentRoleId = null)
    {
        if (tenantId == Guid.Empty || systemSuiteId == Guid.Empty)
            return Result<Role>.Failure("Tenant and system identifiers are required.");

        var codeValue = global::Ums.Domain.Kernel.ValueObjects.Code.Create(code);

        if (string.IsNullOrWhiteSpace(name))
            return Result<Role>.Failure(DomainErrors.NameRequired);

        if (string.IsNullOrWhiteSpace(description))
            return Result<Role>.Failure(DomainErrors.DescriptionRequired);

        var props = new RoleProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId.Load(systemSuiteId),
            codeValue,
            global::Ums.Domain.Kernel.ValueObjects.Name.Create(name.Trim()),
            global::Ums.Domain.Kernel.ValueObjects.Description.Create(description.Trim()),
            parentRoleId.HasValue ? IdValueObject.Load(parentRoleId.Value) : null);

        var role = new Role(props);
        return Result<Role>.Success(role);
    }
}

public class PermissionTemplateProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId SystemSuiteId { get; private set; }
    public Code Code { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.Name Name { get; private set; }
    public StringValueObject TemplateVersion { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.Description Description { get; private set; }
    public IdValueObject CreatedBy { get; private set; }
    public LifecycleStatus Status { get; set; }
    public AuditValueObject Audit { get; private set; }

    public PermissionTemplateProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId systemSuiteId, Code code, global::Ums.Domain.Kernel.ValueObjects.Name name, global::Ums.Domain.Kernel.ValueObjects.Version version, global::Ums.Domain.Kernel.ValueObjects.Description description, IdValueObject createdBy)
    {
        Id = id;
        TenantId = tenantId;
        SystemSuiteId = systemSuiteId;
        Code = code;
        Name = name;
        TemplateVersion = version;
        Description = description;
        CreatedBy = createdBy;
        Status = LifecycleStatus.Draft;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public sealed class PermissionTemplate : AggregateRoot<PermissionTemplate, PermissionTemplateProps>
{
    private readonly List<AuthorizationGrant> _grants = new();

    private PermissionTemplate(PermissionTemplateProps props) : base(props) { }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid SystemSuiteId => Props.SystemSuiteId.GetValue();
    public string Code => Props.Code.GetValue();
    public string Name => Props.Name.GetValue();
    public string TemplateVersion => Props.TemplateVersion.GetValue();
    public string Description => Props.Description.GetValue();
    public Guid CreatedBy => Props.CreatedBy.GetValue();
    public LifecycleStatus Status => Props.Status;
    public IReadOnlyCollection<AuthorizationGrant> Grants => _grants.AsReadOnly();

    public static Result<PermissionTemplate> Create(Guid tenantId, Guid systemSuiteId, string code, string name, string version, string description, Guid createdBy)
    {
        if (tenantId == Guid.Empty || systemSuiteId == Guid.Empty || createdBy == Guid.Empty)
            return Result<PermissionTemplate>.Failure("Tenant, system, and creator identifiers are required.");

        var codeValue = global::Ums.Domain.Kernel.ValueObjects.Code.Create(code);

        if (string.IsNullOrWhiteSpace(name))
            return Result<PermissionTemplate>.Failure(DomainErrors.NameRequired);

        if (string.IsNullOrWhiteSpace(description))
            return Result<PermissionTemplate>.Failure(DomainErrors.DescriptionRequired);

        var props = new PermissionTemplateProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId.Load(systemSuiteId),
            codeValue,
            global::Ums.Domain.Kernel.ValueObjects.Name.Create(name.Trim()),
            global::Ums.Domain.Kernel.ValueObjects.Version.Create(version.Trim()),
            global::Ums.Domain.Kernel.ValueObjects.Description.Create(description.Trim()),
            IdValueObject.Load(createdBy));

        return Result<PermissionTemplate>.Success(new PermissionTemplate(props));
    }

    public Result AddGrant(Guid functionalActionId, PermissionEffect effect)
    {
        if (functionalActionId == Guid.Empty)
            return Result.Failure("Functional action identifier is required.");

        if (_grants.Any(grant => grant.FunctionalActionId == functionalActionId))
            return Result.Failure("Template already declares a grant for this action.");

        var grantProps = new AuthorizationGrantProps(
            IdValueObject.Create(),
            Props.TenantId,
            global::Ums.Domain.Authorization.ValueObjects.TemplateId.Load(Props.Id.GetValue()),
            null,
            global::Ums.Domain.Authorization.ValueObjects.FunctionalActionId.Load(functionalActionId),
            effect);

        _grants.Add(new AuthorizationGrant(grantProps));
        Props.Audit.Update("system");
        return Result.Success();
    }

    public Result Publish()
    {
        if (!_grants.Any())
            return Result.Failure("Permission template cannot be published without grants.");

        Props.Status = LifecycleStatus.Published;
        DomainEvents.ApplyChange(new PermissionTemplatePublishedEvent(Props.TenantId.GetValue(), Props.Id.GetValue(), Props.Code.GetValue(), Props.TemplateVersion.GetValue()), true);
        Props.Audit.Update("system");
        return Result.Success();
    }
}

public class ProfileProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public IdValueObject? BranchId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.Name Name { get; private set; }
    public IdValueObject? TemplateId { get; private set; }
    public bool AutoAssigned { get; private set; }
    public bool IsActive { get; set; }
    public AuditValueObject Audit { get; private set; }

    public ProfileProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Kernel.ValueObjects.BranchId? branchId, global::Ums.Domain.Kernel.ValueObjects.Name name, global::Ums.Domain.Authorization.ValueObjects.TemplateId? templateId, bool autoAssigned)
    {
        Id = id;
        TenantId = tenantId;
        BranchId = branchId;
        Name = name;
        TemplateId = templateId;
        AutoAssigned = autoAssigned;
        IsActive = true;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public sealed class Profile : AggregateRoot<Profile, ProfileProps>
{
    private readonly List<AuthorizationGrant> _grants = new();
    private readonly List<Guid> _roleIds = new();

    private Profile(ProfileProps props) : base(props)
    {
        if (TrackingState.IsNew)
        {
            DomainEvents.ApplyChange(new ProfileCreatedEvent(Props.TenantId.GetValue(), Props.Id.GetValue(), Props.Name.GetValue()), true);
        }
    }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid? BranchId => Props.BranchId?.GetValue();
    public string Name => Props.Name.GetValue();
    public Guid? TemplateId => Props.TemplateId?.GetValue();
    public bool AutoAssigned => Props.AutoAssigned;
    public bool IsActive => Props.IsActive;
    public IReadOnlyCollection<AuthorizationGrant> Grants => _grants.AsReadOnly();
    public IReadOnlyCollection<Guid> RoleIds => _roleIds.AsReadOnly();

    public static Result<Profile> Create(Guid tenantId, string name, Guid? branchId = null, Guid? templateId = null, bool autoAssigned = false)
    {
        if (tenantId == Guid.Empty)
            return Result<Profile>.Failure(DomainErrors.TenantRequired);

        if (string.IsNullOrWhiteSpace(name))
            return Result<Profile>.Failure(DomainErrors.NameRequired);

        var props = new ProfileProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            branchId.HasValue ? global::Ums.Domain.Kernel.ValueObjects.BranchId.Load(branchId.Value) : null,
            global::Ums.Domain.Kernel.ValueObjects.Name.Create(name.Trim()),
            templateId.HasValue ? global::Ums.Domain.Authorization.ValueObjects.TemplateId.Load(templateId.Value) : null,
            autoAssigned);

        var profile = new Profile(props);
        return Result<Profile>.Success(profile);
    }

    public Result AddRole(Guid roleId)
    {
        if (roleId == Guid.Empty)
            return Result.Failure("Role identifier is required.");

        if (_roleIds.Contains(roleId))
            return Result.Failure("Role is already assigned to profile.");

        _roleIds.Add(roleId);
        Props.Audit.Update("system");
        return Result.Success();
    }

    public Result AddGrant(Guid functionalActionId, PermissionEffect effect)
    {
        if (functionalActionId == Guid.Empty)
            return Result.Failure("Functional action identifier is required.");

        var existingGrant = _grants.FirstOrDefault(grant => grant.FunctionalActionId == functionalActionId);
        if (existingGrant is not null)
            return existingGrant.ChangeEffect(effect);

        var grantProps = new AuthorizationGrantProps(
            IdValueObject.Create(),
            Props.TenantId,
            null,
            global::Ums.Domain.Authorization.ValueObjects.ProfileId.Load(Props.Id.GetValue()),
            global::Ums.Domain.Authorization.ValueObjects.FunctionalActionId.Load(functionalActionId),
            effect);

        _grants.Add(new AuthorizationGrant(grantProps));
        DomainEvents.ApplyChange(new ProfilePermissionChangedEvent(Props.TenantId.GetValue(), Props.Id.GetValue(), functionalActionId), true);
        Props.Audit.Update("system");
        return Result.Success();
    }
}

public class AuthorizationGrantProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public IdValueObject? TemplateId { get; private set; }
    public IdValueObject? ProfileId { get; private set; }
    public global::Ums.Domain.Authorization.ValueObjects.FunctionalActionId FunctionalActionId { get; private set; }
    public PermissionEffect Effect { get; set; }
    public AuditValueObject Audit { get; private set; }

    public AuthorizationGrantProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Authorization.ValueObjects.TemplateId? templateId, global::Ums.Domain.Authorization.ValueObjects.ProfileId? profileId, global::Ums.Domain.Authorization.ValueObjects.FunctionalActionId functionalActionId, PermissionEffect effect)
    {
        Id = id;
        TenantId = tenantId;
        TemplateId = templateId;
        ProfileId = profileId;
        FunctionalActionId = functionalActionId;
        Effect = effect;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public sealed class AuthorizationGrant : Entity<AuthorizationGrant, AuthorizationGrantProps>
{
    internal AuthorizationGrant(AuthorizationGrantProps props) : base(props) { }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid? TemplateId => Props.TemplateId?.GetValue();
    public Guid? ProfileId => Props.ProfileId?.GetValue();
    public Guid FunctionalActionId => Props.FunctionalActionId.GetValue();
    public PermissionEffect Effect => Props.Effect;

    public Result ChangeEffect(PermissionEffect effect)
    {
        Props.Effect = effect;
        Props.Audit.Update("system");
        return Result.Success();
    }
}

public class TemplateAssignmentRuleProps : ParametricCatalogProps
{
    public global::Ums.Domain.Authorization.ValueObjects.TemplateId TemplateId { get; set; } = default!;
    public global::Ums.Domain.Authorization.ValueObjects.PredicateExpression PredicateExpression { get; set; } = default!;
    public int Priority { get; set; }

    public TemplateAssignmentRuleProps()
    {
        Id = IdValueObject.Create();
        Audit = AuditValueObject.Create("system");
    }
}

public sealed class TemplateAssignmentRule : ParametricCatalogEntity<TemplateAssignmentRule, TemplateAssignmentRuleProps>
{
    private TemplateAssignmentRule(TemplateAssignmentRuleProps props) : base(props) { }

    public Guid TemplateId => Props.TemplateId.GetValue();
    public string PredicateExpression => Props.PredicateExpression.GetValue();
    public int Priority => Props.Priority;

    public static Result<TemplateAssignmentRule> Create(
        Guid tenantId,
        Guid templateId,
        string code,
        string value,
        string description,
        string predicateExpression,
        int priority,
        string version = "1.0.0")
    {
        if (templateId == Guid.Empty)
            return Result<TemplateAssignmentRule>.Failure("Template identifier is required.");

        if (string.IsNullOrWhiteSpace(predicateExpression))
            return Result<TemplateAssignmentRule>.Failure("Predicate expression is required.");

        var props = new TemplateAssignmentRuleProps
        {
            TemplateId = global::Ums.Domain.Authorization.ValueObjects.TemplateId.Load(templateId),
            PredicateExpression = global::Ums.Domain.Authorization.ValueObjects.PredicateExpression.Create(predicateExpression.Trim()),
            Priority = priority
        };

        var rule = new TemplateAssignmentRule(props);
        var result = rule.SetCatalogFields(tenantId, code, value, description, version);
        
        return result.IsFailure ? Result<TemplateAssignmentRule>.Failure(result.Error) : Result<TemplateAssignmentRule>.Success(rule);
    }
}

public class CompiledPolicyGraphProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.UserId UserId { get; private set; }
    public global::Ums.Domain.Authorization.ValueObjects.GraphHash GraphHash { get; private set; }
    public global::Ums.Domain.Authorization.ValueObjects.Payload Payload { get; private set; }
    public DateTimeOffset CompiledAt { get; private set; }

    public CompiledPolicyGraphProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Kernel.ValueObjects.UserId userId, global::Ums.Domain.Authorization.ValueObjects.GraphHash graphHash, global::Ums.Domain.Authorization.ValueObjects.Payload payload)
    {
        Id = id;
        TenantId = tenantId;
        UserId = userId;
        GraphHash = graphHash;
        Payload = payload;
        CompiledAt = DateTimeOffset.UtcNow;
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public sealed class CompiledPolicyGraph : Entity<CompiledPolicyGraph, CompiledPolicyGraphProps>
{
    private CompiledPolicyGraph(CompiledPolicyGraphProps props) : base(props) { }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid UserId => Props.UserId.GetValue();
    public string GraphHash => Props.GraphHash.GetValue();
    public string Payload => Props.Payload.GetValue();
    public DateTimeOffset CompiledAt => Props.CompiledAt;

    public static Result<CompiledPolicyGraph> Create(Guid tenantId, Guid userId, string graphHash, string payload)
    {
        if (tenantId == Guid.Empty || userId == Guid.Empty)
            return Result<CompiledPolicyGraph>.Failure("Tenant and user identifiers are required.");

        if (string.IsNullOrWhiteSpace(graphHash))
            return Result<CompiledPolicyGraph>.Failure("Graph hash is required.");

        var props = new CompiledPolicyGraphProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            global::Ums.Domain.Kernel.ValueObjects.UserId.Load(userId),
            global::Ums.Domain.Authorization.ValueObjects.GraphHash.Create(graphHash.Trim()),
            global::Ums.Domain.Authorization.ValueObjects.Payload.Create(string.IsNullOrWhiteSpace(payload) ? "{}" : payload.Trim()));

        return Result<CompiledPolicyGraph>.Success(new CompiledPolicyGraph(props));
    }
}
