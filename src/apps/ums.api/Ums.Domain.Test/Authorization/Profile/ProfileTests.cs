namespace Ums.Domain.Test.Authorization.Profile;

using Ums.Domain.Authorization.Profile;
using Ums.Domain.Authorization.Template;
using Ums.Domain.Authorization.Template.PermissionTemplateItem;
using Xunit;

public class ProfileTests
{
    private static readonly TenantId ValidTenantId = TenantId.Load(Guid.NewGuid().ToString());
    private static readonly UserId ValidUserId = UserId.Load(Guid.NewGuid().ToString());
    private static readonly RoleId ValidRoleId = RoleId.Load(Guid.NewGuid().ToString());
    private static readonly BranchId? ValidBranchId = BranchId.Load(Guid.NewGuid().ToString());
    private static readonly ActorId ValidActor = ActorId.Create("user-001");
    #region Create

    [Fact]
    public void Create_WithBranchId_ReturnsSuccessWithBranchScoped()
    {
        var result = Profile.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidBranchId, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidTenantId, result.Value.TenantId);
        Assert.Equal(ValidUserId, result.Value.UserId);
        Assert.Equal(ValidRoleId, result.Value.RoleId);
        Assert.Equal(ValidBranchId, result.Value.BranchId);
        Assert.Equal(ProfileScope.BranchScoped, result.Value.Scope);
        Assert.True(result.Value.IsActive);
        Assert.Empty(result.Value.Permissions);
    }

    [Fact]
    public void Create_WithoutBranchId_ReturnsSuccessWithOrgWide()
    {
        var result = Profile.Create(ValidTenantId, ValidUserId, ValidRoleId, null, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.BranchId);
        Assert.Equal(ProfileScope.OrgWide, result.Value.Scope);
    }

    [Fact]
    public void Create_RaisesProfileCreatedEvent()
    {
        var result = Profile.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidBranchId, ValidActor);

        Assert.True(result.IsSuccess);
        var events = result.Value.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Single(events);
        Assert.IsType<ProfileCreatedEvent>(events[0]);
    }

    #endregion

    #region Deactivate

    [Fact]
    public void Deactivate_WhenActive_ReturnsSuccess()
    {
        var profile = Profile.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidBranchId, ValidActor).Value;

        var result = profile.Deactivate(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.False(profile.IsActive);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ReturnsFailure()
    {
        var profile = Profile.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidBranchId, ValidActor).Value;
        profile.Deactivate(ValidActor);

        var result = profile.Deactivate(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Authorization.ProfileAlreadyInactive, result.Error);
    }

    [Fact]
    public void Deactivate_RaisesProfileDeactivatedEvent()
    {
        var profile = Profile.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidBranchId, ValidActor).Value;

        profile.Deactivate(ValidActor);

        var events = profile.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is ProfileDeactivatedEvent);
    }

    #endregion

    #region Activate

    [Fact]
    public void Activate_WhenInactive_ReturnsSuccess()
    {
        var profile = Profile.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidBranchId, ValidActor).Value;
        profile.Deactivate(ValidActor);

        var result = profile.Activate(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.True(profile.IsActive);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ReturnsFailure()
    {
        var profile = Profile.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidBranchId, ValidActor).Value;

        var result = profile.Activate(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Authorization.ProfileAlreadyActive, result.Error);
    }

    [Fact]
    public void Activate_RaisesProfileActivatedEvent()
    {
        var profile = Profile.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidBranchId, ValidActor).Value;
        profile.Deactivate(ValidActor);

        profile.Activate(ValidActor);

        var events = profile.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is ProfileActivatedEvent);
    }

    #endregion

    #region AssignTemplate

    [Fact]
    public void AssignTemplate_WithValidTemplate_ReturnsSuccess()
    {
        var profile = Profile.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidBranchId, ValidActor).Value;
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, SystemSuiteId.Load(Guid.NewGuid().ToString()), ValidActor).Value;
        template.AddItem(ExclusiveArcTarget.Module, IdValueObject.Create(), ActionId.Load(Guid.NewGuid().ToString()), true, false, ValidActor);
        template.Publish(ValidActor);

        var result = profile.AssignTemplate(template, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(profile.Permissions);
    }

    [Fact]
    public void AssignTemplate_WhenProfileInactive_ReturnsFailure()
    {
        var profile = Profile.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidBranchId, ValidActor).Value;
        profile.Deactivate(ValidActor);
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, SystemSuiteId.Load(Guid.NewGuid().ToString()), ValidActor).Value;
        template.Publish(ValidActor);

        var result = profile.AssignTemplate(template, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Authorization.ProfileAlreadyInactive, result.Error);
    }

    [Fact]
    public void AssignTemplate_WhenTenantMismatch_ReturnsFailure()
    {
        var profile = Profile.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidBranchId, ValidActor).Value;
        var otherTenantId = TenantId.Load(Guid.NewGuid().ToString());
        var template = PermissionTemplate.Create(otherTenantId, ValidRoleId, SystemSuiteId.Load(Guid.NewGuid().ToString()), ValidActor).Value;
        template.Publish(ValidActor);

        var result = profile.AssignTemplate(template, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Authorization.TemplateTenantMismatch, result.Error);
    }

    [Fact]
    public void AssignTemplate_WhenTemplateNotPublished_ReturnsFailure()
    {
        var profile = Profile.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidBranchId, ValidActor).Value;
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, SystemSuiteId.Load(Guid.NewGuid().ToString()), ValidActor).Value;

        var result = profile.AssignTemplate(template, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Authorization.TemplateNotPublishedForProfile, result.Error);
    }

    [Fact]
    public void AssignTemplate_WhenAlreadyLinked_ReturnsFailure()
    {
        var profile = Profile.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidBranchId, ValidActor).Value;
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, SystemSuiteId.Load(Guid.NewGuid().ToString()), ValidActor).Value;
        template.AddItem(ExclusiveArcTarget.Module, IdValueObject.Create(), ActionId.Load(Guid.NewGuid().ToString()), true, false, ValidActor);
        template.Publish(ValidActor);
        profile.AssignTemplate(template, ValidActor);

        var result = profile.AssignTemplate(template, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Authorization.ProfileTemplateAlreadyLinked, result.Error);
    }

    [Fact]
    public void AssignTemplate_RaisesTemplateLinkedToProfileEvent()
    {
        var profile = Profile.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidBranchId, ValidActor).Value;
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, SystemSuiteId.Load(Guid.NewGuid().ToString()), ValidActor).Value;
        template.Publish(ValidActor);

        profile.AssignTemplate(template, ValidActor);

        var events = profile.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is TemplateLinkedToProfileEvent);
    }

    #endregion

    #region Permission Overrides

    [Fact]
    public void OverridePermissionAllow_WhenProfileInactive_ReturnsFailure()
    {
        var profile = Profile.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidBranchId, ValidActor).Value;
        profile.Deactivate(ValidActor);
        var fakeId = IdValueObject.Create();

        var result = profile.OverridePermissionAllow(fakeId, ValidActor);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void OverridePermissionDeny_WhenProfileInactive_ReturnsFailure()
    {
        var profile = Profile.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidBranchId, ValidActor).Value;
        profile.Deactivate(ValidActor);
        var fakeId = IdValueObject.Create();

        var result = profile.OverridePermissionDeny(fakeId, ValidActor);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void OverridePermissionNeutral_WhenProfileInactive_ReturnsFailure()
    {
        var profile = Profile.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidBranchId, ValidActor).Value;
        profile.Deactivate(ValidActor);
        var fakeId = IdValueObject.Create();

        var result = profile.OverridePermissionNeutral(fakeId, ValidActor);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void OverridePermissionAllow_WhenPermissionNotFound_ReturnsFailure()
    {
        var profile = Profile.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidBranchId, ValidActor).Value;
        var fakeId = IdValueObject.Create();

        var result = profile.OverridePermissionAllow(fakeId, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Authorization.PermissionNotFound, result.Error);
    }

    [Fact]
    public void DeactivatePermission_WhenPermissionNotFound_ReturnsFailure()
    {
        var profile = Profile.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidBranchId, ValidActor).Value;
        var fakeId = IdValueObject.Create();

        var result = profile.DeactivatePermission(fakeId, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Authorization.PermissionNotFound, result.Error);
    }

    [Fact]
    public void ActivatePermission_WhenPermissionNotFound_ReturnsFailure()
    {
        var profile = Profile.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidBranchId, ValidActor).Value;
        var fakeId = IdValueObject.Create();

        var result = profile.ActivatePermission(fakeId, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Authorization.PermissionNotFound, result.Error);
    }

    #endregion
}
