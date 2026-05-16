namespace Ums.Domain.Identity;

public sealed class UserAccount : AggregateRoot<UserAccount, UserAccountProps>
{
    private readonly List<UserProfileAssignment> _profileAssignments = new();

    private UserAccount(UserAccountProps props) : base(props)
    {
        if (TrackingState.IsNew)
        {
            DomainEvents.ApplyChange(new UserRegisteredEvent(Props.TenantId.GetValue(), Props.Id.GetValue(), Props.Email.GetValue()), true);
        }
    }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid? BranchId => Props.BranchId?.GetValue();
    public string Email => Props.Email.GetValue();
    public string IdentityReference => Props.IdentityReference.GetValue();
    public string? PasswordHash => Props.PasswordHash?.GetValue();
    public UserAccountStatus Status => Props.Status;
    public IReadOnlyCollection<UserProfileAssignment> ProfileAssignments => _profileAssignments.AsReadOnly();

    public static Result<UserAccount> Register(Guid tenantId, string email, string identityReference, Guid? branchId = null, string? passwordHash = null)
    {
        if (tenantId == Guid.Empty)
            return Result<UserAccount>.Failure(DomainErrors.TenantRequired);

        var emailResult = global::Ums.Domain.Kernel.ValueObjects.EmailAddress.Create(email);
        if (emailResult.IsFailure) return Result<UserAccount>.Failure(emailResult.Error);
        var emailValue = emailResult.Value;
        
        if (string.IsNullOrWhiteSpace(identityReference))
            return Result<UserAccount>.Failure("Identity reference is required.");

        var props = new UserAccountProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            branchId.HasValue ? global::Ums.Domain.Kernel.ValueObjects.BranchId.Load(branchId.Value) : null,
            emailValue,
            global::Ums.Domain.Identity.ValueObjects.IdentityReference.Create(identityReference.Trim()),
            passwordHash != null ? global::Ums.Domain.Kernel.ValueObjects.Value.Create(passwordHash) : null);

        var user = new UserAccount(props);
        return Result<UserAccount>.Success(user);
    }

    public Result Activate()
    {
        if (Props.Status == UserAccountStatus.Terminated)
            return Result.Failure("Terminated users cannot be activated.");

        Props.Status = UserAccountStatus.Active;
        DomainEvents.ApplyChange(new UserActivatedEvent(Props.TenantId.GetValue(), Props.Id.GetValue()), true);
        Props.Audit.Update("system");
        return Result.Success();
    }

    public Result Block(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure("Block reason is required.");

        Props.Status = UserAccountStatus.Blocked;
        DomainEvents.ApplyChange(new UserBlockedEvent(Props.TenantId.GetValue(), Props.Id.GetValue(), reason.Trim()), true);
        Props.Audit.Update("system");
        return Result.Success();
    }

    public Result AssignProfile(Guid profileId, Guid? branchId = null)
    {
        if (profileId == Guid.Empty)
            return Result.Failure("Profile identifier is required.");

        if (_profileAssignments.Any(item => item.ProfileId == profileId && item.BranchId == branchId))
            return Result.Failure("Profile is already assigned to the user in this scope.");

        var assignmentProps = new UserProfileAssignmentProps(
            IdValueObject.Create(),
            Props.TenantId,
            IdValueObject.Load(Props.Id.GetValue()),
            global::Ums.Domain.Authorization.ValueObjects.ProfileId.Load(profileId),
            branchId.HasValue ? global::Ums.Domain.Kernel.ValueObjects.BranchId.Load(branchId.Value) : null);
        
        _profileAssignments.Add(new UserProfileAssignment(assignmentProps));
        Props.Audit.Update("system");
        return Result.Success();
    }
}
