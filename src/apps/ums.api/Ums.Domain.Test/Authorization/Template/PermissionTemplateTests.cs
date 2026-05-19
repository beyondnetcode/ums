namespace Ums.Domain.Test.Authorization.Template;

using Ums.Domain.Authorization.Template;
using Xunit;

public class PermissionTemplateTests
{
    private static readonly TenantId ValidTenantId = TenantId.Load(Guid.NewGuid().ToString());
    private static readonly RoleId ValidRoleId = RoleId.Load(Guid.NewGuid().ToString());
    private static readonly SystemSuiteId ValidSystemSuiteId = SystemSuiteId.Load(Guid.NewGuid().ToString());
    private static readonly ActorId ValidActor = ActorId.Create("user-001");
    private static readonly ExclusiveArcTarget ValidTargetType = ExclusiveArcTarget.Module;
    private static readonly IdValueObject ValidTargetId = IdValueObject.Create();
    private static readonly ActionId ValidActionId = ActionId.Load(Guid.NewGuid().ToString());
    #region Create

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidTenantId, result.Value.TenantId);
        Assert.Equal(ValidRoleId, result.Value.RoleId);
        Assert.Equal(ValidSystemSuiteId, result.Value.SystemSuiteId);
        Assert.Equal(TemplateStatus.Draft, result.Value.Status);
        Assert.Empty(result.Value.Items);
    }

    [Fact]
    public void Create_RaisesPermissionTemplateCreatedEvent()
    {
        var result = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor);

        Assert.True(result.IsSuccess);
        var events = result.Value.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Single(events);
        Assert.IsType<PermissionTemplateCreatedEvent>(events[0]);
    }

    #endregion

    #region Publish

    [Fact]
    public void Publish_WhenDraft_ReturnsSuccess()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;

        var result = template.Publish(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(TemplateStatus.Published, template.Status);
    }

    [Fact]
    public void Publish_WhenNotDraft_ReturnsFailure()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;
        template.Publish(ValidActor);

        var result = template.Publish(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Authorization.TemplateNotDraft, result.Error);
    }

    [Fact]
    public void Publish_RaisesPermissionTemplatePublishedEvent()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;

        template.Publish(ValidActor);

        var events = template.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is PermissionTemplatePublishedEvent);
    }

    #endregion

    #region Deprecate

    [Fact]
    public void Deprecate_WhenPublished_ReturnsSuccess()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;
        template.Publish(ValidActor);

        var result = template.Deprecate(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(TemplateStatus.Deprecated, template.Status);
    }

    [Fact]
    public void Deprecate_WhenDraft_ReturnsFailure()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;

        var result = template.Deprecate(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Authorization.TemplateNotPublished, result.Error);
    }

    [Fact]
    public void Deprecate_WhenAlreadyDeprecated_ReturnsFailure()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;
        template.Publish(ValidActor);
        template.Deprecate(ValidActor);

        var result = template.Deprecate(ValidActor);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Deprecate_RaisesPermissionTemplateDeprecatedEvent()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;
        template.Publish(ValidActor);

        template.Deprecate(ValidActor);

        var events = template.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is PermissionTemplateDeprecatedEvent);
    }

    #endregion

    #region AddItem

    [Fact]
    public void AddItem_WhenDraft_ReturnsSuccess()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;

        var result = template.AddItem(ValidTargetType, ValidTargetId, ValidActionId, true, false, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Single(template.Items);
    }

    [Fact]
    public void AddItem_WhenNotDraft_ReturnsFailure()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;
        template.Publish(ValidActor);

        var result = template.AddItem(ValidTargetType, ValidTargetId, ValidActionId, true, false, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Authorization.TemplateNotDraft, result.Error);
    }

    [Fact]
    public void AddItem_WithDuplicateTarget_ReturnsFailure()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;
        template.AddItem(ValidTargetType, ValidTargetId, ValidActionId, true, false, ValidActor);

        var result = template.AddItem(ValidTargetType, ValidTargetId, ValidActionId, false, true, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Authorization.TemplateItemTargetAlreadyExists, result.Error);
    }

    [Fact]
    public void AddItem_RaisesPermissionTemplateMutatedEvent()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;

        template.AddItem(ValidTargetType, ValidTargetId, ValidActionId, true, false, ValidActor);

        var events = template.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is PermissionTemplateMutatedEvent);
    }

    #endregion

    #region RemoveItem

    [Fact]
    public void RemoveItem_WhenItemExists_ReturnsSuccess()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;
        template.AddItem(ValidTargetType, ValidTargetId, ValidActionId, true, false, ValidActor);
        var itemId = template.Items.First().Id;

        var result = template.RemoveItem(itemId, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Empty(template.Items);
    }

    [Fact]
    public void RemoveItem_WhenNotFound_ReturnsFailure()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;
        var fakeId = IdValueObject.Create();

        var result = template.RemoveItem(fakeId, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Common.NotFound, result.Error);
    }

    [Fact]
    public void RemoveItem_WhenNotDraft_ReturnsFailure()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;
        template.AddItem(ValidTargetType, ValidTargetId, ValidActionId, true, false, ValidActor);
        template.Publish(ValidActor);
        var itemId = template.Items.First().Id;

        var result = template.RemoveItem(itemId, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Authorization.TemplateNotDraft, result.Error);
    }

    #endregion

    #region SetItemAllow

    [Fact]
    public void SetItemAllow_WhenItemExists_ReturnsSuccess()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;
        template.AddItem(ValidTargetType, ValidTargetId, ValidActionId, false, false, ValidActor);
        var itemId = template.Items.First().Id;

        var result = template.SetItemAllow(itemId, ValidActor);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void SetItemAllow_WhenItemNotFound_ReturnsFailure()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;
        var fakeId = IdValueObject.Create();

        var result = template.SetItemAllow(fakeId, ValidActor);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void SetItemAllow_WhenNotDraft_ReturnsFailure()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;
        template.AddItem(ValidTargetType, ValidTargetId, ValidActionId, true, false, ValidActor);
        template.Publish(ValidActor);
        var itemId = template.Items.First().Id;

        var result = template.SetItemAllow(itemId, ValidActor);

        Assert.True(result.IsFailure);
    }

    #endregion

    #region SetItemDeny

    [Fact]
    public void SetItemDeny_WhenItemExists_ReturnsSuccess()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;
        template.AddItem(ValidTargetType, ValidTargetId, ValidActionId, false, false, ValidActor);
        var itemId = template.Items.First().Id;

        var result = template.SetItemDeny(itemId, ValidActor);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void SetItemDeny_WhenNotDraft_ReturnsFailure()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;
        template.AddItem(ValidTargetType, ValidTargetId, ValidActionId, true, false, ValidActor);
        template.Publish(ValidActor);
        var itemId = template.Items.First().Id;

        var result = template.SetItemDeny(itemId, ValidActor);

        Assert.True(result.IsFailure);
    }

    #endregion

    #region SetItemNeutral

    [Fact]
    public void SetItemNeutral_WhenItemExists_ReturnsSuccess()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;
        template.AddItem(ValidTargetType, ValidTargetId, ValidActionId, true, false, ValidActor);
        var itemId = template.Items.First().Id;

        var result = template.SetItemNeutral(itemId, ValidActor);

        Assert.True(result.IsSuccess);
    }

    #endregion

    #region ActivateItem

    [Fact]
    public void ActivateItem_WhenItemExists_ReturnsSuccess()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;
        template.AddItem(ValidTargetType, ValidTargetId, ValidActionId, true, false, ValidActor);
        var itemId = template.Items.First().Id;

        var result = template.ActivateItem(itemId, ValidActor);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ActivateItem_WhenNotDraft_ReturnsFailure()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;
        template.AddItem(ValidTargetType, ValidTargetId, ValidActionId, true, false, ValidActor);
        template.Publish(ValidActor);
        var itemId = template.Items.First().Id;

        var result = template.ActivateItem(itemId, ValidActor);

        Assert.True(result.IsFailure);
    }

    #endregion

    #region DeactivateItem

    [Fact]
    public void DeactivateItem_WhenItemExists_ReturnsSuccess()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;
        template.AddItem(ValidTargetType, ValidTargetId, ValidActionId, true, false, ValidActor);
        var itemId = template.Items.First().Id;

        var result = template.DeactivateItem(itemId, ValidActor);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void DeactivateItem_WhenNotDraft_ReturnsFailure()
    {
        var template = PermissionTemplate.Create(ValidTenantId, ValidRoleId, ValidSystemSuiteId, ValidActor).Value;
        template.AddItem(ValidTargetType, ValidTargetId, ValidActionId, true, false, ValidActor);
        template.Publish(ValidActor);
        var itemId = template.Items.First().Id;

        var result = template.DeactivateItem(itemId, ValidActor);

        Assert.True(result.IsFailure);
    }

    #endregion
}
