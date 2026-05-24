namespace Ums.Infrastructure.Persistence.IGA.Entities;

public sealed class RoleMaturityStatusRecord : IAuditableRecord
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public int CurrentMaturityLevelId { get; set; }
    public int? NextEligibleMaturityLevelId { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime CurrentLevelSince { get; set; }
    public DateTime? EligibleForPromotionAt { get; set; }
    public int CompletedCertificationsCount { get; set; }
    public int CompletedTrainingsCount { get; set; }
    public decimal PerformanceScore { get; set; }
    public bool HasNoComplianceIssues { get; set; }
    public string? BlockingFactor { get; set; }
    public DateTime? LastReviewedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string AuditTimeSpan { get; set; } = string.Empty;
    public byte[] RowVersion { get; set; } = [];
}
