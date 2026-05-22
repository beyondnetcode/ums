namespace Ums.Infrastructure.Persistence.Identity.Entities;

public sealed class TenantRecord
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int OrganizationTypeId { get; set; }
    public int IdpStrategyId { get; set; }
    public string? CompanyReference { get; set; }
    public Guid? ParentTenantId { get; set; }
    public int StatusId { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string AuditTimeSpan { get; set; } = string.Empty;

    public List<TenantBranchRecord> Branches { get; set; } = [];
    public List<TenantIdentityProviderRecord> IdentityProviders { get; set; } = [];
    public TenantBrandingRecord? Branding { get; set; }
}
