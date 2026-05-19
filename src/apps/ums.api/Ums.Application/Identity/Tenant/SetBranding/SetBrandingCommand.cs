namespace Ums.Application.Identity.Tenant.SetBranding;


public sealed record SetBrandingCommand(
    Guid TenantId,
    string Logo,
    string LogoFormat,
    string PrimaryColor,
    string BackgroundStyle,
    string HeadlineText,
    string SecondaryText,
    string PrimaryButtonLabel,
    string FooterText,
    string? CustomDomain,
    bool MagicLinkFallbackEnabled) : ICommand<SetBrandingResponse>;
