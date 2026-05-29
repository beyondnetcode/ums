namespace Ums.Domain.Authorization.SystemSuite.Module;

using Ums.Domain.Authorization.SystemSuite.Menu;
using MenuEntity = Ums.Domain.Authorization.SystemSuite.Menu.Menu;

public sealed class Module : Entity<Module, ModuleProps>
{
    private readonly List<MenuEntity> _menus = new();

    private Module(ModuleProps props) : base(props)
    {
    }

    public SystemId SystemId => Props.SystemId;
    public Code Code => Props.Code;
    public Name Name => Props.Name;
    public Description Description => Props.Description;
    public ModuleStatus Status => Props.Status;
    public int SortOrder => Props.SortOrder;

    public IReadOnlyCollection<MenuEntity> Menus => _menus.AsReadOnly();

    public ModuleId GetId() => ModuleId.Load(Props.Id.GetValue());

    public static Result<Module> Create(
        SystemId systemId,
        Code code,
        Name name,
        Description description,
        int sortOrder,
        ActorId createdBy)
    {
        var props = new ModuleProps(IdValueObject.Create(), systemId, code, name, description, ModuleStatus.Inactive, sortOrder, createdBy);
        var module = new Module(props);

        if (!module.IsValid())
        {
            return Result<Module>.Failure(module.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<Module>.Success(module);
    }

    public Result Update(Name name, Description description, int sortOrder, ActorId updatedBy)
    {
        SetProps(Props.WithName(name).WithDescription(description).WithSortOrder(sortOrder));

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result Activate(ActorId updatedBy)
    {
        BrokenRules.Clear();

        if (Status == ModuleStatus.Active)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.SystemSuite.ModuleAlreadyActive));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        SetProps(Props.WithStatus(ModuleStatus.Active));
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result Deactivate(ActorId updatedBy)
    {
        BrokenRules.Clear();

        if (Status == ModuleStatus.Inactive)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.SystemSuite.ModuleAlreadyInactive));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        SetProps(Props.WithStatus(ModuleStatus.Inactive));
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result AddMenu(Code code, Name label, Description description, int sortOrder, ActorId createdBy)
    {
        BrokenRules.Clear();

        if (Status == ModuleStatus.Inactive)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.SystemSuite.ModuleInactiveCannotAddMenu));
        }

        if (_menus.Any(m => m.Code == code))
        {
            BrokenRules.Add(new BrokenRule(nameof(Menus), DomainErrors.SystemSuite.MenuCodeNotUnique));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var menuResult = MenuEntity.Create(GetId(), code, label, description, sortOrder, createdBy);
        if (menuResult.IsFailure)
        {
            return Result.Failure(menuResult.Error);
        }

        _menus.Add(menuResult.Value);
        Props.Audit.Update(createdBy.GetValue());
        return Result.Success();
    }

    public Result RemoveMenu(IdValueObject menuId, ActorId updatedBy)
    {
        var menu = FindMenu(menuId);
        if (menu.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(Menus), DomainErrors.Common.NotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        _menus.Remove(menu.Value);
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result UpdateMenu(IdValueObject menuId, Name label, Description description, int sortOrder, ActorId updatedBy)
    {
        var menu = FindMenu(menuId);
        if (menu.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(Menus), DomainErrors.Common.NotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var updateResult = menu.Value.Update(label, description, sortOrder, updatedBy);
        if (updateResult.IsFailure)
        {
            return Result.Failure(updateResult.Error);
        }

        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    private Result<MenuEntity> FindMenu(IdValueObject menuId)
    {
        var menu = _menus.FirstOrDefault(m =>
            m.Props.Id.GetValue() == menuId.GetValue() ||
            m.Id.GetValue() == menuId.GetValue());
        return menu is null
            ? Result<MenuEntity>.Failure(DomainErrors.Common.NotFound)
            : Result<MenuEntity>.Success(menu);
    }
}
