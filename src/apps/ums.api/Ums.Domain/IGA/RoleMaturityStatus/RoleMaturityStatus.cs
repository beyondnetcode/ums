namespace Ums.Domain.IGA.RoleMaturityStatus;

public sealed class RoleMaturityStatus : AggregateRoot<RoleMaturityStatus, RoleMaturityStatusProps>
{
    private RoleMaturityStatus(RoleMaturityStatusProps props) : base(props)
    {
    }

    public TenantId TenantId => Props.TenantId;
    public UserId UserId => Props.UserId;
    public RoleId RoleId => Props.RoleId;
    public RoleMaturityLevel CurrentMaturityLevel => Props.CurrentMaturityLevel;
    public RoleMaturityLevel? NextEligibleMaturityLevel => Props.NextEligibleMaturityLevel;
    public DateTime AssignedAt => Props.AssignedAt;
    public DateTime CurrentLevelSince => Props.CurrentLevelSince;
    public DateTime? EligibleForPromotionAt => Props.EligibleForPromotionAt;
    public int CompletedCertificationsCount => Props.CompletedCertificationsCount;
    public int CompletedTrainingsCount => Props.CompletedTrainingsCount;
    public decimal PerformanceScore => Props.PerformanceScore;
    public bool HasNoComplianceIssues => Props.HasNoComplianceIssues;
    public TextValueObject? BlockingFactor => Props.BlockingFactor;
    public DateTime? LastReviewedAt => Props.LastReviewedAt;

    public RoleMaturityStatusId GetId() => RoleMaturityStatusId.Load(Props.Id.GetValue());

    public static Result<RoleMaturityStatus> Create(
        TenantId tenantId,
        UserId userId,
        RoleId roleId,
        RoleMaturityLevel currentMaturityLevel,
        ActorId createdBy)
    {
        var now = DateTime.UtcNow;
        var props = new RoleMaturityStatusProps(IdValueObject.Create(), tenantId, userId, roleId, currentMaturityLevel, now, now, createdBy);
        props.NextEligibleMaturityLevel = CalculateNextEligibleLevel(currentMaturityLevel);
        var status = new RoleMaturityStatus(props);

        if (!status.IsValid())
        {
            return Result<RoleMaturityStatus>.Failure(status.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<RoleMaturityStatus>.Success(status);
    }

    public Result UpdateMaturityLevel(RoleMaturityLevel newLevel, ActorId updatedBy)
    {
        if (newLevel == Props.CurrentMaturityLevel)
        {
            BrokenRules.Add(new BrokenRule(nameof(CurrentMaturityLevel), DomainErrors.IGA.MaturityLevelUnchanged));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.CurrentMaturityLevel = newLevel;
        Props.CurrentLevelSince = DateTime.UtcNow;
        Props.NextEligibleMaturityLevel = CalculateNextEligibleLevel(newLevel);
        Props.EligibleForPromotionAt = null;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result RecordCertificationCompletion(ActorId updatedBy)
    {
        Props.CompletedCertificationsCount++;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result RecordTrainingCompletion(ActorId updatedBy)
    {
        Props.CompletedTrainingsCount++;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result UpdatePerformanceScore(decimal score, ActorId updatedBy)
    {
        if (score < 0 || score > 5)
        {
            BrokenRules.Add(new BrokenRule(nameof(PerformanceScore), DomainErrors.IGA.InvalidPerformanceScore));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.PerformanceScore = score;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result MarkComplianceIssue(TextValueObject reason, ActorId updatedBy)
    {
        Props.HasNoComplianceIssues = false;
        Props.BlockingFactor = reason;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result ResolveComplianceIssue(ActorId updatedBy)
    {
        Props.HasNoComplianceIssues = true;
        Props.BlockingFactor = null;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result ReviewEligibility(ActorId reviewedBy)
    {
        Props.LastReviewedAt = DateTime.UtcNow;
        Props.EligibleForPromotionAt = IsEligibleForPromotion() ? DateTime.UtcNow : null;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(reviewedBy.GetValue());
        return Result.Success();
    }

    private bool IsEligibleForPromotion()
    {
        if (!Props.HasNoComplianceIssues) return false;
        if (Props.PerformanceScore < 3.0m) return false;
        if (Props.NextEligibleMaturityLevel is null) return false;

        var monthsAtCurrentLevel = (DateTime.UtcNow - Props.CurrentLevelSince).TotalDays / 30;
        return Props.CurrentMaturityLevel switch
        {
            RoleMaturityLevel.Junior => monthsAtCurrentLevel >= 6,
            RoleMaturityLevel.Intermediate => monthsAtCurrentLevel >= 12,
            RoleMaturityLevel.Senior => monthsAtCurrentLevel >= 18,
            RoleMaturityLevel.Lead => monthsAtCurrentLevel >= 24,
            RoleMaturityLevel.Principal => false,
            _ => false
        };
    }

    private static RoleMaturityLevel? CalculateNextEligibleLevel(RoleMaturityLevel current)
    {
        return current switch
        {
            RoleMaturityLevel.Junior => RoleMaturityLevel.Intermediate,
            RoleMaturityLevel.Intermediate => RoleMaturityLevel.Senior,
            RoleMaturityLevel.Senior => RoleMaturityLevel.Lead,
            RoleMaturityLevel.Lead => RoleMaturityLevel.Principal,
            RoleMaturityLevel.Principal => null,
            _ => null
        };
    }
}
