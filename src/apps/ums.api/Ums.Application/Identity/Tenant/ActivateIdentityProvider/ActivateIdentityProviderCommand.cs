namespace Ums.Application.Identity.Tenant.ActivateIdentityProvider;


public sealed record ActivateIdentityProviderCommand(
    Guid TenantId,
    Guid IdentityProviderId) : ICommand<ActivateIdentityProviderResponse>;
