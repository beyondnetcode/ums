namespace Ums.Domain.Authorization;

public class PermissionTemplateProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId SystemSuiteId { get; private set; }
    public Code Code { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.Name Name { get; private set; }
    public StringValueObject TemplateVersion { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.Description Description { get; private set; }
    public IdValueObject CreatedBy { get; private set; }
    public LifecycleStatus Status { get; set; }
    public AuditValueObject Audit { get; private set; }

    public PermissionTemplateProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId systemSuiteId, Code code, global::Ums.Domain.Kernel.ValueObjects.Name name, global::Ums.Domain.Kernel.ValueObjects.Version version, global::Ums.Domain.Kernel.ValueObjects.Description description, IdValueObject createdBy)
    {
        Id = id;
        TenantId = tenantId;
        SystemSuiteId = systemSuiteId;
        Code = code;
        Name = name;
        TemplateVersion = version;
        Description = description;
        CreatedBy = createdBy;
        Status = LifecycleStatus.Draft;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
