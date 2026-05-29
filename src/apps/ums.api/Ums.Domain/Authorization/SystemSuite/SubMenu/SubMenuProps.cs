namespace Ums.Domain.Authorization.SystemSuite.SubMenu;

using Ums.Domain.Authorization.SystemSuite.Option;
using OptionEntity = Ums.Domain.Authorization.SystemSuite.Option.Option;

public class SubMenuProps : IProps
{
    public IdValueObject Id { get; private set; }
    public MenuId MenuId { get; private set; }
    public Code Code { get; private set; }
    public Name Label { get; private set; }
    public Description Description { get; private set; }
    public int SortOrder { get; private set; }
    public AuditValueObject Audit { get; private set; }

    public SubMenuProps(
        IdValueObject id,
        MenuId menuId,
        Code code,
        Name label,
        Description description,
        int sortOrder,
        ActorId createdBy)
    {
        Id = id;
        MenuId = menuId;
        Code = code;
        Label = label;
        Description = description;
        SortOrder = sortOrder;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public SubMenuProps WithLabel(Name label)
    {
        var clone = (SubMenuProps)MemberwiseClone();
        clone.Label = label;
        return clone;
    }

    public SubMenuProps WithDescription(Description description)
    {
        var clone = (SubMenuProps)MemberwiseClone();
        clone.Description = description;
        return clone;
    }

    public SubMenuProps WithSortOrder(int sortOrder)
    {
        var clone = (SubMenuProps)MemberwiseClone();
        clone.SortOrder = sortOrder;
        return clone;
    }

    public object Clone() => MemberwiseClone();
}