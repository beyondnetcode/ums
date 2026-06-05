namespace Ums.Domain.Identity.Auth;

/// <summary>
/// Identifies the context in which an authentication request is made.
///
/// ADR-0077: Separates UMS portal management access from external API authentication.
/// - PortalManagement: user logs into the UMS web portal to manage their tenant data.
///   Always resolved via local BCrypt regardless of the tenant's IDP configuration.
/// - ExternalApi: downstream system calls the public auth API (/api/v1/client/authenticate).
///   Resolved via the tenant's configured IDP when AUTH_USE_EXTERNAL_IDP = true.
/// </summary>
public enum AuthAccessScope
{
    /// <summary>
    /// UMS management portal login (/api/v1/auth/login).
    /// Uses local BCrypt. IDP is NOT required or consulted.
    /// </summary>
    PortalManagement,

    /// <summary>
    /// External client API authentication (/api/v1/client/authenticate).
    /// Uses IDP when AUTH_USE_EXTERNAL_IDP = true for the tenant.
    /// </summary>
    ExternalApi,

    /// <summary>
    /// Internal administrative preview of the effective auth graph.
    /// Resolves the tenant's configured mode, but does not require an active
    /// Identity Provider to render the preview payload.
    /// </summary>
    InternalPreview,
}
