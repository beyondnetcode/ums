namespace Ums.Domain.Identity.UserManagementDelegation;

public class UserManagementDelegationProps : IProps
{
    public DelegationId Id { get; set; }
    public TenantId TenantId { get; set; }
    public UserAccountId DelegatingAdminId { get; set; }
    public UserAccountId DelegatedAdminId { get; set; }
    public DelegationScopeType ScopeType { get; set; }
    public Guid? ScopeId { get; set; }
    public IReadOnlyList<DelegatedAction> AllowedActions { get; set; }
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset ValidUntil { get; set; }
    public int? MaxDurationDays { get; set; }
    public bool RequiresApproval { get; set; }
    public Guid? ApprovalRequestId { get; set; }
    public DelegationStatus Status { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public Guid? RevokedBy { get; set; }
    public string? RevocationReason { get; set; }
    public AuditValueObject Audit { get; private set; }

    public UserManagementDelegationProps(
        DelegationId id,
        TenantId tenantId,
        UserAccountId delegatingAdminId,
        UserAccountId delegatedAdminId,
        DelegationScopeType scopeType,
        Guid? scopeId,
        IReadOnlyList<DelegatedAction> allowedActions,
        DateTimeOffset validFrom,
        DateTimeOffset validUntil,
        int? maxDurationDays,
        bool requiresApproval,
        ActorId createdBy)
    {
        Id = id;
        TenantId = tenantId;
        DelegatingAdminId = delegatingAdminId;
        DelegatedAdminId = delegatedAdminId;
        ScopeType = scopeType;
        ScopeId = scopeId;
        AllowedActions = allowedActions;
        ValidFrom = validFrom;
        ValidUntil = validUntil;
        MaxDurationDays = maxDurationDays;
        RequiresApproval = requiresApproval;
        Status = DelegationStatus.Draft;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public object Clone() => MemberwiseClone();
}
