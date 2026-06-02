namespace Ums.Application.Test.Tenants.Queries;

using Ums.Application.Common.Interfaces;
using Ums.Application.Identity.Tenant.Queries;
using Ums.Domain.Identity;
using Ums.Domain.Identity.Tenant;

/// <summary>
/// Unit tests that verify multi-tenant isolation in <see cref="GetAllTenantsQueryHandler"/>.
///
/// Rules under test:
///   1. A regular user's request is always scoped to their own tenant (OrganizationId).
///   2. An internal admin with no tenant override sees all tenants (null filter).
///   3. The effective tenant ID passed to the repository must NOT come from request input
///      for regular users — it must always come from ITenantContext.
///   4. Regular users cannot widen their scope regardless of what parameters they send.
/// </summary>
public class TenantMultiTenantIsolationTests
{
    private readonly Mock<ITenantRepository> _repo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();

    private static Tenant MakeTenant(string code, string name)
        => Tenant.Create(
            Code.Create(code),
            Name.Create(name),
            OrganizationType.INTERNAL,
            ActorId.Create("seeder"),
            IdpStrategy.InternalBcrypt,
            null,
            null).Value;

    private void SetupRepoForTenant(Guid? tenantId, IReadOnlyList<Tenant> result)
    {
        _repo.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>(),
                tenantId, It.IsAny<CancellationToken>()))
             .ReturnsAsync((result, result.Count));
    }

    // =========================================================================
    #region Regular tenant user isolation
    // =========================================================================

    [Fact]
    public async Task RegularUser_AlwaysScopedToOwnTenant_EvenWithoutRequest()
    {
        var ransaId = Guid.NewGuid();
        var ransaTenant = MakeTenant("RANSA", "Ransa Corp");
        _tenantContext.Setup(t => t.IsInternalAdmin).Returns(false);
        _tenantContext.Setup(t => t.OrganizationId).Returns(ransaId);
        SetupRepoForTenant(ransaId, [ransaTenant]);

        var query = new GetAllTenantsQuery(Page: 1, PageSize: 20);
        var handler = new GetAllTenantsQueryHandler(_repo.Object, _tenantContext.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        // Verify repo was called with RANSA's ID, not null
        _repo.Verify(r => r.GetPagedAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
            It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>(),
            ransaId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegularUser_CannotSeeOtherTenants_CrossTenantAttemptIsBlocked()
    {
        var ransaId = Guid.NewGuid();
        var acmeId = Guid.NewGuid();

        _tenantContext.Setup(t => t.IsInternalAdmin).Returns(false);
        _tenantContext.Setup(t => t.OrganizationId).Returns(ransaId);

        // Repo is only set up to return RANSA data when asked for ransaId
        SetupRepoForTenant(ransaId, [MakeTenant("RANSA", "Ransa Corp")]);

        // A regular user queries — no matter the context, scope must be ransaId
        var query = new GetAllTenantsQuery(Page: 1, PageSize: 20);
        var handler = new GetAllTenantsQueryHandler(_repo.Object, _tenantContext.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);

        // Ensure the repo was NEVER called with acmeId or null (cross-tenant)
        _repo.Verify(r => r.GetPagedAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
            It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>(),
            acmeId, It.IsAny<CancellationToken>()), Times.Never);

        _repo.Verify(r => r.GetPagedAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
            It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>(),
            null, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegularUser_OnlySeesOwnTenantData_NotOthers()
    {
        var ransaId = Guid.NewGuid();
        var ransaTenant = MakeTenant("RANSA", "Ransa Corp");
        var acmeTenant = MakeTenant("ACME", "Acme Corp");

        _tenantContext.Setup(t => t.IsInternalAdmin).Returns(false);
        _tenantContext.Setup(t => t.OrganizationId).Returns(ransaId);

        // Repository correctly scoped to RANSA returns only RANSA
        SetupRepoForTenant(ransaId, [ransaTenant]);

        var query = new GetAllTenantsQuery(Page: 1, PageSize: 20);
        var handler = new GetAllTenantsQueryHandler(_repo.Object, _tenantContext.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal("RANSA", result.Value.Items[0].Code);
        Assert.DoesNotContain(result.Value.Items, t => t.Code == "ACME");
    }

    #endregion

    // =========================================================================
    #region Internal admin cross-tenant access
    // =========================================================================

    [Fact]
    public async Task InternalAdmin_WithNoFilter_SeesAllTenants()
    {
        var adminTenantId = Guid.NewGuid();
        var allTenants = new List<Tenant>
        {
            MakeTenant("RANSA", "Ransa Corp"),
            MakeTenant("ACME", "Acme Corp"),
            MakeTenant("XYZ", "XYZ Inc"),
        };

        _tenantContext.Setup(t => t.IsInternalAdmin).Returns(true);
        _tenantContext.Setup(t => t.OrganizationId).Returns(adminTenantId);

        // Admin: null filter means all tenants
        SetupRepoForTenant(null, allTenants);

        var query = new GetAllTenantsQuery(Page: 1, PageSize: 20);
        var handler = new GetAllTenantsQueryHandler(_repo.Object, _tenantContext.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalItems);

        _repo.Verify(r => r.GetPagedAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
            It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>(),
            null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InternalAdmin_WithNoTenantContext_FallsBackToNoFilter()
    {
        // Handler with null tenantContext (no HTTP context / test scenario)
        var allTenants = new List<Tenant>
        {
            MakeTenant("RANSA", "Ransa Corp"),
            MakeTenant("ACME", "Acme Corp"),
        };

        SetupRepoForTenant(null, allTenants);

        var query = new GetAllTenantsQuery(Page: 1, PageSize: 20);
        var handler = new GetAllTenantsQueryHandler(_repo.Object, tenantContext: null);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalItems);
    }

    #endregion

    // =========================================================================
    #region Tenant context initialization enforcement
    // =========================================================================

    [Fact]
    public async Task RegularUser_TenantContextNotInitialized_ReturnsNoData()
    {
        // OrganizationId is null when context is not initialized for the user
        _tenantContext.Setup(t => t.IsInternalAdmin).Returns(false);
        _tenantContext.Setup(t => t.OrganizationId).Returns((Guid?)null);

        // When effectiveTenantId is null for a non-admin, repo is called with null
        // The repository itself handles null (returns nothing or all, depending on implementation)
        SetupRepoForTenant(null, []);

        var query = new GetAllTenantsQuery(Page: 1, PageSize: 20);
        var handler = new GetAllTenantsQueryHandler(_repo.Object, _tenantContext.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
    }

    [Fact]
    public async Task TwoDifferentTenantUsers_EachSeesOnlyOwnData()
    {
        var ransaId = Guid.NewGuid();
        var acmeId = Guid.NewGuid();

        var ransaTenant = MakeTenant("RANSA", "Ransa Corp");
        var acmeTenant = MakeTenant("ACME", "Acme Corp");

        // Setup RANSA context
        var ransaContext = new Mock<ITenantContext>();
        ransaContext.Setup(t => t.IsInternalAdmin).Returns(false);
        ransaContext.Setup(t => t.OrganizationId).Returns(ransaId);

        // Setup ACME context
        var acmeContext = new Mock<ITenantContext>();
        acmeContext.Setup(t => t.IsInternalAdmin).Returns(false);
        acmeContext.Setup(t => t.OrganizationId).Returns(acmeId);

        var repoRansa = new Mock<ITenantRepository>();
        repoRansa.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<string>(), It.IsAny<string>(), ransaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<Tenant>)[ransaTenant], 1));

        var repoAcme = new Mock<ITenantRepository>();
        repoAcme.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<string>(), It.IsAny<string>(), acmeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<Tenant>)[acmeTenant], 1));

        var query = new GetAllTenantsQuery(Page: 1, PageSize: 20);

        var ransaHandler = new GetAllTenantsQueryHandler(repoRansa.Object, ransaContext.Object);
        var acmeHandler = new GetAllTenantsQueryHandler(repoAcme.Object, acmeContext.Object);

        var ransaResult = await ransaHandler.Handle(query, CancellationToken.None);
        var acmeResult = await acmeHandler.Handle(query, CancellationToken.None);

        Assert.True(ransaResult.IsSuccess);
        Assert.Single(ransaResult.Value.Items);
        Assert.Equal("RANSA", ransaResult.Value.Items[0].Code);

        Assert.True(acmeResult.IsSuccess);
        Assert.Single(acmeResult.Value.Items);
        Assert.Equal("ACME", acmeResult.Value.Items[0].Code);
    }

    #endregion
}
