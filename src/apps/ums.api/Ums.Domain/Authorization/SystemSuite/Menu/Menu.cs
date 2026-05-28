namespace Ums.Domain.Authorization.SystemSuite.Menu;

using Ums.Domain.Authorization.SystemSuite.SubMenu;
using SubMenuEntity = Ums.Domain.Authorization.SystemSuite.SubMenu.SubMenu;

public sealed class Menu : Entity<Menu, MenuProps>
{
    private readonly List<SubMenuEntity> _subMenus = new();

    private Menu(MenuProps props) : base(props)
    {
    }

    public ModuleId ModuleId => Props.ModuleId;
    public Code Code => Props.Code;
    public Name Label => Props.Label;
    public Description Description => Props.Description;
    public int SortOrder => Props.SortOrder;

    public IReadOnlyCollection<SubMenuEntity> SubMenus => _subMenus.AsReadOnly();

    public MenuId GetId() => MenuId.Load(Props.Id.GetValue());

    public static Result<Menu> Create(
        ModuleId moduleId,
        Code code,
        Name label,
        Description description,
        int sortOrder,
        ActorId createdBy)
    {
        var props = new MenuProps(IdValueObject.Create(), moduleId, code, label, description, sortOrder, createdBy);
        var menu = new Menu(props);

        if (!menu.IsValid())
        {
            return Result<Menu>.Failure(menu.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<Menu>.Success(menu);
    }

    public Result Update(Name label, Description description, int sortOrder, ActorId updatedBy)
    {
        Props.Label = label;
        Props.Description = description;
        Props.SortOrder = sortOrder;

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result AddSubMenu(Code code, Name label, Description description, int sortOrder, ActorId createdBy)
    {
        BrokenRules.Clear();

        if (_subMenus.Any(sm => sm.Code == code))
        {
            BrokenRules.Add(new BrokenRule(nameof(SubMenus), DomainErrors.SystemSuite.SubMenuCodeNotUnique));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var subMenuResult = SubMenuEntity.Create(GetId(), code, label, description, sortOrder, createdBy);
        if (subMenuResult.IsFailure)
        {
            return Result.Failure(subMenuResult.Error);
        }

        _subMenus.Add(subMenuResult.Value);
        Props.Audit.Update(createdBy.GetValue());
        return Result.Success();
    }

    public Result RemoveSubMenu(IdValueObject subMenuId, ActorId updatedBy)
    {
        var subMenu = FindSubMenu(subMenuId);
        if (subMenu.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(SubMenus), DomainErrors.Common.NotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        _subMenus.Remove(subMenu.Value);
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result UpdateSubMenu(IdValueObject subMenuId, Name label, Description description, int sortOrder, ActorId updatedBy)
    {
        var subMenu = FindSubMenu(subMenuId);
        if (subMenu.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(SubMenus), DomainErrors.Common.NotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var updateResult = subMenu.Value.Update(label, description, sortOrder, updatedBy);
        if (updateResult.IsFailure)
        {
            return Result.Failure(updateResult.Error);
        }

        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    private Result<SubMenuEntity> FindSubMenu(IdValueObject subMenuId)
    {
        var subMenu = _subMenus.FirstOrDefault(sm =>
            sm.Props.Id.GetValue() == subMenuId.GetValue() ||
            sm.Id.GetValue() == subMenuId.GetValue());
        return subMenu is null
            ? Result<SubMenuEntity>.Failure(DomainErrors.Common.NotFound)
            : Result<SubMenuEntity>.Success(subMenu);
    }
}
