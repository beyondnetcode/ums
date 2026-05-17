namespace Ums.Domain.Identity.System.Option;

public sealed class Option : Entity<Option, OptionProps>
{
    private Option(OptionProps props) : base(props)
    {
    }

    public SubMenuId SubMenuId => Props.SubMenuId;
    public Code Code => Props.Code;
    public Name Label => Props.Label;
    public Description Description => Props.Description;
    public ActionCode ActionCode => Props.ActionCode;
    public int SortOrder => Props.SortOrder;

    public OptionId GetId() => OptionId.Load(Props.Id.GetValue());

    public static Result<Option> Create(
        SubMenuId subMenuId,
        Code code,
        Name label,
        Description description,
        ActionCode actionCode,
        int sortOrder,
        ActorId createdBy)
    {
        var props = new OptionProps(IdValueObject.Create(), subMenuId, code, label, description, actionCode, sortOrder, createdBy);
        var option = new Option(props);

        if (!option.IsValid())
        {
            return Result<Option>.Failure(option.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<Option>.Success(option);
    }

    public Result Update(Name label, Description description, ActionCode actionCode, int sortOrder, ActorId updatedBy)
    {
        Props.Label = label;
        Props.Description = description;
        Props.ActionCode = actionCode;
        Props.SortOrder = sortOrder;

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }
}
