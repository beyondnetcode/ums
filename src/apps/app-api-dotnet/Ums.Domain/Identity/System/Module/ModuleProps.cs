namespace Ums.Domain.Identity.System.Module;

using Ums.Domain.Identity.System.Menu;
using MenuEntity = Ums.Domain.Identity.System.Menu.Menu;

public class ModuleProps : IProps
{
    public IdValueObject Id { get; set; }
    public SystemId SystemId { get; set; }
    public Code Code { get; set; }
    public Name Name { get; set; }
    public Description Description { get; set; }
    public ModuleStatus Status { get; set; }
    public int SortOrder { get; set; }
    public AuditValueObject Audit { get; private set; }

    public ModuleProps(
        IdValueObject id,
        SystemId systemId,
        Code code,
        Name name,
        Description description,
        ModuleStatus status,
        int sortOrder,
        ActorId createdBy)
    {
        Id = id;
        SystemId = systemId;
        Code = code;
        Name = name;
        Description = description;
        Status = status;
        SortOrder = sortOrder;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public object Clone() => MemberwiseClone();
}
