namespace Ums.Application.Test.Approvals.ApprovalRequest;

using Ums.Application.Common.Interfaces;
using Ums.Application.Approvals.ApprovalRequest.Commands;
using Ums.Domain.Approvals;
using Ums.Domain.Approvals.ApprovalRequest;
using Xunit;

using ApprovalRequestAggregate = Ums.Domain.Approvals.ApprovalRequest.ApprovalRequest;

/// <summary>
/// Application-layer tests for ApprovalRequest command handlers.
///
/// Coverage intent:
///   – Auth guard: unauthenticated callers are rejected before any domain work.
///   – Not-found guard in Approve and Reject handlers.
///   – Domain failure (RequestNotPending) surfaced through Approve/Reject handlers
///     when called on a terminal-state request.
///   – Repository interactions (AddAsync / UpdateAsync / SaveEntities) called once on success.
///   – Null TargetProfileId path accepted by Create handler.
///
/// Excluded intentionally:
///   – Query handlers (read-only projections — no business logic).
///   – FluentValidation pipeline (tested via ValidationBehaviorTests).
///   – ApprovalWorkflow aggregate behaviour (separate handler tests).
/// </summary>
public class ApprovalRequestCommandHandlerTests
{
    // -------------------------------------------------------------------------
    // Shared mocks & helpers
    // -------------------------------------------------------------------------

    private readonly Mock<IApprovalRequestRepository> _repo = new();
    private readonly Mock<IUnitOfWork>                _uow  = new();
    private readonly Mock<IUserContext>               _ctx  = new();

    public ApprovalRequestCommandHandlerTests()
    {
        _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
        _uow.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }

    private static readonly Guid ValidWorkflowId    = Guid.NewGuid();
    private static readonly Guid ValidTargetUserId  = Guid.NewGuid();

    private static CreateApprovalRequestCommand ValidCreateCmd => new(
        WorkflowId: ValidWorkflowId,
        TargetUserId: ValidTargetUserId,
        TargetProfileId: Guid.NewGuid());

    private static ApprovalRequestAggregate MakePending() =>
        ApprovalRequestAggregate.Create(
            ApprovalWorkflowId.Load(ValidWorkflowId.ToString()),
            UserId.Load(ValidTargetUserId.ToString()),
            ProfileId.Load(Guid.NewGuid().ToString()),
            ActorId.Create("user-001")).Value;

    private static ApprovalRequestAggregate MakeApproved()
    {
        var r = MakePending();
        r.Approve(ActorId.Create("approver-001"));
        return r;
    }

    private static ApprovalRequestAggregate MakeRejected()
    {
        var r = MakePending();
        r.Reject(ActorId.Create("rejecter-001"));
        return r;
    }

    // =========================================================================
    #region CreateApprovalRequestCommandHandler
    // =========================================================================

    [Fact]
    public async Task Create_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");

        var handler = new CreateApprovalRequestCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(ValidCreateCmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.ApprovalRequestId);
    }

    [Fact]
    public async Task Create_SavesAggregateToRepository()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");

        var handler = new CreateApprovalRequestCommandHandler(_repo.Object, _ctx.Object);
        await handler.Handle(ValidCreateCmd, CancellationToken.None);

        _repo.Verify(r => r.AddAsync(It.IsAny<ApprovalRequestAggregate>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WithNullTargetProfileId_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var cmd = ValidCreateCmd with { TargetProfileId = null };

        var handler = new CreateApprovalRequestCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Create_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var handler = new CreateApprovalRequestCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(ValidCreateCmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Authenticated user is required", result.Error);
    }

    [Fact]
    public async Task Create_WhenUserIdNull_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns((string?)null);

        var handler = new CreateApprovalRequestCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(ValidCreateCmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region ApproveRequestCommandHandler
    // =========================================================================

    [Fact]
    public async Task Approve_WhenPending_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("approver-001");
        var request = MakePending();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(request);

        var handler = new ApproveRequestCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(new ApproveRequestCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.UpdateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Approve_WhenNotFound_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("approver-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((ApprovalRequestAggregate?)null);

        var handler = new ApproveRequestCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(new ApproveRequestCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task Approve_WhenAlreadyApproved_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("approver-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeApproved()); // already in terminal state

        var handler = new ApproveRequestCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(new ApproveRequestCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Approvals.RequestNotPending, result.Error);
    }

    [Fact]
    public async Task Approve_WhenRejected_ReturnsFailure()
    {
        // Rejected is a terminal state — approve is forbidden
        _ctx.Setup(u => u.UserId).Returns("approver-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeRejected());

        var handler = new ApproveRequestCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(new ApproveRequestCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Approvals.RequestNotPending, result.Error);
    }

    [Fact]
    public async Task Approve_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var handler = new ApproveRequestCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(new ApproveRequestCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Authenticated user is required", result.Error);
    }

    #endregion

    // =========================================================================
    #region RejectRequestCommandHandler
    // =========================================================================

    [Fact]
    public async Task Reject_WhenPending_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("rejecter-001");
        var request = MakePending();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(request);

        var handler = new RejectRequestCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(new RejectRequestCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.UpdateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reject_WhenNotFound_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("rejecter-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((ApprovalRequestAggregate?)null);

        var handler = new RejectRequestCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(new RejectRequestCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task Reject_WhenAlreadyRejected_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("rejecter-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeRejected());

        var handler = new RejectRequestCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(new RejectRequestCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Approvals.RequestNotPending, result.Error);
    }

    [Fact]
    public async Task Reject_WhenApproved_ReturnsFailure()
    {
        // Approved is a terminal state — reject is forbidden
        _ctx.Setup(u => u.UserId).Returns("rejecter-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeApproved());

        var handler = new RejectRequestCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(new RejectRequestCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Approvals.RequestNotPending, result.Error);
    }

    [Fact]
    public async Task Reject_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var handler = new RejectRequestCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(new RejectRequestCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Authenticated user is required", result.Error);
    }

    #endregion
}
