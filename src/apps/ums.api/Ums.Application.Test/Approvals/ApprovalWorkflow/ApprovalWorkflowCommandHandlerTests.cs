namespace Ums.Application.Test.Approvals.ApprovalWorkflow;

using Ums.Application.Common.Interfaces;
using Ums.Application.Approvals.ApprovalWorkflow.Commands;
using Ums.Domain.Approvals.ApprovalWorkflow;
using Ums.Domain.Kernel;
using Ums.Domain.Enums;
using Ums.Domain.Approvals;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

public class ApprovalWorkflowCommandHandlerTests
{
    private readonly Mock<IApprovalWorkflowRepository> _repo = new();
    private readonly Mock<IUnitOfWork>                 _uow  = new();
    private readonly Mock<IUserContext>                _ctx  = new();

    public ApprovalWorkflowCommandHandlerTests()
    {
        _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
        _uow.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _ctx.Setup(u => u.UserId).Returns("user-001");
    }

    // =========================================================================
    #region CreateApprovalWorkflowCommandHandler
    // =========================================================================

    [Fact]
    public async Task Create_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");

        var cmd = new CreateApprovalWorkflowCommand(
            TenantId: Guid.NewGuid(),
            Code: "WORKFLOW-001",
            Name: "Test Workflow",
            Description: "Test Workflow Description",
            TargetUserCategory: "Internal",
            RequiresApproval: true,
            SystemSuiteId: null);

        var handler = new CreateApprovalWorkflowCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.ApprovalWorkflowId);
        _repo.Verify(r => r.AddAsync(It.IsAny<ApprovalWorkflow>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new CreateApprovalWorkflowCommand(
            TenantId: Guid.NewGuid(),
            Code: "WORKFLOW-001",
            Name: "Test Workflow",
            Description: "Test Workflow Description",
            TargetUserCategory: "Internal",
            RequiresApproval: true,
            SystemSuiteId: null);

        var handler = new CreateApprovalWorkflowCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion
}
