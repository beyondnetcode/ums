namespace Ums.Application.Test.IGA.PromotionRequest;

using Ums.Application.IGA.PromotionRequest.Queries;
using Ums.Application.IGA.PromotionRequest.DTOs;
using Ums.Domain.IGA.PromotionRequest;
using Ums.Domain.IGA;
using Ums.Domain.Enums;
using Ums.Domain.Kernel;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class PromotionRequestQueryHandlerTests
{
    private readonly Mock<IPromotionRequestRepository> _repo = new();

    private static PromotionRequest MakePromotionRequest(PromotionStatus status)
    {
        var req = PromotionRequest.Create(
            TenantId.Load(Guid.NewGuid()),
            UserId.Load(Guid.NewGuid()),
            RoleId.Load(Guid.NewGuid()),
            RoleId.Load(Guid.NewGuid()),
            UserId.Load(Guid.NewGuid()),
            TextValueObject.Create("Reason for promotion"),
            ActorId.Create("user-001")).Value;

        if (status == PromotionStatus.PendingManagerApproval)
        {
            req.Submit(ActorId.Create("user-001"));
        }
        else if (status == PromotionStatus.PendingSecurityReview)
        {
            req.Submit(ActorId.Create("user-001"));
            req.ManagerApprove(ActorId.Create("user-001"));
        }
        else if (status == PromotionStatus.Rejected)
        {
            req.Submit(ActorId.Create("user-001"));
            req.ManagerReject(ActorId.Create("user-001"));
        }

        return req;
    }

    // =========================================================================
    #region GetPromotionRequestByIdQueryHandler
    // =========================================================================

    [Fact]
    public async Task GetById_WhenFound_ReturnsSuccess()
    {
        var req = MakePromotionRequest(PromotionStatus.Draft);
        var reqId = req.Props.Id.GetValue();
        _repo.Setup(r => r.GetByIdAsync(reqId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(req);

        var query = new GetPromotionRequestByIdQuery(reqId);
        var handler = new GetPromotionRequestByIdQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(reqId, result.Value.PromotionRequestId);
        Assert.Equal("Draft", result.Value.Status);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((PromotionRequest?)null);

        var query = new GetPromotionRequestByIdQuery(Guid.NewGuid());
        var handler = new GetPromotionRequestByIdQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("promotion request not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    // =========================================================================
    #region GetAllPromotionRequestsQueryHandler
    // =========================================================================

    [Fact]
    public async Task GetAll_WithoutFilters_ReturnsAll()
    {
        var r1 = MakePromotionRequest(PromotionStatus.Draft);
        var r2 = MakePromotionRequest(PromotionStatus.Rejected);
        var list = new List<PromotionRequest> { r1, r2 };

        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(list);

        var query = new GetAllPromotionRequestsQuery(
            TenantId: null,
            UserId: null,
            Status: "all",
            SortBy: null,
            SortOrder: null,
            Page: 1,
            PageSize: 10);

        var handler = new GetAllPromotionRequestsQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalItems);
        Assert.Equal(2, result.Value.Items.Count);
    }

    [Fact]
    public async Task GetAll_WithUserIdFilter_ReturnsUserItems()
    {
        var userId = Guid.NewGuid();
        var r1 = MakePromotionRequest(PromotionStatus.Draft);
        var list = new List<PromotionRequest> { r1 };

        _repo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(list);

        var query = new GetAllPromotionRequestsQuery(
            TenantId: null,
            UserId: userId,
            Status: "all",
            SortBy: null,
            SortOrder: null,
            Page: 1,
            PageSize: 10);

        var handler = new GetAllPromotionRequestsQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalItems);
        _repo.Verify(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithStatusFilter_FiltersStatus()
    {
        var r1 = MakePromotionRequest(PromotionStatus.Draft);
        var r2 = MakePromotionRequest(PromotionStatus.Rejected);
        var list = new List<PromotionRequest> { r1, r2 };

        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(list);

        var query = new GetAllPromotionRequestsQuery(
            TenantId: null,
            UserId: null,
            Status: "Rejected",
            SortBy: null,
            SortOrder: null,
            Page: 1,
            PageSize: 10);

        var handler = new GetAllPromotionRequestsQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalItems);
        Assert.Equal("Rejected", result.Value.Items[0].Status);
    }

    #endregion
}
