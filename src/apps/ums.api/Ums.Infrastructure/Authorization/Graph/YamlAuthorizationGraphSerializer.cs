using System.Text;
using Ums.Application.Authorization.Graph.Serializers;
using Ums.Domain.Authorization.Graph;

namespace Ums.Infrastructure.Authorization.Graph;

/// <summary>Serializes AuthorizationGraph to YAML.</summary>
public sealed class YamlAuthorizationGraphSerializer : IAuthorizationGraphSerializer
{
    public string ContentType   => "application/x-yaml";
    public string FileExtension => "yaml";

    public string Serialize(AuthorizationGraph g, GraphSerializationOptions? options = null)
    {
        var opts = options ?? GraphSerializationOptions.Default;
        var sb   = new StringBuilder();

        sb.AppendLine("# UMS Authorization Graph");
        sb.AppendLine($"# Generated: {g.GeneratedAt:O}");
        sb.AppendLine($"# Valid Until: {g.ValidUntil:O}");
        sb.AppendLine();

        sb.AppendLine("context:");
        sb.AppendLine("  user:");
        if (opts.IncludeTechnicalMetadata) sb.AppendLine($"    id: {g.Context.User.Id}");
        sb.AppendLine($"    email: {g.Context.User.Email}");
        sb.AppendLine($"    username: {g.Context.User.Username}");
        sb.AppendLine($"    value: {g.Context.User.DisplayName}");
        sb.AppendLine($"    status: {g.Context.User.Status}");
        sb.AppendLine("  tenant:");
        if (opts.IncludeTechnicalMetadata) sb.AppendLine($"    id: {g.Context.Tenant.Id}");
        sb.AppendLine($"    code: {g.Context.Tenant.Code}");
        sb.AppendLine($"    value: {g.Context.Tenant.Name}");
        sb.AppendLine($"    status: {g.Context.Tenant.Status}");
        sb.AppendLine($"    isManagementOwner: {g.Context.Tenant.IsManagementOwner.ToString().ToLower()}");
        sb.AppendLine("  systemSuite:");
        if (opts.IncludeTechnicalMetadata) sb.AppendLine($"    id: {g.Context.SystemSuite.Id}");
        sb.AppendLine($"    code: {g.Context.SystemSuite.Code}");
        sb.AppendLine($"    value: {g.Context.SystemSuite.Name}");
        sb.AppendLine($"    status: {g.Context.SystemSuite.Status}");
        sb.AppendLine("  role:");
        if (opts.IncludeTechnicalMetadata) sb.AppendLine($"    id: {g.Context.Role.Id}");
        sb.AppendLine($"    code: {g.Context.Role.Code}");
        sb.AppendLine($"    value: {g.Context.Role.Name}");
        sb.AppendLine($"    hierarchyLevel: {g.Context.Role.HierarchyLevel}");
        sb.AppendLine("  profile:");
        if (opts.IncludeTechnicalMetadata) sb.AppendLine($"    id: {g.Context.Profile.Id}");
        sb.AppendLine($"    scope: {g.Context.Profile.Scope}");
        sb.AppendLine($"    isActive: {g.Context.Profile.IsActive.ToString().ToLower()}");
        if (g.Context.Branch is not null)
        {
            sb.AppendLine("  branch:");
            if (opts.IncludeTechnicalMetadata) sb.AppendLine($"    id: {g.Context.Branch.Id}");
            sb.AppendLine($"    code: {g.Context.Branch.Code}");
            sb.AppendLine($"    value: {g.Context.Branch.Name}");
        }

        sb.AppendLine();
        sb.AppendLine("authentication:");
        sb.AppendLine($"  method: {g.Authentication.Method}");
        sb.AppendLine($"  mfaRequired: {g.Authentication.MfaRequired.ToString().ToLower()}");
        sb.AppendLine($"  issuedAt: {g.Authentication.IssuedAt:O}");
        sb.AppendLine($"  sessionExpiresAt: {g.Authentication.SessionExpiresAt:O}");
        if (g.Authentication.Provider is not null)
        {
            sb.AppendLine("  provider:");
            if (opts.IncludeTechnicalMetadata) sb.AppendLine($"    id: {g.Authentication.Provider.Id}");
            sb.AppendLine($"    code: {g.Authentication.Provider.Code}");
            sb.AppendLine($"    name: {g.Authentication.Provider.Name}");
            sb.AppendLine($"    value: {g.Authentication.Provider.Strategy}");
        }

        sb.AppendLine();
        sb.AppendLine("actions:");
        foreach (var a in g.Actions)
        {
            sb.AppendLine($"  - code: {a.Code}");
            sb.AppendLine($"    value: {a.Name}");
        }

        sb.AppendLine();
        sb.AppendLine("menuAccess:");
        foreach (var module in g.MenuAccess)
        {
            sb.AppendLine($"  - code: {module.Code}");
            sb.AppendLine($"    value: {module.Name}");
            sb.AppendLine($"    status: {module.Status}");
            sb.AppendLine("    menus:");
            foreach (var menu in module.Menus)
            {
                sb.AppendLine($"      - code: {menu.Code}");
                sb.AppendLine($"        value: {menu.Label}");
                sb.AppendLine("        subMenus:");
                foreach (var sub in menu.SubMenus)
                {
                    sb.AppendLine($"          - code: {sub.Code}");
                    sb.AppendLine($"            value: {sub.Label}");
                    sb.AppendLine("            options:");
                    foreach (var opt in sub.Options)
                    {
                        sb.AppendLine($"              - code: {opt.Code}");
                        sb.AppendLine($"                value: {opt.Label}");
                        sb.AppendLine($"                actionCode: {opt.ActionCode}");
                        sb.AppendLine($"                effect: {opt.Effect}");
                        sb.AppendLine($"                source: {opt.Source}");
                    }
                }
            }
        }

        sb.AppendLine();
        sb.AppendLine("domainPermissions:");
        foreach (var res in g.DomainPermissions)
        {
            sb.AppendLine("  -");
            if (opts.IncludeTechnicalMetadata) sb.AppendLine($"    id: {res.ResourceId}");
            if (opts.IncludeTechnicalMetadata && res.ModuleId.HasValue)
                sb.AppendLine($"    moduleId: {res.ModuleId.Value}");
            if (opts.IncludeTechnicalMetadata && res.ParentResourceId.HasValue)
                sb.AppendLine($"    parentResourceId: {res.ParentResourceId.Value}");
            sb.AppendLine($"    type: {res.ResourceType}");
            sb.AppendLine($"    code: {res.ResourceCode}");
            sb.AppendLine($"    value: {res.ResourceName}");
            sb.AppendLine("    actions:");
            foreach (var act in res.Actions)
            {
                sb.AppendLine("      -");
                if (opts.IncludeTechnicalMetadata) sb.AppendLine($"        id: {act.ActionId}");
                sb.AppendLine($"        code: {act.ActionCode}");
                sb.AppendLine($"        value: {act.ActionName}");
                sb.AppendLine($"        effect: {act.Effect}");
                sb.AppendLine($"        source: {act.Source}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("featureFlags:");
        foreach (var f in g.FeatureFlags)
        {
            sb.AppendLine("  -");
            sb.AppendLine($"    code: {f.FlagCode}");
            sb.AppendLine($"    isEnabled: {f.IsEnabled.ToString().ToLower()}");
            if (!string.IsNullOrWhiteSpace(f.MatchedCriteriaType))
                sb.AppendLine($"    matchedCriteria: {f.MatchedCriteriaType}");
        }

        sb.AppendLine();
        sb.AppendLine("effectiveConfig:");
        sb.AppendLine($"  sessionTimeoutMinutes: {g.EffectiveConfig.SessionTimeoutMinutes}");
        sb.AppendLine($"  maxLoginAttempts: {g.EffectiveConfig.MaxLoginAttempts}");
        sb.AppendLine($"  mfaRequiredForAdmin: {g.EffectiveConfig.MfaRequiredForAdmin.ToString().ToLower()}");
        sb.AppendLine($"  mfaAllowedMethods: [{string.Join(", ", g.EffectiveConfig.MfaAllowedMethods)}]");

        sb.AppendLine();
        sb.AppendLine("scopes:");
        foreach (var scope in g.Scopes)
            sb.AppendLine($"  - {scope}");

        return sb.ToString();
    }
}
