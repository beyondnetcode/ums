namespace Ums.Application.Configuration.Services;

using Microsoft.Extensions.DependencyInjection;
using Ums.Domain.Configuration.AppConfiguration;

public sealed class ConfigurationValues
{
    private readonly IConfigurationProvider _provider;
    private readonly Guid? _tenantId;

    public ConfigurationValues(IConfigurationProvider provider, Guid? tenantId = null)
    {
        _provider = provider;
        _tenantId = tenantId;
    }

    public int SessionTimeoutMinutes => _provider.GetValueAs<int>(AppConfigurationCodes.SessionTimeoutMinutes, _tenantId, AppConfigurationDefaults.SessionTimeoutMinutes);

    public int MaxLoginAttempts => _provider.GetValueAs<int>(AppConfigurationCodes.MaxLoginAttempts, _tenantId, AppConfigurationDefaults.MaxLoginAttempts);

    public int AccessTokenDurationMs => _provider.GetValueAs<int>(AppConfigurationCodes.AccessTokenDurationMs, _tenantId, AppConfigurationDefaults.AccessTokenDurationMs);

    public int RefreshTokenDurationMs => _provider.GetValueAs<int>(AppConfigurationCodes.RefreshTokenDurationMs, _tenantId, AppConfigurationDefaults.RefreshTokenDurationMs);

    public int MinPasswordLength => _provider.GetValueAs<int>(AppConfigurationCodes.MinPasswordLength, _tenantId, AppConfigurationDefaults.MinPasswordLength);

    public int MaxValidityPeriodDays => _provider.GetValueAs<int>(AppConfigurationCodes.MaxValidityPeriodDays, _tenantId, AppConfigurationDefaults.MaxValidityPeriodDays);

    public bool MfaRequiredForAdmin => _provider.GetValueAs<bool>(AppConfigurationCodes.MfaRequiredForAdmin, _tenantId, AppConfigurationDefaults.MfaRequiredForAdmin);

    public bool CustomBrandingEnabled => _provider.GetValueAs<bool>(AppConfigurationCodes.UiCustomBrandingEnabled, _tenantId, AppConfigurationDefaults.UiCustomBrandingEnabled);

    public string DefaultLanguage => _provider.GetValue(AppConfigurationCodes.UiLanguageDefault, _tenantId, AppConfigurationDefaults.UiLanguageDefault);

    public string DefaultTimezone => _provider.GetValue(AppConfigurationCodes.UiTimezoneDefault, _tenantId, AppConfigurationDefaults.UiTimezoneDefault);
}

public static class ConfigurationValuesExtensions
{
    public static ConfigurationValues GetConfigurationValues(this IServiceProvider services)
        => services.GetRequiredService<IConfigurationProvider>().Global();

    public static ConfigurationValues ForTenant(this IConfigurationProvider provider, Guid tenantId)
        => new ConfigurationValues(provider, tenantId);

    public static ConfigurationValues Global(this IConfigurationProvider provider)
        => new ConfigurationValues(provider, null);
}
