namespace Ums.Application.Test.IGA.PromotionRequest;

using Ums.Application.Common.Interfaces;
using Ums.Application.IGA.PromotionRequest.Commands;
using Ums.Domain.IGA;
using Ums.Domain.IGA.PromotionRequest;
using Ums.Domain.Kernel;
using Ums.Domain;
using Ums.Domain.Enums;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

public class PromotionRequestCommandHandlerTests
{
    private readonly Mock<IPromotionRequestRepository> _repo = new();
    private readonly Mock<IUnitOfWork>                 _uow  = new();
    private readonly Mock<IUserContext>                _ctx  = new();

    public PromotionRequestCommandHandlerTests()
    {
        _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
        _uow.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _ctx.Setup(u => u.UserId).Returns("user-001");
    }

    private static PromotionRequest MakePromotionRequest()
    {
        return PromotionRequest.Create(
            TenantId.Load(Guid.NewGuid()),
            UserId.Load(Guid.NewGuid()),
            RoleId.Load(Guid.NewGuid()),
            RoleId.Load(Guid.NewGuid()),
            UserId.Load(Guid.NewGuid()),
            TextValueObject.Create("Reason"),
            ActorId.Create("user-001")).Value;
    }

    // =========================================================================
    #region CreatePromotionRequestCommandHandler
    // =========================================================================

    [Fact]
    public async Task Create_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");

        var cmd = new CreatePromotionRequestCommand(
            TenantId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            CurrentRoleId: Guid.NewGuid(),
            TargetRoleId: Guid.NewGuid(),
            ManagerId: Guid.NewGuid(),
            RequestReason: "Career advancement");

