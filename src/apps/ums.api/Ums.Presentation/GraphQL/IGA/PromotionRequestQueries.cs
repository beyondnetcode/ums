namespace Ums.Presentation.GraphQL.IGA;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.IGA.PromotionRequest.DTOs;
using Ums.Application.IGA.PromotionRequest.Queries;

[ExtendObjectType("Query")]
public sealed class PromotionRequestQueries
{
    public async Task<PagedResult<PromotionRequestDto>> GetPromotionRequestsAsync(
        int page, int pageSize, string? search, string? criteria, string? status, string? sortBy, string? sortOrder,
        Guid? tenantId, Guid? userId, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllPromotionRequestsQuery(
            page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, search,
            string.IsNullOrWhiteSpace(criteria) ? "status" : criteria,
            string.IsNullOrWhiteSpace(status) ? "all" : status,
            string.IsNullOrWhiteSpace(sortBy) ? "status" : sortBy,
            string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder, tenantId, userId), cancellationToken);
        if (result.IsFailure) throw new GraphQLException(ErrorBuilder.New().SetMessage(result.Error).SetCode("UMS_QUERY_ERROR").Build());
        return result.Value;
    }

    public async Task<PromotionRequestDto?> GetPromotionRequestByIdAsync(Guid id, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPromotionRequestByIdQuery(id), cancellationToken);
        return result.IsFailure ? null : result.Value;
    }
}
