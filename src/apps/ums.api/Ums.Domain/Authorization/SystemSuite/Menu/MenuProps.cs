namespace Ums.Domain.Authorization.SystemSuite.Menu;

using Ums.Domain.Authorization.SystemSuite.SubMenu;
using SubMenuEntity = Ums.Domain.Authorization.SystemSuite.SubMenu.SubMenu;

public class MenuProps : IProps
{
    public IdValueObject Id { get; private set; }
    public ModuleId ModuleId { get; private set; }
    public Code Code { get; private set; }
    public Name Label { get; private set; }
    public Description Description { get; private set; }
    public int SortOrder { get; private set; }
    public AuditValueObject Audit { get; private set; }

    public MenuProps(
        IdValueObject id,
        ModuleId moduleId,
        Code code,
        Name label,
        Description description,
        int sortOrder,
        ActorId createdBy)
    {
        Id = id;
        ModuleId = moduleId;
        Code = code;
        Label = label;
        Description = description;
        SortOrder = sortOrder;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public MenuProps WithLabel(Name label)
    {
        var clone = (MenuProps)MemberwiseClone();
        clone.Label = label;
        return clone;
    }

    public MenuProps WithDescription(Description description)
    {
        var clone = (MenuProps)MemberwiseClone();
        clone.Description = description;
        return clone;
    }

    public MenuProps WithSortOrder(int sortOrder)
    {
        var clone = (MenuProps)MemberwiseClone();
        clone.SortOrder = sortOrder;
        return clone;
    }

    public object Clone() => MemberwiseClone();
}