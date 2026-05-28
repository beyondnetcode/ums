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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class UserAccountPasswordMfaCommandHandlerTests
{
    private readonly Mock<IUserAccountRepository> _repo = new();
    private readonly Mock<IUnitOfWork>            _uow  = new();
    private readonly Mock<IUserContext>            _ctx  = new();
    private readonly Mock<IPasswordHashingService> _passwordHashing = new();

    public UserAccountPasswordMfaCommandHandlerTests()
    {
        _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
        _uow.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _passwordHashing.Setup(s => s.Hash(It.IsAny<string>())).Returns("$2a$12$server-generated-hash");
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

    [Fact]
    public async Task ActivatePassword_WithValidCommand_ReturnsSuccess()
    {
        var user = MakeUserAccount();
        user.AddPassword(PasswordHash.Create("$2a$12$hash"), ActorId.Create("user-001"));
        var credential = user.PasswordCredentials.Single();

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new ActivatePasswordCommand(user.GetId().GetValue(), credential.Id.GetValue());
        var handler = new ActivatePasswordCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActivatePassword_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((UserAccount?)null);

        var cmd = new ActivatePasswordCommand(Guid.NewGuid(), Guid.NewGuid());
        var handler = new ActivatePasswordCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ActivatePassword_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new ActivatePasswordCommand(Guid.NewGuid(), Guid.NewGuid());
        var handler = new ActivatePasswordCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ActivatePassword_WhenCredentialNotFound_ReturnsFailure()
    {
        var user = MakeUserAccount();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new ActivatePasswordCommand(user.GetId().GetValue(), Guid.NewGuid());
        var handler = new ActivatePasswordCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task ActivateUserAccountPassword_WithValidCommand_ReturnsSuccess()
    {
        var user = MakeUserAccount();
        user.AddPassword(PasswordHash.Create("$2a$12$hash"), ActorId.Create("user-001"));
        var credential = user.PasswordCredentials.Single();

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new ActivateUserAccountPasswordCommand(user.GetId().GetValue(), credential.Id.GetValue());
        var handler = new ActivateUserAccountPasswordCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActivateUserAccountPassword_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((UserAccount?)null);

        var cmd = new ActivateUserAccountPasswordCommand(Guid.NewGuid(), Guid.NewGuid());
        var handler = new ActivateUserAccountPasswordCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ActivateUserAccountPassword_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new ActivateUserAccountPasswordCommand(Guid.NewGuid(), Guid.NewGuid());
        var handler = new ActivateUserAccountPasswordCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ActivateUserAccountPassword_WhenCredentialNotFound_ReturnsFailure()
    {
        var user = MakeUserAccount();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new ActivateUserAccountPasswordCommand(user.GetId().GetValue(), Guid.NewGuid());
        var handler = new ActivateUserAccountPasswordCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task AddPassword_WithValidCommand_ReturnsSuccess()
    {
        var user = MakeUserAccount();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new AddPasswordCommand(user.GetId().GetValue(), "$2a$12$prehashed");
        var handler = new AddPasswordCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.CredentialId);
        _repo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddPassword_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((UserAccount?)null);

        var cmd = new AddPasswordCommand(Guid.NewGuid(), "$2a$12$hash");
        var handler = new AddPasswordCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AddPassword_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new AddPasswordCommand(Guid.NewGuid(), "$2a$12$hash");
        var handler = new AddPasswordCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AddPassword_WhenBlocked_ReturnsFailure()
    {
        var user = MakeUserAccount();
        user.Block(Reason.Create("Security risk"), ActorId.Create("user-001"));
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new AddPasswordCommand(user.GetId().GetValue(), "$2a$12$hash");
        var handler = new AddPasswordCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task RemovePassword_WithValidCommand_ReturnsSuccess()
    {
        var user = MakeUserAccount();
        user.AddPassword(PasswordHash.Create("$2a$12$hash1"), ActorId.Create("user-001"));
        user.AddPassword(PasswordHash.Create("$2a$12$hash2"), ActorId.Create("user-001"));
        var credential1 = user.PasswordCredentials.First();

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new RemovePasswordCommand(user.GetId().GetValue(), credential1.Id.GetValue());
        var handler = new RemovePasswordCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemovePassword_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((UserAccount?)null);

        var cmd = new RemovePasswordCommand(Guid.NewGuid(), Guid.NewGuid());
        var handler = new RemovePasswordCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RemovePassword_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new RemovePasswordCommand(Guid.NewGuid(), Guid.NewGuid());
        var handler = new RemovePasswordCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RemovePassword_WhenCredentialNotFound_ReturnsFailure()
    {
        var user = MakeUserAccount();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new RemovePasswordCommand(user.GetId().GetValue(), Guid.NewGuid());
        var handler = new RemovePasswordCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task RemovePassword_WhenLastPassword_ReturnsFailure()
    {
        var user = MakeUserAccount();
        user.AddPassword(PasswordHash.Create("$2a$12$hash"), ActorId.Create("user-001"));
        var credential = user.PasswordCredentials.Single();

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new RemovePasswordCommand(user.GetId().GetValue(), credential.Id.GetValue());
        var handler = new RemovePasswordCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task RemoveUserAccountPassword_WithValidCommand_ReturnsSuccess()
    {
        var user = MakeUserAccount();
        user.AddPassword(PasswordHash.Create("$2a$12$hash1"), ActorId.Create("user-001"));
        user.AddPassword(PasswordHash.Create("$2a$12$hash2"), ActorId.Create("user-001"));
        var credential1 = user.PasswordCredentials.First();

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new RemoveUserAccountPasswordCommand(user.GetId().GetValue(), credential1.Id.GetValue());
        var handler = new RemoveUserAccountPasswordCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveUserAccountPassword_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((UserAccount?)null);

        var cmd = new RemoveUserAccountPasswordCommand(Guid.NewGuid(), Guid.NewGuid());
        var handler = new RemoveUserAccountPasswordCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RemoveUserAccountPassword_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new RemoveUserAccountPasswordCommand(Guid.NewGuid(), Guid.NewGuid());
        var handler = new RemoveUserAccountPasswordCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RemoveUserAccountPassword_WhenCredentialNotFound_ReturnsFailure()
    {
        var user = MakeUserAccount();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new RemoveUserAccountPasswordCommand(user.GetId().GetValue(), Guid.NewGuid());
        var handler = new RemoveUserAccountPasswordCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task RemoveUserAccountPassword_WhenLastPassword_ReturnsFailure()
    {
        var user = MakeUserAccount();
        user.AddPassword(PasswordHash.Create("$2a$12$hash"), ActorId.Create("user-001"));
        var credential = user.PasswordCredentials.Single();

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new RemoveUserAccountPasswordCommand(user.GetId().GetValue(), credential.Id.GetValue());
        var handler = new RemoveUserAccountPasswordCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task EnrollMfa_WithValidCommand_ReturnsSuccess()
    {
        var user = MakeUserAccount();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new EnrollUserAccountMfaCommand(user.GetId().GetValue(), "Totp");
        var handler = new EnrollUserAccountMfaCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.EnrollmentId);
        Assert.Single(user.MfaEnrollments);
        _repo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnrollMfa_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((UserAccount?)null);

        var cmd = new EnrollUserAccountMfaCommand(Guid.NewGuid(), "Totp");
        var handler = new EnrollUserAccountMfaCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EnrollMfa_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new EnrollUserAccountMfaCommand(Guid.NewGuid(), "Totp");
        var handler = new EnrollUserAccountMfaCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EnrollMfa_WhenInvalidMethod_ReturnsFailure()
    {
        var user = MakeUserAccount();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new EnrollUserAccountMfaCommand(user.GetId().GetValue(), "InvalidMethod");
        var handler = new EnrollUserAccountMfaCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("invalid", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EnrollMfa_WhenAlreadyEnrolled_ReturnsFailure()
    {
        var user = MakeUserAccount();
        user.EnrollMfa(MfaMethod.Totp, ActorId.Create("user-001"));

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new EnrollUserAccountMfaCommand(user.GetId().GetValue(), "Totp");
        var handler = new EnrollUserAccountMfaCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task EnrollMfa_WhenBlocked_ReturnsFailure()
    {
        var user = MakeUserAccount();
        user.Block(Reason.Create("Security risk"), ActorId.Create("user-001"));
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new EnrollUserAccountMfaCommand(user.GetId().GetValue(), "Totp");
        var handler = new EnrollUserAccountMfaCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task VerifyMfa_WithValidCommand_ReturnsSuccess()
    {
        var user = MakeUserAccount();
        user.EnrollMfa(MfaMethod.Totp, ActorId.Create("user-001"));
        var enrollment = user.MfaEnrollments.Single();

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new VerifyUserAccountMfaCommand(user.GetId().GetValue(), enrollment.Id.GetValue());
        var handler = new VerifyUserAccountMfaCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyMfa_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((UserAccount?)null);

        var cmd = new VerifyUserAccountMfaCommand(Guid.NewGuid(), Guid.NewGuid());
        var handler = new VerifyUserAccountMfaCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task VerifyMfa_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new VerifyUserAccountMfaCommand(Guid.NewGuid(), Guid.NewGuid());
        var handler = new VerifyUserAccountMfaCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task VerifyMfa_WhenEnrollmentNotFound_ReturnsFailure()
    {
        var user = MakeUserAccount();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new VerifyUserAccountMfaCommand(user.GetId().GetValue(), Guid.NewGuid());
        var handler = new VerifyUserAccountMfaCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task RecordAuthenticationAttempt_WithValidCommand_ReturnsSuccess()
    {
        var user = MakeUserAccount();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new RecordAuthenticationAttemptCommand(
            user.GetId().GetValue(),
            true,
            "Password valid",
            "192.168.1.1");
        var handler = new RecordAuthenticationAttemptCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordAuthenticationAttempt_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((UserAccount?)null);

        var cmd = new RecordAuthenticationAttemptCommand(Guid.NewGuid(), true, "Reason", "127.0.0.1");
        var handler = new RecordAuthenticationAttemptCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RecordAuthenticationAttempt_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new RecordAuthenticationAttemptCommand(Guid.NewGuid(), true, "Reason", "127.0.0.1");
        var handler = new RecordAuthenticationAttemptCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RecordAuthenticationAttempt_WithFailedAttempt_ReturnsSuccess()
    {
        var user = MakeUserAccount();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var cmd = new RecordAuthenticationAttemptCommand(
            user.GetId().GetValue(),
            false,
            "Invalid password",
            "10.0.0.1");
        var handler = new RecordAuthenticationAttemptCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
