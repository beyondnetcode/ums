namespace Ums.Domain.Authorization;

public class SystemSuiteProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public Code Code { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.Name Name { get; private set; }
    public StringValueObject BaseUrl { get; private set; }
    public StringValueObject? ApiCredentialHash { get; set; }
    public LifecycleStatus Status { get; set; }
    public AuditValueObject Audit { get; private set; }

    public SystemSuiteProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, Code code, global::Ums.Domain.Kernel.ValueObjects.Name name, StringValueObject baseUrl)
    {
        Id = id;
        TenantId = tenantId;
        Code = code;
        Name = name;
        BaseUrl = baseUrl;
        Status = LifecycleStatus.Draft;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
