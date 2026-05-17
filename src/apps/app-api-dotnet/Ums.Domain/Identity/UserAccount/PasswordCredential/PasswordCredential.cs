namespace Ums.Domain.Identity.UserAccount.PasswordCredential;

public sealed class PasswordCredential : Entity<PasswordCredential, PasswordCredentialProps>
{
    private PasswordCredential(PasswordCredentialProps props) : base(props)
    {
    }

    public UserAccountId UserAccountId => Props.UserAccountId;
    public PasswordHash PasswordHash => Props.PasswordHash;
    public bool IsActive => Props.IsActive;

    public PasswordCredentialId GetId() => PasswordCredentialId.Load(Props.Id.GetValue());

    public static Result<PasswordCredential> Create(
        UserAccountId userAccountId,
        PasswordHash passwordHash,
        ActorId createdBy)
    {
        var props = new PasswordCredentialProps(IdValueObject.Create(), userAccountId, passwordHash, createdBy);
        var credential = new PasswordCredential(props);

        if (!credential.IsValid())
        {
            return Result<PasswordCredential>.Failure(credential.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<PasswordCredential>.Success(credential);
    }

    internal void ActivateInternal()
    {
        Props.IsActive = true;
    }

    internal void DeactivateInternal()
    {
        Props.IsActive = false;
    }
}
