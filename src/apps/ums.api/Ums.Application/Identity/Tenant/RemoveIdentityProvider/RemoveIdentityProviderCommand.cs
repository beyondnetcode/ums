namespace Ums.Application.Identity.Tenant.RemoveIdentityProvider;


public sealed record RemoveIdentityProviderCommand(
    Guid TenantId,
    Guid IdentityProviderId) : ICommand<RemoveIdentityProviderResponse>;
