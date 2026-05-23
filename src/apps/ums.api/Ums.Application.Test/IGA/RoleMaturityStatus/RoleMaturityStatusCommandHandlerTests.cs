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
}
