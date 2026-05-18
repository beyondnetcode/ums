namespace Ums.Domain.Authorization.SystemSuite;

public class SystemSuiteProps : IProps
{
    public IdValueObject Id { get; set; }
    public TenantId TenantId { get; set; }
    public Code Code { get; set; }
    public Name Name { get; set; }
    public Description Description { get; set; }
    public SystemStatus Status { get; set; }
    public AuditValueObject Audit { get; private set; }

    public SystemSuiteProps(
        IdValueObject id,
        TenantId tenantId,
        Code code,
        Name name,
        Description description,
        SystemStatus status,
        ActorId createdBy)
    {
        Id = id;
        TenantId = tenantId;
        Code = code;
        Name = name;
        Description = description;
        Status = status;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public object Clone() => MemberwiseClone();
}
