namespace Ums.Application.Identity.Tenant.DeactivateIdentityProvider;


public sealed record DeactivateIdentityProviderCommand(
    Guid TenantId,
    Guid IdentityProviderId) : ICommand<DeactivateIdentityProviderResponse>;
