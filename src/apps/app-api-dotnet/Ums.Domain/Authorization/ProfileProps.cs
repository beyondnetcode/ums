namespace Ums.Domain.Authorization;

public class ProfileProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public IdValueObject? BranchId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.Name Name { get; private set; }
    public IdValueObject? TemplateId { get; private set; }
    public bool AutoAssigned { get; private set; }
    public bool IsActive { get; set; }
    public AuditValueObject Audit { get; private set; }

    public ProfileProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Kernel.ValueObjects.BranchId? branchId, global::Ums.Domain.Kernel.ValueObjects.Name name, global::Ums.Domain.Authorization.ValueObjects.TemplateId? templateId, bool autoAssigned)
    {
        Id = id;
        TenantId = tenantId;
        BranchId = branchId;
        Name = name;
        TemplateId = templateId;
        AutoAssigned = autoAssigned;
        IsActive = true;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
