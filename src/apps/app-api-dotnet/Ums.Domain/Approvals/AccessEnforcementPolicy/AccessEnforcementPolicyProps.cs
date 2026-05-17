namespace Ums.Domain.Approvals.AccessEnforcementPolicy;

public class AccessEnforcementPolicyProps : IProps
{
    public IdValueObject Id { get; set; }
    public TenantId TenantId { get; set; }
    public ProfileId? ProfileId { get; set; }
    public RoleId? RoleId { get; set; }
    public AccessEnforcementAction EnforcementAction { get; set; }
    public bool IsActive { get; set; }
    public AuditValueObject Audit { get; private set; }

    public AccessEnforcementPolicyProps(
        IdValueObject id,
        TenantId tenantId,
        ProfileId? profileId,
        RoleId? roleId,
        AccessEnforcementAction enforcementAction,
        bool isActive,
        ActorId createdBy)
    {
        Id = id;
        TenantId = tenantId;
        ProfileId = profileId;
        RoleId = roleId;
        EnforcementAction = enforcementAction;
        IsActive = isActive;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public object Clone() => MemberwiseClone();
}
