namespace Ums.Application.Test.Authorization.Profile;

using Ums.Application.Common.Interfaces;
using Ums.Application.Authorization.Profile.Queries;
using Ums.Application.Authorization.Profile.DTOs;
using Ums.Domain.Authorization.Profile;
using Ums.Domain.Kernel;
using Ums.Domain.Authorization;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class ProfileQueryHandlerTests
{
    private readonly Mock<IProfileRepository> _repo = new();
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly Mock<ISystemSuiteRepository> _suiteRepo = new();

    private static Profile MakeProfile(bool isActive = true)
    {
        var profile = Profile.Create(
            TenantId.Load(Guid.NewGuid()),
            UserId.Load(Guid.NewGuid()),
            RoleId.Load(Guid.NewGuid()),
            null,
            ActorId.Create("user-001")).Value;

        if (!isActive)
        {
            profile.Deactivate(ActorId.Create("user-001"));
        }
        return profile;
    }

    // =========================================================================
    #region GetProfileByIdQueryHandler
    // =========================================================================

    [Fact]
    public async Task GetById_WhenFound_ReturnsSuccess()
    {
        var profile = MakeProfile();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(profile);

        var query = new GetProfileByIdQuery(Guid.NewGuid());
        var handler = new GetProfileByIdQueryHandler(_repo.Object, _roleRepo.Object, _suiteRepo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(profile.UserId.GetValue(), result.Value.UserId);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Profile?)null);

        var query = new GetProfileByIdQuery(Guid.NewGuid());
        var handler = new GetProfileByIdQueryHandler(_repo.Object, _roleRepo.Object, _suiteRepo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    // =========================================================================
    #region GetAllProfilesQueryHandler
    // =========================================================================

    [Fact]
    public async Task GetAll_WithoutFilters_ReturnsAll()
    {
        var profiles = new List<Profile>
        {
            MakeProfile(true),
            MakeProfile(false)
        };

        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(profiles);

        var query = new GetAllProfilesQuery(
            TenantId: null,
            UserId: null,
            Page: 1,
            PageSize: 10,
            Criteria: null,
            Status: "all",
            SortBy: null,
            SortOrder: null,
            Search: null);

        var handler = new GetAllProfilesQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalItems);
    }

    [Fact]
    public async Task GetAll_FilterByUserId_ReturnsFiltered()
    {
        var userId = Guid.NewGuid();
        var profiles = new List<Profile>
        {
            MakeProfile(true)
        };

        _repo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(profiles);

        var query = new GetAllProfilesQuery(
            TenantId: null,
            UserId: userId,
            Page: 1,
            PageSize: 10,
            Criteria: null,
            Status: "all",
            SortBy: null,
            SortOrder: null,
            Search: null);

        var handler = new GetAllProfilesQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_FilterByTenantId_ReturnsFiltered()
    {
        var tenantId = Guid.NewGuid();
        var profiles = new List<Profile>
        {
            MakeProfile(true)
        };

        _repo.Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(profiles);

        var query = new GetAllProfilesQuery(
            TenantId: tenantId,
            UserId: null,
            Page: 1,
            PageSize: 10,
            Criteria: null,
            Status: "all",
            SortBy: null,
            SortOrder: null,
            Search: null);

        var handler = new GetAllProfilesQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithStatusActive_ReturnsActiveOnly()
    {
        var profiles = new List<Profile>
        {
            MakeProfile(true),
            MakeProfile(false)
        };

        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(profiles);

        var query = new GetAllProfilesQuery(
            TenantId: null,
            UserId: null,
            Page: 1,
            PageSize: 10,
            Criteria: null,
            Status: "active",
            SortBy: null,
            SortOrder: null,
            Search: null);

        var handler = new GetAllProfilesQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.True(result.Value.Items[0].IsActive);
    }

    #endregion
}
