namespace Ums.Domain.Identity;

public class UserProfileAssignmentProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public IdValueObject UserAccountId { get; private set; }
    public global::Ums.Domain.Authorization.ValueObjects.ProfileId ProfileId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.BranchId? BranchId { get; private set; }
    public DateTimeOffset AssignedAt { get; private set; }

    public UserProfileAssignmentProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, IdValueObject userAccountId, global::Ums.Domain.Authorization.ValueObjects.ProfileId profileId, global::Ums.Domain.Kernel.ValueObjects.BranchId? branchId)
    {
        Id = id;
        TenantId = tenantId;
        UserAccountId = userAccountId;
        ProfileId = profileId;
        BranchId = branchId;
        AssignedAt = DateTimeOffset.UtcNow;
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
