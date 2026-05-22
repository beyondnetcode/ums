namespace Ums.Presentation.GraphQL.Authorization;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Authorization.Template.DTOs;
using Ums.Application.Authorization.Template.Queries;
using Ums.Presentation.Extensions;
using static Ums.Application.Common.QueryRequestNormalizer;

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
            NormalizePage(page),
            NormalizePageSize(pageSize),
            search,
            NormalizeText(criteria, "version"),
            NormalizeText(status, "all"),
            NormalizeText(sortBy, "version"),
            NormalizeText(sortOrder, "asc"),
            tenantId), cancellationToken);

        return result.UnwrapGraphQl();
    }

    public async Task<PermissionTemplateDto?> GetPermissionTemplateByIdAsync(
        Guid templateId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPermissionTemplateByIdQuery(templateId), cancellationToken);

        return result.UnwrapGraphQlOrNull();
    }
}
