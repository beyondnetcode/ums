namespace Ums.Application.Configuration.Services;

using Microsoft.Extensions.DependencyInjection;

public sealed class ConfigurationValues
{
    private readonly IConfigurationProvider _provider;
    private readonly Guid? _tenantId;

    public ConfigurationValues(IConfigurationProvider provider, Guid? tenantId = null)
    {
        _provider = provider;
        _tenantId = tenantId;
    }

    public int SessionTimeoutMinutes => _provider.GetValueAs<int>("SESSION_TIMEOUT_MINUTES", _tenantId, 30);

    public int MaxLoginAttempts => _provider.GetValueAs<int>("MAX_LOGIN_ATTEMPTS", _tenantId, 5);

    public int AccessTokenDurationMs => _provider.GetValueAs<int>("ACCESS_TOKEN_DURATION_MS", _tenantId, 3600000);

    public int RefreshTokenDurationMs => _provider.GetValueAs<int>("REFRESH_TOKEN_DURATION_MS", _tenantId, 604800000);

    public int MinPasswordLength => _provider.GetValueAs<int>("MIN_PASSWORD_LENGTH", _tenantId, 12);

    public int MaxValidityPeriodDays => _provider.GetValueAs<int>("MAX_VALIDITY_PERIOD_DAYS", _tenantId, 365);

    public bool MfaRequiredForAdmin => _provider.GetValueAs<bool>("MFA_REQUIRED_FOR_ADMIN", _tenantId, false);

    public bool CustomBrandingEnabled => _provider.GetValueAs<bool>("UI_CUSTOM_BRANDING_ENABLED", _tenantId, false);

    public string DefaultLanguage => _provider.GetValue("UI_LANGUAGE_DEFAULT", _tenantId, "es");

    public string DefaultTimezone => _provider.GetValue("UI_TIMEZONE_DEFAULT", _tenantId, "America/Lima");
}

public static class ConfigurationValuesExtensions
{
    public static IConfigurationProvider GetConfigurationValues(this IServiceProvider services)
        => services.GetRequiredService<IConfigurationProvider>();

    public static ConfigurationValues ForTenant(this IConfigurationProvider provider, Guid tenantId)
        => new ConfigurationValues(provider, tenantId);

    public static ConfigurationValues Global(this IConfigurationProvider provider)
        => new ConfigurationValues(provider, null);
}