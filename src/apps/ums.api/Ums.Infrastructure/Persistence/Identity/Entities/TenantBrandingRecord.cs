namespace Ums.Infrastructure.Persistence.Identity.Entities;

public sealed class TenantBrandingRecord
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Logo { get; set; } = string.Empty;
    public int LogoFormatId { get; set; }
    public string PrimaryColor { get; set; } = string.Empty;
    public int BackgroundStyleId { get; set; }
    public string HeadlineText { get; set; } = string.Empty;
    public string SecondaryText { get; set; } = string.Empty;
    public string PrimaryButtonLabel { get; set; } = string.Empty;
    public string FooterText { get; set; } = string.Empty;
    public string? CustomDomain { get; set; }
    public int DnsVerificationStatusId { get; set; }
    public string DnsCnameTarget { get; set; } = string.Empty;
    public bool MagicLinkFallbackEnabled { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string AuditTimeSpan { get; set; } = string.Empty;

    public TenantRecord Tenant { get; set; } = default!;
}
