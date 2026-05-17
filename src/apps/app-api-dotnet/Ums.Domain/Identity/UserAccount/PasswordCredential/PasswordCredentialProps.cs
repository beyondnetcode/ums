namespace Ums.Domain.Identity.UserAccount.PasswordCredential;

public class PasswordCredentialProps : IProps
{
    public IdValueObject Id { get; set; }
    public UserAccountId UserAccountId { get; set; }
    public PasswordHash PasswordHash { get; set; }
    public bool IsActive { get; set; }
    public AuditValueObject Audit { get; private set; }

    public PasswordCredentialProps(
        IdValueObject id,
        UserAccountId userAccountId,
        PasswordHash passwordHash,
        ActorId createdBy)
    {
        Id = id;
        UserAccountId = userAccountId;
        PasswordHash = passwordHash;
        IsActive = false;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public object Clone() => MemberwiseClone();
}
