namespace Ums.Domain.Identity.Tenant.Branding;

public class BrandingProps : IProps
{
    public IdValueObject Id { get; set; }
    public TenantId TenantId { get; set; }
    public Logo Logo { get; set; }
    public LogoFormat LogoFormat { get; set; }
    public HexColor PrimaryColor { get; set; }
    public BackgroundStyle BackgroundStyle { get; set; }
    public LoginText HeadlineText { get; set; }
    public LoginText SecondaryText { get; set; }
    public LoginText PrimaryButtonLabel { get; set; }
    public LoginText FooterText { get; set; }
    public CustomDomain? CustomDomain { get; set; }
    public DnsVerificationStatus DnsVerificationStatus { get; set; }
    public DnsCnameTarget DnsCnameTarget { get; set; }
    public bool MagicLinkFallbackEnabled { get; set; }
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

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
