namespace Ums.Domain.Authorization.Graph;

/// <summary>
/// The complete, immutable authorization graph for an authenticated user.
///
/// This is the core output of the UMS authentication engine. After a user
/// authenticates (Local BCrypt or external IDP), UMS resolves their tenant,
/// system suite, role, branch, and profile, then builds this graph containing
/// every permission, menu access node, domain resource authorization,
/// feature flag evaluation, and effective configuration value needed by the
/// client system to operate without re-querying UMS on subsequent requests.
///
/// The graph is intentionally self-contained and stateless — clients cache it
/// for the duration of the session (until ValidUntil) and use it to make
/// authorization decisions locally.
/// </summary>
public sealed record AuthorizationGraph
{
    /// <summary>
    /// Schema version of this payload. Governed by ADR-0074 and aligned with the canonical
    /// `auth-graph.schema.json` in <c>src/libs/sdk/contracts/</c>. Defaults to the value
    /// constant declared by <c>Ums.Sdk.Contracts.SchemaVersion.Current</c>.
    /// </summary>
    public string SchemaVersion { get; init; } = global::Ums.Sdk.Contracts.SchemaVersion.Current;

    /// <summary>Who is authenticated and in which context (tenant, suite, role, branch).</summary>
    public GraphContext Context { get; init; }

    /// <summary>How the user authenticated and session timing.</summary>
    public GraphAuthentication Authentication { get; init; }

    /// <summary>All actions registered in the SystemSuite — the full action catalogue.</summary>
    public IReadOnlyList<GraphAction> Actions { get; init; }

    /// <summary>
    /// Module → Menu → SubMenu → Option tree with resolved permission per option.
    /// Only modules that have at least one reachable (Allow) option are included.
    /// Options with Deny or NotGranted are included so the client can render them
    /// as disabled/hidden according to its own UX policy.
    /// </summary>
    public IReadOnlyList<GraphMenuModule> MenuAccess { get; init; }

    /// <summary>
    /// Domain resources (Aggregates, Entities) with resolved authorization per action.
    /// Covers fine-grained backend access control beyond UI menus.
    /// </summary>
    public IReadOnlyList<GraphDomainPermission> DomainPermissions { get; init; }

    /// <summary>Feature flags evaluated against this user's context at auth time.</summary>
    public IReadOnlyList<GraphFeatureFlag> FeatureFlags { get; init; }

    /// <summary>Effective tenant configuration resolved with tenant-level override precedence.</summary>
    public GraphEffectiveConfig EffectiveConfig { get; init; }

    /// <summary>
    /// OAuth2-style scopes derived from all Allow permissions.
    /// Format: "resourceCode.actionCode" (lowercase), e.g. "users.read", "inventory.write".
    /// Useful for downstream services implementing OAuth2 bearer token validation.
    /// </summary>
    public IReadOnlyList<string> Scopes { get; init; }

    /// <summary>UTC timestamp when this graph was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>
    /// UTC timestamp when this graph expires (= GeneratedAt + SessionTimeoutMinutes).
    /// Clients should re-authenticate after this point.
    /// </summary>
    public DateTime ValidUntil { get; init; }

    private AuthorizationGraph() { }

    /// <summary>Factory — builds the graph from its constituent parts.</summary>
    public static AuthorizationGraph Build(
        GraphContext                       context,
        GraphAuthentication                authentication,
        IReadOnlyList<GraphAction>         actions,
        IReadOnlyList<GraphMenuModule>     menuAccess,
        IReadOnlyList<GraphDomainPermission> domainPermissions,
        IReadOnlyList<GraphFeatureFlag>    featureFlags,
        GraphEffectiveConfig               effectiveConfig,
        IReadOnlyList<string>              scopes,
        DateTime                           generatedAt)
    {
        var validUntil = generatedAt.AddMinutes(effectiveConfig.SessionTimeoutMinutes);

        return new AuthorizationGraph
        {
            SchemaVersion     = global::Ums.Sdk.Contracts.SchemaVersion.Current,
            Context           = context,
            Authentication    = authentication,
            Actions           = actions,
            MenuAccess        = menuAccess,
            DomainPermissions = domainPermissions,
            FeatureFlags      = featureFlags,
            EffectiveConfig   = effectiveConfig,
            Scopes            = scopes,
            GeneratedAt       = generatedAt,
            ValidUntil        = validUntil,
        };
    }
}
