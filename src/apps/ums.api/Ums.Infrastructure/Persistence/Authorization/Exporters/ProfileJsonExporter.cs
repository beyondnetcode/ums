using System.Text.Json;
using Ums.Application.Authorization.Profile.DTOs;
using Ums.Application.Authorization.Profile.Exporters;

namespace Ums.Infrastructure.Persistence.Authorization.Exporters;

public sealed class ProfileJsonExporter : IProfileExporter
{
    public string Export(ProfileDto profile)
    {
        var modelPermissions = profile.Permissions
            .Where(p => p.TargetType == "Module" || p.TargetType == "Submodule" || p.TargetType == "Option" || p.TargetType == "SystemSuite")
            .Select(p => new
            {
                p.PermissionId,
                p.TargetType,
                p.TargetId,
                p.TargetName,
                p.ActionId,
                p.ActionName,
                p.IsAllowed,
                p.IsDenied,
                p.IsActive,
                p.IsOverride
            })
            .ToList();

        var domainResources = profile.Permissions
            .Where(p => p.TargetType == "Aggregate" || p.TargetType == "Entity")
            .Select(p => new
            {
                p.PermissionId,
                p.TargetType,
                p.TargetId,
                p.TargetName,
                p.ActionId,
                p.ActionName,
                p.IsAllowed,
                p.IsDenied,
                p.IsActive,
                p.IsOverride
            })
            .ToList();

        var result = new
        {
            profileId = profile.ProfileId,
            metadata = new
            {
                tenantId = profile.TenantId,
                userId = profile.UserId,
                roleId = profile.RoleId,
                branchId = profile.BranchId,
                scope = profile.Scope,
                isActive = profile.IsActive
            },
            authorizationGraph = new
            {
                portalStructure = modelPermissions,
                domainResources = domainResources
            }
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}
