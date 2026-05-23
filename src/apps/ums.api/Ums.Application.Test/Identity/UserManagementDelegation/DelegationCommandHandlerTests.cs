namespace Ums.Application.Test.Identity.UserManagementDelegation;

using Ums.Application.Common.Interfaces;
using Ums.Application.Identity.UserManagementDelegation.Commands;
using Ums.Domain.Identity;
using Ums.Domain.Identity.UserManagementDelegation;
using Ums.Domain.Kernel;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class DelegationCommandHandlerTests
{
    private readonly Mock<IUserManagementDelegationRepository> _repo = new();
    private readonly Mock<IUnitOfWork>                           _uow  = new();
    private readonly Mock<IUserContext>                          _ctx  = new();

    public DelegationCommandHandlerTests()
    {
        _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
        _uow.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _ctx.Setup(u => u.UserId).Returns("user-001");
    }

    private static UserManagementDelegation MakeDelegation()
    {
        return UserManagementDelegation.Create(
            TenantId.Load(Guid.NewGuid()),
            UserAccountId.Load(Guid.NewGuid()),
            UserAccountId.Load(Guid.NewGuid()),
            DelegationScopeType.Tenant,
            null,
            new List<DelegatedAction> { DelegatedAction.CreateUser },
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(5),
            10,
            false,
            ActorId.Create("user-001")).Value;
    }

    // =========================================================================
    #region CreateDelegationCommandHandler
    // =========================================================================

    [Fact]
    public async Task Create_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");

        var cmd = new CreateDelegationCommand(
            TenantId: Guid.NewGuid(),
            DelegatingAdminId: Guid.NewGuid(),
            DelegatedAdminId: Guid.NewGuid(),
            ScopeType: "Tenant",
            ScopeId: null,
            AllowedActions: new[] { "CreateUser" },
            ValidFrom: DateTimeOffset.UtcNow,
            ValidUntil: DateTimeOffset.UtcNow.AddDays(5),
            MaxDurationDays: 10,
            RequiresApproval: false);

        var handler = new CreateDelegationCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.DelegationId);
        _repo.Verify(r => r.AddAsync(It.IsAny<UserManagementDelegation>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WhenInvalidScopeType_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");

        var cmd = new CreateDelegationCommand(
            TenantId: Guid.NewGuid(),
            DelegatingAdminId: Guid.NewGuid(),
            DelegatedAdminId: Guid.NewGuid(),
            ScopeType: "InvalidScope",
            ScopeId: null,
            AllowedActions: new[] { "CreateUser" },
            ValidFrom: DateTimeOffset.UtcNow,
            ValidUntil: DateTimeOffset.UtcNow.AddDays(5),
            MaxDurationDays: 10,
            RequiresApproval: false);

        var handler = new CreateDelegationCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("invalid scope type", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_WhenEmptyAllowedActions_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");

        var cmd = new CreateDelegationCommand(
            TenantId: Guid.NewGuid(),
            DelegatingAdminId: Guid.NewGuid(),
            DelegatedAdminId: Guid.NewGuid(),
            ScopeType: "Tenant",
            ScopeId: null,
            AllowedActions: new string[] { },
            ValidFrom: DateTimeOffset.UtcNow,
            ValidUntil: DateTimeOffset.UtcNow.AddDays(5),
            MaxDurationDays: 10,
            RequiresApproval: false);

        var handler = new CreateDelegationCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("at least one valid allowed action is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new CreateDelegationCommand(
            TenantId: Guid.NewGuid(),
            DelegatingAdminId: Guid.NewGuid(),
            DelegatedAdminId: Guid.NewGuid(),
            ScopeType: "Tenant",
            ScopeId: null,
            AllowedActions: new[] { "CreateUser" },
            ValidFrom: DateTimeOffset.UtcNow,
            ValidUntil: DateTimeOffset.UtcNow.AddDays(5),
            MaxDurationDays: 10,
            RequiresApproval: false);

        var handler = new CreateDelegationCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    // =========================================================================
    #region ActivateDelegationCommandHandler
    // =========================================================================

    [Fact]
    public async Task Activate_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var delegation = MakeDelegation(); // Born Draft / Pending status
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(delegation);

        var cmd = new ActivateDelegationCommand(Guid.NewGuid());
        var handler = new ActivateDelegationCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(DelegationStatus.Active, delegation.Status);
        _repo.Verify(r => r.UpdateAsync(delegation, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Activate_WhenNotFound_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((UserManagementDelegation?)null);

        var cmd = new ActivateDelegationCommand(Guid.NewGuid());
        var handler = new ActivateDelegationCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    // =========================================================================
    #region RevokeDelegationCommandHandler
    // =========================================================================

    [Fact]
    public async Task Revoke_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var delegation = MakeDelegation();
        delegation.Activate(ActorId.Create("user-001")); // set Active

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(delegation);

        var cmd = new RevokeDelegationCommand(Guid.NewGuid(), "Admin action");
        var handler = new RevokeDelegationCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(DelegationStatus.Revoked, delegation.Status);
        _repo.Verify(r => r.UpdateAsync(delegation, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Revoke_WhenNotFound_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((UserManagementDelegation?)null);

        var cmd = new RevokeDelegationCommand(Guid.NewGuid(), "Reason");
        var handler = new RevokeDelegationCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region ExpireDelegationCommandHandler
    // =========================================================================

    [Fact]
    public async Task Expire_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var delegation = MakeDelegation();
        delegation.Activate(ActorId.Create("user-001")); // set Active

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(delegation);

        var cmd = new ExpireDelegationCommand(Guid.NewGuid());
        var handler = new ExpireDelegationCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(DelegationStatus.Expired, delegation.Status);
        _repo.Verify(r => r.UpdateAsync(delegation, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Expire_WhenNotFound_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((UserManagementDelegation?)null);

        var cmd = new ExpireDelegationCommand(Guid.NewGuid());
        var handler = new ExpireDelegationCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion
}
