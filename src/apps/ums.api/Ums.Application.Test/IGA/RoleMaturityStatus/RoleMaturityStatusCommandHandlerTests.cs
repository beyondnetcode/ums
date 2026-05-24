namespace Ums.Application.Test.IGA.RoleMaturityStatus;

using Ums.Application.Common.Interfaces;
using Ums.Application.IGA.RoleMaturityStatus.Commands;
using Ums.Domain.IGA;
using Ums.Domain.IGA.RoleMaturityStatus;
using Ums.Domain.Kernel;
using Ums.Domain.Enums;
using Ums.Domain;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

public class RoleMaturityStatusCommandHandlerTests
{
    private readonly Mock<IRoleMaturityStatusRepository> _repo = new();
    private readonly Mock<IUnitOfWork>                   _uow  = new();
    private readonly Mock<IUserContext>                  _ctx  = new();

    public RoleMaturityStatusCommandHandlerTests()
    {
        _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
        _uow.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _ctx.Setup(u => u.UserId).Returns("user-001");
    }

    private static RoleMaturityStatus MakeRoleMaturityStatus()
    {
        return RoleMaturityStatus.Create(
            TenantId.Load(Guid.NewGuid()),
            UserId.Load(Guid.NewGuid()),
            RoleId.Load(Guid.NewGuid()),
            RoleMaturityLevel.Junior,
            ActorId.Create("user-001")).Value;
    }

    // =========================================================================
    #region CreateRoleMaturityStatusCommandHandler
    // =========================================================================

    [Fact]
    public async Task Create_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");

        var cmd = new CreateRoleMaturityStatusCommand(
            TenantId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            RoleId: Guid.NewGuid(),
            CurrentMaturityLevel: "Junior");

        var handler = new CreateRoleMaturityStatusCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.RoleMaturityStatusId);
        _repo.Verify(r => r.AddAsync(It.IsAny<RoleMaturityStatus>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new CreateRoleMaturityStatusCommand(
            TenantId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            RoleId: Guid.NewGuid(),
            CurrentMaturityLevel: "Junior");

        var handler = new CreateRoleMaturityStatusCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region UpdateRoleMaturityLevelCommandHandler
    // =========================================================================

    [Fact]
    public async Task Update_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var status = MakeRoleMaturityStatus(); // Junior level

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(status);

        var cmd = new UpdateRoleMaturityLevelCommand(Guid.NewGuid(), "Intermediate");
        var handler = new UpdateRoleMaturityLevelCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(RoleMaturityLevel.Intermediate, status.CurrentMaturityLevel);
        _repo.Verify(r => r.UpdateAsync(status, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WhenNotFound_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((RoleMaturityStatus?)null);

        var cmd = new UpdateRoleMaturityLevelCommand(Guid.NewGuid(), "Intermediate");
        var handler = new UpdateRoleMaturityLevelCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Update_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new UpdateRoleMaturityLevelCommand(Guid.NewGuid(), "Intermediate");
        var handler = new UpdateRoleMaturityLevelCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Update_WhenLevelUnchanged_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var status = MakeRoleMaturityStatus(); // Junior level

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(status);

        var cmd = new UpdateRoleMaturityLevelCommand(Guid.NewGuid(), "Junior"); // same level
        var handler = new UpdateRoleMaturityLevelCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region RecordCertificationCompletionCommandHandler
    // =========================================================================

    [Fact]
    public async Task RecordCertification_WithValidCommand_IncrementsCount()
    {
        var status = MakeRoleMaturityStatus();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(status);

        var handler = new RecordCertificationCompletionCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new RecordCertificationCompletionCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, status.CompletedCertificationsCount);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordCertification_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((RoleMaturityStatus?)null);
        var handler = new RecordCertificationCompletionCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new RecordCertificationCompletionCommand(Guid.NewGuid()), CancellationToken.None);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task RecordCertification_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");
        var handler = new RecordCertificationCompletionCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new RecordCertificationCompletionCommand(Guid.NewGuid()), CancellationToken.None);
        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region RecordTrainingCompletionCommandHandler
    // =========================================================================

    [Fact]
    public async Task RecordTraining_WithValidCommand_IncrementsCount()
    {
        var status = MakeRoleMaturityStatus();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(status);

        var handler = new RecordTrainingCompletionCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new RecordTrainingCompletionCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, status.CompletedTrainingsCount);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordTraining_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((RoleMaturityStatus?)null);
        var handler = new RecordTrainingCompletionCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new RecordTrainingCompletionCommand(Guid.NewGuid()), CancellationToken.None);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task RecordTraining_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");
        var handler = new RecordTrainingCompletionCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new RecordTrainingCompletionCommand(Guid.NewGuid()), CancellationToken.None);
        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region UpdatePerformanceScoreCommandHandler
    // =========================================================================

    [Fact]
    public async Task UpdateScore_WithValidScore_ReturnsSuccess()
    {
        var status = MakeRoleMaturityStatus();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(status);

        var handler = new UpdatePerformanceScoreCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new UpdatePerformanceScoreCommand(Guid.NewGuid(), 4.2m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(4.2m, status.PerformanceScore);
    }

    [Fact]
    public async Task UpdateScore_WithOutOfRangeScore_ReturnsFailure()
    {
        var status = MakeRoleMaturityStatus();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(status);

        var handler = new UpdatePerformanceScoreCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new UpdatePerformanceScoreCommand(Guid.NewGuid(), 6m), CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task UpdateScore_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((RoleMaturityStatus?)null);
        var handler = new UpdatePerformanceScoreCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new UpdatePerformanceScoreCommand(Guid.NewGuid(), 3m), CancellationToken.None);
        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region MarkComplianceIssueCommandHandler & ResolveComplianceIssueCommandHandler
    // =========================================================================

    [Fact]
    public async Task MarkCompliance_WithReason_BlocksEligibility()
    {
        var status = MakeRoleMaturityStatus();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(status);

        var handler = new MarkComplianceIssueCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new MarkComplianceIssueCommand(Guid.NewGuid(), "Policy violation"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(status.HasNoComplianceIssues);
        Assert.NotNull(status.BlockingFactor);
    }

    [Fact]
    public async Task ResolveCompliance_AfterIssue_ClearsBlockingFactor()
    {
        var status = MakeRoleMaturityStatus();
        status.MarkComplianceIssue(TextValueObject.Create("Issue"), ActorId.Create("user-001"));
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(status);

        var handler = new ResolveComplianceIssueCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new ResolveComplianceIssueCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(status.HasNoComplianceIssues);
        Assert.Null(status.BlockingFactor);
    }

    [Fact]
    public async Task MarkCompliance_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((RoleMaturityStatus?)null);
        var handler = new MarkComplianceIssueCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new MarkComplianceIssueCommand(Guid.NewGuid(), "Issue"), CancellationToken.None);
        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region ReviewEligibilityCommandHandler
    // =========================================================================

    [Fact]
    public async Task ReviewEligibility_WithValidStatus_SetsLastReviewedAt()
    {
        var status = MakeRoleMaturityStatus();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(status);

        var handler = new ReviewEligibilityCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new ReviewEligibilityCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(status.LastReviewedAt);
    }

    [Fact]
    public async Task ReviewEligibility_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((RoleMaturityStatus?)null);
        var handler = new ReviewEligibilityCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(new ReviewEligibilityCommand(Guid.NewGuid()), CancellationToken.None);
        Assert.True(result.IsFailure);
    }

    #endregion
}
