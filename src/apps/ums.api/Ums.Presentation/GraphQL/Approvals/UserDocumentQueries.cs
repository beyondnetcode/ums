namespace Ums.Presentation.GraphQL.Approvals;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Approvals.UserDocument.DTOs;
using Ums.Application.Approvals.UserDocument.Queries;
using Ums.Presentation.Extensions;
using static Ums.Application.Common.QueryRequestNormalizer;

[ExtendObjectType("Query")]
public sealed class UserDocumentQueries
{
    public async Task<PagedResult<UserDocumentDto>> GetUserDocumentsAsync(
        int page, int pageSize, string? search, string? criteria, string? status, string? sortBy, string? sortOrder,
        Guid? userId, Guid? tenantId, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllUserDocumentsQuery(
            NormalizePage(page), NormalizePageSize(pageSize), search,
            NormalizeText(criteria, "status"),
            NormalizeText(status, "all"),
            NormalizeText(sortBy, "status"),
            NormalizeText(sortOrder, "asc"), userId, tenantId), cancellationToken);
        return result.UnwrapGraphQl();
    }

    public async Task<UserDocumentDto?> GetUserDocumentByIdAsync(Guid id, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserDocumentByIdQuery(id), cancellationToken);
        return result.UnwrapGraphQlOrNull();
    }
}
