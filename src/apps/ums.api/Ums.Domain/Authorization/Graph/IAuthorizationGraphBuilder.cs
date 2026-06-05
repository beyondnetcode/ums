using Ums.Domain.Identity.Auth;
using UserAccountAggregate = Ums.Domain.Identity.UserAccount.UserAccount;

namespace Ums.Domain.Authorization.Graph;

/// <summary>
/// Port for building the complete AuthorizationGraph for an authenticated user.
///
/// The command handler that resolves credentials passes the already-fetched
/// UserAccount so the builder does not perform a redundant DB lookup.
/// </summary>
public interface IAuthorizationGraphBuilder
{
    /// <summary>
    /// Builds the full authorization graph for the given user and tenant.
    /// </summary>
    /// <param name="userAccount">The authenticated user — already fetched by the command handler.</param>
    /// <param name="tenantId">The tenant in which the user authenticated.</param>
    /// <param name="authMethod">The resolved auth method (Local or IDP).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Result<AuthorizationGraph>> BuildAsync(
        UserAccountAggregate userAccount,
        Guid                 tenantId,
        AuthMethod           authMethod,
        CancellationToken    cancellationToken = default);

    /// <summary>
    /// Builds the authorization graph for a specific profile of the given user and tenant.
    /// Used by the admin preview flow to avoid resolving a different active profile.
    /// </summary>
    Task<Result<AuthorizationGraph>> BuildForProfileAsync(
        UserAccountAggregate userAccount,
        Guid                 tenantId,
        Guid                 profileId,
        AuthMethod           authMethod,
        CancellationToken    cancellationToken = default);
}
