namespace Ums.Sdk.Authorization.AspNetCore;

/// <summary>Behavior knobs for <see cref="UmsAuthGraphMiddleware"/>.</summary>
public sealed class UmsAuthGraphMiddlewareOptions
{
    /// <summary>JWT body claim that carries the serialized graph. Default: <c>"graph"</c>.</summary>
    public string JwtBodyClaim { get; set; } = "graph";

    /// <summary>When true, the middleware returns 401 + AUTH_201 when the bound graph is expired.</summary>
    public bool RejectExpiredGraphs { get; set; } = false;

    /// <summary>
    /// When true, the middleware returns 401 + AUTH_204/AUTH_205 on missing or incompatible
    /// schemaVersion. When false (default), the graph is silently dropped and the request
    /// proceeds without authorization context — downstream code will see <c>Current = null</c>.
    /// </summary>
    public bool RejectIncompatibleGraphs { get; set; } = true;
}
