namespace Ums.Domain.Authorization;

public class FunctionalSubmoduleProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId SystemSuiteId { get; private set; }
    public IdValueObject ModuleId { get; private set; }
    public Code Code { get; private set; }
    public StringValueObject Label { get; private set; }
    public int DisplayOrder { get; private set; }
    public StringValueObject? IconCode { get; private set; }
    public bool IsActive { get; set; }
    public AuditValueObject Audit { get; private set; }

    public FunctionalSubmoduleProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId systemSuiteId, IdValueObject moduleId, Code code, StringValueObject label, int displayOrder, StringValueObject? iconCode)
    {
        Id = id;
        TenantId = tenantId;
        SystemSuiteId = systemSuiteId;
        ModuleId = moduleId;
        Code = code;
        Label = label;
        DisplayOrder = displayOrder;
        IconCode = iconCode;
        IsActive = true;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
