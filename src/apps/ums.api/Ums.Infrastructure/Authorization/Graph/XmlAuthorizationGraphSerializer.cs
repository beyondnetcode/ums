using System.Text;
using System.Xml.Linq;
using Ums.Application.Authorization.Graph.Serializers;
using Ums.Domain.Authorization.Graph;

namespace Ums.Infrastructure.Authorization.Graph;

/// <summary>Serializes AuthorizationGraph to XML.</summary>
public sealed class XmlAuthorizationGraphSerializer : IAuthorizationGraphSerializer
{
    public string ContentType   => "application/xml";
    public string FileExtension => "xml";

    public string Serialize(AuthorizationGraph g, GraphSerializationOptions? options = null)
    {
        var opts = options ?? GraphSerializationOptions.Default;

        XElement? BuildBranch()
        {
            if (g.Context.Branch is null)
                return null;

            return new XElement("branch",
                opts.IncludeTechnicalMetadata ? new XAttribute("id", g.Context.Branch.Id) : null,
                new XAttribute("code", g.Context.Branch.Code),
                new XAttribute("value", g.Context.Branch.Name));
        }

        XElement? BuildProvider()
        {
            if (g.Authentication.Provider is null)
                return null;

            return new XElement("provider",
                opts.IncludeTechnicalMetadata ? new XAttribute("id", g.Authentication.Provider.Id) : null,
                new XAttribute("code", g.Authentication.Provider.Code),
                new XAttribute("name", g.Authentication.Provider.Name),
                new XAttribute("value", g.Authentication.Provider.Strategy));
        }

        XElement BuildContext()
            => new("context",
                new XElement("user",
                    opts.IncludeTechnicalMetadata ? new XAttribute("id", g.Context.User.Id) : null,
                    new XAttribute("email", g.Context.User.Email),
                    new XAttribute("username", g.Context.User.Username),
                    new XAttribute("value", g.Context.User.DisplayName),
                    new XAttribute("status", g.Context.User.Status)),
                new XElement("tenant",
                    opts.IncludeTechnicalMetadata ? new XAttribute("id", g.Context.Tenant.Id) : null,
                    new XAttribute("code", g.Context.Tenant.Code),
                    new XAttribute("value", g.Context.Tenant.Name),
                    new XAttribute("status", g.Context.Tenant.Status),
                    new XAttribute("isManagementOwner", g.Context.Tenant.IsManagementOwner)),
                new XElement("systemSuite",
                    opts.IncludeTechnicalMetadata ? new XAttribute("id", g.Context.SystemSuite.Id) : null,
                    new XAttribute("code", g.Context.SystemSuite.Code),
                    new XAttribute("value", g.Context.SystemSuite.Name),
                    new XAttribute("status", g.Context.SystemSuite.Status)),
                new XElement("role",
                    opts.IncludeTechnicalMetadata ? new XAttribute("id", g.Context.Role.Id) : null,
                    new XAttribute("code", g.Context.Role.Code),
                    new XAttribute("value", g.Context.Role.Name),
                    new XAttribute("level", g.Context.Role.HierarchyLevel)),
                new XElement("profile",
                    opts.IncludeTechnicalMetadata ? new XAttribute("id", g.Context.Profile.Id) : null,
                    new XAttribute("scope", g.Context.Profile.Scope),
                    new XAttribute("isActive", g.Context.Profile.IsActive)),
                BuildBranch());

        XElement BuildAuthentication()
            => new("authentication",
                new XAttribute("method", g.Authentication.Method),
                new XAttribute("mfaRequired", g.Authentication.MfaRequired),
                new XAttribute("issuedAt", g.Authentication.IssuedAt.ToString("O")),
                new XAttribute("sessionExpiresAt", g.Authentication.SessionExpiresAt.ToString("O")),
                BuildProvider());

        XElement BuildActions()
            => new("actions",
                g.Actions.Select(a => new XElement("action",
                    new XAttribute("code", a.Code),
                    new XAttribute("value", a.Name))));

        XElement BuildMenuAccess()
            => new("menuAccess",
                g.MenuAccess.Select(m => new XElement("module",
                    new XAttribute("code", m.Code),
                    new XAttribute("value", m.Name),
                    new XAttribute("status", m.Status),
                    m.Menus.Select(menu => new XElement("menu",
                        new XAttribute("code", menu.Code),
                        new XAttribute("value", menu.Label),
                        menu.SubMenus.Select(sub => new XElement("subMenu",
                            new XAttribute("code", sub.Code),
                            new XAttribute("value", sub.Label),
                            sub.Options.Select(o => new XElement("option",
                                new XAttribute("code", o.Code),
                                new XAttribute("value", o.Label),
                                new XAttribute("actionCode", o.ActionCode),
                                new XAttribute("effect", o.Effect.ToString()),
                                new XAttribute("source", o.Source.ToString()))))))))));

        XElement BuildDomainPermissions()
            => new("domainPermissions",
                g.DomainPermissions.Select(r => new XElement("resource",
                    opts.IncludeTechnicalMetadata ? new XAttribute("id", r.ResourceId) : null,
                    opts.IncludeTechnicalMetadata && r.ModuleId.HasValue ? new XAttribute("moduleId", r.ModuleId.Value) : null,
                    opts.IncludeTechnicalMetadata && r.ParentResourceId.HasValue ? new XAttribute("parentResourceId", r.ParentResourceId.Value) : null,
                    new XAttribute("type", r.ResourceType),
                    new XAttribute("code", r.ResourceCode),
                    new XAttribute("value", r.ResourceName),
                    r.Actions.Select(a => new XElement("action",
                        new XAttribute("code", a.ActionCode),
                        new XAttribute("value", a.ActionName),
                        new XAttribute("effect", a.Effect.ToString()),
                        new XAttribute("source", a.Source.ToString()))))));

        XElement BuildFeatureFlags()
            => new("featureFlags",
                g.FeatureFlags.Select(f => new XElement("flag",
                    new XAttribute("code", f.FlagCode),
                    new XAttribute("isEnabled", f.IsEnabled),
                    f.MatchedCriteriaType is null ? null
                        : new XAttribute("matchedCriteria", f.MatchedCriteriaType))));

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null),
            new XElement("authorizationGraph",
                new XAttribute("generatedAt", g.GeneratedAt.ToString("O")),
                new XAttribute("validUntil", g.ValidUntil.ToString("O")),
                BuildContext(),
                BuildAuthentication(),
                BuildActions(),
                BuildMenuAccess(),
                BuildDomainPermissions(),
                BuildFeatureFlags(),
                new XElement("effectiveConfig",
                    new XAttribute("sessionTimeoutMinutes", g.EffectiveConfig.SessionTimeoutMinutes),
                    new XAttribute("maxLoginAttempts", g.EffectiveConfig.MaxLoginAttempts),
                    new XAttribute("mfaRequiredForAdmin", g.EffectiveConfig.MfaRequiredForAdmin),
                    new XAttribute("mfaAllowedMethods", string.Join(",", g.EffectiveConfig.MfaAllowedMethods))),
                new XElement("scopes",
                    g.Scopes.Select(s => new XElement("scope", s)))));

        var sb = new StringBuilder();
        doc.Save(new System.IO.StringWriter(sb));
        return sb.ToString();
    }
}
