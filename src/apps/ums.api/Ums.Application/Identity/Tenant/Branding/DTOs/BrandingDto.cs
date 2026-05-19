namespace Ums.Application.Identity.Tenant.Branding.DTOs;

public sealed record BrandingDto(
    string Logo,
    string LogoFormat,
    string PrimaryColor,
    string BackgroundStyle,
    string HeadlineText,
    string SecondaryText,
    string PrimaryButtonLabel,
    string FooterText,
    string? CustomDomain,
    bool MagicLinkFallbackEnabled,
    string? DnsVerificationStatus);
