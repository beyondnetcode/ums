namespace Ums.Presentation.GraphQL.Approvals;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Approvals.DocumentType.DTOs;
using Ums.Application.Approvals.DocumentType.Queries;

[ExtendObjectType("Query")]
public sealed class DocumentTypeQueries
{
    public async Task<PagedResult<DocumentTypeDto>> GetDocumentTypesAsync(
        int page, int pageSize, string? search, string? criteria, string? sortBy, string? sortOrder,
        Guid? tenantId, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllDocumentTypesQuery(
            page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, search,
            string.IsNullOrWhiteSpace(criteria) ? "name" : criteria,
            string.IsNullOrWhiteSpace(sortBy) ? "name" : sortBy,
            string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder, tenantId), cancellationToken);
        if (result.IsFailure) throw new GraphQLException(ErrorBuilder.New().SetMessage(result.Error).SetCode("UMS_QUERY_ERROR").Build());
        return result.Value;
    }

    public async Task<DocumentTypeDto?> GetDocumentTypeByIdAsync(Guid id, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDocumentTypeByIdQuery(id), cancellationToken);
        return result.IsFailure ? null : result.Value;
    }
}
