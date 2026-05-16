namespace Ums.Domain.Identity;

public sealed class UserProfileAssignment : Entity<UserProfileAssignment, UserProfileAssignmentProps>
{
    internal UserProfileAssignment(UserProfileAssignmentProps props) : base(props) { }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid UserAccountId => Props.UserAccountId.GetValue();
    public Guid ProfileId => Props.ProfileId.GetValue();
    public Guid? BranchId => Props.BranchId?.GetValue();
    public DateTimeOffset AssignedAt => Props.AssignedAt;
}
