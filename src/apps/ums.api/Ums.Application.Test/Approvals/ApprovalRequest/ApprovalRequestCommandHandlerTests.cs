namespace Ums.Application.Test.Approvals.ApprovalRequest;

using Ums.Application.Common.Interfaces;
using Ums.Application.Approvals.ApprovalRequest.Commands;
using Ums.Application.Approvals.ApprovalRequest.DTOs;
using Ums.Domain.Approvals.ApprovalRequest;
using Ums.Domain.Approvals;
using Ums.Domain.Enums;
using Ums.Domain.Kernel;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

public class ApprovalRequestCommandHandlerTests
{
    private readonly Mock<IApprovalRequestRepository> _repo = new();
    private readonly Mock<IUnitOfWork>               _uow  = new();
    private readonly Mock<IUserContext>              _ctx  = new();

    public ApprovalRequestCommandHandlerTests()
    {
        _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
        _uow.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _ctx.Setup(u => u.UserId).Returns("user-001");
    }

    private static ApprovalRequest MakeApprovalRequest()
    {
        return ApprovalRequest.Create(
            ApprovalWorkflowId.Load(Guid.NewGuid()),
            UserId.Load(Guid.NewGuid()),
            ProfileId.Load(Guid.NewGuid()),
            ActorId.Create("user-001")).Value;
    }

    // =========================================================================
    #region CreateApprovalRequestCommandHandler
    // =========================================================================

    [Fact]
    public async Task Create_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");

        var cmd = new CreateApprovalRequestCommand(
            WorkflowId: Guid.NewGuid(),
            TargetUserId: Guid.NewGuid(),
            TargetProfileId: Guid.NewGuid());

        var handler = new CreateApprovalRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.ApprovalRequestId);
        _repo.Verify(r => r.AddAsync(It.IsAny<ApprovalRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new CreateApprovalRequestCommand(
            WorkflowId: Guid.NewGuid(),
            TargetUserId: Guid.NewGuid(),
            TargetProfileId: Guid.NewGuid());

        var handler = new CreateApprovalRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    // =========================================================================
    #region ApproveRequestCommandHandler
    // =========================================================================

    [Fact]
    public async Task Approve_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var req = MakeApprovalRequest();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(req);

        var cmd = new ApproveRequestCommand(req.Props.Id.GetValue());
        var handler = new ApproveRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ApprovalStatus.Approved, req.Status);
        _repo.Verify(r => r.UpdateAsync(req, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Approve_WhenNotFound_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((ApprovalRequest?)null);

        var cmd = new ApproveRequestCommand(Guid.NewGuid());
        var handler = new ApproveRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("approval request not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Approve_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new ApproveRequestCommand(Guid.NewGuid());
        var handler = new ApproveRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Approve_WhenNotPending_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var req = MakeApprovalRequest();
        req.Approve(ActorId.Create("user-001")); // Set to approved

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(req);

        var cmd = new ApproveRequestCommand(req.Props.Id.GetValue());
        var handler = new ApproveRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region RejectRequestCommandHandler
    // =========================================================================

    [Fact]
    public async Task Reject_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var req = MakeApprovalRequest();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(req);

        var cmd = new RejectRequestCommand(req.Props.Id.GetValue());
        var handler = new RejectRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ApprovalStatus.Rejected, req.Status);
        _repo.Verify(r => r.UpdateAsync(req, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reject_WhenNotFound_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((ApprovalRequest?)null);

        var cmd = new RejectRequestCommand(Guid.NewGuid());
        var handler = new RejectRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("approval request not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Reject_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new RejectRequestCommand(Guid.NewGuid());
        var handler = new RejectRequestCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
