using Ums.Application.Identity.Tenant.Branding.DTOs;



namespace Ums.Application.Identity.Tenant.Branding.Commands;


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
