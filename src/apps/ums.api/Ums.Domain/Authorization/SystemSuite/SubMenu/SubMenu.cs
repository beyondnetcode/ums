namespace Ums.Domain.Authorization.SystemSuite.SubMenu;

using Ums.Domain.Authorization.SystemSuite.Option;
using OptionEntity = Ums.Domain.Authorization.SystemSuite.Option.Option;

public sealed class SubMenu : Entity<SubMenu, SubMenuProps>
{
    private readonly List<OptionEntity> _options = new();

    private SubMenu(SubMenuProps props) : base(props)
    {
    }

    public MenuId MenuId => Props.MenuId;
    public Code Code => Props.Code;
    public Name Label => Props.Label;
    public Description Description => Props.Description;
    public int SortOrder => Props.SortOrder;

    public IReadOnlyCollection<OptionEntity> Options => _options.AsReadOnly();

    public SubMenuId GetId() => SubMenuId.Load(Props.Id.GetValue());

    public static Result<SubMenu> Create(
        MenuId menuId,
        Code code,
        Name label,
        Description description,
        int sortOrder,
        ActorId createdBy)
    {
        var props = new SubMenuProps(IdValueObject.Create(), menuId, code, label, description, sortOrder, createdBy);
        var subMenu = new SubMenu(props);

        if (!subMenu.IsValid())
        {
            return Result<SubMenu>.Failure(subMenu.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<SubMenu>.Success(subMenu);
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

    public Result AddOption(Code code, Name label, Description description, ActionCode actionCode, int sortOrder, ActorId createdBy)
    {
        if (_options.Any(o => o.Code == code))
        {
            BrokenRules.Add(new BrokenRule(nameof(Options), DomainErrors.SystemSuite.OptionCodeNotUnique));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var optionResult = OptionEntity.Create(GetId(), code, label, description, actionCode, sortOrder, createdBy);
        if (optionResult.IsFailure)
        {
            return Result.Failure(optionResult.Error);
        }

        _options.Add(optionResult.Value);
        Props.Audit.Update(createdBy.GetValue());
        return Result.Success();
    }

    public Result RemoveOption(IdValueObject optionId, ActorId updatedBy)
    {
        var option = FindOption(optionId);
        if (option.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(Options), DomainErrors.Common.NotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        _options.Remove(option.Value);
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result UpdateOption(IdValueObject optionId, Name label, Description description, ActionCode actionCode, int sortOrder, ActorId updatedBy)
    {
        var option = FindOption(optionId);
        if (option.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(Options), DomainErrors.Common.NotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var updateResult = option.Value.Update(label, description, actionCode, sortOrder, updatedBy);
        if (updateResult.IsFailure)
        {
            return Result.Failure(updateResult.Error);
        }

        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    private Result<OptionEntity> FindOption(IdValueObject optionId)
    {
        // Use Props.Id (stable database GUID). Entity.Id is a transient random GUID
        // generated on each rehydration and must not be used for cross-request lookups.
        var option = _options.FirstOrDefault(o => o.Props.Id.GetValue() == optionId.GetValue());
        return option is null
            ? Result<OptionEntity>.Failure(DomainErrors.Common.NotFound)
            : Result<OptionEntity>.Success(option);
    }
}
