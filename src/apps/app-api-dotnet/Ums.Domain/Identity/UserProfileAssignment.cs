namespace Ums.Domain.Identity;

using Ums.Domain.Kernel.ValueObjects;
using Ums.Domain.Identity.ValueObjects;
using Ums.Domain.Authorization.ValueObjects;

public sealed class UserProfileAssignment : Entity<UserProfileAssignment, UserProfileAssignmentProps>
{
    internal UserProfileAssignment(UserProfileAssignmentProps props) : base(props) { }

    public TenantId TenantId => Props.TenantId;
    public UserAccountId UserAccountId => Props.UserAccountId;
    public ProfileId ProfileId => Props.ProfileId;
    public BranchId? BranchId => Props.BranchId;
    public DateTimeOffset AssignedAt => Props.AssignedAt;
}

