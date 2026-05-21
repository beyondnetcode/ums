namespace Ums.Presentation.GraphQL.Authorization;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Authorization.Template.DTOs;
using Ums.Application.Authorization.Template.Queries;

[ExtendObjectType("Query")]
public sealed class PermissionTemplateQueries
{
    public async Task<PagedResult<PermissionTemplateDto>> GetPermissionTemplatesAsync(
        int page,
        int pageSize,
        string? search,
        string? criteria,
        string? status,
        string? sortBy,
        string? sortOrder,
        Guid? tenantId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllPermissionTemplatesQuery(
            page <= 0 ? 1 : page,
            pageSize <= 0 ? 20 : pageSize,
            search,
            string.IsNullOrWhiteSpace(criteria) ? "version" : criteria,
            string.IsNullOrWhiteSpace(status) ? "all" : status,
            string.IsNullOrWhiteSpace(sortBy) ? "version" : sortBy,
            string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder,
            tenantId), cancellationToken);

        if (result.IsFailure)
        {
            throw BuildQueryException(result.Error);
        }

        return result.Value;
    }

    public async Task<PermissionTemplateDto?> GetPermissionTemplateByIdAsync(
        Guid templateId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPermissionTemplateByIdQuery(templateId), cancellationToken);

        if (result.IsFailure)
        {
            return null;
        }

        return result.Value;
    }

    private static GraphQLException BuildQueryException(string message) =>
        new(ErrorBuilder.New()
            .SetMessage(message)
            .SetCode("UMS_QUERY_ERROR")
            .Build());
}
