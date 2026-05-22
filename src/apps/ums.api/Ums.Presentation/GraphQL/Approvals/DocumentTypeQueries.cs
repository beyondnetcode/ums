namespace Ums.Presentation.GraphQL.Approvals;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Approvals.DocumentType.DTOs;
using Ums.Application.Approvals.DocumentType.Queries;
using Ums.Presentation.Extensions;
using static Ums.Application.Common.QueryRequestNormalizer;

[ExtendObjectType("Query")]
public sealed class DocumentTypeQueries
{
    public async Task<PagedResult<DocumentTypeDto>> GetDocumentTypesAsync(
        int page, int pageSize, string? search, string? criteria, string? sortBy, string? sortOrder,
        Guid? tenantId, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllDocumentTypesQuery(
            NormalizePage(page), NormalizePageSize(pageSize), search,
            NormalizeText(criteria, "name"),
            NormalizeText(sortBy, "name"),
            NormalizeText(sortOrder, "asc"), tenantId), cancellationToken);
        return result.UnwrapGraphQl();
    }

    public async Task<DocumentTypeDto?> GetDocumentTypeByIdAsync(Guid id, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDocumentTypeByIdQuery(id), cancellationToken);
        return result.UnwrapGraphQlOrNull();
    }
}
