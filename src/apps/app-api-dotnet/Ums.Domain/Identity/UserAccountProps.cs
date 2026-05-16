namespace Ums.Domain.Identity;

using Ums.Domain.Kernel.ValueObjects;
using Ums.Domain.Identity.ValueObjects;

public class UserAccountProps : IProps
{
    public UserAccountId Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public BranchId? BranchId { get; private set; }
    public EmailAddress Email { get; private set; }
    public IdentityReference IdentityReference { get; private set; }
    public Value? PasswordHash { get; private set; }
    public UserAccountStatus Status { get; set; }
    public AuditValueObject Audit { get; private set; }

    public UserAccountProps(UserAccountId id, TenantId tenantId, BranchId? branchId, EmailAddress email, IdentityReference identityReference, Value? passwordHash, string createdBy)
    {
        Id = id;
        TenantId = tenantId;
        BranchId = branchId;
        Email = email;
        IdentityReference = identityReference;
        PasswordHash = passwordHash;
        Status = UserAccountStatus.Pending;
        Audit = AuditValueObject.Create(createdBy);
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

