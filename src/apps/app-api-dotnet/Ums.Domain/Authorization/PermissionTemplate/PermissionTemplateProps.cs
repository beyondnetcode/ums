namespace Ums.Domain.Authorization.PermissionTemplate;

public class PermissionTemplateProps : IProps
{
    public IdValueObject Id { get; set; }
    public RoleId RoleId { get; set; }
    public SystemSuiteId SystemSuiteId { get; set; }
    public Code Code { get; set; }
    public Name Name { get; set; }
    public Description Description { get; set; }
    public TemplateVersion Version { get; set; }
    public TemplateStatus Status { get; set; }
    public AuditValueObject Audit { get; private set; }

    public PermissionTemplateProps(
        IdValueObject id,
        RoleId roleId,
        SystemSuiteId systemSuiteId,
        Code code,
        Name name,
        Description description,
        TemplateVersion version,
        TemplateStatus status,
        ActorId createdBy)
    {
        Id = id;
        RoleId = roleId;
        SystemSuiteId = systemSuiteId;
        Code = code;
        Name = name;
        Description = description;
        Version = version;
        Status = status;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public object Clone() => MemberwiseClone();
}
