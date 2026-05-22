namespace Ums.Presentation.GraphQL.IGA;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.IGA.PromotionRequest.DTOs;
using Ums.Application.IGA.PromotionRequest.Queries;
using Ums.Presentation.Extensions;
using static Ums.Application.Common.QueryRequestNormalizer;

[ExtendObjectType("Query")]
public sealed class PromotionRequestQueries
{
    public async Task<PagedResult<PromotionRequestDto>> GetPromotionRequestsAsync(
        int page, int pageSize, string? search, string? criteria, string? status, string? sortBy, string? sortOrder,
        Guid? tenantId, Guid? userId, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllPromotionRequestsQuery(
            NormalizePage(page), NormalizePageSize(pageSize), search,
            NormalizeText(criteria, "status"),
            NormalizeText(status, "all"),
            NormalizeText(sortBy, "status"),
            NormalizeText(sortOrder, "asc"), tenantId, userId), cancellationToken);
        return result.UnwrapGraphQl();
    }

    public async Task<PromotionRequestDto?> GetPromotionRequestByIdAsync(Guid id, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPromotionRequestByIdQuery(id), cancellationToken);
        return result.UnwrapGraphQlOrNull();
    }
}
