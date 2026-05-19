namespace Ums.Application.Identity.Tenant.RegisterIdentityProvider;

public sealed record RegisterIdentityProviderResponse(Guid TenantId, Guid IdentityProviderId);
