namespace Ums.Application.Test.Authorization.SystemSuite;

using Ums.Application.Authorization.SystemSuite.Queries;
using Ums.Application.Authorization.SystemSuite.DTOs;
using Ums.Domain.Authorization.SystemSuite;
using Ums.Domain.Authorization;
using Ums.Domain.Kernel;
using Ums.Application.Common.Interfaces;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class SystemSuiteQueryHandlerTests
{
    private readonly Mock<ISystemSuiteRepository> _repo = new();
    private readonly Mock<IUserContext> _userContext = new();

    private static SystemSuite MakeSystemSuite(string code, string name, string statusName)
    {
        var status = statusName == "Maintenance" ? SystemStatus.Maintenance : SystemStatus.Active;
        var suite = SystemSuite.Create(
            TenantId.Load(Guid.NewGuid()),
            Code.Create(code),
            Name.Create(name),
            Description.Create("Description"),
            ActorId.Create("user-001")).Value;

        if (statusName != "Active")
        {
            suite.SetStatus(status, ActorId.Create("user-001"));
        }

        return suite;
    }

    // =========================================================================
    #region GetSystemSuiteByIdQueryHandler
    // =========================================================================

    [Fact]
    public async Task GetById_WhenFound_ReturnsSuccess()
    {
        var suite = MakeSystemSuite("SUITE-01", "Suite Alpha", "Active");
        var suiteId = suite.Props.Id.GetValue();
        _repo.Setup(r => r.GetByIdAsync(suiteId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(suite);

        var query = new GetSystemSuiteByIdQuery(suiteId);
        var handler = new GetSystemSuiteByIdQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(suiteId, result.Value.SystemSuiteId);
        Assert.Equal("SUITE-01", result.Value.Code);
        Assert.Equal("Suite Alpha", result.Value.Name);
        Assert.Equal("Active", result.Value.Status);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((SystemSuite?)null);

        var query = new GetSystemSuiteByIdQuery(Guid.NewGuid());
        var handler = new GetSystemSuiteByIdQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("suite not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    // =========================================================================
    #region GetAllSystemSuitesQueryHandler
    // =========================================================================

    [Fact]
    public async Task GetAll_WithoutTenantFilter_ReturnsAllItems()
    {
        var s1 = MakeSystemSuite("SUITE-01", "Suite Alpha", "Active");
        var s2 = MakeSystemSuite("SUITE-02", "Suite Beta", "Maintenance");
        var list = new List<SystemSuite> { s1, s2 };

        _repo.Setup(r => r.GetAllAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(list);

        var query = new GetAllSystemSuitesQuery(
            TenantId: null,
            Criteria: "name",
            Status: "all",
            Search: null,
            SortBy: null,
            SortOrder: null,
            Page: 1,
            PageSize: 10);

        var handler = new GetAllSystemSuitesQueryHandler(_repo.Object, _userContext.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalItems);
        Assert.Equal(2, result.Value.Items.Count);
    }

    [Fact]
    public async Task GetAll_WithTenantFilter_ReturnsTenantItems()
    {
        var tenantId = Guid.NewGuid();
        var s1 = MakeSystemSuite("SUITE-01", "Suite Alpha", "Active");
        var list = new List<SystemSuite> { s1 };

        _repo.Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(list);

        var query = new GetAllSystemSuitesQuery(
            TenantId: tenantId,
            Criteria: "name",
            Status: "all",
            Search: null,
            SortBy: null,
            SortOrder: null,
            Page: 1,
            PageSize: 10);

        var handler = new GetAllSystemSuitesQueryHandler(_repo.Object, _userContext.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalItems);
        _repo.Verify(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithStatusFilter_FiltersStatus()
    {
        var s1 = MakeSystemSuite("SUITE-01", "Suite Alpha", "Active");
        var s2 = MakeSystemSuite("SUITE-02", "Suite Beta", "Maintenance");
        var list = new List<SystemSuite> { s1, s2 };

        _repo.Setup(r => r.GetAllAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(list);

        var query = new GetAllSystemSuitesQuery(
            TenantId: null,
            Criteria: "name",
            Status: "Maintenance",
            Search: null,
            SortBy: null,
            SortOrder: null,
            Page: 1,
            PageSize: 10);

        var handler = new GetAllSystemSuitesQueryHandler(_repo.Object, _userContext.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalItems);
        Assert.Equal("Maintenance", result.Value.Items[0].Status);
    }

    [Fact]
    public async Task GetAll_WithSearch_FiltersSearch()
    {
        var s1 = MakeSystemSuite("SUITE-01", "Suite Alpha", "Active");
        var s2 = MakeSystemSuite("SUITE-02", "Suite Beta", "Active");
        var list = new List<SystemSuite> { s1, s2 };

        _repo.Setup(r => r.GetAllAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(list);

        var query = new GetAllSystemSuitesQuery(
            TenantId: null,
            Criteria: "code",
            Status: "all",
            Search: "SUITE-02",
            SortBy: null,
            SortOrder: null,
            Page: 1,
            PageSize: 10);

        var handler = new GetAllSystemSuitesQueryHandler(_repo.Object, _userContext.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalItems);
        Assert.Equal("SUITE-02", result.Value.Items[0].Code);
    }

    #endregion
}
