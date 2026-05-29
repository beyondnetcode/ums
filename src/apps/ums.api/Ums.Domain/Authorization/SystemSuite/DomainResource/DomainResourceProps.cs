namespace Ums.Domain.Authorization.SystemSuite.DomainResource;

public class DomainResourceProps : IProps
{
    public IdValueObject Id { get; private set; }
    public SystemSuiteId SystemSuiteId { get; private set; }
    public ModuleId? ModuleId { get; private set; }
    public DomainResourceType Type { get; private set; }
    public Code Code { get; private set; }
    public Name Name { get; private set; }
    public Description Description { get; private set; }
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

    public DomainResourceProps WithName(Name name)
    {
        var clone = (DomainResourceProps)MemberwiseClone();
        clone.Name = name;
        return clone;
    }

    public DomainResourceProps WithDescription(Description description)
    {
        var clone = (DomainResourceProps)MemberwiseClone();
        clone.Description = description;
        return clone;
    }

    public object Clone() => MemberwiseClone();
}