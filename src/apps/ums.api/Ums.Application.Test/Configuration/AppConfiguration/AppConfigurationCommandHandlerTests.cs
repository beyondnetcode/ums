namespace Ums.Application.Test.Configuration.AppConfiguration;

using Ums.Application.Common.Interfaces;
using Ums.Application.Configuration.AppConfiguration.Commands;
using Ums.Domain.Configuration;
using Xunit;

using AppConfigurationAggregate = Ums.Domain.Configuration.AppConfiguration.AppConfiguration;

/// <summary>
/// Application-layer tests for AppConfiguration command handlers.
///
/// Coverage intent:
///   – Auth guard: unauthenticated callers are rejected before touching the domain.
///   – Duplicate scope+code guard in CreateAppConfigurationCommandHandler.
///   – Domain failure (illegal state transitions) surfaced through lifecycle handlers.
///   – Not-found guard in all handlers that load by Id.
///   – Repository interactions (AddAsync / UpdateAsync / SaveEntities) called exactly once on success.
///   – Global scope path (all nullable Ids omitted) validated through create handler.
///
/// Excluded intentionally:
///   – Query handlers (read-only projections — no business logic).
///   – FluentValidation pipeline (tested via ValidationBehaviorTests).
///   – Scope resolution itself (covered by domain tests in AppConfigurationTests).
/// </summary>
public class AppConfigurationCommandHandlerTests
{
    // -------------------------------------------------------------------------
    // Shared mocks & helpers
    // -------------------------------------------------------------------------

    private readonly Mock<IAppConfigurationRepository> _repo = new();
    private readonly Mock<IUnitOfWork>                 _uow  = new();
    private readonly Mock<IUserContext>                _ctx  = new();

    public AppConfigurationCommandHandlerTests()
    {
        _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
        _uow.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }

    private static CreateAppConfigurationCommand ValidCreateCmd => new(
        TenantId: Guid.NewGuid(),
        SystemSuiteId: Guid.NewGuid(),
        ModuleId: null,
        Code: "CFG-001",
        Value: "value123",
        Description: "Test configuration",
        IsInheritable: true,
        IsEncrypted: false);

    private static AppConfigurationAggregate MakeDraft() =>
        AppConfigurationAggregate.Create(
            TenantId.Load(Guid.NewGuid()),
            SystemSuiteId.Load(Guid.NewGuid()),
            null,
            Code.Create("CFG-001"),
            ConfigurationValue.Create("value123"),
            Description.Create("Test configuration"),
            true, false,
            ActorId.Create("user-001")).Value;

    private static AppConfigurationAggregate MakePublished()
    {
        var c = MakeDraft();
        c.Publish(ActorId.Create("user-001"));
        return c;
    }

    private static AppConfigurationAggregate MakeArchived()
    {
        var c = MakePublished();
        c.Archive(ActorId.Create("user-001"));
        return c;
    }

    // =========================================================================
    #region CreateAppConfigurationCommandHandler
    // =========================================================================

    [Fact]
    public async Task Create_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByScopeAndCodeAsync(
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((AppConfigurationAggregate?)null);

        var handler = new CreateAppConfigurationCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(ValidCreateCmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.AppConfigurationId);
    }

