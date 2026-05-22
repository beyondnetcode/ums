namespace Ums.Domain.Identity.UserAccount;

using Ums.Domain.Events;
using Ums.Domain.Identity.UserAccount.MfaEnrollment;
using Ums.Domain.Identity.UserAccount.PasswordCredential;
using MfaEnrollmentEntity = Ums.Domain.Identity.UserAccount.MfaEnrollment.MfaEnrollment;
using PasswordCredentialEntity = Ums.Domain.Identity.UserAccount.PasswordCredential.PasswordCredential;

public sealed class UserAccount : AggregateRoot<UserAccount, UserAccountProps>
{
    private readonly List<MfaEnrollmentEntity> _mfaEnrollments = new();
    private readonly List<PasswordCredentialEntity> _passwordCredentials = new();

    public new UserAccountDomainEventsManager DomainEvents { get; }

    private UserAccount(UserAccountProps props) : base(props)
    {
        DomainEvents = new UserAccountDomainEventsManager(this);

        if (TrackingState.IsNew)
        {
            DomainEvents.RaiseEvent(new UserRegisteredEvent(
                Props.Id.GetValue(),
                Props.TenantId.GetValue(),
                Props.BranchId?.GetValue(),
                Props.Category.Name,
                Props.IdentityReference?.GetValue()));
        }
    }

    public TenantId TenantId => Props.TenantId;
    public BranchId? BranchId => Props.BranchId;
    public Email Email => Props.Email;
    public UserCategory Category => Props.Category;
    public UserStatus Status => Props.Status;
    public IdentityReference? IdentityReference => Props.IdentityReference;
    public IdentityReferenceType? IdentityReferenceType => Props.IdentityReferenceType;

    public IReadOnlyCollection<MfaEnrollmentEntity> MfaEnrollments => _mfaEnrollments.AsReadOnly();
    public IReadOnlyCollection<PasswordCredentialEntity> PasswordCredentials => _passwordCredentials.AsReadOnly();

    public PasswordHash? ActivePasswordHash => _passwordCredentials.FirstOrDefault(c => c.IsActive)?.PasswordHash;

    public UserAccountId GetId() => UserAccountId.Load(Props.Id.GetValue());

    public static Result<UserAccount> Create(
        TenantId tenantId,
        Email email,
        UserCategory category,
        IdentityReference? identityReference,
        IdentityReferenceType? identityReferenceType,
        ActorId createdBy,
        UserAccountId? userAccountId = null)
    {
        var id = userAccountId ?? UserAccountId.Load(IdValueObject.Create().GetValue());
        var props = new UserAccountProps(
            id,
            tenantId,
            email,
            category,
            identityReference,
            identityReferenceType,
            createdBy);

        var userAccount = new UserAccount(props);

        if (!userAccount.IsValid())
        {
            return Result<UserAccount>.Failure(userAccount.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<UserAccount>.Success(userAccount);
    }

    public Result Activate(ActorId updatedBy)
    {
        if (Status != UserStatus.Pending)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.UserAccount.CannotActivate));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Status = UserStatus.Active;
        DomainEvents.RaiseEvent(new UserActivatedEvent(Props.Id.GetValue(), Props.TenantId.GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result Block(Reason reason, ActorId updatedBy)
    {
        if (Status == UserStatus.Blocked)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.UserAccount.AlreadyBlocked));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Status = UserStatus.Blocked;
        DomainEvents.RaiseEvent(new UserBlockedEvent(
            Props.Id.GetValue(),
            Props.TenantId.GetValue(),
            reason.GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result Restore(ActorId updatedBy)
    {
        if (Status != UserStatus.Blocked)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.UserAccount.CannotRestore));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Status = UserStatus.Active;
        DomainEvents.RaiseEvent(new UserRestoredEvent(Props.Id.GetValue(), Props.TenantId.GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result AddPassword(PasswordHash passwordHash, ActorId createdBy)
    {
        if (Status == UserStatus.Blocked)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.UserAccount.BlockedCannotUpdateCredentials));
        }

        if (passwordHash.IsEmpty())
        {
            BrokenRules.Add(new BrokenRule(nameof(PasswordHash), DomainErrors.UserAccount.PasswordHashRequired));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var credentialResult = PasswordCredentialEntity.Create(GetId(), passwordHash, createdBy);
        if (credentialResult.IsFailure)
        {
            return Result.Failure(credentialResult.Error);
        }

        foreach (var credential in _passwordCredentials)
        {
            credential.DeactivateInternal();
        }
        credentialResult.Value.ActivateInternal();

        _passwordCredentials.Add(credentialResult.Value);
        TrackingState.MarkAsDirty();
        Props.Audit.Update(createdBy.GetValue());
        return Result.Success();
    }

    public Result ActivatePassword(IdValueObject credentialId, ActorId updatedBy)
    {
        if (Status == UserStatus.Blocked)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.UserAccount.BlockedCannotUpdateCredentials));
        }

        var credential = FindPasswordCredential(credentialId);
        if (credential.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(PasswordCredentials), DomainErrors.Common.NotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        foreach (var c in _passwordCredentials)
        {
            c.DeactivateInternal();
        }

        credential.Value.ActivateInternal();
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result RemovePassword(IdValueObject credentialId, ActorId updatedBy)
    {
        if (Status == UserStatus.Blocked)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.UserAccount.BlockedCannotUpdateCredentials));
        }

