namespace Ums.Application.Identity.Tenant.RegisterIdentityProvider;


public sealed record RegisterIdentityProviderCommand(
    Guid TenantId,
    string Code,
    string Name,
    string Description,
    string Strategy) : ICommand<RegisterIdentityProviderResponse>;
