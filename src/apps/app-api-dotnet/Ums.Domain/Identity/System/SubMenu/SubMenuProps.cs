namespace Ums.Domain.Identity.System.SubMenu;

using Ums.Domain.Identity.System.Option;
using OptionEntity = Ums.Domain.Identity.System.Option.Option;

public class SubMenuProps : IProps
{
    public IdValueObject Id { get; set; }
    public MenuId MenuId { get; set; }
    public Code Code { get; set; }
    public Name Label { get; set; }
    public Description Description { get; set; }
    public int SortOrder { get; set; }
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

    public object Clone() => MemberwiseClone();
}
