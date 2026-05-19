namespace Ums.Application.Identity.Tenant.UpdateBranding;


public sealed record UpdateBrandingCommand(
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
    bool MagicLinkFallbackEnabled) : ICommand<UpdateBrandingResponse>;
