namespace Ums.Domain.Configuration.AppConfiguration;

public static class AppConfigurationCodes
{
    public const string SessionTimeoutMinutes = "SESSION_TIMEOUT_MINUTES";
    public const string MaxLoginAttempts = "MAX_LOGIN_ATTEMPTS";
    public const string AccessTokenDurationMs = "ACCESS_TOKEN_DURATION_MS";
    public const string RefreshTokenDurationMs = "REFRESH_TOKEN_DURATION_MS";
    public const string MinPasswordLength = "MIN_PASSWORD_LENGTH";
    public const string MaxValidityPeriodDays = "MAX_VALIDITY_PERIOD_DAYS";
    public const string MfaRequiredForAdmin = "MFA_REQUIRED_FOR_ADMIN";
    public const string UiCustomBrandingEnabled = "UI_CUSTOM_BRANDING_ENABLED";
    public const string FrontendConfigTransport = "FRONTEND_CONFIG_TRANSPORT";
    public const string AuthUseExternalIdp = "AUTH_USE_EXTERNAL_IDP";
    public const string UiLanguageDefault = "UI_LANGUAGE_DEFAULT";
    public const string UiTimezoneDefault = "UI_TIMEZONE_DEFAULT";
}