    [Fact]
    public async Task Create_SavesAggregateToRepository()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByScopeAndCodeAsync(
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((AppConfigurationAggregate?)null);

        var handler = new CreateAppConfigurationCommandHandler(_repo.Object, _ctx.Object);
        await handler.Handle(ValidCreateCmd, CancellationToken.None);

        _repo.Verify(r => r.AddAsync(It.IsAny<AppConfigurationAggregate>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WithGlobalScope_ReturnsSuccess()
    {
        // All scope IDs null → ConfigurationScope.Global
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByScopeAndCodeAsync(null, null, null, It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((AppConfigurationAggregate?)null);
        var cmd = ValidCreateCmd with { TenantId = null, SystemSuiteId = null, ModuleId = null };

        var handler = new CreateAppConfigurationCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Create_WhenCodeAlreadyExistsForScope_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByScopeAndCodeAsync(
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeDraft()); // duplicate found

        var handler = new CreateAppConfigurationCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(ValidCreateCmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("already exists", result.Error);
    }

    [Fact]
    public async Task Create_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var handler = new CreateAppConfigurationCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(ValidCreateCmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Authenticated user is required", result.Error);
    }

    [Fact]
    public async Task Create_WhenUserIdNull_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns((string?)null);

        var handler = new CreateAppConfigurationCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(ValidCreateCmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region PublishAppConfigurationCommandHandler
    // =========================================================================

    [Fact]
    public async Task Publish_WhenDraft_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var config = MakeDraft();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(config);

        var handler = new PublishAppConfigurationCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(new PublishAppConfigurationCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.UpdateAsync(config, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Publish_WhenNotFound_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((AppConfigurationAggregate?)null);

        var handler = new PublishAppConfigurationCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(new PublishAppConfigurationCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task Publish_WhenAlreadyPublished_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakePublished());

        var handler = new PublishAppConfigurationCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(new PublishAppConfigurationCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Configuration.AppConfigNotDraft, result.Error);
    }

    [Fact]
    public async Task Publish_WhenArchived_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeArchived());

        var handler = new PublishAppConfigurationCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(new PublishAppConfigurationCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Configuration.AppConfigNotDraft, result.Error);
    }

    [Fact]
    public async Task Publish_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var handler = new PublishAppConfigurationCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(new PublishAppConfigurationCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Authenticated user is required", result.Error);
    }

    #endregion

    // =========================================================================
    #region ArchiveAppConfigurationCommandHandler
    // =========================================================================

    [Fact]
    public async Task Archive_WhenPublished_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var config = MakePublished();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(config);

        var handler = new ArchiveAppConfigurationCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(new ArchiveAppConfigurationCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.UpdateAsync(config, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Archive_WhenNotFound_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((AppConfigurationAggregate?)null);

        var handler = new ArchiveAppConfigurationCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(new ArchiveAppConfigurationCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task Archive_WhenDraft_ReturnsFailure()
    {
        // Draft → Archive is forbidden; must go Draft → Published → Archived
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeDraft());

        var handler = new ArchiveAppConfigurationCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(new ArchiveAppConfigurationCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Configuration.AppConfigNotPublished, result.Error);
    }

    [Fact]
    public async Task Archive_WhenAlreadyArchived_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeArchived());

        var handler = new ArchiveAppConfigurationCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(new ArchiveAppConfigurationCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Configuration.AppConfigNotPublished, result.Error);
    }

    [Fact]
    public async Task Archive_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var handler = new ArchiveAppConfigurationCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(new ArchiveAppConfigurationCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Authenticated user is required", result.Error);
    }

    #endregion

    // =========================================================================
    #region UpdateAppConfigurationCommandHandler
    // =========================================================================

    [Fact]
    public async Task Update_WhenDraft_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var config = MakeDraft();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(config);

        var handler = new UpdateAppConfigurationCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(
            new UpdateAppConfigurationCommand(Guid.NewGuid(), "newvalue", "Updated description"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.UpdateAsync(config, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WhenNotFound_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((AppConfigurationAggregate?)null);

        var handler = new UpdateAppConfigurationCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(
            new UpdateAppConfigurationCommand(Guid.NewGuid(), "v", "d"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task Update_WhenPublished_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakePublished());

        var handler = new UpdateAppConfigurationCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(
            new UpdateAppConfigurationCommand(Guid.NewGuid(), "v", "d"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Configuration.AppConfigNotDraft, result.Error);
    }

    [Fact]
    public async Task Update_WhenArchived_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(MakeArchived());

        var handler = new UpdateAppConfigurationCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(
            new UpdateAppConfigurationCommand(Guid.NewGuid(), "v", "d"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Configuration.AppConfigNotDraft, result.Error);
    }

    [Fact]
    public async Task Update_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var handler = new UpdateAppConfigurationCommandHandler(_repo.Object, _ctx.Object);
        var result  = await handler.Handle(
            new UpdateAppConfigurationCommand(Guid.NewGuid(), "v", "d"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Authenticated user is required", result.Error);
    }

    #endregion
}
