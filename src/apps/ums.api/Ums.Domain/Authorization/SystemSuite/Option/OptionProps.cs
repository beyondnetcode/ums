namespace Ums.Domain.Authorization.SystemSuite.Option;

public class OptionProps : IProps
{
    public IdValueObject Id { get; private set; }
    public SubMenuId SubMenuId { get; private set; }
    public Code Code { get; private set; }
    public Name Label { get; private set; }
    public Description Description { get; private set; }
    public ActionCode ActionCode { get; private set; }
    public int SortOrder { get; private set; }
    public AuditValueObject Audit { get; private set; }

    public OptionProps(
        IdValueObject id,
        SubMenuId subMenuId,
        Code code,
        Name label,
        Description description,
        ActionCode actionCode,
        int sortOrder,
        ActorId createdBy)
    {
        Id = id;
        SubMenuId = subMenuId;
        Code = code;
        Label = label;
        Description = description;
        ActionCode = actionCode;
        SortOrder = sortOrder;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public OptionProps WithLabel(Name label)
    {
        var clone = (OptionProps)MemberwiseClone();
        clone.Label = label;
        return clone;
    }

    public OptionProps WithDescription(Description description)
    {
        var clone = (OptionProps)MemberwiseClone();
        clone.Description = description;
        return clone;
    }

    public OptionProps WithActionCode(ActionCode actionCode)
    {
        var clone = (OptionProps)MemberwiseClone();
        clone.ActionCode = actionCode;
        return clone;
    }

    public OptionProps WithSortOrder(int sortOrder)
    {
        var clone = (OptionProps)MemberwiseClone();
        clone.SortOrder = sortOrder;
        return clone;
    }

    public object Clone() => MemberwiseClone();
}