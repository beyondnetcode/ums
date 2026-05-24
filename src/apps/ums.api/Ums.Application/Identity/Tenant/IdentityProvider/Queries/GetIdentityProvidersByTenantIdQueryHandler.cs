using Ums.Application.Identity.Tenant.IdentityProvider.DTOs;
using Ums.Domain.Identity.Tenant;

namespace Ums.Application.Identity.Tenant.IdentityProvider.Queries;

public sealed class GetIdentityProvidersByTenantIdQueryHandler : IQueryHandler<GetIdentityProvidersByTenantIdQuery, IReadOnlyList<IdentityProviderDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetIdentityProvidersByTenantIdQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]

    public async Task<Result<IReadOnlyList<IdentityProviderDto>>> Handle(
        GetIdentityProvidersByTenantIdQuery request,
        CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);

        if (tenant is null)
        {
            return Result<IReadOnlyList<IdentityProviderDto>>.Failure("Tenant not found.");
        }

        var identityProviders = tenant.IdentityProviders
            .Select(idp => new IdentityProviderDto(
                idp.GetId().GetValue(),
                idp.Code.GetValue(),
                idp.Name.GetValue(),
                idp.Description.GetValue(),
                idp.Strategy.ToString(),
                idp.IsActive))
            .ToList();

        return Result<IReadOnlyList<IdentityProviderDto>>.Success(identityProviders.AsReadOnly());
    }
}
