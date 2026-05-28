using System.Text;
using Ums.Application.Authorization.Profile.DTOs;
using Ums.Application.Authorization.Profile.Exporters;

namespace Ums.Infrastructure.Persistence.Authorization.Exporters;

public sealed class ProfileCsvExporter : IProfileExporter
{
    public string Export(ProfileDto profile)
    {
        var sb = new StringBuilder();
        
        // Header
        sb.AppendLine("PermissionId,ProfileId,TemplateId,TargetType,TargetId,TargetName,ActionId,ActionName,IsAllowed,IsDenied,IsActive,IsOverride");

        foreach (var p in profile.Permissions)
        {
            var targetNameEscaped = p.TargetName.Replace("\"", "\"\"");
            var actionNameEscaped = p.ActionName.Replace("\"", "\"\"");

            sb.AppendLine($"{p.PermissionId},{p.ProfileId},{p.TemplateId},{p.TargetType},{p.TargetId},\"{targetNameEscaped}\",{p.ActionId},\"{actionNameEscaped}\",{p.IsAllowed},{p.IsDenied},{p.IsActive},{p.IsOverride}");
        }

        return sb.ToString();
    }
}
