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

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null),
            new XElement("authorizationGraph",
                new XAttribute("generatedAt", g.GeneratedAt.ToString("O")),
                new XAttribute("validUntil",  g.ValidUntil.ToString("O")),

                new XElement("context",
                    new XElement("user",
                        opts.IncludeTechnicalMetadata ? new XAttribute("id", g.Context.User.Id) : null,
                        new XAttribute("email",    g.Context.User.Email),
                        new XAttribute("username", g.Context.User.Username)),
                    new XElement("tenant",
                        opts.IncludeTechnicalMetadata ? new XAttribute("id", g.Context.Tenant.Id) : null,
                        new XAttribute("code", g.Context.Tenant.Code),
                        new XAttribute("name", g.Context.Tenant.Name)),
                    new XElement("systemSuite",
                        new XAttribute("code", g.Context.SystemSuite.Code),
                        new XAttribute("name", g.Context.SystemSuite.Name)),
                    new XElement("role",
                        new XAttribute("code", g.Context.Role.Code),
                        new XAttribute("name", g.Context.Role.Name),
                        new XAttribute("level", g.Context.Role.HierarchyLevel)),
                    g.Context.Branch is null ? null
                        : new XElement("branch",
                            new XAttribute("code", g.Context.Branch.Code),
                            new XAttribute("name", g.Context.Branch.Name))),

                new XElement("authentication",
                    new XAttribute("method",           g.Authentication.Method),
                    new XAttribute("mfaRequired",      g.Authentication.MfaRequired),
                    new XAttribute("issuedAt",         g.Authentication.IssuedAt.ToString("O")),
                    new XAttribute("sessionExpiresAt", g.Authentication.SessionExpiresAt.ToString("O")),
                    g.Authentication.Provider is null ? null
                        : new XElement("provider",
                            new XAttribute("name",     g.Authentication.Provider.Name),
                            new XAttribute("strategy", g.Authentication.Provider.Strategy))),

                new XElement("actions",
                    g.Actions.Select(a => new XElement("action",
                        new XAttribute("code", a.Code),
                        new XAttribute("name", a.Name)))),

                new XElement("menuAccess",
                    g.MenuAccess.Select(m => new XElement("module",
                        new XAttribute("code",   m.Code),
                        new XAttribute("name",   m.Name),
                        m.Menus.Select(menu => new XElement("menu",
                            new XAttribute("code",  menu.Code),
                            new XAttribute("label", menu.Label),
                            menu.SubMenus.Select(sub => new XElement("subMenu",
                                new XAttribute("code",  sub.Code),
                                new XAttribute("label", sub.Label),
                                sub.Options.Select(o => new XElement("option",
                                    new XAttribute("code",       o.Code),
                                    new XAttribute("label",      o.Label),
                                    new XAttribute("actionCode", o.ActionCode),
                                    new XAttribute("effect",     o.Effect.ToString()),
                                    new XAttribute("source",     o.Source.ToString())))))))))),

                new XElement("domainPermissions",
                    g.DomainPermissions.Select(r => new XElement("resource",
                        new XAttribute("type", r.ResourceType),
                        new XAttribute("code", r.ResourceCode),
                        new XAttribute("name", r.ResourceName),
                        r.Actions.Select(a => new XElement("action",
                            new XAttribute("code",   a.ActionCode),
                            new XAttribute("name",   a.ActionName),
                            new XAttribute("effect", a.Effect.ToString()),
                            new XAttribute("source", a.Source.ToString())))))),

                new XElement("featureFlags",
                    g.FeatureFlags.Select(f => new XElement("flag",
                        new XAttribute("code",      f.FlagCode),
                        new XAttribute("isEnabled", f.IsEnabled),
                        f.MatchedCriteriaType is null ? null
                            : new XAttribute("matchedCriteria", f.MatchedCriteriaType)))),

                new XElement("effectiveConfig",
                    new XAttribute("sessionTimeoutMinutes", g.EffectiveConfig.SessionTimeoutMinutes),
                    new XAttribute("maxLoginAttempts",      g.EffectiveConfig.MaxLoginAttempts),
                    new XAttribute("mfaRequiredForAdmin",   g.EffectiveConfig.MfaRequiredForAdmin)),

                new XElement("scopes",
                    g.Scopes.Select(s => new XElement("scope", s)))));

        var sb = new StringBuilder();
        doc.Save(new System.IO.StringWriter(sb));
        return sb.ToString();
    }
}
