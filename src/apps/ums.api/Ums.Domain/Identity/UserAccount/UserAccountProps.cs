namespace Ums.Domain.Identity.UserAccount;

public class UserAccountProps : IProps
{
    public IdValueObject Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public BranchId? BranchId { get; private set; }
    public Email Email { get; private set; }
    public Name? DisplayName { get; private set; }
    public UserCategory Category { get; private set; }
    public UserStatus Status { get; private set; }
    public IdentityReference? IdentityReference { get; private set; }
    public IdentityReferenceType? IdentityReferenceType { get; private set; }
    public AuditValueObject Audit { get; private set; }

    public UserAccountProps(
        IdValueObject id,
        TenantId tenantId,
        Email email,
        UserCategory category,
        IdentityReference? identityReference,
        IdentityReferenceType? identityReferenceType,
        ActorId createdBy,
        BranchId? branchId = null,
        Name? displayName = null)
    {
        Id = id;
        TenantId = tenantId;
        BranchId = branchId;
        Email = email;
        DisplayName = displayName;
        Category = category;
        Status = UserStatus.Pending;
        IdentityReference = identityReference;
        IdentityReferenceType = identityReferenceType;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public UserAccountProps(
        IdValueObject id,
        TenantId tenantId,
        BranchId? branchId,
        Email email,
        UserCategory category,
        UserStatus status,
        IdentityReference? identityReference,
        IdentityReferenceType? identityReferenceType,
        AuditValueObject audit,
        Name? displayName = null)
    {
        Id = id;
        TenantId = tenantId;
        BranchId = branchId;
        Email = email;
        DisplayName = displayName;
        Category = category;
        Status = status;
        IdentityReference = identityReference;
        IdentityReferenceType = identityReferenceType;
        Audit = audit;
    }

    public UserAccountProps WithStatus(UserStatus status)
    {
        var clone = (UserAccountProps)MemberwiseClone();
        clone.Status = status;
        return clone;
    }

    public object Clone() => MemberwiseClone();
}
