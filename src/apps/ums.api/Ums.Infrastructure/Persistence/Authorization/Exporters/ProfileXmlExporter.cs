using System.Xml.Linq;
using Ums.Application.Authorization.Profile.DTOs;
using Ums.Application.Authorization.Profile.Exporters;

namespace Ums.Infrastructure.Persistence.Authorization.Exporters;

public sealed class ProfileXmlExporter : IProfileExporter
{
    public string Export(ProfileDto profile)
    {
        var doc = new XDocument(
            new XElement("ProfileAuthorization",
                new XElement("Metadata",
                    new XElement("ProfileId", profile.ProfileId),
                    new XElement("TenantId", profile.TenantId),
                    new XElement("UserId", profile.UserId),
                    new XElement("RoleId", profile.RoleId),
                    new XElement("BranchId", profile.BranchId?.ToString() ?? string.Empty),
                    new XElement("Scope", profile.Scope),
                    new XElement("IsActive", profile.IsActive)
                ),
                new XElement("Permissions",
                    profile.Permissions.Select(p =>
                        new XElement("Permission",
                            new XAttribute("Id", p.PermissionId),
                            new XElement("TemplateId", p.TemplateId),
                            new XElement("TargetType", p.TargetType),
                            new XElement("TargetId", p.TargetId),
                            new XElement("TargetName", p.TargetName),
                            new XElement("ActionId", p.ActionId),
                            new XElement("ActionName", p.ActionName),
                            new XElement("IsAllowed", p.IsAllowed),
                            new XElement("IsDenied", p.IsDenied),
                            new XElement("IsActive", p.IsActive),
                            new XElement("IsOverride", p.IsOverride)
                        )
                    )
                )
            )
        );

        return doc.ToString();
    }
}
