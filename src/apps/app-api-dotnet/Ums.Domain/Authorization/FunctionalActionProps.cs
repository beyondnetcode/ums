namespace Ums.Domain.Authorization;

public class FunctionalActionProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId SystemSuiteId { get; private set; }
    public Code Code { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.Name Name { get; private set; }
    public FunctionalActionLevel Level { get; private set; }
    public IdValueObject? LevelId { get; private set; }
    public bool IsActive { get; set; }
    public AuditValueObject Audit { get; private set; }

    public FunctionalActionProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId systemSuiteId, Code code, global::Ums.Domain.Kernel.ValueObjects.Name name, FunctionalActionLevel level, IdValueObject? levelId)
    {
        Id = id;
        TenantId = tenantId;
        SystemSuiteId = systemSuiteId;
        Code = code;
        Name = name;
        Level = level;
        LevelId = levelId;
        IsActive = true;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
