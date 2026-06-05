namespace Ums.Domain.Identity.Auth;

/// <summary>
/// Resolves the authentication method for a tenant by reading
/// AUTH_USE_EXTERNAL_IDP from the in-memory configuration provider
/// (never hits the database per request).
///
/// Portal management access always resolves to local authentication.
/// Returns AuthMethod.Local() when AUTH_USE_EXTERNAL_IDP == false.
/// Returns AuthMethod.Idp(provider) when true and an active IDP exists.
/// Returns failure when IDP mode is configured but no active provider is found,
/// except for InternalPreview where IDP mode is still returned without a provider
/// so administrative previews can render the effective graph context.
/// </summary>
public interface IAuthMethodResolver
{
    Task<Result<AuthMethod>> ResolveAsync(
        Guid tenantId,
        AuthAccessScope scope,
        CancellationToken cancellationToken = default);
}
