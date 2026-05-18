namespace Ums.Domain.Test.IGA.PromotionRequest;

using Ums.Domain.IGA.PromotionRequest;
using Xunit;

public class PromotionRequestTests
{
    private static readonly TenantId ValidTenantId = TenantId.Load(Guid.NewGuid().ToString());
    private static readonly UserId ValidUserId = UserId.Load(Guid.NewGuid().ToString());
    private static readonly RoleId ValidCurrentRoleId = RoleId.Load(Guid.NewGuid().ToString());
    private static readonly RoleId ValidTargetRoleId = RoleId.Load(Guid.NewGuid().ToString());
    private static readonly UserId ValidManagerId = UserId.Load(Guid.NewGuid().ToString());
    private static readonly TextValueObject? ValidReason = TextValueObject.Create("Career progression");
    private static readonly ActorId ValidActor = ActorId.Create("user-001");
    #region Create

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, ValidReason, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidTenantId, result.Value.TenantId);
        Assert.Equal(ValidUserId, result.Value.UserId);
        Assert.Equal(ValidCurrentRoleId, result.Value.CurrentRoleId);
        Assert.Equal(ValidTargetRoleId, result.Value.TargetRoleId);
        Assert.Equal(ValidManagerId, result.Value.ManagerId);
        Assert.Equal(ApprovalDecision.Pending, result.Value.ManagerApprovalStatus);
        Assert.Equal(ApprovalDecision.Pending, result.Value.SecurityApprovalStatus);
        Assert.Equal(PromotionStatus.Draft, result.Value.Status);
        Assert.Empty(result.Value.ImpactAnalyses);
    }

    [Fact]
    public void Create_WithoutReason_ReturnsSuccess()
    {
        var result = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, null, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.RequestReason);
    }

    #endregion

    #region Submit

    [Fact]
    public void Submit_WhenDraft_ReturnsSuccess()
    {
        var request = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, ValidReason, ValidActor).Value;

        var result = request.Submit(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(PromotionStatus.PendingManagerApproval, request.Status);
    }

    [Fact]
    public void Submit_WhenNotDraft_ReturnsFailure()
    {
        var request = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, ValidReason, ValidActor).Value;
        request.Submit(ValidActor);

        var result = request.Submit(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.IGA.PromotionNotInDraft, result.Error);
    }

    #endregion

    #region ManagerApprove

    [Fact]
    public void ManagerApprove_WhenPendingManagerApproval_ReturnsSuccess()
    {
        var request = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, ValidReason, ValidActor).Value;
        request.Submit(ValidActor);

        var result = request.ManagerApprove(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(ApprovalDecision.Approved, request.ManagerApprovalStatus);
        Assert.Equal(PromotionStatus.PendingSecurityReview, request.Status);
        Assert.NotNull(request.ManagerDecisionAt);
    }

    [Fact]
    public void ManagerApprove_WhenNotPendingManagerApproval_ReturnsFailure()
    {
        var request = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, ValidReason, ValidActor).Value;

        var result = request.ManagerApprove(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.IGA.PromotionNotPendingManager, result.Error);
    }

    #endregion

    #region ManagerReject

    [Fact]
    public void ManagerReject_WhenPendingManagerApproval_ReturnsSuccess()
    {
        var request = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, ValidReason, ValidActor).Value;
        request.Submit(ValidActor);

        var result = request.ManagerReject(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(ApprovalDecision.Rejected, request.ManagerApprovalStatus);
        Assert.Equal(PromotionStatus.Rejected, request.Status);
    }

    [Fact]
    public void ManagerReject_WhenNotPendingManagerApproval_ReturnsFailure()
    {
        var request = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, ValidReason, ValidActor).Value;

        var result = request.ManagerReject(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.IGA.PromotionNotPendingManager, result.Error);
    }

    #endregion

    #region SecurityReviewLowRisk

    [Fact]
    public void SecurityReviewLowRisk_WhenPendingSecurityReview_ReturnsSuccess()
    {
        var request = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, ValidReason, ValidActor).Value;
        request.Submit(ValidActor);
        request.ManagerApprove(ValidActor);

        var result = request.SecurityReviewLowRisk(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(ApprovalDecision.Approved, request.SecurityApprovalStatus);
        Assert.Equal(PromotionStatus.ApprovedReadyToExecute, request.Status);
        Assert.NotNull(request.SecurityDecisionAt);
    }

    [Fact]
    public void SecurityReviewLowRisk_WhenNotPendingSecurityReview_ReturnsFailure()
    {
        var request = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, ValidReason, ValidActor).Value;

        var result = request.SecurityReviewLowRisk(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.IGA.PromotionNotPendingSecurity, result.Error);
    }

    #endregion

    #region SecurityReviewHighRisk

    [Fact]
    public void SecurityReviewHighRisk_WhenPendingSecurityReview_ReturnsSuccess()
    {
        var request = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, ValidReason, ValidActor).Value;
        request.Submit(ValidActor);
        request.ManagerApprove(ValidActor);

        var result = request.SecurityReviewHighRisk(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(PromotionStatus.PendingSecurityApproval, request.Status);
    }

    #endregion

    #region SecurityApprove

    [Fact]
    public void SecurityApprove_WhenPendingSecurityApproval_ReturnsSuccess()
    {
        var request = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, ValidReason, ValidActor).Value;
        request.Submit(ValidActor);
        request.ManagerApprove(ValidActor);
        request.SecurityReviewHighRisk(ValidActor);

        var result = request.SecurityApprove(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(ApprovalDecision.Approved, request.SecurityApprovalStatus);
        Assert.Equal(PromotionStatus.ApprovedReadyToExecute, request.Status);
    }

    [Fact]
    public void SecurityApprove_WhenNotPendingSecurityApproval_ReturnsFailure()
    {
        var request = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, ValidReason, ValidActor).Value;

        var result = request.SecurityApprove(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.IGA.PromotionNotPendingSecurity, result.Error);
    }

    #endregion

    #region SecurityReject

    [Fact]
    public void SecurityReject_WhenPendingSecurityApproval_ReturnsSuccess()
    {
        var request = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, ValidReason, ValidActor).Value;
        request.Submit(ValidActor);
        request.ManagerApprove(ValidActor);
        request.SecurityReviewHighRisk(ValidActor);

        var result = request.SecurityReject(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(ApprovalDecision.Rejected, request.SecurityApprovalStatus);
        Assert.Equal(PromotionStatus.Rejected, request.Status);
    }

    [Fact]
    public void SecurityReject_WhenPendingSecurityReview_ReturnsSuccess()
    {
        var request = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, ValidReason, ValidActor).Value;
        request.Submit(ValidActor);
        request.ManagerApprove(ValidActor);

        var result = request.SecurityReject(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(PromotionStatus.Rejected, request.Status);
    }

    #endregion

    #region Execute

    [Fact]
    public void Execute_WhenApprovedReadyToExecute_ReturnsSuccess()
    {
        var request = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, ValidReason, ValidActor).Value;
        request.Submit(ValidActor);
        request.ManagerApprove(ValidActor);
        request.SecurityReviewLowRisk(ValidActor);

        var result = request.Execute(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(PromotionStatus.Executed, request.Status);
        Assert.NotNull(request.ExecutedAt);
        Assert.Equal(ValidActor, request.ExecutedBy);
    }

    [Fact]
    public void Execute_WhenNotApproved_ReturnsFailure()
    {
        var request = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, ValidReason, ValidActor).Value;

        var result = request.Execute(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.IGA.PromotionNotApproved, result.Error);
    }

    #endregion

    #region Verify

    [Fact]
    public void Verify_WhenExecuted_ReturnsSuccess()
    {
        var request = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, ValidReason, ValidActor).Value;
        request.Submit(ValidActor);
        request.ManagerApprove(ValidActor);
        request.SecurityReviewLowRisk(ValidActor);
        request.Execute(ValidActor);

        var result = request.Verify(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(PromotionStatus.Verified, request.Status);
        Assert.NotNull(request.VerifiedAt);
    }

    [Fact]
    public void Verify_WhenNotExecuted_ReturnsFailure()
    {
        var request = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, ValidReason, ValidActor).Value;

        var result = request.Verify(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.IGA.PromotionAlreadyExecuted, result.Error);
    }

    #endregion

    #region MarkVerificationFailed

    [Fact]
    public void MarkVerificationFailed_WhenExecuted_ReturnsSuccess()
    {
        var request = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, ValidReason, ValidActor).Value;
        request.Submit(ValidActor);
        request.ManagerApprove(ValidActor);
        request.SecurityReviewLowRisk(ValidActor);
        request.Execute(ValidActor);

        var result = request.MarkVerificationFailed(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(PromotionStatus.VerificationFailed, request.Status);
    }

    [Fact]
    public void MarkVerificationFailed_WhenNotExecuted_ReturnsFailure()
    {
        var request = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, ValidReason, ValidActor).Value;

        var result = request.MarkVerificationFailed(ValidActor);

        Assert.True(result.IsFailure);
    }

    #endregion

    #region AddImpactAnalysis

    [Fact]
    public void AddImpactAnalysis_WithValidData_ReturnsSuccess()
    {
        var request = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, ValidReason, ValidActor).Value;
        var riskLevel = TextValueObject.Create("Low");
        var riskFactors = TextValueObject.Create("No significant risks");
        var mitigations = TextValueObject.Create("Standard monitoring");
        var conflicts = TextValueObject.Create("None");

        var result = request.AddImpactAnalysis(
            25.0m, riskLevel, 5, 2, 3, conflicts, riskFactors, mitigations, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Single(request.ImpactAnalyses);
    }

    [Fact]
    public void AddImpactAnalysis_WhenAlreadyExists_ReturnsFailure()
    {
        var request = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, ValidReason, ValidActor).Value;
        var riskLevel = TextValueObject.Create("Low");
        request.AddImpactAnalysis(25.0m, riskLevel, 5, 2, 3, null, null, null, ValidActor);

        var result = request.AddImpactAnalysis(
            30.0m, TextValueObject.Create("Medium"), 6, 3, 4, null, null, null, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.IGA.ImpactAnalysisAlreadyExists, result.Error);
    }

    [Fact]
    public void AddImpactAnalysis_WithInvalidRiskScore_ReturnsFailure()
    {
        var request = PromotionRequest.Create(
            ValidTenantId, ValidUserId, ValidCurrentRoleId, ValidTargetRoleId, ValidManagerId, ValidReason, ValidActor).Value;
        var riskLevel = TextValueObject.Create("Low");

        var result = request.AddImpactAnalysis(
            150.0m, riskLevel, 5, 2, 3, null, null, null, ValidActor);

        Assert.True(result.IsFailure);
    }

    #endregion
}
