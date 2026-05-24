namespace Ums.Application.Test.Approvals.NotificationRule;

using Ums.Application.Common.Interfaces;
using Ums.Application.Approvals.NotificationRule.Commands;
using Ums.Domain.Approvals.NotificationRule;
using Ums.Domain.Kernel;
using Ums.Domain.Enums;
using Ums.Domain.Approvals;
using Ums.Domain;
using Ums.Application.Approvals.NotificationRule.Services;
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
    private readonly Mock<INotificationRecipientResolver> _resolver = new();

    public NotificationRuleCommandHandlerTests()
    {
        _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
        _uow.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _resolver
            .Setup(r => r.Normalize(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string _, string recipient) => Result<string>.Success(recipient.Trim().ToLowerInvariant()));
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

        var handler = new CreateNotificationRuleCommandHandler(_repo.Object, _ctx.Object, _resolver.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.NotificationRuleId);
        _repo.Verify(r => r.AddAsync(It.IsAny<NotificationRule>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WithResolverNormalization_PersistsNormalizedRecipient()
    {
        NotificationRule? saved = null;
        _repo.Setup(r => r.AddAsync(It.IsAny<NotificationRule>(), It.IsAny<CancellationToken>()))
            .Callback<NotificationRule, CancellationToken>((rule, _) => saved = rule)
            .Returns(Task.CompletedTask);
        _resolver.Setup(r => r.Normalize("Email", "  RECIPIENT@Test.Com  "))
            .Returns(Result<string>.Success("recipient@test.com"));

        var cmd = new CreateNotificationRuleCommand(Guid.NewGuid(), "Email", "  RECIPIENT@Test.Com  ");

        var handler = new CreateNotificationRuleCommandHandler(_repo.Object, _ctx.Object, _resolver.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(saved);
        Assert.Equal("recipient@test.com", saved!.Props.Recipient.GetValue());
    }

    [Fact]
    public async Task Create_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new CreateNotificationRuleCommand(
            TenantId: Guid.NewGuid(),
            Channel: "Email",
            Recipient: "recipient@test.com");

        var handler = new CreateNotificationRuleCommandHandler(_repo.Object, _ctx.Object, _resolver.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Create_WhenResolverRejectsRecipient_ReturnsFailure()
    {
        _resolver.Setup(r => r.Normalize("Sms", "invalid"))
            .Returns(Result<string>.Failure("SMS notification recipient is invalid."));

        var cmd = new CreateNotificationRuleCommand(Guid.NewGuid(), "Sms", "invalid");

        var handler = new CreateNotificationRuleCommandHandler(_repo.Object, _ctx.Object, _resolver.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("SMS notification recipient is invalid.", result.Error);
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
