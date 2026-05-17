namespace Ums.Domain.Identity.System.Role;

public class RoleProps : IProps
{
    public IdValueObject Id { get; set; }
    public SystemId SystemId { get; set; }
    public Code Code { get; set; }
    public Name Name { get; set; }
    public Description Description { get; set; }
    public bool IsActive { get; set; }
    public AuditValueObject Audit { get; private set; }

    public RoleProps(
        IdValueObject id,
        SystemId systemId,
        Code code,
        Name name,
        Description description,
        ActorId createdBy)
    {
        Id = id;
        SystemId = systemId;
        Code = code;
        Name = name;
        Description = description;
        IsActive = true;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public object Clone() => MemberwiseClone();
}
