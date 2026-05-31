using Ums.Sdk.Contracts;

namespace Ums.Sdk.Authorization;

/// <summary>
/// Reference implementation of <see cref="IAuthorizationValidator"/>.
/// Enforces the rules documented in <c>docs/domain/identity/auth-graph.md §3</c>:
/// Override over Template, Deny over Allow, NotGranted on missing entries, Expired on past validUntil,
/// SchemaUnsupported on MAJOR mismatch, SchemaMissing when schemaVersion is absent.
/// </summary>
public sealed class AuthorizationValidator : IAuthorizationValidator
{
    private const string PrimScope    = "RequiresScope";
    private const string PrimMenu     = "RequiresMenuOption";
    private const string PrimDomain   = "RequiresDomainAccess";
    private const string PrimFlag     = "RequiresFeatureFlag";

    public AuthorizationDecision RequireScope(AuthorizationGraph? graph, string scope)
    {
        var pre = PreCheck(graph, PrimScope, scope);
        if (pre is not null) return pre;

        // Deny precedence: a deny anywhere in menuAccess or domainPermissions matching this scope
        // beats an Allow. The scope is reconstructed as "{code}.{actionCode}" — we check both surfaces.
        var (resource, action) = ParseScope(scope);
        if (resource is null || action is null)
        {
            return AuthorizationDecision.NotGranted(PrimScope, scope,
                UmsErrorCodes.ScopeNotGranted,
                $"Scope '{scope}' is not in the canonical 'RESOURCE.ACTION' format.",
                graphRequestId: null, validUntil: graph!.ValidUntil);
        }

        if (IsDeniedInGraph(graph!, resource, action))
        {
            return AuthorizationDecision.Deny(PrimScope, scope,
                UmsErrorCodes.ScopeDenied,
                $"Scope '{scope}' is explicitly denied in the authorization graph.",
                graphRequestId: null, validUntil: graph!.ValidUntil);
        }

        return graph!.Scopes.Contains(scope, StringComparer.Ordinal)
            ? AuthorizationDecision.Granted(PrimScope, scope, graphRequestId: null, validUntil: graph.ValidUntil)
            : AuthorizationDecision.NotGranted(PrimScope, scope,
                UmsErrorCodes.ScopeNotGranted,
                $"Scope '{scope}' is not present in the authorization graph.",
                graphRequestId: null, validUntil: graph.ValidUntil);
    }

    public AuthorizationDecision RequireMenuOption(AuthorizationGraph? graph, string optionCode)
    {
        var pre = PreCheck(graph, PrimMenu, optionCode);
        if (pre is not null) return pre;

        foreach (var module in graph!.MenuAccess)
        foreach (var menu in module.Menus)
        foreach (var sub in menu.SubMenus)
        foreach (var opt in sub.Options)
        {
            if (!string.Equals(opt.Code, optionCode, StringComparison.Ordinal)) continue;
            return opt.Effect switch
            {
                AccessEffect.Allow => AuthorizationDecision.Granted(PrimMenu, optionCode,
                    validUntil: graph.ValidUntil),
                AccessEffect.Deny => AuthorizationDecision.Deny(PrimMenu, optionCode,
                    UmsErrorCodes.MenuOptionDenied,
                    $"Menu option '{optionCode}' is explicitly denied (source: {opt.Source}).",
                    validUntil: graph.ValidUntil),
                _ => AuthorizationDecision.NotGranted(PrimMenu, optionCode,
                    UmsErrorCodes.MenuOptionNotGranted,
                    $"Menu option '{optionCode}' resolves to NotGranted.",
                    validUntil: graph.ValidUntil)
            };
        }

        return AuthorizationDecision.NotGranted(PrimMenu, optionCode,
            UmsErrorCodes.MenuOptionNotGranted,
            $"Menu option '{optionCode}' is not present in the authorization graph.",
            validUntil: graph.ValidUntil);
    }

