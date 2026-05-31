using BeyondNetCode.Shell.Factory.Interfaces;
using Ums.Application.Identity.Auth;
using Ums.Domain.Identity.Auth;
using Ums.Domain.Identity.Tenant.IdentityProvider;

namespace Ums.Infrastructure.Identity.Auth;

/// <summary>
/// Infrastructure implementation of IIdpAuthStrategy.
/// Uses Shell.Factory to resolve the correct IIdpAuthAdapter by strategy name.
/// </summary>
public sealed class IdpAuthStrategyDispatcher : IIdpAuthStrategy
{
    private readonly IFactory _factory;

    public IdpAuthStrategyDispatcher(IFactory factory)
    {
        _factory = factory;
    }

    public async Task<Result<ExternalIdentity>> AuthenticateAsync(
        Guid              tenantId,
        string            credential,
        IdentityProvider  provider,
        CancellationToken cancellationToken = default)
    {
        var criteria = new IdpAuthAdapterCriteria(provider.Props.Strategy.Name);
        var adapter  = _factory
            .Create<IdpAuthAdapterCriteria, IIdpAuthAdapter>(criteria)
            .SingleOrDefault();

        if (adapter is null)
            return Result<ExternalIdentity>.Failure(
                $"AUTH_012: No IDP adapter registered for strategy '{provider.Props.Strategy.Name}'. " +
                "Register the appropriate IIdpAuthAdapter via Shell.Factory.");

        return await adapter.ValidateAsync(provider, credential, cancellationToken);
    }
}
