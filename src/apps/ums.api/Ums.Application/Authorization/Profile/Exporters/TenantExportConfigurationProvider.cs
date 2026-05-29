using Ums.Application.Identity.Tenant.TenantParameter.Services;
using Ums.Domain.Identity.Tenant.TenantParameter;

namespace Ums.Application.Authorization.Profile.Exporters;

public sealed class TenantExportConfigurationProvider : ITenantExportConfigurationProvider
{
    private readonly ITenantParameterProvider _parameterProvider;

    public TenantExportConfigurationProvider(ITenantParameterProvider parameterProvider)
    {
        _parameterProvider = parameterProvider;
    }

    public async Task<ExportConfiguration> GetConfigurationAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var allowedFormats = await _parameterProvider.GetStringListValueAsync(
            tenantId,
            TenantParameterCodes.ExportProfilePermissionGraphAllowedFormats,
            ["JSON", "XML", "YAML", "CSV"],
            cancellationToken);

        var defaultFormatRaw = await _parameterProvider.GetValueAsync(
            tenantId,
            TenantParameterCodes.ExportProfilePermissionGraphDefaultFormat,
            cancellationToken) ?? "JSON";

        var includeTechnicalMetadata = await _parameterProvider.GetBoolValueAsync(
            tenantId,
            TenantParameterCodes.ExportProfilePermissionGraphIncludeTechnicalMetadata,
            true,
            cancellationToken);

        var maskGuids = await _parameterProvider.GetBoolValueAsync(
            tenantId,
            TenantParameterCodes.ExportProfilePermissionGraphMaskGuids,
            false,
            cancellationToken);

        var includeFeatureFlags = await _parameterProvider.GetBoolValueAsync(
            tenantId,
            TenantParameterCodes.ExportProfilePermissionGraphIncludeFeatureFlags,
            true,
            cancellationToken);

        var includeEffectivePermissionsSummary = await _parameterProvider.GetBoolValueAsync(
            tenantId,
            TenantParameterCodes.ExportProfilePermissionGraphIncludeEffectivePermissionsSummary,
            true,
            cancellationToken);

        var maxItems = await _parameterProvider.GetIntValueAsync(
            tenantId,
            TenantParameterCodes.ExportProfilePermissionGraphMaxItems,
            10000,
            cancellationToken);

        var defaultFormat = defaultFormatRaw.ToUpperInvariant();
        if (!allowedFormats.Any(f => f.Equals(defaultFormat, StringComparison.OrdinalIgnoreCase)))
        {
            defaultFormat = "JSON";
        }

        return new ExportConfiguration(
            allowedFormats,
            defaultFormat,
            includeTechnicalMetadata,
            maskGuids,
            includeFeatureFlags,
            includeEffectivePermissionsSummary,
            maxItems);
    }
}