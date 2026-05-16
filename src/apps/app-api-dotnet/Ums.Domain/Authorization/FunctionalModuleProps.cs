namespace Ums.Domain.Authorization;

public class FunctionalModuleProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId SystemSuiteId { get; private set; }
    public Code Code { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.Name Name { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.Description Description { get; private set; }
    public bool IsActive { get; set; }
    public AuditValueObject Audit { get; private set; }

    public FunctionalModuleProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId systemSuiteId, Code code, global::Ums.Domain.Kernel.ValueObjects.Name name, global::Ums.Domain.Kernel.ValueObjects.Description description)
    {
        Id = id;
        TenantId = tenantId;
        SystemSuiteId = systemSuiteId;
        Code = code;
        Name = name;
        Description = description;
        IsActive = true;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