    public AuthorizationDecision RequireDomainAccess(AuthorizationGraph? graph, string resourceCode, string actionCode)
    {
        var target = $"{resourceCode}.{actionCode}";
        var pre = PreCheck(graph, PrimDomain, target);
        if (pre is not null) return pre;

        foreach (var dr in graph!.DomainPermissions)
        {
            if (!string.Equals(dr.Resource.Code, resourceCode, StringComparison.Ordinal)) continue;
            foreach (var act in dr.Actions)
            {
                if (!string.Equals(act.ActionCode, actionCode, StringComparison.Ordinal)) continue;
                return act.Effect switch
                {
                    AccessEffect.Allow => AuthorizationDecision.Granted(PrimDomain, target,
                        validUntil: graph.ValidUntil),
                    AccessEffect.Deny => AuthorizationDecision.Deny(PrimDomain, target,
                        UmsErrorCodes.DomainAccessDenied,
                        $"Domain access '{target}' is explicitly denied (source: {act.Source}).",
                        validUntil: graph.ValidUntil),
                    _ => AuthorizationDecision.NotGranted(PrimDomain, target,
                        UmsErrorCodes.DomainAccessNotGranted,
                        $"Domain access '{target}' resolves to NotGranted.",
                        validUntil: graph.ValidUntil)
                };
            }
        }

        return AuthorizationDecision.NotGranted(PrimDomain, target,
            UmsErrorCodes.DomainAccessNotGranted,
            $"Domain access '{target}' is not present in the authorization graph.",
            validUntil: graph.ValidUntil);
    }

    public AuthorizationDecision RequireFeatureFlag(AuthorizationGraph? graph, string flagCode)
    {
        var pre = PreCheck(graph, PrimFlag, flagCode);
        if (pre is not null) return pre;

        var flag = graph!.FeatureFlags.FirstOrDefault(
            f => string.Equals(f.FlagCode, flagCode, StringComparison.Ordinal));

        if (flag is null)
        {
            return AuthorizationDecision.NotGranted(PrimFlag, flagCode,
                UmsErrorCodes.FeatureFlagNotFound,
                $"Feature flag '{flagCode}' is not present in the authorization graph.",
                validUntil: graph.ValidUntil);
        }

        return flag.IsEnabled
            ? AuthorizationDecision.Granted(PrimFlag, flagCode, validUntil: graph.ValidUntil)
            : AuthorizationDecision.NotGranted(PrimFlag, flagCode,
                UmsErrorCodes.FeatureFlagDisabled,
                $"Feature flag '{flagCode}' is present but isEnabled is false.",
                validUntil: graph.ValidUntil);
    }

    public AuthorizationDecision AssertTenant(AuthorizationGraph? graph, string expectedTenantCode)
    {
        var pre = PreCheck(graph, "AssertTenant", expectedTenantCode);
        if (pre is not null) return pre;

        return string.Equals(graph!.Context.Tenant.Code, expectedTenantCode, StringComparison.Ordinal)
            ? AuthorizationDecision.Granted("AssertTenant", expectedTenantCode, validUntil: graph.ValidUntil)
            : AuthorizationDecision.TenantMismatch(expectedTenantCode, graph.Context.Tenant.Code);
    }

    // --- Helpers ---------------------------------------------------------

    private static AuthorizationDecision? PreCheck(AuthorizationGraph? graph, string primitive, string target)
    {
        if (graph is null)
            return AuthorizationDecision.GraphMissing(primitive, target);
        if (string.IsNullOrWhiteSpace(graph.SchemaVersion))
            return AuthorizationDecision.SchemaMissing(primitive, target);
        if (!SchemaVersion.IsSupported(graph.SchemaVersion))
            return AuthorizationDecision.SchemaUnsupported(primitive, target, graph.SchemaVersion);
        if (graph.ValidUntil <= DateTimeOffset.UtcNow)
            return AuthorizationDecision.Expired(primitive, target, graph.ValidUntil);
        return null;
    }

    private static (string? Resource, string? Action) ParseScope(string scope)
    {
        var idx = scope.IndexOf('.');
        if (idx <= 0 || idx == scope.Length - 1) return (null, null);
        return (scope[..idx], scope[(idx + 1)..]);
    }

    private static bool IsDeniedInGraph(AuthorizationGraph graph, string resourceCode, string actionCode)
    {
        // Probe menuAccess: any option whose code equals the resource (UI surface) AND actionCode matches.
        foreach (var module in graph.MenuAccess)
        foreach (var menu in module.Menus)
        foreach (var sub in menu.SubMenus)
        foreach (var opt in sub.Options)
        {
            if (opt.Effect != AccessEffect.Deny) continue;
            if (string.Equals(opt.Code, resourceCode, StringComparison.Ordinal) &&
                string.Equals(opt.ActionCode, actionCode, StringComparison.Ordinal))
                return true;
        }

        // Probe domainPermissions.
        foreach (var dr in graph.DomainPermissions)
        {
            if (!string.Equals(dr.Resource.Code, resourceCode, StringComparison.Ordinal)) continue;
            foreach (var act in dr.Actions)
            {
                if (act.Effect == AccessEffect.Deny &&
                    string.Equals(act.ActionCode, actionCode, StringComparison.Ordinal))
                    return true;
            }
        }

        return false;
    }
}
