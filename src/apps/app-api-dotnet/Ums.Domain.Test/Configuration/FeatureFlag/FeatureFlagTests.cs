namespace Ums.Domain.Test.Configuration.FeatureFlag;

using Ums.Domain.Configuration.FeatureFlag;
using Xunit;

public class FeatureFlagTests
{
    private static readonly string ValidFlagCode = "FEATURE-001";
    private static readonly FlagType ValidFlagType = FlagType.Boolean;
    private static readonly string ValidFlagTargets = "all";
    private static readonly ActorId ValidActor = ActorId.Create("user-001");
    #region Create

    [Fact]
    public void Create_WithValidBinaryFlag_ReturnsSuccess()
    {
        var result = FeatureFlag.Create(ValidFlagCode, ValidFlagType, ValidFlagTargets, null, null, null, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidFlagCode, result.Value.FlagCode);
        Assert.Equal(ValidFlagType, result.Value.FlagType);
        Assert.Equal(FlagStatus.Inactive, result.Value.Status);
        Assert.Null(result.Value.RolloutPercentage);
    }

    [Fact]
    public void Create_WithValidPercentageFlag_ReturnsSuccess()
    {
        var result = FeatureFlag.Create(ValidFlagCode, FlagType.Percentage, ValidFlagTargets, null, null, 50, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(50, result.Value.RolloutPercentage);
    }

    [Fact]
    public void Create_WithPercentageFlagWithoutRollout_ReturnsFailure()
    {
        var result = FeatureFlag.Create(ValidFlagCode, FlagType.Percentage, ValidFlagTargets, null, null, null, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Configuration.FlagPercentageOutOfRange, result.Error);
    }

    [Fact]
    public void Create_WithPercentageOutOfRange_ReturnsFailure()
    {
        var result = FeatureFlag.Create(ValidFlagCode, FlagType.Percentage, ValidFlagTargets, null, null, 150, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Configuration.FlagPercentageOutOfRange, result.Error);
    }

    [Fact]
    public void Create_WithNegativePercentage_ReturnsFailure()
    {
        var result = FeatureFlag.Create(ValidFlagCode, FlagType.Percentage, ValidFlagTargets, null, null, -10, ValidActor);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_RaisesFeatureFlagCreatedEvent()
    {
        var result = FeatureFlag.Create(ValidFlagCode, ValidFlagType, ValidFlagTargets, null, null, null, ValidActor);

        Assert.True(result.IsSuccess);
        var events = result.Value.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Single(events);
        Assert.IsType<FeatureFlagCreatedEvent>(events[0]);
    }

    #endregion

    #region Activate

    [Fact]
    public void Activate_WhenInactive_ReturnsSuccess()
    {
        var flag = FeatureFlag.Create(ValidFlagCode, ValidFlagType, ValidFlagTargets, null, null, null, ValidActor).Value;

        var result = flag.Activate(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(FlagStatus.Active, flag.Status);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ReturnsFailure()
    {
        var flag = FeatureFlag.Create(ValidFlagCode, ValidFlagType, ValidFlagTargets, null, null, null, ValidActor).Value;
        flag.Activate(ValidActor);

        var result = flag.Activate(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Configuration.FlagAlreadyActive, result.Error);
    }

    [Fact]
    public void Activate_WhenArchived_ReturnsFailure()
    {
        var flag = FeatureFlag.Create(ValidFlagCode, ValidFlagType, ValidFlagTargets, null, null, null, ValidActor).Value;
        flag.Archive(ValidActor);

        var result = flag.Activate(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Configuration.FlagArchivedCannotChange, result.Error);
    }

    [Fact]
    public void Activate_RaisesFeatureFlagActivatedEvent()
    {
        var flag = FeatureFlag.Create(ValidFlagCode, ValidFlagType, ValidFlagTargets, null, null, null, ValidActor).Value;

        flag.Activate(ValidActor);

        var events = flag.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is FeatureFlagActivatedEvent);
    }

    #endregion

    #region Deactivate

    [Fact]
    public void Deactivate_WhenActive_ReturnsSuccess()
    {
        var flag = FeatureFlag.Create(ValidFlagCode, ValidFlagType, ValidFlagTargets, null, null, null, ValidActor).Value;
        flag.Activate(ValidActor);

        var result = flag.Deactivate(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(FlagStatus.Inactive, flag.Status);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ReturnsFailure()
    {
        var flag = FeatureFlag.Create(ValidFlagCode, ValidFlagType, ValidFlagTargets, null, null, null, ValidActor).Value;

        var result = flag.Deactivate(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Configuration.FlagAlreadyInactive, result.Error);
    }

    [Fact]
    public void Deactivate_WhenArchived_ReturnsFailure()
    {
        var flag = FeatureFlag.Create(ValidFlagCode, ValidFlagType, ValidFlagTargets, null, null, null, ValidActor).Value;
        flag.Archive(ValidActor);

        var result = flag.Deactivate(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Configuration.FlagArchivedCannotChange, result.Error);
    }

    [Fact]
    public void Deactivate_RaisesFeatureFlagDeactivatedEvent()
    {
        var flag = FeatureFlag.Create(ValidFlagCode, ValidFlagType, ValidFlagTargets, null, null, null, ValidActor).Value;
        flag.Activate(ValidActor);

        flag.Deactivate(ValidActor);

        var events = flag.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is FeatureFlagDeactivatedEvent);
    }

    #endregion

    #region Archive

    [Fact]
    public void Archive_WhenActive_ReturnsSuccess()
    {
        var flag = FeatureFlag.Create(ValidFlagCode, ValidFlagType, ValidFlagTargets, null, null, null, ValidActor).Value;
        flag.Activate(ValidActor);

        var result = flag.Archive(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(FlagStatus.Archived, flag.Status);
    }

    [Fact]
    public void Archive_WhenInactive_ReturnsSuccess()
    {
        var flag = FeatureFlag.Create(ValidFlagCode, ValidFlagType, ValidFlagTargets, null, null, null, ValidActor).Value;

        var result = flag.Archive(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(FlagStatus.Archived, flag.Status);
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ReturnsFailure()
    {
        var flag = FeatureFlag.Create(ValidFlagCode, ValidFlagType, ValidFlagTargets, null, null, null, ValidActor).Value;
        flag.Archive(ValidActor);

        var result = flag.Archive(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Configuration.FlagArchivedCannotChange, result.Error);
    }

    [Fact]
    public void Archive_RaisesFeatureFlagArchivedEvent()
    {
        var flag = FeatureFlag.Create(ValidFlagCode, ValidFlagType, ValidFlagTargets, null, null, null, ValidActor).Value;

        flag.Archive(ValidActor);

        var events = flag.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is FeatureFlagArchivedEvent);
    }

    #endregion

    #region Evaluate

    [Fact]
    public void Evaluate_WhenActive_ReturnsTrue()
    {
        var flag = FeatureFlag.Create(ValidFlagCode, ValidFlagType, ValidFlagTargets, null, null, null, ValidActor).Value;
        flag.Activate(ValidActor);

        var result = flag.Evaluate(Guid.NewGuid(), "test context");

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public void Evaluate_WhenInactive_ReturnsFalse()
    {
        var flag = FeatureFlag.Create(ValidFlagCode, ValidFlagType, ValidFlagTargets, null, null, null, ValidActor).Value;

        var result = flag.Evaluate(Guid.NewGuid(), "test context");

        Assert.True(result.IsSuccess);
        Assert.False(result.Value);
    }

    [Fact]
    public void Evaluate_WhenArchived_ReturnsFailure()
    {
        var flag = FeatureFlag.Create(ValidFlagCode, ValidFlagType, ValidFlagTargets, null, null, null, ValidActor).Value;
        flag.Archive(ValidActor);

        var result = flag.Evaluate(Guid.NewGuid(), "test context");

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Configuration.FlagArchivedCannotChange, result.Error);
    }

    [Fact]
    public void Evaluate_RaisesFlagEvaluatedEvent()
    {
        var flag = FeatureFlag.Create(ValidFlagCode, ValidFlagType, ValidFlagTargets, null, null, null, ValidActor).Value;

        flag.Evaluate(Guid.NewGuid(), "test context");

        var events = flag.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is FlagEvaluatedEvent);
    }

    #endregion
}
