namespace Ums.Presentation.GraphQL.Identity;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Identity.Tenant.Branch.DTOs;
using Ums.Application.Identity.Tenant.Branch.Queries;
using Ums.Application.Identity.Tenant.Branding.DTOs;
using Ums.Application.Identity.Tenant.Branding.Queries;
using Ums.Application.Identity.Tenant.DTOs;
using Ums.Application.Identity.Tenant.IdentityProvider.DTOs;
using Ums.Application.Identity.Tenant.IdentityProvider.Queries;
using Ums.Application.Identity.Tenant.Queries;

[ExtendObjectType("Query")]
public sealed class TenantQueries
{
    public async Task<PagedResult<TenantDto>> GetTenantsAsync(
        int page,
        int pageSize,
        string? search,
        string? criteria,
        string? status,
        string? sortBy,
        string? sortOrder,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllTenantsQuery(
            page <= 0 ? 1 : page,
            pageSize <= 0 ? 20 : pageSize,
            search,
            string.IsNullOrWhiteSpace(criteria) ? "name" : criteria,
            string.IsNullOrWhiteSpace(status) ? "all" : status,
            string.IsNullOrWhiteSpace(sortBy) ? "name" : sortBy,
            string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder), cancellationToken);

        if (result.IsFailure)
        {
            throw BuildQueryException(result.Error);
        }

        return result.Value;
    }

    public async Task<TenantDto?> GetTenantByIdAsync(
        Guid tenantId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTenantByIdQuery(tenantId), cancellationToken);

        if (result.IsFailure)
        {
            return null;
        }

        return result.Value;
    }

    public async Task<IReadOnlyList<BranchDto>> GetTenantBranchesAsync(
        Guid tenantId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetBranchesByTenantIdQuery(tenantId), cancellationToken);

        if (result.IsFailure)
        {
            throw BuildQueryException(result.Error);
        }

        return result.Value;
    }

    public async Task<BrandingDto?> GetTenantBrandingAsync(
        Guid tenantId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetBrandingByTenantIdQuery(tenantId), cancellationToken);

        if (result.IsFailure)
        {
            return null;
        }

        return result.Value;
    }

    public async Task<IReadOnlyList<IdentityProviderDto>> GetTenantIdentityProvidersAsync(
        Guid tenantId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetIdentityProvidersByTenantIdQuery(tenantId), cancellationToken);

        if (result.IsFailure)
        {
            throw BuildQueryException(result.Error);
        }

        return result.Value;
    }

    private static GraphQLException BuildQueryException(string message) =>
        new(ErrorBuilder.New()
            .SetMessage(message)
            .SetCode("UMS_QUERY_ERROR")
            .Build());
}
