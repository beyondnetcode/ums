namespace Ums.Domain.Test.Approvals.ApprovalRequest;

using Ums.Domain.Approvals.ApprovalRequest;
using Xunit;

public class ApprovalRequestTests
{
    private static readonly ApprovalWorkflowId ValidWorkflowId = ApprovalWorkflowId.Load(Guid.NewGuid().ToString());
    private static readonly UserId ValidUserId = UserId.Load(Guid.NewGuid().ToString());
    private static readonly ProfileId? ValidProfileId = ProfileId.Load(Guid.NewGuid().ToString());
    private static readonly ActorId ValidActor = ActorId.Create("user-001");
    #region Create

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = ApprovalRequest.Create(ValidWorkflowId, ValidUserId, ValidProfileId, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidWorkflowId, result.Value.WorkflowId);
        Assert.Equal(ValidUserId, result.Value.TargetUserId);
        Assert.Equal(ValidProfileId, result.Value.TargetProfileId);
        Assert.Equal(ApprovalStatus.Pending, result.Value.Status);
    }

    [Fact]
    public void Create_WithoutProfileId_ReturnsSuccess()
    {
        var result = ApprovalRequest.Create(ValidWorkflowId, ValidUserId, null, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.TargetProfileId);
    }

    #endregion

    #region Approve

    [Fact]
    public void Approve_WhenPending_ReturnsSuccess()
    {
        var request = ApprovalRequest.Create(ValidWorkflowId, ValidUserId, ValidProfileId, ValidActor).Value;

        var result = request.Approve(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(ApprovalStatus.Approved, request.Status);
    }

    [Fact]
    public void Approve_WhenNotPending_ReturnsFailure()
    {
        var request = ApprovalRequest.Create(ValidWorkflowId, ValidUserId, ValidProfileId, ValidActor).Value;
        request.Approve(ValidActor);

        var result = request.Approve(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Approvals.RequestNotPending, result.Error);
    }

    [Fact]
    public void Approve_AfterRejection_ReturnsFailure()
    {
        var request = ApprovalRequest.Create(ValidWorkflowId, ValidUserId, ValidProfileId, ValidActor).Value;
        request.Reject(ValidActor);

        var result = request.Approve(ValidActor);

        Assert.True(result.IsFailure);
    }

    #endregion

    #region Reject

    [Fact]
    public void Reject_WhenPending_ReturnsSuccess()
    {
        var request = ApprovalRequest.Create(ValidWorkflowId, ValidUserId, ValidProfileId, ValidActor).Value;

        var result = request.Reject(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(ApprovalStatus.Rejected, request.Status);
    }

    [Fact]
    public void Reject_WhenNotPending_ReturnsFailure()
    {
        var request = ApprovalRequest.Create(ValidWorkflowId, ValidUserId, ValidProfileId, ValidActor).Value;
        request.Reject(ValidActor);

        var result = request.Reject(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Approvals.RequestNotPending, result.Error);
    }

    [Fact]
    public void Reject_AfterApproval_ReturnsFailure()
    {
        var request = ApprovalRequest.Create(ValidWorkflowId, ValidUserId, ValidProfileId, ValidActor).Value;
        request.Approve(ValidActor);

        var result = request.Reject(ValidActor);

        Assert.True(result.IsFailure);
    }

    #endregion
}
