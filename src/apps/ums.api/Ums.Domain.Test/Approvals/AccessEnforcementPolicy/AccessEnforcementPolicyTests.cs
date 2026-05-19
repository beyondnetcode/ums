namespace Ums.Domain.Test.Approvals.AccessEnforcementPolicy;

using Ums.Domain.Approvals.AccessEnforcementPolicy;
using Xunit;

public class AccessEnforcementPolicyTests
{
    private static readonly TenantId ValidTenantId = TenantId.Load(Guid.NewGuid().ToString());
    private static readonly ProfileId? ValidProfileId = ProfileId.Load(Guid.NewGuid().ToString());
    private static readonly RoleId? ValidRoleId = RoleId.Load(Guid.NewGuid().ToString());
    private static readonly AccessEnforcementAction ValidAction = AccessEnforcementAction.RestrictProfile;
    private static readonly ActorId ValidActor = ActorId.Create("user-001");
    #region Create

    [Fact]
    public void Create_WithProfileId_ReturnsSuccess()
    {
        var result = AccessEnforcementPolicy.Create(ValidTenantId, ValidProfileId, null, ValidAction, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidTenantId, result.Value.TenantId);
        Assert.Equal(ValidProfileId, result.Value.ProfileId);
        Assert.Null(result.Value.RoleId);
        Assert.Equal(ValidAction, result.Value.EnforcementAction);
        Assert.True(result.Value.IsActive);
    }

    [Fact]
    public void Create_WithRoleId_ReturnsSuccess()
    {
        var result = AccessEnforcementPolicy.Create(ValidTenantId, null, ValidRoleId, ValidAction, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.ProfileId);
        Assert.Equal(ValidRoleId, result.Value.RoleId);
    }

    [Fact]
    public void Create_WithBothProfileAndRole_ReturnsSuccess()
    {
        var result = AccessEnforcementPolicy.Create(ValidTenantId, ValidProfileId, ValidRoleId, ValidAction, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidProfileId, result.Value.ProfileId);
        Assert.Equal(ValidRoleId, result.Value.RoleId);
    }

    [Fact]
    public void Create_WithoutProfileOrRole_ReturnsFailure()
    {
        var result = AccessEnforcementPolicy.Create(ValidTenantId, null, null, ValidAction, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Approvals.PolicyRequiresProfileOrRole, result.Error);
    }

    #endregion

    #region Deactivate

    [Fact]
    public void Deactivate_WhenActive_ReturnsSuccess()
    {
        var policy = AccessEnforcementPolicy.Create(ValidTenantId, ValidProfileId, null, ValidAction, ValidActor).Value;

        var result = policy.Deactivate(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.False(policy.IsActive);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ReturnsFailure()
    {
        var policy = AccessEnforcementPolicy.Create(ValidTenantId, ValidProfileId, null, ValidAction, ValidActor).Value;
        policy.Deactivate(ValidActor);

        var result = policy.Deactivate(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Approvals.PolicyAlreadyInactive, result.Error);
    }

    #endregion

    #region UpdateAction

    [Fact]
    public void UpdateAction_WithValidData_ReturnsSuccess()
    {
        var policy = AccessEnforcementPolicy.Create(ValidTenantId, ValidProfileId, null, ValidAction, ValidActor).Value;
        var newAction = AccessEnforcementAction.BlockUser;

        var result = policy.UpdateAction(newAction, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(newAction, policy.EnforcementAction);
    }

    [Fact]
    public void UpdateAction_WhenInactive_StillSucceeds()
    {
        var policy = AccessEnforcementPolicy.Create(ValidTenantId, ValidProfileId, null, ValidAction, ValidActor).Value;
        policy.Deactivate(ValidActor);
        var newAction = AccessEnforcementAction.RestrictProfile;

        var result = policy.UpdateAction(newAction, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(newAction, policy.EnforcementAction);
    }

    #endregion
}
