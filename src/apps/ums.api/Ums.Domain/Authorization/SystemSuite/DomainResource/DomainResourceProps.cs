namespace Ums.Domain.Authorization.SystemSuite.DomainResource;

public class DomainResourceProps : IProps
{
    public IdValueObject Id { get; set; }
    public SystemSuiteId SystemSuiteId { get; set; }
    public ModuleId? ModuleId { get; set; } // Optional grouping
    public DomainResourceType Type { get; set; }
    public Code Code { get; set; }
    public Name Name { get; set; }
    public Description Description { get; set; }
    public AuditValueObject Audit { get; private set; }

    public DomainResourceProps(
        IdValueObject id,
        SystemSuiteId systemSuiteId,
        ModuleId? moduleId,
        DomainResourceType type,
        Code code,
        Name name,
        Description description,
        ActorId createdBy)
    {
        Id = id;
        SystemSuiteId = systemSuiteId;
        ModuleId = moduleId;
        Type = type;
        Code = code;
        Name = name;
        Description = description;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public object Clone() => MemberwiseClone();
}
