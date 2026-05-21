namespace Ums.Presentation.GraphQL.Approvals;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Approvals.UserDocument.DTOs;
using Ums.Application.Approvals.UserDocument.Queries;

[ExtendObjectType("Query")]
public sealed class UserDocumentQueries
{
    public async Task<PagedResult<UserDocumentDto>> GetUserDocumentsAsync(
        int page, int pageSize, string? search, string? criteria, string? status, string? sortBy, string? sortOrder,
        Guid? userId, Guid? tenantId, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllUserDocumentsQuery(
            page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, search,
            string.IsNullOrWhiteSpace(criteria) ? "status" : criteria,
            string.IsNullOrWhiteSpace(status) ? "all" : status,
            string.IsNullOrWhiteSpace(sortBy) ? "status" : sortBy,
            string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder, userId, tenantId), cancellationToken);
        if (result.IsFailure) throw new GraphQLException(ErrorBuilder.New().SetMessage(result.Error).SetCode("UMS_QUERY_ERROR").Build());
        return result.Value;
    }

    public async Task<UserDocumentDto?> GetUserDocumentByIdAsync(Guid id, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserDocumentByIdQuery(id), cancellationToken);
        return result.IsFailure ? null : result.Value;
    }
}
