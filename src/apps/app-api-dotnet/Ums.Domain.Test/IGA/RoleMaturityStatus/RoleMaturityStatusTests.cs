namespace Ums.Domain.Test.IGA.RoleMaturityStatus;

using Ums.Domain.IGA.RoleMaturityStatus;
using Xunit;

public class RoleMaturityStatusTests
{
    private static readonly TenantId ValidTenantId = TenantId.Load(Guid.NewGuid().ToString());
    private static readonly UserId ValidUserId = UserId.Load(Guid.NewGuid().ToString());
    private static readonly RoleId ValidRoleId = RoleId.Load(Guid.NewGuid().ToString());
    private static readonly RoleMaturityLevel ValidMaturityLevel = RoleMaturityLevel.Junior;
    private static readonly ActorId ValidActor = ActorId.Create("user-001");
    #region Create

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = RoleMaturityStatus.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidMaturityLevel, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidTenantId, result.Value.TenantId);
        Assert.Equal(ValidUserId, result.Value.UserId);
        Assert.Equal(ValidRoleId, result.Value.RoleId);
        Assert.Equal(ValidMaturityLevel, result.Value.CurrentMaturityLevel);
        Assert.Equal(RoleMaturityLevel.Intermediate, result.Value.NextEligibleMaturityLevel);
        Assert.Equal(0, result.Value.CompletedCertificationsCount);
        Assert.Equal(0, result.Value.CompletedTrainingsCount);
        Assert.Equal(0m, result.Value.PerformanceScore);
        Assert.True(result.Value.HasNoComplianceIssues);
        Assert.Null(result.Value.BlockingFactor);
        Assert.Null(result.Value.EligibleForPromotionAt);
    }

    #endregion

    #region UpdateMaturityLevel

    [Fact]
    public void UpdateMaturityLevel_WithDifferentLevel_ReturnsSuccess()
    {
        var status = RoleMaturityStatus.Create(ValidTenantId, ValidUserId, ValidRoleId, RoleMaturityLevel.Junior, ValidActor).Value;

        var result = status.UpdateMaturityLevel(RoleMaturityLevel.Intermediate, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(RoleMaturityLevel.Intermediate, status.CurrentMaturityLevel);
        Assert.Equal(RoleMaturityLevel.Senior, status.NextEligibleMaturityLevel);
        Assert.Null(status.EligibleForPromotionAt);
    }

    [Fact]
    public void UpdateMaturityLevel_WithSameLevel_ReturnsFailure()
    {
        var status = RoleMaturityStatus.Create(ValidTenantId, ValidUserId, ValidRoleId, RoleMaturityLevel.Junior, ValidActor).Value;

        var result = status.UpdateMaturityLevel(RoleMaturityLevel.Junior, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.IGA.MaturityLevelUnchanged, result.Error);
    }

    [Fact]
    public void UpdateMaturityLevel_FromSenior_SetsNextToLead()
    {
        var status = RoleMaturityStatus.Create(ValidTenantId, ValidUserId, ValidRoleId, RoleMaturityLevel.Senior, ValidActor).Value;

        status.UpdateMaturityLevel(RoleMaturityLevel.Lead, ValidActor);

        Assert.Equal(RoleMaturityLevel.Principal, status.NextEligibleMaturityLevel);
    }

    [Fact]
    public void UpdateMaturityLevel_FromPrincipal_SetsNextToNull()
    {
        var status = RoleMaturityStatus.Create(ValidTenantId, ValidUserId, ValidRoleId, RoleMaturityLevel.Lead, ValidActor).Value;
        status.UpdateMaturityLevel(RoleMaturityLevel.Principal, ValidActor);

        var result = status.UpdateMaturityLevel(RoleMaturityLevel.Principal, ValidActor);

        Assert.True(result.IsFailure);
    }

    #endregion

    #region RecordCertificationCompletion

    [Fact]
    public void RecordCertificationCompletion_IncrementsCount()
    {
        var status = RoleMaturityStatus.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidMaturityLevel, ValidActor).Value;

        status.RecordCertificationCompletion(ValidActor);
        status.RecordCertificationCompletion(ValidActor);

        Assert.Equal(2, status.CompletedCertificationsCount);
    }

    #endregion

    #region RecordTrainingCompletion

    [Fact]
    public void RecordTrainingCompletion_IncrementsCount()
    {
        var status = RoleMaturityStatus.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidMaturityLevel, ValidActor).Value;

        status.RecordTrainingCompletion(ValidActor);
        status.RecordTrainingCompletion(ValidActor);
        status.RecordTrainingCompletion(ValidActor);

        Assert.Equal(3, status.CompletedTrainingsCount);
    }

    #endregion

    #region UpdatePerformanceScore

    [Fact]
    public void UpdatePerformanceScore_WithValidScore_ReturnsSuccess()
    {
        var status = RoleMaturityStatus.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidMaturityLevel, ValidActor).Value;

        var result = status.UpdatePerformanceScore(4.5m, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(4.5m, status.PerformanceScore);
    }

    [Fact]
    public void UpdatePerformanceScore_WithZeroScore_ReturnsSuccess()
    {
        var status = RoleMaturityStatus.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidMaturityLevel, ValidActor).Value;

        var result = status.UpdatePerformanceScore(0m, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, status.PerformanceScore);
    }

    [Fact]
    public void UpdatePerformanceScore_WithMaxScore_ReturnsSuccess()
    {
        var status = RoleMaturityStatus.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidMaturityLevel, ValidActor).Value;

        var result = status.UpdatePerformanceScore(5m, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(5m, status.PerformanceScore);
    }

    [Fact]
    public void UpdatePerformanceScore_WithNegativeScore_ReturnsFailure()
    {
        var status = RoleMaturityStatus.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidMaturityLevel, ValidActor).Value;

        var result = status.UpdatePerformanceScore(-1m, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.IGA.InvalidPerformanceScore, result.Error);
    }

    [Fact]
    public void UpdatePerformanceScore_WithScoreAboveMax_ReturnsFailure()
    {
        var status = RoleMaturityStatus.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidMaturityLevel, ValidActor).Value;

        var result = status.UpdatePerformanceScore(5.1m, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.IGA.InvalidPerformanceScore, result.Error);
    }

    #endregion

    #region MarkComplianceIssue

    [Fact]
    public void MarkComplianceIssue_SetsHasNoComplianceIssuesToFalse()
    {
        var status = RoleMaturityStatus.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidMaturityLevel, ValidActor).Value;
        var reason = TextValueObject.Create("Failed background check");

        var result = status.MarkComplianceIssue(reason, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.False(status.HasNoComplianceIssues);
        Assert.Equal(reason, status.BlockingFactor);
    }

    #endregion

    #region ResolveComplianceIssue

    [Fact]
    public void ResolveComplianceIssue_SetsHasNoComplianceIssuesToTrue()
    {
        var status = RoleMaturityStatus.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidMaturityLevel, ValidActor).Value;
        var reason = TextValueObject.Create("Failed background check");
        status.MarkComplianceIssue(reason, ValidActor);

        var result = status.ResolveComplianceIssue(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.True(status.HasNoComplianceIssues);
        Assert.Null(status.BlockingFactor);
    }

    #endregion

    #region ReviewEligibility

    [Fact]
    public void ReviewEligibility_WhenEligible_SetsEligibleForPromotionAt()
    {
        var status = RoleMaturityStatus.Create(ValidTenantId, ValidUserId, ValidRoleId, RoleMaturityLevel.Junior, ValidActor).Value;
        status.UpdatePerformanceScore(4.0m, ValidActor);
        status.RecordCertificationCompletion(ValidActor);
        status.RecordTrainingCompletion(ValidActor);

        var result = status.ReviewEligibility(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.NotNull(status.LastReviewedAt);
    }

    [Fact]
    public void ReviewEligibility_WhenComplianceIssues_NotEligible()
    {
        var status = RoleMaturityStatus.Create(ValidTenantId, ValidUserId, ValidRoleId, RoleMaturityLevel.Junior, ValidActor).Value;
        status.MarkComplianceIssue(TextValueObject.Create("Issue"), ValidActor);
        status.UpdatePerformanceScore(4.0m, ValidActor);

        status.ReviewEligibility(ValidActor);

        Assert.Null(status.EligibleForPromotionAt);
    }

    [Fact]
    public void ReviewEligibility_WhenLowPerformance_NotEligible()
    {
        var status = RoleMaturityStatus.Create(ValidTenantId, ValidUserId, ValidRoleId, RoleMaturityLevel.Junior, ValidActor).Value;
        status.UpdatePerformanceScore(2.0m, ValidActor);

        status.ReviewEligibility(ValidActor);

        Assert.Null(status.EligibleForPromotionAt);
    }

    [Fact]
    public void ReviewEligibility_UpdatesLastReviewedAt()
    {
        var status = RoleMaturityStatus.Create(ValidTenantId, ValidUserId, ValidRoleId, ValidMaturityLevel, ValidActor).Value;

        status.ReviewEligibility(ValidActor);

        Assert.NotNull(status.LastReviewedAt);
    }

    #endregion
}
