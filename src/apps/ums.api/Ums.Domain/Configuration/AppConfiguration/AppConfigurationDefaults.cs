namespace Ums.Domain.Configuration.AppConfiguration;

public static class AppConfigurationDefaults
{
    public const int SessionTimeoutMinutes = 30;
    public const int MaxLoginAttempts = 5;
    public const int AccessTokenDurationMs = 3_600_000;
    public const int RefreshTokenDurationMs = 604_800_000;
    public const int MinPasswordLength = 12;
    public const int MaxValidityPeriodDays = 365;
    public const bool MfaRequiredForAdmin = false;
    public const bool UiCustomBrandingEnabled = false;
    public const string FrontendConfigTransport = "rest";
    public const bool AuthUseExternalIdp = false;
    public const string UiLanguageDefault = "es";
    public const string UiTimezoneDefault = "America/Lima";
}
