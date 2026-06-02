namespace Ums.Application.Test.Authorization.Profile;

using Ums.Application.Common.Interfaces;
using Ums.Application.Authorization.Profile.Commands;
using Ums.Domain.Authorization.Profile;
using Ums.Domain.Kernel;
using Ums.Domain.Authorization;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

public class ProfileCommandHandlerTests
{
    private readonly Mock<IProfileRepository> _repo = new();
    private readonly Mock<IUnitOfWork>         _uow  = new();
    private readonly Mock<IUserContext>        _ctx  = new();
    private readonly Mock<ITenantScopePolicy>  _scopePolicy = new();

    public ProfileCommandHandlerTests()
    {
        _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
        _uow.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _scopePolicy.Setup(s => s.EnsureManagementOwnerScopeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
    }

    private static Profile MakeProfile()
    {
        return Profile.Create(
            TenantId.Load(Guid.NewGuid()),
            UserId.Load(Guid.NewGuid()),
            RoleId.Load(Guid.NewGuid()),
            null,
            ActorId.Create("user-001")).Value;
    }

    // =========================================================================
    #region CreateProfileCommandHandler
    // =========================================================================

    [Fact]
    public async Task Create_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");

        var cmd = new CreateProfileCommand(
            TenantId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            RoleId: Guid.NewGuid(),
            BranchId: null);

        var handler = new CreateProfileCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.ProfileId);
        _repo.Verify(r => r.AddAsync(It.IsAny<Profile>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new CreateProfileCommand(
            TenantId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            RoleId: Guid.NewGuid(),
            BranchId: null);

        var handler = new CreateProfileCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_WhenTenantIsNotManagementOwner_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _scopePolicy.Setup(s => s.EnsureManagementOwnerScopeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("AUTH_015: Tenant is not marked as management owner."));

        var cmd = new CreateProfileCommand(
            TenantId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            RoleId: Guid.NewGuid(),
            BranchId: null);

        var handler = new CreateProfileCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("management owner", result.Error, StringComparison.OrdinalIgnoreCase);
        _repo.Verify(r => r.AddAsync(It.IsAny<Profile>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    // =========================================================================
    #region ActivateProfileCommandHandler
    // =========================================================================

    [Fact]
    public async Task Activate_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var profile = MakeProfile();
        // Born Active, let's Deactivate it first so we can Activate it again
        profile.Deactivate(ActorId.Create("user-001"));
        
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(profile);

        var cmd = new ActivateProfileCommand(Guid.NewGuid());
        var handler = new ActivateProfileCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(profile.IsActive);
        _repo.Verify(r => r.UpdateAsync(profile, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Activate_WhenNotFound_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Profile?)null);

        var cmd = new ActivateProfileCommand(Guid.NewGuid());
        var handler = new ActivateProfileCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Activate_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new ActivateProfileCommand(Guid.NewGuid());
        var handler = new ActivateProfileCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Activate_WhenAlreadyActive_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var profile = MakeProfile(); // already active

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(profile);

        var cmd = new ActivateProfileCommand(Guid.NewGuid());
        var handler = new ActivateProfileCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region DeactivateProfileCommandHandler
    // =========================================================================

    [Fact]
    public async Task Deactivate_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var profile = MakeProfile(); // active
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(profile);

        var cmd = new DeactivateProfileCommand(Guid.NewGuid());
        var handler = new DeactivateProfileCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(profile.IsActive);
        _repo.Verify(r => r.UpdateAsync(profile, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Deactivate_WhenNotFound_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Profile?)null);

        var cmd = new DeactivateProfileCommand(Guid.NewGuid());
        var handler = new DeactivateProfileCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Deactivate_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new DeactivateProfileCommand(Guid.NewGuid());
        var handler = new DeactivateProfileCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Deactivate_WhenAlreadyInactive_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var profile = MakeProfile();
        profile.Deactivate(ActorId.Create("user-001")); // inactive now

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(profile);

        var cmd = new DeactivateProfileCommand(Guid.NewGuid());
        var handler = new DeactivateProfileCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion
}
