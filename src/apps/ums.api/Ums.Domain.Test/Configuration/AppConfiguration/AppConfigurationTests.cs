namespace Ums.Domain.Test.Configuration.AppConfiguration;

using Ums.Domain.Configuration.AppConfiguration;
using Xunit;

public class AppConfigurationTests
{
    private static readonly TenantId? ValidTenantId = TenantId.Load(Guid.NewGuid().ToString());
    private static readonly SystemSuiteId? ValidSystemSuiteId = SystemSuiteId.Load(Guid.NewGuid().ToString());
    private static readonly IdValueObject? ValidModuleId = IdValueObject.Create();
    private static readonly Code ValidCode = Code.Create("CONFIG-001");
    private static readonly ConfigurationValue ValidValue = ConfigurationValue.Create("value123");
    private static readonly Description ValidDescription = Description.Create("Test configuration");
    private static readonly ActorId ValidActor = ActorId.Create("user-001");
    #region Create

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = AppConfiguration.Create(
            ValidTenantId, ValidSystemSuiteId, ValidModuleId, ValidCode, ValidValue, ValidDescription, true, false, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidCode, result.Value.Code);
        Assert.Equal(ConfigStatus.Draft, result.Value.Status);
        Assert.True(result.Value.IsInheritable);
        Assert.NotNull(result.Value.Version);
    }

    [Fact]
    public void Create_WithNullTenantId_ReturnsSuccess()
    {
        var result = AppConfiguration.Create(
            null, ValidSystemSuiteId, ValidModuleId, ValidCode, ValidValue, ValidDescription, true, false, ValidActor);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_RaisesAppConfigCreatedEvent()
    {
        var result = AppConfiguration.Create(
            ValidTenantId, ValidSystemSuiteId, ValidModuleId, ValidCode, ValidValue, ValidDescription, true, false, ValidActor);

        Assert.True(result.IsSuccess);
        var events = result.Value.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Single(events);
        Assert.IsType<AppConfigCreatedEvent>(events[0]);
    }

    #endregion

    #region Publish

    [Fact]
    public void Publish_WhenDraft_ReturnsSuccess()
    {
        var config = AppConfiguration.Create(
            ValidTenantId, ValidSystemSuiteId, ValidModuleId, ValidCode, ValidValue, ValidDescription, true, false, ValidActor).Value;

        var result = config.Publish(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(ConfigStatus.Published, config.Status);
    }

    [Fact]
    public void Publish_WhenNotDraft_ReturnsFailure()
    {
        var config = AppConfiguration.Create(
            ValidTenantId, ValidSystemSuiteId, ValidModuleId, ValidCode, ValidValue, ValidDescription, true, false, ValidActor).Value;
        config.Publish(ValidActor);

        var result = config.Publish(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Configuration.AppConfigNotDraft, result.Error);
    }

    [Fact]
    public void Publish_RaisesAppConfigPublishedEvent()
    {
        var config = AppConfiguration.Create(
            ValidTenantId, ValidSystemSuiteId, ValidModuleId, ValidCode, ValidValue, ValidDescription, true, false, ValidActor).Value;

        config.Publish(ValidActor);

        var events = config.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is AppConfigPublishedEvent);
    }

    #endregion

    #region Archive

    [Fact]
    public void Archive_WhenPublished_ReturnsSuccess()
    {
        var config = AppConfiguration.Create(
            ValidTenantId, ValidSystemSuiteId, ValidModuleId, ValidCode, ValidValue, ValidDescription, true, false, ValidActor).Value;
        config.Publish(ValidActor);

        var result = config.Archive(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(ConfigStatus.Archived, config.Status);
    }

    [Fact]
    public void Archive_WhenNotPublished_ReturnsFailure()
    {
        var config = AppConfiguration.Create(
            ValidTenantId, ValidSystemSuiteId, ValidModuleId, ValidCode, ValidValue, ValidDescription, true, false, ValidActor).Value;

        var result = config.Archive(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Configuration.AppConfigNotPublished, result.Error);
    }

    [Fact]
    public void Archive_RaisesAppConfigArchivedEvent()
    {
        var config = AppConfiguration.Create(
            ValidTenantId, ValidSystemSuiteId, ValidModuleId, ValidCode, ValidValue, ValidDescription, true, false, ValidActor).Value;
        config.Publish(ValidActor);

        config.Archive(ValidActor);

        var events = config.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is AppConfigArchivedEvent);
    }

    #endregion

    #region Update

    [Fact]
    public void Update_WhenDraft_ReturnsSuccess()
    {
        var config = AppConfiguration.Create(
            ValidTenantId, ValidSystemSuiteId, ValidModuleId, ValidCode, ValidValue, ValidDescription, true, false, ValidActor).Value;
        var newValue = ConfigurationValue.Create("newvalue456");
        var newDescription = Description.Create("Updated description");

        var result = config.Update(newValue, newDescription, ValidActor);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Update_WhenNotDraft_ReturnsFailure()
    {
        var config = AppConfiguration.Create(
            ValidTenantId, ValidSystemSuiteId, ValidModuleId, ValidCode, ValidValue, ValidDescription, true, false, ValidActor).Value;
        config.Publish(ValidActor);
        var newValue = ConfigurationValue.Create("newvalue456");
        var newDescription = Description.Create("Updated description");

        var result = config.Update(newValue, newDescription, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Configuration.AppConfigNotDraft, result.Error);
    }

    [Fact]
    public void Update_RaisesAppConfigUpdatedEvent()
    {
        var config = AppConfiguration.Create(
            ValidTenantId, ValidSystemSuiteId, ValidModuleId, ValidCode, ValidValue, ValidDescription, true, false, ValidActor).Value;
        var newValue = ConfigurationValue.Create("newvalue456");
        var newDescription = Description.Create("Updated description");

        config.Update(newValue, newDescription, ValidActor);

        var events = config.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is AppConfigUpdatedEvent);
    }

    [Fact]
    public void Update_BumpsMinorVersion()
    {
        var config = AppConfiguration.Create(
            ValidTenantId, ValidSystemSuiteId, ValidModuleId, ValidCode, ValidValue, ValidDescription, true, false, ValidActor).Value;
        var newValue = ConfigurationValue.Create("newvalue456");
        var newDescription = Description.Create("Updated description");

        config.Update(newValue, newDescription, ValidActor);

        Assert.NotEqual("1.0.0", config.Version);
    }

    #endregion
}
