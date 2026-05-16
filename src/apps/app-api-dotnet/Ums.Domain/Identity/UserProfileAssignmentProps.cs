namespace Ums.Domain.Identity;

using Ums.Domain.Kernel.ValueObjects;
using Ums.Domain.Identity.ValueObjects;
using Ums.Domain.Authorization.ValueObjects;

public class UserProfileAssignmentProps : IProps
{
    public IdValueObject Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public UserAccountId UserAccountId { get; private set; }
    public ProfileId ProfileId { get; private set; }
    public BranchId? BranchId { get; private set; }
    public DateTimeOffset AssignedAt { get; private set; }

    public UserProfileAssignmentProps(IdValueObject id, TenantId tenantId, UserAccountId userAccountId, ProfileId profileId, BranchId? branchId)
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

