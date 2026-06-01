using Ums.Sdk.Authorization;

namespace Ums.Sdk.Client;

/// <summary>HTTP client for the UMS authentication surface.</summary>
public interface IUmsAuthClient
{
    Task<Result<ClientAuthResult>> AuthenticateAsync(ClientAuthRequest request, CancellationToken ct = default);
}
