namespace Ums.Domain.Authorization.Role;

public sealed class RoleProps : IProps
{
    public IdValueObject Id { get; set; }
    public TenantId TenantId { get; set; }
    public SystemSuiteId SystemSuiteId { get; set; }
    public RoleId? ParentRoleId { get; set; }
    public Code Code { get; set; }
    public Name Value { get; set; }
    public Description Description { get; set; }
    public int HierarchyLevel { get; set; }
    public int PromotionOrder { get; set; }
    public bool IsActive { get; set; }
    public AuditValueObject Audit { get; private set; }

    public RoleProps(
        IdValueObject id,
        TenantId tenantId,
        SystemSuiteId systemSuiteId,
        RoleId? parentRoleId,
        Code code,
        Name value,
        Description description,
        int hierarchyLevel,
        int promotionOrder,
        ActorId createdBy)
    {
        Id = id;
        TenantId = tenantId;
        SystemSuiteId = systemSuiteId;
        ParentRoleId = parentRoleId;
        Code = code;
        Value = value;
        Description = description;
        HierarchyLevel = hierarchyLevel;
        PromotionOrder = promotionOrder;
        IsActive = true;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public object Clone() => MemberwiseClone();
}
