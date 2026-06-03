namespace Ums.Application.Test.Authorization.Template;

using Ums.Application.Common.Interfaces;
using Ums.Application.Authorization.Template.Commands;
using Ums.Application.Authorization.Template.DTOs;
using Ums.Domain.Authorization.Template;
using Ums.Domain.Kernel;
using Ums.Domain.Authorization;
using Ums.Domain.Enums;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

public class TemplateCommandHandlerTests
{
    private readonly Mock<IPermissionTemplateRepository> _repo = new();
    private readonly Mock<IUnitOfWork>                 _uow  = new();
    private readonly Mock<IUserContext>                _ctx  = new();
    private readonly Mock<ITenantScopePolicy>          _scopePolicy = new();

    public TemplateCommandHandlerTests()
    {
        _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
        _uow.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _scopePolicy.Setup(s => s.EnsureManagementOwnerScopeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
    }

    private static PermissionTemplate MakeTemplate()
    {
        var template = PermissionTemplate.Create(
            TenantId.Load(Guid.NewGuid()),
            RoleId.Load(Guid.NewGuid()),
            SystemSuiteId.Load(Guid.NewGuid()),
            ActorId.Create("user-001")).Value;

        var itemResult = template.AddItem(
            ExclusiveArcTarget.SystemSuite,
            IdValueObject.Create(),
            ActionId.Load(Guid.NewGuid()),
            isAllowed: true,
            isDenied: false,
            ActorId.Create("user-001"));

        if (itemResult.IsFailure)
        {
            throw new InvalidOperationException(itemResult.Error);
        }

        return template;
    }

    // =========================================================================
    #region CreatePermissionTemplateCommandHandler
    // =========================================================================

    [Fact]
    public async Task Create_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");

        var cmd = new CreatePermissionTemplateCommand(
            TenantId: Guid.NewGuid(),
            RoleId: Guid.NewGuid(),
            SystemSuiteId: Guid.NewGuid());

        var handler = new CreatePermissionTemplateCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.TemplateId);
        _repo.Verify(r => r.AddAsync(It.IsAny<PermissionTemplate>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new CreatePermissionTemplateCommand(
            TenantId: Guid.NewGuid(),
            RoleId: Guid.NewGuid(),
            SystemSuiteId: Guid.NewGuid());

        var handler = new CreatePermissionTemplateCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
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

        var cmd = new CreatePermissionTemplateCommand(
            TenantId: Guid.NewGuid(),
            RoleId: Guid.NewGuid(),
            SystemSuiteId: Guid.NewGuid());

        var handler = new CreatePermissionTemplateCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("management owner", result.Error, StringComparison.OrdinalIgnoreCase);
        _repo.Verify(r => r.AddAsync(It.IsAny<PermissionTemplate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    // =========================================================================
    #region PublishPermissionTemplateCommandHandler
    // =========================================================================

    [Fact]
    public async Task Publish_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var template = MakeTemplate();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(template);

        var cmd = new PublishPermissionTemplateCommand(template.Props.Id.GetValue());
        var handler = new PublishPermissionTemplateCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(TemplateStatus.Published, template.Status);
        _repo.Verify(r => r.UpdateAsync(template, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Publish_WhenNotFound_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((PermissionTemplate?)null);

        var cmd = new PublishPermissionTemplateCommand(Guid.NewGuid());
        var handler = new PublishPermissionTemplateCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("template was not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Publish_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new PublishPermissionTemplateCommand(Guid.NewGuid());
        var handler = new PublishPermissionTemplateCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Publish_WhenAlreadyPublished_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var template = MakeTemplate();
        // Publish it first
        template.Publish(ActorId.Create("user-001"));

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(template);

        var cmd = new PublishPermissionTemplateCommand(template.Props.Id.GetValue());
        var handler = new PublishPermissionTemplateCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion
}
