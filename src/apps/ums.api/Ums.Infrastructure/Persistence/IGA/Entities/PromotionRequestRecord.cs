namespace Ums.Infrastructure.Persistence.IGA.Entities;

public sealed class PromotionRequestRecord : IAuditableRecord
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid CurrentRoleId { get; set; }
    public Guid TargetRoleId { get; set; }
    public DateTime RequestedAt { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public string? RequestReason { get; set; }
    public Guid ManagerId { get; set; }
    public int ManagerApprovalStatusId { get; set; }
    public DateTime? ManagerDecisionAt { get; set; }
    public int SecurityApprovalStatusId { get; set; }
    public DateTime? SecurityDecisionAt { get; set; }
    public int StatusId { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public string? ExecutedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string AuditTimeSpan { get; set; } = string.Empty;
    public byte[] RowVersion { get; set; } = [];

    public List<PromotionImpactAnalysisRecord> ImpactAnalyses { get; set; } = [];
}