        var credential = FindPasswordCredential(credentialId);
        if (credential.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(PasswordCredentials), DomainErrors.Common.NotFound));
        }

        if (credential.IsSuccess && credential.Value.IsActive && _passwordCredentials.Count == 1)
        {
            BrokenRules.Add(new BrokenRule(nameof(PasswordCredentials), DomainErrors.UserAccount.CannotRemoveLastPassword));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        _passwordCredentials.Remove(credential.Value);
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result EnrollMfa(MfaMethod method, ActorId createdBy)
    {
        if (Status == UserStatus.Blocked)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.UserAccount.BlockedCannotEnrollMfa));
        }

        if (_mfaEnrollments.Any(e => e.Method == method && e.Status != MfaEnrollmentStatus.NotEnrolled))
        {
            BrokenRules.Add(new BrokenRule(nameof(MfaEnrollments), DomainErrors.UserAccount.MfaAlreadyEnrolled));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var enrollmentResult = MfaEnrollmentEntity.Create(GetId(), method, createdBy);
        if (enrollmentResult.IsFailure)
        {
            return Result.Failure(enrollmentResult.Error);
        }

        _mfaEnrollments.Add(enrollmentResult.Value);
        DomainEvents.RaiseEvent(new MfaEnrolledEvent(
            Props.Id.GetValue(),
            Props.TenantId.GetValue(),
            method.Name));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(createdBy.GetValue());
        return Result.Success();
    }

    public Result VerifyMfaChallenge(IdValueObject enrollmentId, ActorId updatedBy)
    {
        var enrollment = FindMfaEnrollment(enrollmentId);
        if (enrollment.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(MfaEnrollments), DomainErrors.Common.NotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        enrollment.Value.Verify(updatedBy);
        DomainEvents.RaiseEvent(new MfaVerifiedEvent(
            Props.Id.GetValue(),
            Props.TenantId.GetValue(),
            enrollment.Value.Method.Name));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result RecordAuthenticationAttempt(bool success, string reason, string ipAddress, ActorId actor)
    {
        DomainEvents.RaiseEvent(new AuthenticationAttemptedEvent(
            Props.Id.GetValue(),
            Props.TenantId.GetValue(),
            success,
            reason,
            ipAddress));
        return Result.Success();
    }

    private Result<PasswordCredentialEntity> FindPasswordCredential(IdValueObject credentialId)
    {
        var credential = _passwordCredentials.FirstOrDefault(c => c.Id.GetValue() == credentialId.GetValue());
        return credential is null
            ? Result<PasswordCredentialEntity>.Failure(DomainErrors.Common.NotFound)
            : Result<PasswordCredentialEntity>.Success(credential);
    }

    private Result<MfaEnrollmentEntity> FindMfaEnrollment(IdValueObject enrollmentId)
    {
        var enrollment = _mfaEnrollments.FirstOrDefault(e => e.Id.GetValue() == enrollmentId.GetValue());
        return enrollment is null
            ? Result<MfaEnrollmentEntity>.Failure(DomainErrors.Common.NotFound)
            : Result<MfaEnrollmentEntity>.Success(enrollment);
    }
}
