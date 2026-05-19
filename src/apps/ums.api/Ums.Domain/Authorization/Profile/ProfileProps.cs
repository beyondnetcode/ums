namespace Ums.Domain.Authorization.Profile;

public class ProfileProps : IProps
{
    public IdValueObject Id { get; set; }
    public TenantId TenantId { get; set; }
    public UserId UserId { get; set; }
    public RoleId RoleId { get; set; }
    public BranchId? BranchId { get; set; }
    public ProfileScope Scope { get; set; }
    public bool IsActive { get; set; }
    public AuditValueObject Audit { get; private set; }

    public ProfileProps(
        IdValueObject id,
        TenantId tenantId,
        UserId userId,
        RoleId roleId,
        BranchId? branchId,
        ProfileScope scope,
        ActorId createdBy)
    {
        Id = id;
        TenantId = tenantId;
        UserId = userId;
        RoleId = roleId;
        BranchId = branchId;
        Scope = scope;
        IsActive = true;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public object Clone() => MemberwiseClone();
}
