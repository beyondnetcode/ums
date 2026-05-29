using System.Text.Json;
using Ums.Application.Authorization.Profile.DTOs;
using Ums.Application.Authorization.Profile.Exporters;

namespace Ums.Infrastructure.Persistence.Authorization.Exporters;

public sealed class ProfileJsonExporter : IProfileExporter
{
    public string ContentType => "application/json";
    public string FileExtension => "json";

    public string Export(ProfileDto profile, ExportConfiguration? configuration = null)
    {
        var config = configuration ?? ExportConfiguration.Default;

        var navigationPermissions = profile.Permissions
            .Where(p => PermissionTargetTypes.IsNavigationPermission(p.TargetType))
            .Select(p => MapPermission(p, config))
            .ToList();

        var domainResources = profile.Permissions
            .Where(p => PermissionTargetTypes.IsDomainResource(p.TargetType))
            .Select(p => MapPermission(p, config))
            .ToList();

        var systemActions = profile.Permissions
            .Where(p => PermissionTargetTypes.IsSystemAction(p.TargetType))
            .Select(p => MapPermission(p, config))
            .ToList();

        var effectivePermissionsSummary = BuildEffectivePermissionsSummary(profile, config);

        object result;

        if (config.IncludeTechnicalMetadata)
        {
            result = new
            {
                tenant = new
                {
                    id = config.MaskGuids ? MaskGuid(profile.TenantId) : profile.TenantId.ToString(),
                    code = profile.TenantCode,
                    name = profile.TenantName,
                    status = "Active"
                },
                system = new
                {
                    id = config.MaskGuids ? MaskGuid(profile.SystemSuiteId) : profile.SystemSuiteId.ToString(),
                    code = profile.SystemSuiteCode,
                    name = profile.SystemSuiteName,
                    status = "Active"
                },
                user = new
                {
                    id = config.MaskGuids ? MaskGuid(profile.UserId) : profile.UserId.ToString(),
                    email = profile.UserEmail,
                    status = "Active"
                },
                profile = new
                {
                    id = config.MaskGuids ? MaskGuid(profile.ProfileId) : profile.ProfileId.ToString(),
                    code = profile.RoleCode,
                    name = profile.RoleName,
                    status = profile.IsActive ? "Active" : "Inactive",
                    roleLevel = profile.Scope,
                    permissionTemplateId = profile.Permissions.FirstOrDefault()?.TemplateId != null
                        ? (config.MaskGuids ? MaskGuid(profile.Permissions.First().TemplateId) : profile.Permissions.First().TemplateId.ToString())
                        : null,
                    permissionCount = profile.PermissionCount
                },
                authorizationGraph = new
                {
                    modules = navigationPermissions,
                    domainResources = domainResources,
                    systemActions = systemActions,
                    effectivePermissionsSummary = config.IncludeEffectivePermissionsSummary ? effectivePermissionsSummary : null
                },
                session = new
                {
                    generatedAt = DateTime.UtcNow.ToString("O"),
                    generatedBy = "System",
                    tenantId = config.MaskGuids ? MaskGuid(profile.TenantId) : profile.TenantId.ToString(),
                    sessionTrackingId = Guid.NewGuid().ToString(),
                    correlationId = Guid.NewGuid().ToString(),
                    language = "en",
                    environment = "Production"
                }
            };
        }
        else
        {
            result = new
            {
                profile = new
                {
                    id = profile.ProfileId,
                    role = profile.RoleCode,
                    name = profile.RoleName,
                    status = profile.IsActive ? "Active" : "Inactive"
                },
                authorizationGraph = new
                {
                    modules = navigationPermissions,
                    domainResources = domainResources,
                    systemActions = systemActions,
                    effectivePermissionsSummary = config.IncludeEffectivePermissionsSummary ? effectivePermissionsSummary : null
                }
            };
        }

        return JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private static object MapPermission(ProfilePermissionDto p, ExportConfiguration config)
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

    private static object BuildEffectivePermissionsSummary(ProfileDto profile, ExportConfiguration config)
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

    private static string MaskGuid(Guid guid) => "****-****-****-****";
}
