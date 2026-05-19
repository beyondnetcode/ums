namespace Ums.Domain.Test.Approvals.DocumentType;

using Ums.Domain.Approvals.DocumentType;
using Xunit;

public class DocumentTypeTests
{
    private static readonly TenantId ValidTenantId = TenantId.Load(Guid.NewGuid().ToString());
    private static readonly Code ValidCode = Code.Create("DOCTYPE-001");
    private static readonly Name ValidName = Name.Create("Passport");
    private static readonly Description ValidDescription = Description.Create("Government-issued passport");
    private static readonly DocumentCriticity ValidCriticity = DocumentCriticity.High;
    private static readonly ActorId ValidActor = ActorId.Create("user-001");
    #region Create

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = DocumentType.Create(ValidTenantId, ValidCode, ValidName, ValidDescription, ValidCriticity, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidTenantId, result.Value.TenantId);
        Assert.Equal(ValidCode, result.Value.Code);
        Assert.Equal(ValidName, result.Value.Name);
        Assert.Equal(ValidDescription, result.Value.Description);
        Assert.Equal(ValidCriticity, result.Value.Criticity);
        Assert.Empty(result.Value.NotificationRules);
        Assert.Null(result.Value.EnforcementPolicy);
    }

    [Fact]
    public void Create_RaisesDocumentTypeRegisteredEvent()
    {
        var result = DocumentType.Create(ValidTenantId, ValidCode, ValidName, ValidDescription, ValidCriticity, ValidActor);

        Assert.True(result.IsSuccess);
        var events = result.Value.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Single(events);
        Assert.IsType<DocumentTypeRegisteredEvent>(events[0]);
    }

    #endregion

    #region Update

    [Fact]
    public void Update_WithValidData_ReturnsSuccess()
    {
        var docType = DocumentType.Create(ValidTenantId, ValidCode, ValidName, ValidDescription, ValidCriticity, ValidActor).Value;
        var newName = Name.Create("Updated Passport");
        var newDescription = Description.Create("Updated description");

        var result = docType.Update(newName, newDescription, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(newName, docType.Name);
        Assert.Equal(newDescription, docType.Description);
    }

    #endregion

    #region ConfigureNotificationRule

    [Fact]
    public void ConfigureNotificationRule_WithValidData_ReturnsSuccess()
    {
        var docType = DocumentType.Create(ValidTenantId, ValidCode, ValidName, ValidDescription, ValidCriticity, ValidActor).Value;
        var channels = new[] { NotificationChannel.Email };
        var ruleCode = Code.Create("RULE-001");
        var ruleDescription = Description.Create("30 day reminder");

        var result = docType.ConfigureNotificationRule(30, channels, ruleCode, ruleDescription, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Single(docType.NotificationRules);
    }

    [Fact]
    public void ConfigureNotificationRule_WithDuplicateDaysBefore_ReturnsFailure()
    {
        var docType = DocumentType.Create(ValidTenantId, ValidCode, ValidName, ValidDescription, ValidCriticity, ValidActor).Value;
        var channels = new[] { NotificationChannel.Email };
        var ruleCode = Code.Create("RULE-001");
        var ruleDescription = Description.Create("30 day reminder");
        docType.ConfigureNotificationRule(30, channels, ruleCode, ruleDescription, ValidActor);

        var result = docType.ConfigureNotificationRule(30, channels, Code.Create("RULE-002"), Description.Create("Another"), ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Compliance.NotificationRuleDaysBeforeNotUnique, result.Error);
    }

    [Fact]
    public void ConfigureNotificationRule_RaisesNotificationRuleConfiguredEvent()
    {
        var docType = DocumentType.Create(ValidTenantId, ValidCode, ValidName, ValidDescription, ValidCriticity, ValidActor).Value;
        var channels = new[] { NotificationChannel.Email };
        var ruleCode = Code.Create("RULE-001");
        var ruleDescription = Description.Create("30 day reminder");

        docType.ConfigureNotificationRule(30, channels, ruleCode, ruleDescription, ValidActor);

        var events = docType.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is NotificationRuleConfiguredEvent);
    }

    #endregion

    #region RemoveNotificationRule

    [Fact]
    public void RemoveNotificationRule_WhenRuleExists_ReturnsSuccess()
    {
        var docType = DocumentType.Create(ValidTenantId, ValidCode, ValidName, ValidDescription, ValidCriticity, ValidActor).Value;
        var channels = new[] { NotificationChannel.Email };
        var ruleCode = Code.Create("RULE-001");
        var ruleDescription = Description.Create("30 day reminder");
        docType.ConfigureNotificationRule(30, channels, ruleCode, ruleDescription, ValidActor);
        var ruleId = docType.NotificationRules.First().Id;

        var result = docType.RemoveNotificationRule(ruleId, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Empty(docType.NotificationRules);
    }

    [Fact]
    public void RemoveNotificationRule_WhenRuleNotFound_ReturnsFailure()
    {
        var docType = DocumentType.Create(ValidTenantId, ValidCode, ValidName, ValidDescription, ValidCriticity, ValidActor).Value;
        var fakeId = IdValueObject.Create();

        var result = docType.RemoveNotificationRule(fakeId, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Compliance.NotificationRuleNotFound, result.Error);
    }

    [Fact]
    public void RemoveNotificationRule_RaisesNotificationRuleRemovedEvent()
    {
        var docType = DocumentType.Create(ValidTenantId, ValidCode, ValidName, ValidDescription, ValidCriticity, ValidActor).Value;
        var channels = new[] { NotificationChannel.Email };
        var ruleCode = Code.Create("RULE-001");
        var ruleDescription = Description.Create("30 day reminder");
        docType.ConfigureNotificationRule(30, channels, ruleCode, ruleDescription, ValidActor);
        var ruleId = docType.NotificationRules.First().Id;

        docType.RemoveNotificationRule(ruleId, ValidActor);

        var events = docType.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is NotificationRuleRemovedEvent);
    }

    #endregion

    #region DefineEnforcementPolicy

    [Fact]
    public void DefineEnforcementPolicy_WithValidData_ReturnsSuccess()
    {
        var docType = DocumentType.Create(ValidTenantId, ValidCode, ValidName, ValidDescription, ValidCriticity, ValidActor).Value;

        var result = docType.DefineEnforcementPolicy(AccessEnforcementAction.RestrictProfile, 7, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.NotNull(docType.EnforcementPolicy);
    }

    [Fact]
    public void DefineEnforcementPolicy_WhenAlreadyDefined_ReturnsFailure()
    {
        var docType = DocumentType.Create(ValidTenantId, ValidCode, ValidName, ValidDescription, ValidCriticity, ValidActor).Value;
        docType.DefineEnforcementPolicy(AccessEnforcementAction.RestrictProfile, 7, ValidActor);

        var result = docType.DefineEnforcementPolicy(AccessEnforcementAction.BlockUser, 14, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Compliance.EnforcementPolicyAlreadyDefined, result.Error);
    }

    [Fact]
    public void DefineEnforcementPolicy_WithCriticalCriticity_AllowsAnyAction()
    {
        var docType = DocumentType.Create(ValidTenantId, ValidCode, ValidName, ValidDescription, DocumentCriticity.Critical, ValidActor).Value;

        var result = docType.DefineEnforcementPolicy(AccessEnforcementAction.BlockUser, 7, ValidActor);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void DefineEnforcementPolicy_WithLowCriticityAndBlockAction_ReturnsFailure()
    {
        var docType = DocumentType.Create(ValidTenantId, ValidCode, ValidName, ValidDescription, DocumentCriticity.Low, ValidActor).Value;

        var result = docType.DefineEnforcementPolicy(AccessEnforcementAction.BlockUser, 7, ValidActor);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void DefineEnforcementPolicy_RaisesEnforcementPolicyDefinedEvent()
    {
        var docType = DocumentType.Create(ValidTenantId, ValidCode, ValidName, ValidDescription, ValidCriticity, ValidActor).Value;

        docType.DefineEnforcementPolicy(AccessEnforcementAction.RestrictProfile, 7, ValidActor);

        var events = docType.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is EnforcementPolicyDefinedEvent);
    }

    #endregion

    #region UpdateEnforcementPolicy

    [Fact]
    public void UpdateEnforcementPolicy_WhenPolicyExists_ReturnsSuccess()
    {
        var docType = DocumentType.Create(ValidTenantId, ValidCode, ValidName, ValidDescription, ValidCriticity, ValidActor).Value;
        docType.DefineEnforcementPolicy(AccessEnforcementAction.RestrictProfile, 7, ValidActor);

        var result = docType.UpdateEnforcementPolicy(AccessEnforcementAction.BlockUser, 14, ValidActor);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void UpdateEnforcementPolicy_WhenPolicyNotDefined_ReturnsFailure()
    {
        var docType = DocumentType.Create(ValidTenantId, ValidCode, ValidName, ValidDescription, ValidCriticity, ValidActor).Value;

        var result = docType.UpdateEnforcementPolicy(AccessEnforcementAction.BlockUser, 14, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Compliance.EnforcementPolicyNotFound, result.Error);
    }

    [Fact]
    public void UpdateEnforcementPolicy_RaisesEnforcementPolicyUpdatedEvent()
    {
        var docType = DocumentType.Create(ValidTenantId, ValidCode, ValidName, ValidDescription, ValidCriticity, ValidActor).Value;
        docType.DefineEnforcementPolicy(AccessEnforcementAction.RestrictProfile, 7, ValidActor);

        docType.UpdateEnforcementPolicy(AccessEnforcementAction.BlockUser, 14, ValidActor);

        var events = docType.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is EnforcementPolicyUpdatedEvent);
    }

    #endregion
}
