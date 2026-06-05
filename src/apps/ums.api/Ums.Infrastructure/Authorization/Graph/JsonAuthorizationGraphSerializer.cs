using System.Text.Json;
using System.Text.Json.Serialization;
using Ums.Application.Authorization.Graph.Serializers;
using Ums.Domain.Authorization.Graph;

namespace Ums.Infrastructure.Authorization.Graph;

/// <summary>Serializes AuthorizationGraph to JSON (default format).</summary>
public sealed class JsonAuthorizationGraphSerializer : IAuthorizationGraphSerializer
{
    public string ContentType   => "application/json";
    public string FileExtension => "json";

    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented        = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters           = { new JsonStringEnumConverter() },
    };

    public string Serialize(AuthorizationGraph graph, GraphSerializationOptions? options = null)
    {
        var opts = options ?? GraphSerializationOptions.Default;
        var model = BuildModel(graph, opts);
        return JsonSerializer.Serialize(model, _options);
    }

    private static object BuildModel(AuthorizationGraph g, GraphSerializationOptions opts)
    {
        var includeMeta = opts.IncludeTechnicalMetadata;

        return new
        {
            context = new
            {
                user = new
                {
                    id = includeMeta ? g.Context.User.Id.ToString() : null,
                    g.Context.User.Email,
                    g.Context.User.Username,
                    value = g.Context.User.DisplayName,
                    g.Context.User.Status,
                },
                tenant = new
                {
                    id = includeMeta ? g.Context.Tenant.Id.ToString() : null,
                    g.Context.Tenant.Code,
                    value = g.Context.Tenant.Name,
                    g.Context.Tenant.Status,
                    g.Context.Tenant.IsManagementOwner,
                },
                systemSuite = new
                {
                    id = includeMeta ? g.Context.SystemSuite.Id.ToString() : null,
                    g.Context.SystemSuite.Code,
                    value = g.Context.SystemSuite.Name,
                    g.Context.SystemSuite.Status,
                },
                role = new
                {
                    id = includeMeta ? g.Context.Role.Id.ToString() : null,
                    g.Context.Role.Code,
                    value = g.Context.Role.Name,
                    g.Context.Role.HierarchyLevel,
                },
                profile = new
                {
                    id = includeMeta ? g.Context.Profile.Id.ToString() : null,
                    g.Context.Profile.Scope,
                    g.Context.Profile.IsActive,
                },
                branch = g.Context.Branch is null ? null : new
                {
                    id = includeMeta ? g.Context.Branch.Id.ToString() : null,
                    g.Context.Branch.Code,
                    value = g.Context.Branch.Name,
                },
            },
            authentication = new
            {
                method           = g.Authentication.Method,
                provider         = g.Authentication.Provider is null ? null : new
                {
                    id = includeMeta ? g.Authentication.Provider.Id.ToString() : null,
                    g.Authentication.Provider.Name,
                    g.Authentication.Provider.Code,
                    value = g.Authentication.Provider.Strategy,
                },
                mfaRequired      = g.Authentication.MfaRequired,
                issuedAt         = g.Authentication.IssuedAt.ToString("O"),
                sessionExpiresAt = g.Authentication.SessionExpiresAt.ToString("O"),
            },
            actions  = g.Actions.Select(a => new { a.Code, value = a.Name }),
            menuAccess = g.MenuAccess.Select(m => new
            {
                m.Code, value = m.Name, m.Status,
                menus = m.Menus.Select(menu => new
                {
                    menu.Code, value = menu.Label,
                    subMenus = menu.SubMenus.Select(sub => new
                    {
                        sub.Code, value = sub.Label,
                        options = sub.Options.Select(o => new
                        {
                            o.Code, value = o.Label, o.ActionCode,
                            effect = o.Effect.ToString(),
                            source = o.Source.ToString(),
                        })
                    })
                })
            }),
            domainPermissions = g.DomainPermissions.Select(r => new
            {
                resourceId = includeMeta ? r.ResourceId.ToString() : null,
                r.ResourceType,
                code = r.ResourceCode,
                value = r.ResourceName,
                moduleId = includeMeta && r.ModuleId.HasValue ? r.ModuleId.Value.ToString() : null,
                parentResourceId = includeMeta && r.ParentResourceId.HasValue ? r.ParentResourceId.Value.ToString() : null,
                actions = r.Actions.Select(a => new
                {
                    code = a.ActionCode,
                    value = a.ActionName,
                    effect = a.Effect.ToString(),
                    source = a.Source.ToString(),
                })
            }),
            featureFlags = g.FeatureFlags.Select(f => new { code = f.FlagCode, isEnabled = f.IsEnabled, f.MatchedCriteriaType }),
            effectiveConfig = new
            {
                g.EffectiveConfig.SessionTimeoutMinutes,
                g.EffectiveConfig.MaxLoginAttempts,
                g.EffectiveConfig.MinPasswordLength,
                g.EffectiveConfig.MfaRequiredForAdmin,
                g.EffectiveConfig.AuthUseExternalIdp,
            },
            scopes     = g.Scopes,
            generatedAt = g.GeneratedAt.ToString("O"),
            validUntil  = g.ValidUntil.ToString("O"),
        };
    }
}
