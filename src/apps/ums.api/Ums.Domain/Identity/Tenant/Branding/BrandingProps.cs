namespace Ums.Domain.Identity.Tenant.Branding;

public class BrandingProps : IProps
{
    public IdValueObject Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public Logo Logo { get; private set; }
    public LogoFormat LogoFormat { get; private set; }
    public HexColor PrimaryColor { get; private set; }
    public BackgroundStyle BackgroundStyle { get; private set; }
    public LoginText HeadlineText { get; private set; }
    public LoginText SecondaryText { get; private set; }
    public LoginText PrimaryButtonLabel { get; private set; }
    public LoginText FooterText { get; private set; }
    public CustomDomain? CustomDomain { get; private set; }
    public DnsVerificationStatus DnsVerificationStatus { get; private set; }
    public DnsCnameTarget DnsCnameTarget { get; private set; }
    public bool MagicLinkFallbackEnabled { get; private set; }
    public AuditValueObject Audit { get; private set; }

    public BrandingProps(
        IdValueObject id,
        TenantId tenantId,
        Logo logo,
        LogoFormat logoFormat,
        HexColor primaryColor,
        BackgroundStyle backgroundStyle,
        LoginText headlineText,
        LoginText secondaryText,
        LoginText primaryButtonLabel,
        LoginText footerText,
        CustomDomain? customDomain,
        DnsCnameTarget dnsCnameTarget,
        bool magicLinkFallbackEnabled,
        ActorId createdBy)
    {
        Id = id;
        TenantId = tenantId;
        Logo = logo;
        LogoFormat = logoFormat;
        PrimaryColor = primaryColor;
        BackgroundStyle = backgroundStyle;
        HeadlineText = headlineText;
        SecondaryText = secondaryText;
        PrimaryButtonLabel = primaryButtonLabel;
        FooterText = footerText;
        CustomDomain = customDomain;
        DnsVerificationStatus = DnsVerificationStatus.Pending;
        DnsCnameTarget = dnsCnameTarget;
        MagicLinkFallbackEnabled = magicLinkFallbackEnabled;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public BrandingProps(
        IdValueObject id,
        TenantId tenantId,
        Logo logo,
        LogoFormat logoFormat,
        HexColor primaryColor,
        BackgroundStyle backgroundStyle,
        LoginText headlineText,
        LoginText secondaryText,
        LoginText primaryButtonLabel,
        LoginText footerText,
        CustomDomain? customDomain,
        DnsVerificationStatus dnsVerificationStatus,
        DnsCnameTarget dnsCnameTarget,
        bool magicLinkFallbackEnabled,
        AuditValueObject audit)
    {
        Id = id;
        TenantId = tenantId;
        Logo = logo;
        LogoFormat = logoFormat;
        PrimaryColor = primaryColor;
        BackgroundStyle = backgroundStyle;
        HeadlineText = headlineText;
        SecondaryText = secondaryText;
        PrimaryButtonLabel = primaryButtonLabel;
        FooterText = footerText;
        CustomDomain = customDomain;
        DnsVerificationStatus = dnsVerificationStatus;
        DnsCnameTarget = dnsCnameTarget;
        MagicLinkFallbackEnabled = magicLinkFallbackEnabled;
        Audit = audit;
    }

    public BrandingProps WithLogo(Logo logo)
    {
        var clone = (BrandingProps)MemberwiseClone();
        clone.Logo = logo;
        return clone;
    }

    public BrandingProps WithLogoFormat(LogoFormat logoFormat)
    {
        var clone = (BrandingProps)MemberwiseClone();
        clone.LogoFormat = logoFormat;
        return clone;
    }

    public BrandingProps WithPrimaryColor(HexColor primaryColor)
    {
        var clone = (BrandingProps)MemberwiseClone();
        clone.PrimaryColor = primaryColor;
        return clone;
    }

    public BrandingProps WithBackgroundStyle(BackgroundStyle backgroundStyle)
    {
        var clone = (BrandingProps)MemberwiseClone();
        clone.BackgroundStyle = backgroundStyle;
        return clone;
    }

    public BrandingProps WithHeadlineText(LoginText headlineText)
    {
        var clone = (BrandingProps)MemberwiseClone();
        clone.HeadlineText = headlineText;
        return clone;
    }

    public BrandingProps WithSecondaryText(LoginText secondaryText)
    {
        var clone = (BrandingProps)MemberwiseClone();
        clone.SecondaryText = secondaryText;
        return clone;
    }

    public BrandingProps WithPrimaryButtonLabel(LoginText primaryButtonLabel)
    {
        var clone = (BrandingProps)MemberwiseClone();
        clone.PrimaryButtonLabel = primaryButtonLabel;
        return clone;
    }

    public BrandingProps WithFooterText(LoginText footerText)
    {
        var clone = (BrandingProps)MemberwiseClone();
        clone.FooterText = footerText;
        return clone;
    }

    public BrandingProps WithCustomDomain(CustomDomain? customDomain)
    {
        var clone = (BrandingProps)MemberwiseClone();
        clone.CustomDomain = customDomain;
        return clone;
    }

    public BrandingProps WithMagicLinkFallbackEnabled(bool magicLinkFallbackEnabled)
    {
        var clone = (BrandingProps)MemberwiseClone();
        clone.MagicLinkFallbackEnabled = magicLinkFallbackEnabled;
        return clone;
    }

    public BrandingProps WithDnsVerificationStatus(DnsVerificationStatus dnsVerificationStatus)
    {
        var clone = (BrandingProps)MemberwiseClone();
        clone.DnsVerificationStatus = dnsVerificationStatus;
        return clone;
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}