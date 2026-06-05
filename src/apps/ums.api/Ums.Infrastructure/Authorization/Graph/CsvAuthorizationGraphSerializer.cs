using System.Text;
using Ums.Application.Authorization.Graph.Serializers;
using Ums.Domain.Authorization.Graph;

namespace Ums.Infrastructure.Authorization.Graph;

/// <summary>
/// Serializes AuthorizationGraph to CSV.
/// Produces a flat table of permissions (one row per option/action).
/// The first block is menu permissions, the second is domain resource permissions.
/// </summary>
public sealed class CsvAuthorizationGraphSerializer : IAuthorizationGraphSerializer
{
    public string ContentType   => "text/csv";
    public string FileExtension => "csv";

    public string Serialize(AuthorizationGraph g, GraphSerializationOptions? options = null)
    {
        var opts = options ?? GraphSerializationOptions.Default;
        var sb   = new StringBuilder();

        sb.AppendLine($"# UMS Authorization Graph — {g.Context.Tenant.Code} / {g.Context.SystemSuite.Code} / {g.Context.Role.Code}");
        sb.AppendLine($"# Generated: {g.GeneratedAt:O}  ValidUntil: {g.ValidUntil:O}");
        sb.AppendLine($"# Auth: {g.Authentication.Method}  MFA: {g.Authentication.MfaRequired}");
        sb.AppendLine();

        sb.AppendLine("Section,ModuleCode,ModuleValue,MenuCode,MenuValue,SubMenuCode,SubMenuValue,OptionCode,OptionValue,ActionCode,Effect,Source");
        foreach (var module in g.MenuAccess)
        foreach (var menu in module.Menus)
        foreach (var sub in menu.SubMenus)
        foreach (var opt in sub.Options)
        {
            sb.AppendLine(string.Join(",",
                "Menu",
                Esc(module.Code), Esc(module.Name),
                Esc(menu.Code), Esc(menu.Label),
                Esc(sub.Code), Esc(sub.Label),
                Esc(opt.Code), Esc(opt.Label),
                Esc(opt.ActionCode),
                opt.Effect.ToString(),
                opt.Source.ToString()));
        }

        sb.AppendLine();

        sb.AppendLine("Section,ResourceType,ResourceCode,ResourceValue,ActionCode,ActionValue,Effect,Source");
        foreach (var res in g.DomainPermissions)
        foreach (var act in res.Actions)
        {
            sb.AppendLine(string.Join(",",
                "Domain",
                Esc(res.ResourceType), Esc(res.ResourceCode), Esc(res.ResourceName),
                Esc(act.ActionCode), Esc(act.ActionName),
                act.Effect.ToString(), act.Source.ToString()));
        }

        sb.AppendLine();

        sb.AppendLine("Section,FlagCode,IsEnabled,MatchedCriteria");
        foreach (var f in g.FeatureFlags)
        {
            sb.AppendLine(string.Join(",",
                "Feature",
                Esc(f.FlagCode),
                f.IsEnabled.ToString().ToLower(),
                Esc(f.MatchedCriteriaType ?? "")));
        }

        sb.AppendLine();

        sb.AppendLine("Section,Scope");
        foreach (var s in g.Scopes)
            sb.AppendLine($"Scope,{Esc(s)}");

        if (opts.IncludeTechnicalMetadata)
        {
            sb.AppendLine();
            sb.AppendLine("Section,TechnicalField,Value");
            sb.AppendLine($"Technical,UserId,{Esc(g.Context.User.Id.ToString())}");
            sb.AppendLine($"Technical,TenantId,{Esc(g.Context.Tenant.Id.ToString())}");
            sb.AppendLine($"Technical,SystemSuiteId,{Esc(g.Context.SystemSuite.Id.ToString())}");
            sb.AppendLine($"Technical,RoleId,{Esc(g.Context.Role.Id.ToString())}");
            if (g.Context.Branch is not null)
                sb.AppendLine($"Technical,BranchId,{Esc(g.Context.Branch.Id.ToString())}");
        }

        return sb.ToString();
    }

    private static string Esc(string? v)
    {
        if (v is null) return "";
        if (v.Contains(',') || v.Contains('"') || v.Contains('\n'))
            return $"\"{v.Replace("\"", "\"\"")}\"";
        return v;
    }
}
