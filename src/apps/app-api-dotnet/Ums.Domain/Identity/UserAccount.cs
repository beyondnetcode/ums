namespace Ums.Domain.Identity;

using Ums.Domain.Kernel.ValueObjects;
using Ums.Domain.Identity.ValueObjects;
using Ums.Domain.Authorization.ValueObjects;

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

    public TenantId TenantId => Props.TenantId;
    public BranchId? BranchId => Props.BranchId;
    public EmailAddress Email => Props.Email;
    public IdentityReference IdentityReference => Props.IdentityReference;
    public Value? PasswordHash => Props.PasswordHash;
    public UserAccountStatus Status => Props.Status;
    public IReadOnlyCollection<UserProfileAssignment> ProfileAssignments => _profileAssignments.AsReadOnly();

    public static Result<UserAccount> Register(TenantId tenantId, EmailAddress email, IdentityReference identityReference, string createdBy, BranchId? branchId = null, Value? passwordHash = null)
    {
        var props = new UserAccountProps(
            UserAccountId.Create(),
            tenantId,
            branchId,
            email,
            identityReference,
            passwordHash,
            createdBy);

        var user = new UserAccount(props);

        if (!user.IsValid())
        {
            return Result<UserAccount>.Failure(user.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<UserAccount>.Success(user);
    }


    public Result Activate(string updatedBy)
    {
        if (Props.Status == UserAccountStatus.Terminated)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.User.TerminatedCannotActivate));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Status = UserAccountStatus.Active;
        DomainEvents.ApplyChange(new UserActivatedEvent(Props.TenantId.GetValue(), Props.Id.GetValue()), true);
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy);
        return Result.Success();
    }

    public Result Block(string reason, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            BrokenRules.Add(new BrokenRule("Reason", DomainErrors.User.BlockReasonRequired));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Status = UserAccountStatus.Blocked;
        DomainEvents.ApplyChange(new UserBlockedEvent(Props.TenantId.GetValue(), Props.Id.GetValue(), reason.Trim()), true);
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy);
        return Result.Success();
    }

    public Result AssignProfile(ProfileId profileId, string updatedBy, BranchId? branchId = null)
    {
        if (profileId == null)
        {
            BrokenRules.Add(new BrokenRule(nameof(profileId), DomainErrors.User.ProfileIdRequired));
        }

        if (profileId != null && _profileAssignments.Any(item => item.ProfileId == profileId && item.BranchId == branchId))
        {
            BrokenRules.Add(new BrokenRule(nameof(ProfileAssignments), DomainErrors.User.ProfileAlreadyAssigned));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var assignmentProps = new UserProfileAssignmentProps(
            IdValueObject.Create(),
            Props.TenantId,
            Props.Id,
            profileId!,
            branchId);
        
        _profileAssignments.Add(new UserProfileAssignment(assignmentProps));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy);
        return Result.Success();
    }
}

