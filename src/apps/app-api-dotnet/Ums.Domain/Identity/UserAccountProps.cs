namespace Ums.Domain.Identity;

public class UserAccountProps : IProps
{
    public IdValueObject Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public BranchId? BranchId { get; private set; }
    public EmailAddress Email { get; private set; }
    public IdentityReference IdentityReference { get; private set; }
    public Value? PasswordHash { get; private set; }
    public UserAccountStatus Status { get; set; }
    public AuditValueObject Audit { get; private set; }

    public UserAccountProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Kernel.ValueObjects.BranchId? branchId, EmailAddress email, global::Ums.Domain.Identity.ValueObjects.IdentityReference identityReference, global::Ums.Domain.Kernel.ValueObjects.Value? passwordHash)
    {
        Id = id;
        TenantId = tenantId;
        BranchId = branchId;
        Email = email;
        IdentityReference = identityReference;
        PasswordHash = passwordHash;
        Status = UserAccountStatus.Pending;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
