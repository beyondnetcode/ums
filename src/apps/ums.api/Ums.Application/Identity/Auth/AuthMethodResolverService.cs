using Ums.Application.Configuration.Services;
using Ums.Domain.Identity;
using Ums.Domain.Identity.Auth;

namespace Ums.Application.Identity.Auth;

/// <summary>
/// Resolves the authentication method for a tenant.
/// Reads AUTH_USE_EXTERNAL_IDP from the in-memory IConfigurationProvider
/// (zero DB hits per request — the provider loads once and caches in memory).
///
/// - false → AuthMethod.Local()
/// - true + active IDP → AuthMethod.Idp(provider)
/// - true + no active IDP → Result.Failure("AUTH_011")
/// </summary>
public sealed class AuthMethodResolverService : IAuthMethodResolver
{
    private readonly IConfigurationProvider _config;
    private readonly ITenantRepository      _tenantRepo;

    public AuthMethodResolverService(IConfigurationProvider config, ITenantRepository tenantRepo)
    {
        _config     = config;
        _tenantRepo = tenantRepo;
    }

    public async Task<Result<AuthMethod>> ResolveAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var useExternalIdp = _config.GetValueAs<bool>("AUTH_USE_EXTERNAL_IDP", tenantId, false);

        if (!useExternalIdp)
            return Result<AuthMethod>.Success(AuthMethod.Local());

        // IDP mode — find the active identity provider for this tenant
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, cancellationToken);
        if (tenant is null)
            return Result<AuthMethod>.Failure($"AUTH_002: Tenant {tenantId} not found.");

        var activeIdp = tenant.IdentityProviders.FirstOrDefault(p => p.IsActive);
        if (activeIdp is null)
            return Result<AuthMethod>.Failure(
                "AUTH_011: Tenant is configured for external IDP authentication " +
                "but has no active Identity Provider. Configure and activate an IDP first.");

        return Result<AuthMethod>.Success(AuthMethod.Idp(activeIdp));
    }
}
