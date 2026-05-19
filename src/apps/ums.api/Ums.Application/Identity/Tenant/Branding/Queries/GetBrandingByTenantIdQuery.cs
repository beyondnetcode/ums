using Ums.Application.Identity.Tenant.Branding.DTOs;

namespace Ums.Application.Identity.Tenant.Branding.Queries;

public sealed record GetBrandingByTenantIdQuery(Guid TenantId) : IQuery<BrandingDto>;
