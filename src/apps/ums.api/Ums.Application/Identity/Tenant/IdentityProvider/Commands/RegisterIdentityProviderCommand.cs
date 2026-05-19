using Ums.Application.Identity.Tenant.IdentityProvider.DTOs;



namespace Ums.Application.Identity.Tenant.IdentityProvider.Commands;


public sealed record RegisterIdentityProviderCommand(
    Guid TenantId,
    string Code,
    string Name,
    string Description,
    string Strategy) : ICommand<RegisterIdentityProviderResponse>;
