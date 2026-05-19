namespace Ums.Domain.IGA.PromotionRequest.PromotionImpactAnalysis;

public sealed class PromotionImpactAnalysis : Entity<PromotionImpactAnalysis, PromotionImpactAnalysisProps>
{
    private PromotionImpactAnalysis(PromotionImpactAnalysisProps props) : base(props)
    {
    }

    public PromotionRequestId PromotionRequestId => Props.PromotionRequestId;
    public decimal RiskScore => Props.RiskScore;
    public TextValueObject RiskLevel => Props.RiskLevel;
    public int NewPermissionsCount => Props.NewPermissionsCount;
    public int RemovedPermissionsCount => Props.RemovedPermissionsCount;
    public int AffectedSystemsCount => Props.AffectedSystemsCount;
    public TextValueObject? ConflictingPermissions => Props.ConflictingPermissions;
    public TextValueObject? RiskFactors => Props.RiskFactors;
    public TextValueObject? SuggestedMitigations => Props.SuggestedMitigations;
    public DateTime AnalyzedAt => Props.AnalyzedAt;
    public TextValueObject? AnalyzedBy => Props.AnalyzedBy;

    public PromotionImpactAnalysisId GetId() => PromotionImpactAnalysisId.Load(Props.Id.GetValue());

    public static Result<PromotionImpactAnalysis> Create(
        PromotionRequestId promotionRequestId,
        decimal riskScore,
        TextValueObject riskLevel,
        int newPermissionsCount,
        int removedPermissionsCount,
        int affectedSystemsCount,
        TextValueObject? conflictingPermissions,
        TextValueObject? riskFactors,
        TextValueObject? suggestedMitigations,
        ActorId analyzedBy,
        ActorId createdBy)
    {
        if (riskScore < 0 || riskScore > 100)
        {
            return Result<PromotionImpactAnalysis>.Failure(DomainErrors.IGA.InvalidPerformanceScore);
        }

        var props = new PromotionImpactAnalysisProps(IdValueObject.Create(), promotionRequestId, createdBy)
        {
            RiskScore = riskScore,
            RiskLevel = riskLevel,
            NewPermissionsCount = newPermissionsCount,
            RemovedPermissionsCount = removedPermissionsCount,
            AffectedSystemsCount = affectedSystemsCount,
            ConflictingPermissions = conflictingPermissions,
            RiskFactors = riskFactors,
            SuggestedMitigations = suggestedMitigations,
            AnalyzedBy = TextValueObject.Create(analyzedBy.GetValue().ToString())
        };

        var analysis = new PromotionImpactAnalysis(props);

        if (!analysis.IsValid())
        {
            return Result<PromotionImpactAnalysis>.Failure(analysis.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<PromotionImpactAnalysis>.Success(analysis);
    }
}
