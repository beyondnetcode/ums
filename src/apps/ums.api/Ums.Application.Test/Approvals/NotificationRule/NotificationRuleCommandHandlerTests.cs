namespace Ums.Application.Test.Approvals.NotificationRule;

using Ums.Application.Common.Interfaces;
using Ums.Application.Approvals.NotificationRule.Commands;
using Ums.Domain.Approvals.NotificationRule;
using Ums.Domain.Kernel;
using Ums.Domain.Enums;
using Ums.Domain.Approvals;
using Ums.Domain;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

public class NotificationRuleCommandHandlerTests
{
    private readonly Mock<INotificationRuleRepository> _repo = new();
    private readonly Mock<IUnitOfWork>                 _uow  = new();
    private readonly Mock<IUserContext>                _ctx  = new();

    public NotificationRuleCommandHandlerTests()
    {
        _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
        _uow.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _ctx.Setup(u => u.UserId).Returns("user-001");
    }

    private static NotificationRule MakeNotificationRule()
    {
        return NotificationRule.Create(
            TenantId.Load(Guid.NewGuid()),
            NotificationChannel.Email,
            TextValueObject.Create("recipient@test.com"),
            ActorId.Create("user-001")).Value;
    }

    // =========================================================================
    #region CreateNotificationRuleCommandHandler
    // =========================================================================

    [Fact]
    public async Task Create_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");

        var cmd = new CreateNotificationRuleCommand(
            TenantId: Guid.NewGuid(),
            Channel: "Email",
            Recipient: "recipient@test.com");

        var handler = new CreateNotificationRuleCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.NotificationRuleId);
        _repo.Verify(r => r.AddAsync(It.IsAny<NotificationRule>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new CreateNotificationRuleCommand(
            TenantId: Guid.NewGuid(),
            Channel: "Email",
            Recipient: "recipient@test.com");

        var handler = new CreateNotificationRuleCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region DeactivateNotificationRuleCommandHandler
    // =========================================================================

    [Fact]
    public async Task Deactivate_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var rule = MakeNotificationRule(); // Born active
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(rule);

        var cmd = new DeactivateNotificationRuleCommand(Guid.NewGuid());
        var handler = new DeactivateNotificationRuleCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(rule.IsActive);
        _repo.Verify(r => r.UpdateAsync(rule, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Deactivate_WhenNotFound_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((NotificationRule?)null);

        var cmd = new DeactivateNotificationRuleCommand(Guid.NewGuid());
        var handler = new DeactivateNotificationRuleCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion
}
