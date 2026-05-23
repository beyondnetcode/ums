namespace Ums.Application.Test.Approvals.AccessEnforcementPolicy;

using Ums.Application.Common.Interfaces;
using Ums.Application.Approvals.AccessEnforcementPolicy.Commands;
using Ums.Application.Approvals.AccessEnforcementPolicy.DTOs;
using Ums.Domain.Approvals.AccessEnforcementPolicy;
using Ums.Domain.Approvals;
using Ums.Domain.Enums;
using Ums.Domain.Kernel;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

public class AccessEnforcementPolicyCommandHandlerTests
{
    private readonly Mock<IAccessEnforcementPolicyRepository> _repo = new();
    private readonly Mock<IUnitOfWork>                       _uow  = new();
    private readonly Mock<IUserContext>                      _ctx  = new();

    public AccessEnforcementPolicyCommandHandlerTests()
    {
        _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
        _uow.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _ctx.Setup(u => u.UserId).Returns("user-001");
    }

    private static AccessEnforcementPolicy MakePolicy()
    {
        return AccessEnforcementPolicy.Create(
            TenantId.Load(Guid.NewGuid()),
            ProfileId.Load(Guid.NewGuid()),
            RoleId.Load(Guid.NewGuid()),
            AccessEnforcementAction.BlockUser,
            ActorId.Create("user-001")).Value;
    }

    // =========================================================================
    #region CreateAccessEnforcementPolicyCommandHandler
    // =========================================================================

    [Fact]
    public async Task Create_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");

        var cmd = new CreateAccessEnforcementPolicyCommand(
            TenantId: Guid.NewGuid(),
            ProfileId: Guid.NewGuid(),
            RoleId: Guid.NewGuid(),
            EnforcementAction: "BlockUser");

        var handler = new CreateAccessEnforcementPolicyCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.AccessEnforcementPolicyId);
        _repo.Verify(r => r.AddAsync(It.IsAny<AccessEnforcementPolicy>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new CreateAccessEnforcementPolicyCommand(
            TenantId: Guid.NewGuid(),
            ProfileId: Guid.NewGuid(),
            RoleId: Guid.NewGuid(),
            EnforcementAction: "BlockUser");

        var handler = new CreateAccessEnforcementPolicyCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_WhenNoProfileOrRoleProvided_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");

        var cmd = new CreateAccessEnforcementPolicyCommand(
            TenantId: Guid.NewGuid(),
            ProfileId: null,
            RoleId: null,
            EnforcementAction: "BlockUser");

        var handler = new CreateAccessEnforcementPolicyCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region DeactivateAccessEnforcementPolicyCommandHandler
    // =========================================================================

    [Fact]
    public async Task Deactivate_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var policy = MakePolicy();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(policy);

        var cmd = new DeactivateAccessEnforcementPolicyCommand(policy.Props.Id.GetValue());
        var handler = new DeactivateAccessEnforcementPolicyCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(policy.IsActive);
        _repo.Verify(r => r.UpdateAsync(policy, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Deactivate_WhenNotFound_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((AccessEnforcementPolicy?)null);

        var cmd = new DeactivateAccessEnforcementPolicyCommand(Guid.NewGuid());
        var handler = new DeactivateAccessEnforcementPolicyCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("policy not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Deactivate_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new DeactivateAccessEnforcementPolicyCommand(Guid.NewGuid());
        var handler = new DeactivateAccessEnforcementPolicyCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Deactivate_WhenAlreadyInactive_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var policy = MakePolicy();
        policy.Deactivate(ActorId.Create("user-001"));

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(policy);

        var cmd = new DeactivateAccessEnforcementPolicyCommand(policy.Props.Id.GetValue());
        var handler = new DeactivateAccessEnforcementPolicyCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion
}