        var handler = new CreatePromotionRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.PromotionRequestId);
        _repo.Verify(r => r.AddAsync(It.IsAny<PromotionRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new CreatePromotionRequestCommand(
            TenantId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            CurrentRoleId: Guid.NewGuid(),
            TargetRoleId: Guid.NewGuid(),
            ManagerId: Guid.NewGuid(),
            RequestReason: "Career advancement");

        var handler = new CreatePromotionRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region SubmitPromotionRequestCommandHandler
    // =========================================================================

    [Fact]
    public async Task Submit_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var request = MakePromotionRequest(); // Born in Draft status
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(request);

        var cmd = new SubmitPromotionRequestCommand(Guid.NewGuid());
        var handler = new SubmitPromotionRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PromotionStatus.PendingManagerApproval, request.Status);
        _repo.Verify(r => r.UpdateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Submit_WhenNotFound_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((PromotionRequest?)null);

        var cmd = new SubmitPromotionRequestCommand(Guid.NewGuid());
        var handler = new SubmitPromotionRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Submit_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new SubmitPromotionRequestCommand(Guid.NewGuid());
        var handler = new SubmitPromotionRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion

    // helpers to advance domain state
    private static PromotionRequest InPendingManagerApproval()
    {
        var r = MakePromotionRequest();
        r.Submit(ActorId.Create("user-001"));
        return r;
    }

    private static PromotionRequest InPendingSecurityReview()
    {
        var r = InPendingManagerApproval();
        r.ManagerApprove(ActorId.Create("manager-001"));
        return r;
    }

    private static PromotionRequest InApprovedReadyToExecute()
    {
        var r = InPendingSecurityReview();
        r.SecurityReviewLowRisk(ActorId.Create("security-001"));
        return r;
    }

    private static PromotionRequest InPendingSecurityApproval()
    {
        var r = InPendingSecurityReview();
        r.SecurityReviewHighRisk(ActorId.Create("security-001"));
        return r;
    }

    private static PromotionRequest InExecuted()
    {
        var r = InApprovedReadyToExecute();
        r.Execute(ActorId.Create("executor-001"));
        return r;
    }

    // =========================================================================
    #region ManagerApprovePromotionRequestCommandHandler
    // =========================================================================

    [Fact]
    public async Task ManagerApprove_WithValidRequest_ReturnsSuccess()
    {
        var request = InPendingManagerApproval();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(request);

        var handler = new ManagerApprovePromotionRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new ManagerApprovePromotionRequestCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PromotionStatus.PendingSecurityReview, request.Status);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ManagerApprove_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((PromotionRequest?)null);
        var handler = new ManagerApprovePromotionRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new ManagerApprovePromotionRequestCommand(Guid.NewGuid()), CancellationToken.None);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task ManagerApprove_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");
        var handler = new ManagerApprovePromotionRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new ManagerApprovePromotionRequestCommand(Guid.NewGuid()), CancellationToken.None);
        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region ManagerRejectPromotionRequestCommandHandler
    // =========================================================================

    [Fact]
    public async Task ManagerReject_WithValidRequest_ReturnsSuccess()
    {
        var request = InPendingManagerApproval();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(request);

        var handler = new ManagerRejectPromotionRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new ManagerRejectPromotionRequestCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PromotionStatus.Rejected, request.Status);
    }

    [Fact]
    public async Task ManagerReject_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((PromotionRequest?)null);
        var handler = new ManagerRejectPromotionRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new ManagerRejectPromotionRequestCommand(Guid.NewGuid()), CancellationToken.None);
        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region SecurityReviewPromotionRequestCommandHandler
    // =========================================================================

    [Fact]
    public async Task SecurityReview_LowRisk_MovesToApprovedReadyToExecute()
    {
        var request = InPendingSecurityReview();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(request);

        var handler = new SecurityReviewPromotionRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new SecurityReviewPromotionRequestCommand(Guid.NewGuid(), false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PromotionStatus.ApprovedReadyToExecute, request.Status);
    }

    [Fact]
    public async Task SecurityReview_HighRisk_MovesToPendingSecurityApproval()
    {
        var request = InPendingSecurityReview();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(request);

        var handler = new SecurityReviewPromotionRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new SecurityReviewPromotionRequestCommand(Guid.NewGuid(), true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PromotionStatus.PendingSecurityApproval, request.Status);
    }

    [Fact]
    public async Task SecurityReview_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((PromotionRequest?)null);
        var handler = new SecurityReviewPromotionRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new SecurityReviewPromotionRequestCommand(Guid.NewGuid(), false), CancellationToken.None);
        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region SecurityApprovePromotionRequestCommandHandler
    // =========================================================================

    [Fact]
    public async Task SecurityApprove_WithValidRequest_MovesToApprovedReadyToExecute()
    {
        var request = InPendingSecurityApproval();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(request);

        var handler = new SecurityApprovePromotionRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new SecurityApprovePromotionRequestCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PromotionStatus.ApprovedReadyToExecute, request.Status);
    }

    [Fact]
    public async Task SecurityApprove_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((PromotionRequest?)null);
        var handler = new SecurityApprovePromotionRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new SecurityApprovePromotionRequestCommand(Guid.NewGuid()), CancellationToken.None);
        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region SecurityRejectPromotionRequestCommandHandler
    // =========================================================================

    [Fact]
    public async Task SecurityReject_WithValidRequest_MovesToRejectedBySecurity()
    {
        var request = InPendingSecurityApproval();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(request);

        var handler = new SecurityRejectPromotionRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new SecurityRejectPromotionRequestCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PromotionStatus.Rejected, request.Status);
    }

    [Fact]
    public async Task SecurityReject_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((PromotionRequest?)null);
        var handler = new SecurityRejectPromotionRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new SecurityRejectPromotionRequestCommand(Guid.NewGuid()), CancellationToken.None);
        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region ExecutePromotionRequestCommandHandler
    // =========================================================================

    [Fact]
    public async Task Execute_WithApprovedRequest_MovesToExecuted()
    {
        var request = InApprovedReadyToExecute();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(request);

        var handler = new ExecutePromotionRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new ExecutePromotionRequestCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PromotionStatus.Executed, request.Status);
    }

    [Fact]
    public async Task Execute_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((PromotionRequest?)null);
        var handler = new ExecutePromotionRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new ExecutePromotionRequestCommand(Guid.NewGuid()), CancellationToken.None);
        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region VerifyPromotionRequestCommandHandler
    // =========================================================================

    [Fact]
    public async Task Verify_WithExecutedRequest_MovesToVerified()
    {
        var request = InExecuted();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(request);

        var handler = new VerifyPromotionRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new VerifyPromotionRequestCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PromotionStatus.Verified, request.Status);
    }

    [Fact]
    public async Task Verify_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((PromotionRequest?)null);
        var handler = new VerifyPromotionRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new VerifyPromotionRequestCommand(Guid.NewGuid()), CancellationToken.None);
        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region MarkVerificationFailedCommandHandler
    // =========================================================================

    [Fact]
    public async Task MarkVerificationFailed_WithExecutedRequest_MovesToVerificationFailed()
    {
        var request = InExecuted();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(request);

        var handler = new MarkVerificationFailedCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new MarkVerificationFailedCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PromotionStatus.VerificationFailed, request.Status);
    }

    [Fact]
    public async Task MarkVerificationFailed_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((PromotionRequest?)null);
        var handler = new MarkVerificationFailedCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new MarkVerificationFailedCommand(Guid.NewGuid()), CancellationToken.None);
        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region AddImpactAnalysisCommandHandler
    // =========================================================================

    [Fact]
    public async Task AddImpactAnalysis_WithValidCommand_ReturnsSuccess()
    {
        var request = MakePromotionRequest();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(request);

        var cmd = new AddImpactAnalysisCommand(
            PromotionRequestId: Guid.NewGuid(),
            RiskScore: 72m,
            RiskLevel: "High",
            NewPermissionsCount: 5,
            RemovedPermissionsCount: 2,
            AffectedSystemsCount: 3,
            ConflictingPermissions: null,
            RiskFactors: "External access",
            SuggestedMitigations: "Enable MFA");

        var handler = new AddImpactAnalysisCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddImpactAnalysis_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((PromotionRequest?)null);

        var cmd = new AddImpactAnalysisCommand(Guid.NewGuid(), 50m, "Medium", 2, 1, 1, null, null, null);
        var handler = new AddImpactAnalysisCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task AddImpactAnalysis_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");
        var cmd = new AddImpactAnalysisCommand(Guid.NewGuid(), 50m, "Medium", 2, 1, 1, null, null, null);
        var handler = new AddImpactAnalysisCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.True(result.IsFailure);
    }

    #endregion
}
