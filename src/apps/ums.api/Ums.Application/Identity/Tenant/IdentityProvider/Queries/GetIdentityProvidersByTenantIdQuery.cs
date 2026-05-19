using Ums.Application.Identity.Tenant.IdentityProvider.DTOs;

namespace Ums.Application.Identity.Tenant.IdentityProvider.Queries;

public sealed record GetIdentityProvidersByTenantIdQuery(Guid TenantId) : IQuery<IReadOnlyList<IdentityProviderDto>>;
