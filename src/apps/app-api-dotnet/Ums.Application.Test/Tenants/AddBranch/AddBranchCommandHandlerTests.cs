namespace Ums.Application.Test.Tenants.AddBranch;

using Ums.Application.Tenants.AddBranch;
using Ums.Domain.Identity;
using Ums.Domain.Identity.Tenant;
using Ums.Application.Common.Interfaces;

public class AddBranchCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _tenantRepositoryMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly AddBranchCommandHandler _handler;

    public AddBranchCommandHandlerTests()
    {
        _tenantRepositoryMock = new Mock<ITenantRepository>();
        _userContextMock = new Mock<IUserContext>();
        _handler = new AddBranchCommandHandler(_tenantRepositoryMock.Object, _userContextMock.Object);
    }

    private static AddBranchCommand ValidCommand => new(
        TenantId: Guid.NewGuid(),
        Code: "BR-001",
        Name: "Branch One",
        GeofencingMetadata: null);

    #region Handle - Success Scenarios

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsSuccess()
    {
        var tenantId = Guid.NewGuid();
        _userContextMock.Setup(u => u.UserId).Returns("user-001");
        var tenant = CreateTenant();
        _tenantRepositoryMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        var command = ValidCommand with { TenantId = tenantId };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(tenantId, result.Value.TenantId);
    }

    [Fact]
    public async Task Handle_WithGeofencingMetadata_ReturnsSuccess()
    {
        var tenantId = Guid.NewGuid();
        _userContextMock.Setup(u => u.UserId).Returns("user-001");
        var tenant = CreateTenant();
        _tenantRepositoryMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        var command = ValidCommand with { TenantId = tenantId, GeofencingMetadata = "{\"lat\": 40.7128, \"lon\": -74.0060}" };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WithValidCommand_UpdatesTenant()
    {
        var tenantId = Guid.NewGuid();
        _userContextMock.Setup(u => u.UserId).Returns("user-001");
        var tenant = CreateTenant();
        _tenantRepositoryMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        var command = ValidCommand with { TenantId = tenantId };

        await _handler.Handle(command, CancellationToken.None);

        _tenantRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CallsSaveEntities()
    {
        var tenantId = Guid.NewGuid();
        _userContextMock.Setup(u => u.UserId).Returns("user-001");
        var tenant = CreateTenant();
        _tenantRepositoryMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _tenantRepositoryMock.Setup(r => r.UnitOfWork).Returns(unitOfWorkMock.Object);
        var command = ValidCommand with { TenantId = tenantId };

        await _handler.Handle(command, CancellationToken.None);

        unitOfWorkMock.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Handle - Failure Scenarios

    [Fact]
    public async Task Handle_WhenUserIdIsEmpty_ReturnsFailure()
    {
        var tenantId = Guid.NewGuid();
        _userContextMock.Setup(u => u.UserId).Returns("");
        var command = ValidCommand with { TenantId = tenantId };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Authenticated user is required", result.Error);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ReturnsFailure()
    {
        var tenantId = Guid.NewGuid();
        _userContextMock.Setup(u => u.UserId).Returns((string?)null);
        var command = ValidCommand with { TenantId = tenantId };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Authenticated user is required", result.Error);
    }

    [Fact]
    public async Task Handle_WhenTenantNotFound_ReturnsFailure()
    {
        var tenantId = Guid.NewGuid();
        _userContextMock.Setup(u => u.UserId).Returns("user-001");
        _tenantRepositoryMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);
        var command = ValidCommand with { TenantId = tenantId };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Tenant was not found", result.Error);
    }

    [Fact]
    public async Task Handle_WhenBranchCodeAlreadyExists_ReturnsFailure()
    {
        var tenantId = Guid.NewGuid();
        _userContextMock.Setup(u => u.UserId).Returns("user-001");
        var tenant = CreateTenant();
        tenant.AddBranch(Code.Create("BR-001"), Name.Create("Existing Branch"), ActorId.Create("user-001"), null);
        _tenantRepositoryMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        var command = ValidCommand with { TenantId = tenantId, Code = "BR-001" };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Handle_WhenEmptyBranchCode_ReturnsFailure()
    {
        var tenantId = Guid.NewGuid();
        _userContextMock.Setup(u => u.UserId).Returns("user-001");
        var tenant = CreateTenant();
        _tenantRepositoryMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        var command = ValidCommand with { TenantId = tenantId, Code = "" };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion

    private static Tenant CreateTenant()
    {
        return Tenant.Create(
            Code.Create("TEST-001"),
            Name.Create("Test Tenant"),
            OrganizationType.INTERNAL,
            ActorId.Create("user-001"),
            IdpStrategy.InternalBcrypt,
            null,
            null).Value;
    }
}
