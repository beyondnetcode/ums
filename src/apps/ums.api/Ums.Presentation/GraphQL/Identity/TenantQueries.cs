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
using Ums.Presentation.Extensions;
using static Ums.Application.Common.QueryRequestNormalizer;

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
            NormalizePage(page),
            NormalizePageSize(pageSize),
            search,
            NormalizeText(criteria, "name"),
            NormalizeText(status, "all"),
            NormalizeText(sortBy, "name"),
            NormalizeText(sortOrder, "asc")), cancellationToken);

        return result.UnwrapGraphQl();
    }

    public async Task<TenantDto?> GetTenantByIdAsync(
        Guid tenantId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTenantByIdQuery(tenantId), cancellationToken);

        return result.UnwrapGraphQlOrNull();
    }

    public async Task<IReadOnlyList<BranchDto>> GetTenantBranchesAsync(
        Guid tenantId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetBranchesByTenantIdQuery(tenantId), cancellationToken);

        return result.UnwrapGraphQl();
    }

    public async Task<BrandingDto?> GetTenantBrandingAsync(
        Guid tenantId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetBrandingByTenantIdQuery(tenantId), cancellationToken);

        return result.UnwrapGraphQlOrNull();
    }

    public async Task<IReadOnlyList<IdentityProviderDto>> GetTenantIdentityProvidersAsync(
        Guid tenantId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetIdentityProvidersByTenantIdQuery(tenantId), cancellationToken);

        return result.UnwrapGraphQl();
    }
}
