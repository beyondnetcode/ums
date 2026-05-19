using Ums.Application.Identity.Tenant.DTOs;

namespace Ums.Application.Identity.Tenant.Queries;

public sealed record GetTenantByIdQuery(Guid TenantId) : IQuery<TenantDto>;
