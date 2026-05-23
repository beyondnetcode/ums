namespace Ums.Application.Test.IGA.RoleMaturityStatus;

using Ums.Application.IGA.RoleMaturityStatus.Queries;
using Ums.Application.IGA.RoleMaturityStatus.DTOs;
using Ums.Domain.IGA.RoleMaturityStatus;
using Ums.Domain.IGA;
using Ums.Domain.Enums;
using Ums.Domain.Kernel;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class RoleMaturityStatusQueryHandlerTests
{
    private readonly Mock<IRoleMaturityStatusRepository> _repo = new();

    private static RoleMaturityStatus MakeRoleMaturityStatus(RoleMaturityLevel level)
    {
        return RoleMaturityStatus.Create(
            TenantId.Load(Guid.NewGuid()),
            UserId.Load(Guid.NewGuid()),
            RoleId.Load(Guid.NewGuid()),
            level,
            ActorId.Create("user-001")).Value;
    }

    // =========================================================================
    #region GetRoleMaturityStatusByIdQueryHandler
    // =========================================================================

    [Fact]
    public async Task GetById_WhenFound_ReturnsSuccess()
    {
        var entity = MakeRoleMaturityStatus(RoleMaturityLevel.Intermediate);
        var entityId = entity.Props.Id.GetValue();
        _repo.Setup(r => r.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var query = new GetRoleMaturityStatusByIdQuery(entityId);
        var handler = new GetRoleMaturityStatusByIdQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(entityId, result.Value.RoleMaturityStatusId);
        Assert.Equal("Intermediate", result.Value.CurrentMaturityLevel);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((RoleMaturityStatus?)null);

        var query = new GetRoleMaturityStatusByIdQuery(Guid.NewGuid());
        var handler = new GetRoleMaturityStatusByIdQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("maturity status not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    // =========================================================================
    #region GetAllRoleMaturityStatusesQueryHandler
    // =========================================================================

    [Fact]
    public async Task GetAll_WithoutFilters_ReturnsAll()
    {
        var r1 = MakeRoleMaturityStatus(RoleMaturityLevel.Junior);
        var r2 = MakeRoleMaturityStatus(RoleMaturityLevel.Senior);
        var list = new List<RoleMaturityStatus> { r1, r2 };

        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(list);

        var query = new GetAllRoleMaturityStatusesQuery(
            TenantId: null,
            UserId: null,
            SortBy: null,
            SortOrder: null,
            Page: 1,
            PageSize: 10);

        var handler = new GetAllRoleMaturityStatusesQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalItems);
        Assert.Equal(2, result.Value.Items.Count);
    }

    [Fact]
    public async Task GetAll_WithUserIdFilter_ReturnsUserItems()
    {
        var userId = Guid.NewGuid();
        var r1 = MakeRoleMaturityStatus(RoleMaturityLevel.Junior);
        var list = new List<RoleMaturityStatus> { r1 };

        _repo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(list);

        var query = new GetAllRoleMaturityStatusesQuery(
            TenantId: null,
            UserId: userId,
            SortBy: null,
            SortOrder: null,
            Page: 1,
            PageSize: 10);

        var handler = new GetAllRoleMaturityStatusesQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalItems);
        _repo.Verify(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
