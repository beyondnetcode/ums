using Ums.Application.Identity.Tenant.IdentityProvider.DTOs;



namespace Ums.Application.Identity.Tenant.IdentityProvider.Commands;


public sealed record RemoveIdentityProviderCommand(
    Guid TenantId,
    Guid IdentityProviderId) : ICommand<RemoveIdentityProviderResponse>;
