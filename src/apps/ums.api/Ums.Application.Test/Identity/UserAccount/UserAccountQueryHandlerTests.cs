namespace Ums.Application.Test.Identity.UserAccount;

using Ums.Application.Common.Interfaces;
using Ums.Application.Identity.UserAccount.Queries;
using Ums.Application.Identity.UserAccount.DTOs;
using Ums.Domain.Identity;
using Ums.Domain.Identity.UserAccount;
using Ums.Domain.Kernel;
using Ums.Domain.Enums;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class UserAccountQueryHandlerTests
{
    private readonly Mock<IUserAccountRepository> _repo = new();

    private static UserAccount MakeUserAccount(string email = "test@test.com", UserStatus status = null)
    {
        var user = UserAccount.Create(
            TenantId.Load(Guid.NewGuid()),
            Email.Create(email),
            UserCategory.Internal,
            null,
            null,
            ActorId.Create("user-001"),
            null).Value;

        if (status == UserStatus.Active)
        {
            user.Activate(ActorId.Create("user-001"));
        }
        else if (status == UserStatus.Blocked)
        {
            user.Block(Reason.Create("Blocked"), ActorId.Create("user-001"));
        }
        return user;
    }

    // =========================================================================
    #region GetUserAccountByIdQueryHandler
    // =========================================================================

    [Fact]
    public async Task GetById_WhenFound_ReturnsSuccess()
    {
        var user = MakeUserAccount();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        var query = new GetUserAccountByIdQuery(Guid.NewGuid());
        var handler = new GetUserAccountByIdQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Email.GetValue(), result.Value.Email);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((UserAccount?)null);

        var query = new GetUserAccountByIdQuery(Guid.NewGuid());
        var handler = new GetUserAccountByIdQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    // =========================================================================
    #region GetAllUserAccountsQueryHandler
    // =========================================================================

    [Fact]
    public async Task GetAll_WithoutFilters_ReturnsAll()
    {
        var users = new List<UserAccount>
        {
            MakeUserAccount("a@test.com", UserStatus.Active),
            MakeUserAccount("b@test.com", UserStatus.Pending)
        };

        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(users);

        var query = new GetAllUserAccountsQuery(
            TenantId: null,
            Page: 1,
            PageSize: 10,
            Criteria: null,
            Status: "all",
            SortBy: null,
            SortOrder: null,
            Search: null);

        var handler = new GetAllUserAccountsQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalItems);
    }

    [Fact]
    public async Task GetAll_WithTenantId_FilterByTenant()
    {
        var tenantId = Guid.NewGuid();
        var users = new List<UserAccount>
        {
            MakeUserAccount("a@test.com", UserStatus.Active)
        };

        _repo.Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(users);

        var query = new GetAllUserAccountsQuery(
            TenantId: tenantId,
            Page: 1,
            PageSize: 10,
            Criteria: null,
            Status: "all",
            SortBy: null,
            SortOrder: null,
            Search: null);

        var handler = new GetAllUserAccountsQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        _repo.Verify(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithStatusFilter_ReturnsFiltered()
    {
        var users = new List<UserAccount>
        {
            MakeUserAccount("a@test.com", UserStatus.Active),
            MakeUserAccount("b@test.com", UserStatus.Pending)
        };

        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(users);

        var query = new GetAllUserAccountsQuery(
            TenantId: null,
            Page: 1,
            PageSize: 10,
            Criteria: null,
            Status: "Active",
            SortBy: null,
            SortOrder: null,
            Search: null);

        var handler = new GetAllUserAccountsQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal("Active", result.Value.Items[0].Status);
    }

    [Fact]
    public async Task GetAll_WithSearch_ReturnsSearched()
    {
        var users = new List<UserAccount>
        {
            MakeUserAccount("target@test.com"),
            MakeUserAccount("other@test.com")
        };

        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(users);

        var query = new GetAllUserAccountsQuery(
            TenantId: null,
            Page: 1,
            PageSize: 10,
            Criteria: "email",
            Status: "all",
            SortBy: null,
            SortOrder: null,
            Search: "target");

        var handler = new GetAllUserAccountsQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal("target@test.com", result.Value.Items[0].Email);
    }

    #endregion
}
