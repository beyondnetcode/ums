namespace Ums.Domain.IGA.PromotionRequest.PromotionImpactAnalysis;

public class PromotionImpactAnalysisProps : IProps
{
    public IdValueObject Id { get; set; }
    public PromotionRequestId PromotionRequestId { get; set; }
    public decimal RiskScore { get; set; }
    public TextValueObject RiskLevel { get; set; }
    public int NewPermissionsCount { get; set; }
    public int RemovedPermissionsCount { get; set; }
    public int AffectedSystemsCount { get; set; }
    public TextValueObject? ConflictingPermissions { get; set; }
    public TextValueObject? RiskFactors { get; set; }
    public TextValueObject? SuggestedMitigations { get; set; }
    public DateTime AnalyzedAt { get; set; }
    public TextValueObject? AnalyzedBy { get; set; }
    public AuditValueObject Audit { get; private set; }

    public PromotionImpactAnalysisProps(
        IdValueObject id,
        PromotionRequestId promotionRequestId,
        ActorId createdBy)
    {
        Id = id;
        PromotionRequestId = promotionRequestId;
        RiskLevel = TextValueObject.Create("UNKNOWN");
        AnalyzedAt = DateTime.UtcNow;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public object Clone() => MemberwiseClone();
}
