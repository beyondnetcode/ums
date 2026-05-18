namespace Ums.Domain.Test.IGA.PromotionRequest.PromotionImpactAnalysis;

using Ums.Domain.IGA.PromotionRequest.PromotionImpactAnalysis;
using Xunit;

public class PromotionImpactAnalysisTests
{
    private static readonly PromotionRequestId ValidPromotionRequestId = PromotionRequestId.Load(Guid.NewGuid().ToString());
    private static readonly decimal ValidRiskScore = 25.0m;
    private static readonly TextValueObject ValidRiskLevel = TextValueObject.Create("Low");
    private static readonly int ValidNewPermissionsCount = 5;
    private static readonly int ValidRemovedPermissionsCount = 2;
    private static readonly int ValidAffectedSystemsCount = 3;
    private static readonly ActorId ValidActor = ActorId.Create("user-001");
    #region Create

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = PromotionImpactAnalysis.Create(
            ValidPromotionRequestId, ValidRiskScore, ValidRiskLevel,
            ValidNewPermissionsCount, ValidRemovedPermissionsCount, ValidAffectedSystemsCount,
            null, null, null, ValidActor, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidPromotionRequestId, result.Value.PromotionRequestId);
        Assert.Equal(ValidRiskScore, result.Value.RiskScore);
        Assert.Equal(ValidRiskLevel, result.Value.RiskLevel);
        Assert.Equal(ValidNewPermissionsCount, result.Value.NewPermissionsCount);
        Assert.Equal(ValidRemovedPermissionsCount, result.Value.RemovedPermissionsCount);
        Assert.Equal(ValidAffectedSystemsCount, result.Value.AffectedSystemsCount);
        Assert.Null(result.Value.ConflictingPermissions);
        Assert.Null(result.Value.RiskFactors);
        Assert.Null(result.Value.SuggestedMitigations);
        Assert.NotNull(result.Value.AnalyzedAt);
        Assert.NotNull(result.Value.AnalyzedBy);
    }

    [Fact]
    public void Create_WithAllOptionalFields_ReturnsSuccess()
    {
        var conflicts = TextValueObject.Create("Permission A conflicts with B");
        var riskFactors = TextValueObject.Create("High-risk system access");
        var mitigations = TextValueObject.Create("Implement monitoring");

        var result = PromotionImpactAnalysis.Create(
            ValidPromotionRequestId, ValidRiskScore, ValidRiskLevel,
            ValidNewPermissionsCount, ValidRemovedPermissionsCount, ValidAffectedSystemsCount,
            conflicts, riskFactors, mitigations, ValidActor, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(conflicts, result.Value.ConflictingPermissions);
        Assert.Equal(riskFactors, result.Value.RiskFactors);
        Assert.Equal(mitigations, result.Value.SuggestedMitigations);
    }

    [Fact]
    public void Create_WithZeroRiskScore_ReturnsSuccess()
    {
        var result = PromotionImpactAnalysis.Create(
            ValidPromotionRequestId, 0m, ValidRiskLevel,
            ValidNewPermissionsCount, ValidRemovedPermissionsCount, ValidAffectedSystemsCount,
            null, null, null, ValidActor, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.Value.RiskScore);
    }

    [Fact]
    public void Create_WithMaxRiskScore_ReturnsSuccess()
    {
        var result = PromotionImpactAnalysis.Create(
            ValidPromotionRequestId, 100m, ValidRiskLevel,
            ValidNewPermissionsCount, ValidRemovedPermissionsCount, ValidAffectedSystemsCount,
            null, null, null, ValidActor, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(100m, result.Value.RiskScore);
    }

    [Fact]
    public void Create_WithNegativeRiskScore_ReturnsFailure()
    {
        var result = PromotionImpactAnalysis.Create(
            ValidPromotionRequestId, -1m, ValidRiskLevel,
            ValidNewPermissionsCount, ValidRemovedPermissionsCount, ValidAffectedSystemsCount,
            null, null, null, ValidActor, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.IGA.InvalidPerformanceScore, result.Error);
    }

    [Fact]
    public void Create_WithRiskScoreAbove100_ReturnsFailure()
    {
        var result = PromotionImpactAnalysis.Create(
            ValidPromotionRequestId, 101m, ValidRiskLevel,
            ValidNewPermissionsCount, ValidRemovedPermissionsCount, ValidAffectedSystemsCount,
            null, null, null, ValidActor, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.IGA.InvalidPerformanceScore, result.Error);
    }

    [Fact]
    public void Create_WithZeroCounts_ReturnsSuccess()
    {
        var result = PromotionImpactAnalysis.Create(
            ValidPromotionRequestId, ValidRiskScore, ValidRiskLevel,
            0, 0, 0, null, null, null, ValidActor, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.NewPermissionsCount);
        Assert.Equal(0, result.Value.RemovedPermissionsCount);
        Assert.Equal(0, result.Value.AffectedSystemsCount);
    }

    #endregion
}
