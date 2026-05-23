namespace Ums.Application.Test.Identity.UserAccount;

using Ums.Application.Common.Interfaces;
using Ums.Application.Identity.UserAccount.Commands;
using Ums.Domain.Identity;
using Ums.Domain.Identity.UserAccount;
using Ums.Domain.Kernel;
using Ums.Domain.Enums;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

public class UserAccountCommandHandlerTests
{
    private readonly Mock<IUserAccountRepository> _repo = new();
    private readonly Mock<IUnitOfWork>            _uow  = new();
    private readonly Mock<IUserContext>            _ctx  = new();

    public UserAccountCommandHandlerTests()
    {
        _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
        _uow.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _ctx.Setup(u => u.UserId).Returns("user-001");
    }

    private static UserAccount MakeUserAccount()
    {
        return UserAccount.Create(
            TenantId.Load(Guid.NewGuid()),
            Email.Create("test@test.com"),
            UserCategory.Internal,
            null,
            null,
            ActorId.Create("user-001"),
            null).Value;
    }

    // =========================================================================
    #region CreateUserAccountCommandHandler
    // =========================================================================

    [Fact]
    public async Task Create_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((UserAccount?)null);

        var cmd = new CreateUserAccountCommand(
            TenantId: Guid.NewGuid(),
            BranchId: null,
            Email: "newuser@test.com",
            Category: "Internal",
            IdentityReference: null,
            IdentityReferenceType: null);

        var handler = new CreateUserAccountCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.UserAccountId);
        _repo.Verify(r => r.AddAsync(It.IsAny<UserAccount>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WhenEmailAlreadyExists_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeUserAccount());

        var cmd = new CreateUserAccountCommand(
            TenantId: Guid.NewGuid(),
            BranchId: null,
            Email: "test@test.com",
            Category: "Internal",
            IdentityReference: null,
            IdentityReferenceType: null);

        var handler = new CreateUserAccountCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("email already exists", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new CreateUserAccountCommand(
            TenantId: Guid.NewGuid(),
            BranchId: null,
            Email: "newuser@test.com",
            Category: "Internal",
            IdentityReference: null,
            IdentityReferenceType: null);

        var handler = new CreateUserAccountCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    // =========================================================================
    #region ActivateUserAccountCommandHandler
    // =========================================================================

    [Fact]
    public async Task Activate_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var user = MakeUserAccount(); // born in Pending status
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new ActivateUserAccountCommand(Guid.NewGuid());
        var handler = new ActivateUserAccountCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(UserStatus.Active, user.Status);
        _repo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Activate_WhenNotFound_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((UserAccount?)null);

        var cmd = new ActivateUserAccountCommand(Guid.NewGuid());
        var handler = new ActivateUserAccountCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Activate_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new ActivateUserAccountCommand(Guid.NewGuid());
        var handler = new ActivateUserAccountCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Activate_WhenAlreadyActive_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var user = MakeUserAccount();
        user.Activate(ActorId.Create("user-001")); // Make Active

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new ActivateUserAccountCommand(Guid.NewGuid());
        var handler = new ActivateUserAccountCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region BlockUserAccountCommandHandler
    // =========================================================================

    [Fact]
    public async Task Block_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var user = MakeUserAccount();
        user.Activate(ActorId.Create("user-001")); // Needs to be Active or Pending
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new BlockUserAccountCommand(Guid.NewGuid(), "Security risk");
        var handler = new BlockUserAccountCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(UserStatus.Blocked, user.Status);
        _repo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Block_WhenNotFound_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((UserAccount?)null);

        var cmd = new BlockUserAccountCommand(Guid.NewGuid(), "Reason");
        var handler = new BlockUserAccountCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Block_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new BlockUserAccountCommand(Guid.NewGuid(), "Reason");
        var handler = new BlockUserAccountCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Block_WhenAlreadyBlocked_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var user = MakeUserAccount();
        user.Block(Reason.Create("Reason"), ActorId.Create("user-001")); // already blocked

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new BlockUserAccountCommand(Guid.NewGuid(), "Reason");
        var handler = new BlockUserAccountCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region RestoreUserAccountCommandHandler
    // =========================================================================

    [Fact]
    public async Task Restore_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var user = MakeUserAccount();
        user.Block(Reason.Create("Reason"), ActorId.Create("user-001")); // set to Blocked first
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new RestoreUserAccountCommand(Guid.NewGuid());
        var handler = new RestoreUserAccountCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(UserStatus.Active, user.Status);
        _repo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Restore_WhenNotFound_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((UserAccount?)null);

        var cmd = new RestoreUserAccountCommand(Guid.NewGuid());
        var handler = new RestoreUserAccountCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Restore_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new RestoreUserAccountCommand(Guid.NewGuid());
        var handler = new RestoreUserAccountCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Restore_WhenNotBlocked_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var user = MakeUserAccount(); // born Pending, not Blocked

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new RestoreUserAccountCommand(Guid.NewGuid());
        var handler = new RestoreUserAccountCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion
}
