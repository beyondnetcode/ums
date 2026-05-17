namespace Ums.Domain.Identity.System;

using Ums.Domain.Identity.System.Configuration;
using Ums.Domain.Identity.System.Module;
using Ums.Domain.Identity.System.Role;
using Ums.Domain.Identity.System.Action;
using ModuleEntity = Ums.Domain.Identity.System.Module.Module;
using RoleEntity = Ums.Domain.Identity.System.Role.Role;

public class SystemProps : IProps
{
    public IdValueObject Id { get; set; }
    public Code Code { get; set; }
    public Name Name { get; set; }
    public Description Description { get; set; }
    public SystemStatus Status { get; set; }
    public AuditValueObject Audit { get; private set; }

    public SystemProps(
        IdValueObject id,
        Code code,
        Name name,
        Description description,
        SystemStatus status,
        ActorId createdBy)
    {
        Id = id;
        Code = code;
        Name = name;
        Description = description;
        Status = status;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public object Clone() => MemberwiseClone();
}
