namespace Ums.Domain.Authorization;

public class FunctionalOptionProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId SystemSuiteId { get; private set; }
    public IdValueObject ModuleId { get; private set; }
    public IdValueObject MenuId { get; private set; }
    public Code Code { get; private set; }
    public StringValueObject Label { get; private set; }
    public StringValueObject RoutePath { get; private set; }
    public bool IsActive { get; set; }
    public AuditValueObject Audit { get; private set; }

    public FunctionalOptionProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId systemSuiteId, IdValueObject moduleId, IdValueObject menuId, Code code, StringValueObject label, StringValueObject routePath)
    {
        Id = id;
        TenantId = tenantId;
        SystemSuiteId = systemSuiteId;
        ModuleId = moduleId;
        MenuId = menuId;
        Code = code;
        Label = label;
        RoutePath = routePath;
        IsActive = true;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
