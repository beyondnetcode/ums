namespace Ums.Infrastructure.Persistence.IGA.Entities;

public sealed class PromotionImpactAnalysisRecord : IAuditableRecord
{
    public Guid Id { get; set; }
    public Guid PromotionRequestId { get; set; }
    public decimal RiskScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public int NewPermissionsCount { get; set; }
    public int RemovedPermissionsCount { get; set; }
    public int AffectedSystemsCount { get; set; }
    public string? ConflictingPermissions { get; set; }
    public string? RiskFactors { get; set; }
    public string? SuggestedMitigations { get; set; }
    public DateTime AnalyzedAt { get; set; }
    public string? AnalyzedBy { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string AuditTimeSpan { get; set; } = string.Empty;
}
