namespace Ums.Application.Test.Tenants.Branch;

using Ums.Application.Identity.Tenant.Branch.Commands;
using Ums.Domain.Identity;
using Ums.Domain.Identity.Tenant;
using Ums.Application.Common.Interfaces;
using Ums.Domain.Kernel;

public class BranchCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IUserContext> _ctx = new();
    private readonly Mock<ITenantScopePolicy> _scopePolicy = new();

    public BranchCommandHandlerTests()
    {
        _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
        _uow.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _scopePolicy.Setup(s => s.EnsureManagementOwnerScopeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
    }

    private static Tenant MakeTenant()
    {
        return Tenant.Create(
            Code.Create("TEN-001"),
            Name.Create("Test Tenant"),
            OrganizationType.INTERNAL,
            ActorId.Create("user-001"),
            IdpStrategy.InternalBcrypt,
            null,
            null).Value;
    }

    // =========================================================================
    #region DeactivateBranchCommandHandler
    // =========================================================================

    [Fact]
    public async Task DeactivateBranch_WithValidCommand_ReturnsSuccess()
    {
        var tenant = MakeTenant();
        var branchResult = tenant.AddBranch(Code.Create("BR-001"), Name.Create("Branch One"), ActorId.Create("user-001"), null);
        var branchId = tenant.Branches.First().GetId().GetValue();

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(tenant);

        var cmd = new DeactivateBranchCommand(tenant.Props.Id.GetValue(), branchId);
        var handler = new DeactivateBranchCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        Assert.False(tenant.Branches.First().IsActive);
        _repo.Verify(r => r.UpdateAsync(tenant, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateBranch_WhenTenantNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Tenant?)null);

        var cmd = new DeactivateBranchCommand(Guid.NewGuid(), Guid.NewGuid());
        var handler = new DeactivateBranchCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Tenant was not found", result.Error);
    }

    [Fact]
    public async Task DeactivateBranch_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new DeactivateBranchCommand(Guid.NewGuid(), Guid.NewGuid());
        var handler = new DeactivateBranchCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Authenticated user is required", result.Error);
    }

    [Fact]
    public async Task DeactivateBranch_WhenBranchNotFound_ReturnsFailure()
    {
        var tenant = MakeTenant();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(tenant);

        var cmd = new DeactivateBranchCommand(tenant.Props.Id.GetValue(), Guid.NewGuid());
        var handler = new DeactivateBranchCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region ReactivateBranchCommandHandler
    // =========================================================================

    [Fact]
    public async Task ReactivateBranch_WithValidCommand_ReturnsSuccess()
    {
        var tenant = MakeTenant();
        tenant.AddBranch(Code.Create("BR-001"), Name.Create("Branch One"), ActorId.Create("user-001"), null);
        var branch = tenant.Branches.First();
        tenant.DeactivateBranch(branch.GetId(), ActorId.Create("user-001"));

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(tenant);

        var cmd = new ReactivateBranchCommand(tenant.Props.Id.GetValue(), branch.GetId().GetValue());
        var handler = new ReactivateBranchCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        Assert.True(branch.IsActive);
        _repo.Verify(r => r.UpdateAsync(tenant, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReactivateBranch_WhenTenantNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Tenant?)null);

        var cmd = new ReactivateBranchCommand(Guid.NewGuid(), Guid.NewGuid());
        var handler = new ReactivateBranchCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Tenant was not found", result.Error);
    }

    [Fact]
    public async Task ReactivateBranch_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new ReactivateBranchCommand(Guid.NewGuid(), Guid.NewGuid());
        var handler = new ReactivateBranchCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Authenticated user is required", result.Error);
    }

    [Fact]
    public async Task ReactivateBranch_WhenBranchNotFound_ReturnsFailure()
    {
        var tenant = MakeTenant();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(tenant);

        var cmd = new ReactivateBranchCommand(tenant.Props.Id.GetValue(), Guid.NewGuid());
        var handler = new ReactivateBranchCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region RemoveBranchCommandHandler
    // =========================================================================

    [Fact]
    public async Task RemoveBranch_WithValidCommand_ReturnsSuccess()
    {
        var tenant = MakeTenant();
        tenant.AddBranch(Code.Create("BR-001"), Name.Create("Branch One"), ActorId.Create("user-001"), null);
        var branch = tenant.Branches.First();
        tenant.DeactivateBranch(branch.GetId(), ActorId.Create("user-001"));

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(tenant);

        var cmd = new RemoveBranchCommand(tenant.Props.Id.GetValue(), branch.GetId().GetValue());
        var handler = new RemoveBranchCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Empty(tenant.Branches);
        _repo.Verify(r => r.UpdateAsync(tenant, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveBranch_WhenTenantNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Tenant?)null);

        var cmd = new RemoveBranchCommand(Guid.NewGuid(), Guid.NewGuid());
        var handler = new RemoveBranchCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Tenant was not found", result.Error);
    }

    [Fact]
    public async Task RemoveBranch_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new RemoveBranchCommand(Guid.NewGuid(), Guid.NewGuid());
        var handler = new RemoveBranchCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Authenticated user is required", result.Error);
    }

    [Fact]
    public async Task RemoveBranch_WhenBranchNotFound_ReturnsFailure()
    {
        var tenant = MakeTenant();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(tenant);

        var cmd = new RemoveBranchCommand(tenant.Props.Id.GetValue(), Guid.NewGuid());
        var handler = new RemoveBranchCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion
}
