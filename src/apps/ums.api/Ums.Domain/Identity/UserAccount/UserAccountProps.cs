namespace Ums.Domain.Identity.UserAccount;

public class UserAccountProps : IProps
{
    public IdValueObject Id { get; set; }
    public TenantId TenantId { get; set; }
    public BranchId? BranchId { get; set; }
    public Email Email { get; set; }
    public UserCategory Category { get; set; }
    public UserStatus Status { get; set; }
    public IdentityReference? IdentityReference { get; set; }
    public IdentityReferenceType? IdentityReferenceType { get; set; }
    public AuditValueObject Audit { get; private set; }

    public UserAccountProps(
        IdValueObject id,
        TenantId tenantId,
        Email email,
        UserCategory category,
        IdentityReference? identityReference,
        IdentityReferenceType? identityReferenceType,
        ActorId createdBy)
    {
        Id = id;
        TenantId = tenantId;
        Email = email;
        Category = category;
        Status = UserStatus.Pending;
        IdentityReference = identityReference;
        IdentityReferenceType = identityReferenceType;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public object Clone() => MemberwiseClone();
}
