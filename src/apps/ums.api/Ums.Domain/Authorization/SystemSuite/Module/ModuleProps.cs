namespace Ums.Domain.Authorization.SystemSuite.Module;

using Ums.Domain.Authorization.SystemSuite.Menu;
using MenuEntity = Ums.Domain.Authorization.SystemSuite.Menu.Menu;

public class ModuleProps : IProps
{
    public IdValueObject Id { get; private set; }
    public SystemId SystemId { get; private set; }
    public Code Code { get; private set; }
    public Name Name { get; private set; }
    public Description Description { get; private set; }
    public ModuleStatus Status { get; private set; }
    public int SortOrder { get; private set; }
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

    public ModuleProps WithName(Name name)
    {
        var clone = (ModuleProps)MemberwiseClone();
        clone.Name = name;
        return clone;
    }

    public ModuleProps WithDescription(Description description)
    {
        var clone = (ModuleProps)MemberwiseClone();
        clone.Description = description;
        return clone;
    }

    public ModuleProps WithSortOrder(int sortOrder)
    {
        var clone = (ModuleProps)MemberwiseClone();
        clone.SortOrder = sortOrder;
        return clone;
    }

    public ModuleProps WithStatus(ModuleStatus status)
    {
        var clone = (ModuleProps)MemberwiseClone();
        clone.Status = status;
        return clone;
    }

    public object Clone() => MemberwiseClone();
}