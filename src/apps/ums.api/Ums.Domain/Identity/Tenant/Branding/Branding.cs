namespace Ums.Domain.Identity.Tenant.Branding;

public sealed class Branding : Entity<Branding, BrandingProps>
{
    private Branding(BrandingProps props) : base(props)
    {
    }

    public TenantId TenantId => Props.TenantId;
    public Logo Logo => Props.Logo;
    public LogoFormat LogoFormat => Props.LogoFormat;
    public HexColor PrimaryColor => Props.PrimaryColor;
    public BackgroundStyle BackgroundStyle => Props.BackgroundStyle;
    public LoginText HeadlineText => Props.HeadlineText;
    public LoginText SecondaryText => Props.SecondaryText;
    public LoginText PrimaryButtonLabel => Props.PrimaryButtonLabel;
    public LoginText FooterText => Props.FooterText;
    public CustomDomain? CustomDomain => Props.CustomDomain;
    public DnsVerificationStatus DnsVerificationStatus => Props.DnsVerificationStatus;
    public DnsCnameTarget DnsCnameTarget => Props.DnsCnameTarget;
    // TODO: Implement Magic Link Fallback logic in the authentication flow
    public bool MagicLinkFallbackEnabled => Props.MagicLinkFallbackEnabled;

    public BrandingId GetId() => BrandingId.Load(Props.Id.GetValue());

    public static Result<Branding> Create(TenantId tenantId, BrandingSettings settings, ActorId createdBy)
    {
        var dnsCnameTarget = DnsCnameTarget.Create();

        var props = new BrandingProps(
            IdValueObject.Create(),
            tenantId,
            settings.Logo,
            settings.LogoFormat,
            settings.PrimaryColor,
            settings.BackgroundStyle,
            settings.HeadlineText,
            settings.SecondaryText,
            settings.PrimaryButtonLabel,
            settings.FooterText,
            settings.CustomDomain,
            dnsCnameTarget,
            settings.MagicLinkFallbackEnabled,
            createdBy);

        var branding = new Branding(props);

        if (!branding.IsValid())
        {
            return Result<Branding>.Failure(branding.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<Branding>.Success(branding);
    }

    public Result Update(BrandingSettings settings, ActorId updatedBy)
    {
        var domainChanged = Props.CustomDomain?.GetValue() != settings.CustomDomain?.GetValue();

        Props.Logo = settings.Logo;
        Props.LogoFormat = settings.LogoFormat;
        Props.PrimaryColor = settings.PrimaryColor;
        Props.BackgroundStyle = settings.BackgroundStyle;
        Props.HeadlineText = settings.HeadlineText;
        Props.SecondaryText = settings.SecondaryText;
        Props.PrimaryButtonLabel = settings.PrimaryButtonLabel;
        Props.FooterText = settings.FooterText;
        Props.CustomDomain = settings.CustomDomain;
        Props.MagicLinkFallbackEnabled = settings.MagicLinkFallbackEnabled;

        if (domainChanged)
        {
            Props.DnsVerificationStatus = DnsVerificationStatus.Pending;
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result VerifyDns(ActorId updatedBy)
    {
        if (Props.CustomDomain is null)
        {
            BrokenRules.Add(new BrokenRule(nameof(CustomDomain), DomainErrors.Branding.DnsVerificationRequired));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.DnsVerificationStatus = DnsVerificationStatus.Verified;
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result FailDnsVerification(ActorId updatedBy)
    {
        if (Props.CustomDomain is null)
        {
            BrokenRules.Add(new BrokenRule(nameof(CustomDomain), DomainErrors.Branding.DnsVerificationRequired));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.DnsVerificationStatus = DnsVerificationStatus.Failed;
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }
}
