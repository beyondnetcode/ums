namespace Ums.Domain.Identity.System.Menu;

using Ums.Domain.Identity.System.SubMenu;
using SubMenuEntity = Ums.Domain.Identity.System.SubMenu.SubMenu;

public class MenuProps : IProps
{
    public IdValueObject Id { get; set; }
    public ModuleId ModuleId { get; set; }
    public Code Code { get; set; }
    public Name Label { get; set; }
    public Description Description { get; set; }
    public int SortOrder { get; set; }
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

    public object Clone() => MemberwiseClone();
}
