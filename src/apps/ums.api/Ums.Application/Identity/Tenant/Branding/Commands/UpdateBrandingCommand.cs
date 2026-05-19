using Ums.Application.Identity.Tenant.Branding.DTOs;



namespace Ums.Application.Identity.Tenant.Branding.Commands;


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
