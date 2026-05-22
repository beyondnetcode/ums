namespace Ums.Presentation.GraphQL.Authorization;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Authorization.SystemSuite.DTOs;
using Ums.Application.Authorization.SystemSuite.Queries;
using Ums.Presentation.Extensions;
using static Ums.Application.Common.QueryRequestNormalizer;

[ExtendObjectType("Query")]
public sealed class SystemSuiteQueries
{
    public async Task<PagedResult<SystemSuiteDto>> GetSystemSuitesAsync(
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
        var result = await mediator.Send(new GetAllSystemSuitesQuery(
            NormalizePage(page),
            NormalizePageSize(pageSize),
            search,
            NormalizeText(criteria, "name"),
            NormalizeText(status, "all"),
            NormalizeText(sortBy, "name"),
            NormalizeText(sortOrder, "asc"),
            tenantId), cancellationToken);

        return result.UnwrapGraphQl();
    }

    public async Task<SystemSuiteDto?> GetSystemSuiteByIdAsync(
        Guid systemSuiteId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSystemSuiteByIdQuery(systemSuiteId), cancellationToken);

        return result.UnwrapGraphQlOrNull();
    }
}
