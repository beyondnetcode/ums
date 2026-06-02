namespace Ums.Application.Test.Tenants.Queries;

using Ums.Application.Identity.Tenant.Queries;
using Ums.Application.Identity.Tenant.DTOs;
using Ums.Domain.Identity;
using Ums.Domain.Identity.Tenant;
using Ums.Application.Common.Interfaces;
using Ums.Domain.Kernel;

public class TenantQueryHandlerTests
{
    private readonly Mock<ITenantRepository> _repo = new();

    private static Tenant MakeTenant(string code = "TEN-001", string name = "Test Tenant")
    {
        return Tenant.Create(
            Code.Create(code),
            Name.Create(name),
            OrganizationType.INTERNAL,
            ActorId.Create("user-001"),
            IdpStrategy.InternalBcrypt,
            null,
            null).Value;
    }

    // =========================================================================
    #region GetTenantByIdQueryHandler
    // =========================================================================

    [Fact]
    public async Task GetById_WhenFound_ReturnsSuccess()
    {
        var tenant = MakeTenant();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(tenant);

        var query = new GetTenantByIdQuery(tenant.Props.Id.GetValue());
        var handler = new GetTenantByIdQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("TEN-001", result.Value.Code);
        Assert.Equal("Test Tenant", result.Value.Name);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Tenant?)null);

        var query = new GetTenantByIdQuery(Guid.NewGuid());
        var handler = new GetTenantByIdQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetById_WithExternalTenant_ReturnsCorrectType()
    {
        var tenant = Tenant.Create(
            Code.Create("EXT-001"),
            Name.Create("External Tenant"),
            OrganizationType.CLIENT,
            ActorId.Create("user-001"),
            IdpStrategy.AzureAd,
            CompanyReference.Create("COMP-123"),
            null).Value;

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(tenant);

        var query = new GetTenantByIdQuery(tenant.Props.Id.GetValue());
        var handler = new GetTenantByIdQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("CLIENT", result.Value.Type);
        Assert.Equal("COMP-123", result.Value.CompanyReference);
    }

    #endregion

    // =========================================================================
    #region GetAllTenantsQueryHandler
    // =========================================================================

    [Fact]
    public async Task GetAll_WithoutFilters_ReturnsAll()
    {
        var tenants = new List<Tenant>
        {
            MakeTenant("TEN-001", "Tenant One"),
            MakeTenant("TEN-002", "Tenant Two")
        };

        _repo.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(((IReadOnlyList<Tenant>)tenants, tenants.Count));

        var query = new GetAllTenantsQuery(Page: 1, PageSize: 10);
        var handler = new GetAllTenantsQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalItems);
        Assert.Equal(2, result.Value.Items.Count);
    }

    [Fact]
    public async Task GetAll_WithPagination_ReturnsCorrectPage()
    {
        var tenants = new List<Tenant> { MakeTenant("TEN-001", "Tenant One") };

        _repo.Setup(r => r.GetPagedAsync(
                2, 5, It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(((IReadOnlyList<Tenant>)tenants, 1));

        var query = new GetAllTenantsQuery(Page: 2, PageSize: 5);
        var handler = new GetAllTenantsQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Page);
        Assert.Equal(5, result.Value.PageSize);
    }

    [Fact]
    public async Task GetAll_WithStatusFilter_PassesStatusToRepo()
    {
        var tenants = new List<Tenant> { MakeTenant("TEN-001", "Active Tenant") };

        _repo.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
                "Active", It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(((IReadOnlyList<Tenant>)tenants, tenants.Count));

        var query = new GetAllTenantsQuery(Status: "Active");
        var handler = new GetAllTenantsQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal("Active", result.Value.Items[0].Status);
    }

    [Fact]
    public async Task GetAll_WithSearch_PassesSearchToRepo()
    {
        var tenants = new List<Tenant> { MakeTenant("TEN-001", "Target Tenant") };

        _repo.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(), "target",
                It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(((IReadOnlyList<Tenant>)tenants, tenants.Count));

        var query = new GetAllTenantsQuery(Search: "target");
        var handler = new GetAllTenantsQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
    }

    [Fact]
    public async Task GetAll_WithEmptyResult_ReturnsZeroTotalPages()
    {
        var tenants = new List<Tenant>();

        _repo.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(((IReadOnlyList<Tenant>)tenants, 0));

        var query = new GetAllTenantsQuery(Page: 1, PageSize: 10);
        var handler = new GetAllTenantsQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.TotalItems);
        Assert.Equal(0, result.Value.TotalPages);
    }

    [Fact]
    public async Task GetAll_WithSorting_PassesSortToRepo()
    {
        var tenants = new List<Tenant> { MakeTenant("TEN-001", "Tenant A") };

        _repo.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
                It.IsAny<string?>(), "code", "desc",
                It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(((IReadOnlyList<Tenant>)tenants, tenants.Count));

        var query = new GetAllTenantsQuery(SortBy: "code", SortOrder: "desc");
        var handler = new GetAllTenantsQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    #endregion
}
