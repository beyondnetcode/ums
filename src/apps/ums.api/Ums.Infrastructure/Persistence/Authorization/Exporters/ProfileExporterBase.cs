using Ums.Application.Authorization.Profile.DTOs;
using Ums.Application.Authorization.Profile.Exporters;

namespace Ums.Infrastructure.Persistence.Authorization.Exporters;

public abstract class ProfileExporterBase
{
    protected static object MapPermission(ProfilePermissionDto p, ExportConfiguration config)
    {
        return new
        {
            id = config.MaskGuids ? MaskGuid(p.PermissionId) : p.PermissionId.ToString(),
            targetId = config.MaskGuids ? MaskGuid(p.TargetId) : p.TargetId.ToString(),
            targetType = p.TargetType,
            targetName = p.TargetName,
            actionId = config.MaskGuids ? MaskGuid(p.ActionId) : p.ActionId.ToString(),
            actionName = p.ActionName,
            effect = p.IsAllowed ? "Allow" : (p.IsDenied ? "Deny" : "Neutral"),
            isActive = p.IsActive,
            isOverride = p.IsOverride,
            originalFromTemplate = p.OriginalFromTemplate != null ? new
            {
                id = config.MaskGuids ? MaskGuid(p.OriginalFromTemplate.ItemId) : p.OriginalFromTemplate.ItemId.ToString(),
                targetId = config.MaskGuids ? MaskGuid(p.OriginalFromTemplate.TargetId) : p.OriginalFromTemplate.TargetId.ToString(),
                targetType = p.OriginalFromTemplate.TargetType,
                targetName = p.OriginalFromTemplate.TargetName,
                effect = p.OriginalFromTemplate.IsAllowed ? "Allow" : (p.OriginalFromTemplate.IsDenied ? "Deny" : "Neutral"),
                isActive = p.OriginalFromTemplate.IsActive
            } : null
        };
    }

    protected static object BuildEffectivePermissionsSummary(ProfileDto profile, ExportConfiguration config)
    {
        var totalPermissions = profile.Permissions.Count;
        var allowedPermissions = profile.Permissions.Count(p => p.IsAllowed);
        var deniedPermissions = profile.Permissions.Count(p => p.IsDenied);
        var activePermissions = profile.Permissions.Count(p => p.IsActive);
        var overriddenPermissions = profile.Permissions.Count(p => p.IsOverride);

        return new
        {
            totalPermissions,
            allowedPermissions,
            deniedPermissions,
            neutralPermissions = totalPermissions - allowedPermissions - deniedPermissions,
            activePermissions,
            overriddenPermissions
        };
    }

    protected static string MaskGuid(Guid guid) => "****-****-****-****";

    protected static List<object> FilterNavigationPermissions(IEnumerable<ProfilePermissionDto> permissions, ExportConfiguration config)
        => permissions.Where(p => PermissionTargetTypes.IsNavigationPermission(p.TargetType))
                      .Select(p => MapPermission(p, config))
                      .ToList();

    protected static List<object> FilterDomainResources(IEnumerable<ProfilePermissionDto> permissions, ExportConfiguration config)
        => permissions.Where(p => PermissionTargetTypes.IsDomainResource(p.TargetType))
                      .Select(p => MapPermission(p, config))
                      .ToList();

    protected static List<object> FilterSystemActions(IEnumerable<ProfilePermissionDto> permissions, ExportConfiguration config)
        => permissions.Where(p => PermissionTargetTypes.IsSystemAction(p.TargetType))
                      .Select(p => MapPermission(p, config))
                      .ToList();
}