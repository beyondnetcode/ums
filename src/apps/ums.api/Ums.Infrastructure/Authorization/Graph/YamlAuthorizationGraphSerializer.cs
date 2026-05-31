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

        // Context
        sb.AppendLine("context:");
        sb.AppendLine($"  user:");
        sb.AppendLine($"    email: {g.Context.User.Email}");
        sb.AppendLine($"    username: {g.Context.User.Username}");
        sb.AppendLine($"    status: {g.Context.User.Status}");
        sb.AppendLine($"  tenant:");
        sb.AppendLine($"    code: {g.Context.Tenant.Code}");
        sb.AppendLine($"    name: {g.Context.Tenant.Name}");
        sb.AppendLine($"  systemSuite:");
        sb.AppendLine($"    code: {g.Context.SystemSuite.Code}");
        sb.AppendLine($"    name: {g.Context.SystemSuite.Name}");
        sb.AppendLine($"  role:");
        sb.AppendLine($"    code: {g.Context.Role.Code}");
        sb.AppendLine($"    name: {g.Context.Role.Name}");
        sb.AppendLine($"    hierarchyLevel: {g.Context.Role.HierarchyLevel}");
        if (g.Context.Branch is not null)
        {
            sb.AppendLine($"  branch:");
            sb.AppendLine($"    code: {g.Context.Branch.Code}");
            sb.AppendLine($"    name: {g.Context.Branch.Name}");
        }

        // Authentication
        sb.AppendLine();
        sb.AppendLine("authentication:");
        sb.AppendLine($"  method: {g.Authentication.Method}");
        sb.AppendLine($"  mfaRequired: {g.Authentication.MfaRequired.ToString().ToLower()}");
        sb.AppendLine($"  issuedAt: {g.Authentication.IssuedAt:O}");
        sb.AppendLine($"  sessionExpiresAt: {g.Authentication.SessionExpiresAt:O}");
        if (g.Authentication.Provider is not null)
        {
            sb.AppendLine($"  provider:");
            sb.AppendLine($"    name: {g.Authentication.Provider.Name}");
            sb.AppendLine($"    strategy: {g.Authentication.Provider.Strategy}");
        }

        // Actions
        sb.AppendLine();
        sb.AppendLine("actions:");
        foreach (var a in g.Actions)
            sb.AppendLine($"  - code: {a.Code}");

        // Menu Access
        sb.AppendLine();
        sb.AppendLine("menuAccess:");
        foreach (var module in g.MenuAccess)
        {
            sb.AppendLine($"  - module: {module.Code}");
            foreach (var menu in module.Menus)
            {
                sb.AppendLine($"    - menu: {menu.Code}");
                foreach (var sub in menu.SubMenus)
                {
                    sb.AppendLine($"      - subMenu: {sub.Code}");
                    foreach (var opt in sub.Options)
                        sb.AppendLine($"        - option: {opt.Code} actionCode: {opt.ActionCode} effect: {opt.Effect}");
                }
            }
        }

        // Domain Permissions
        sb.AppendLine();
        sb.AppendLine("domainPermissions:");
        foreach (var res in g.DomainPermissions)
        {
            sb.AppendLine($"  - resource: {res.ResourceCode} type: {res.ResourceType}");
            foreach (var act in res.Actions)
                sb.AppendLine($"    - action: {act.ActionCode} effect: {act.Effect}");
        }

        // Feature Flags
        sb.AppendLine();
        sb.AppendLine("featureFlags:");
        foreach (var f in g.FeatureFlags)
            sb.AppendLine($"  - flagCode: {f.FlagCode} isEnabled: {f.IsEnabled.ToString().ToLower()}");

        // Effective Config
        sb.AppendLine();
        sb.AppendLine("effectiveConfig:");
        sb.AppendLine($"  sessionTimeoutMinutes: {g.EffectiveConfig.SessionTimeoutMinutes}");
        sb.AppendLine($"  maxLoginAttempts: {g.EffectiveConfig.MaxLoginAttempts}");
        sb.AppendLine($"  mfaRequiredForAdmin: {g.EffectiveConfig.MfaRequiredForAdmin.ToString().ToLower()}");

        // Scopes
        sb.AppendLine();
        sb.AppendLine("scopes:");
        foreach (var scope in g.Scopes)
            sb.AppendLine($"  - {scope}");

        return sb.ToString();
    }
}
