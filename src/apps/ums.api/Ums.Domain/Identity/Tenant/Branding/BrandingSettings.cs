namespace Ums.Domain.Identity.Tenant.Branding;

public class BrandingSettings
{
    public Logo Logo { get; private set; } = default!;
    public LogoFormat LogoFormat { get; private set; } = default!;
    public HexColor PrimaryColor { get; private set; } = default!;
    public BackgroundStyle BackgroundStyle { get; private set; } = default!;
    public LoginText HeadlineText { get; private set; } = default!;
    public LoginText SecondaryText { get; private set; } = default!;
    public LoginText PrimaryButtonLabel { get; private set; } = default!;
    public LoginText FooterText { get; private set; } = default!;
    public CustomDomain? CustomDomain { get; private set; }
    public bool MagicLinkFallbackEnabled { get; private set; }

    public class Builder
    {
        private readonly BrandingSettings _settings = new();

        public Builder WithLogo(Logo logo, LogoFormat format)
        {
            _settings.Logo = logo;
            _settings.LogoFormat = format;
            return this;
        }

        public Builder WithTheme(HexColor primaryColor, BackgroundStyle backgroundStyle)
        {
            _settings.PrimaryColor = primaryColor;
            _settings.BackgroundStyle = backgroundStyle;
            return this;
        }

        public Builder WithTexts(LoginText headline, LoginText secondary, LoginText buttonLabel, LoginText footer)
        {
            _settings.HeadlineText = headline;
            _settings.SecondaryText = secondary;
            _settings.PrimaryButtonLabel = buttonLabel;
            _settings.FooterText = footer;
            return this;
        }

        public Builder WithCustomDomain(CustomDomain? customDomain)
        {
            _settings.CustomDomain = customDomain;
            return this;
        }

        public Builder WithMagicLinkFallback(bool enabled)
        {
            _settings.MagicLinkFallbackEnabled = enabled;
            return this;
        }

        public BrandingSettings Build() => _settings;
    }

    public static Builder CreateBuilder() => new();
}
