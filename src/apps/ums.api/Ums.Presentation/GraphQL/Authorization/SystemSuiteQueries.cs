namespace Ums.Presentation.GraphQL.Authorization;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Authorization.SystemSuite.DTOs;
using Ums.Application.Authorization.SystemSuite.Queries;

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
            page <= 0 ? 1 : page,
            pageSize <= 0 ? 20 : pageSize,
            search,
            string.IsNullOrWhiteSpace(criteria) ? "name" : criteria,
            string.IsNullOrWhiteSpace(status) ? "all" : status,
            string.IsNullOrWhiteSpace(sortBy) ? "name" : sortBy,
            string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder,
            tenantId), cancellationToken);

        if (result.IsFailure)
        {
            throw BuildQueryException(result.Error);
        }

        return result.Value;
    }

    public async Task<SystemSuiteDto?> GetSystemSuiteByIdAsync(
        Guid systemSuiteId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSystemSuiteByIdQuery(systemSuiteId), cancellationToken);

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
