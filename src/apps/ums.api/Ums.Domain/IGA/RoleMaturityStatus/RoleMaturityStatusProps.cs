namespace Ums.Domain.IGA.RoleMaturityStatus;

public class RoleMaturityStatusProps : IProps
{
    public IdValueObject Id { get; set; }
    public TenantId TenantId { get; set; }
    public UserId UserId { get; set; }
    public RoleId RoleId { get; set; }
    public RoleMaturityLevel CurrentMaturityLevel { get; set; }
    public RoleMaturityLevel? NextEligibleMaturityLevel { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime CurrentLevelSince { get; set; }
    public DateTime? EligibleForPromotionAt { get; set; }
    public int CompletedCertificationsCount { get; set; }
    public int CompletedTrainingsCount { get; set; }
    public decimal PerformanceScore { get; set; }
    public bool HasNoComplianceIssues { get; set; }
    public TextValueObject? BlockingFactor { get; set; }
    public DateTime? LastReviewedAt { get; set; }
    public AuditValueObject Audit { get; private set; }

    public RoleMaturityStatusProps(
        IdValueObject id,
        TenantId tenantId,
        UserId userId,
        RoleId roleId,
        RoleMaturityLevel currentMaturityLevel,
        DateTime assignedAt,
        DateTime currentLevelSince,
        ActorId createdBy)
    {
        Id = id;
        TenantId = tenantId;
        UserId = userId;
        RoleId = roleId;
        CurrentMaturityLevel = currentMaturityLevel;
        AssignedAt = assignedAt;
        CurrentLevelSince = currentLevelSince;
        HasNoComplianceIssues = true;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public object Clone() => MemberwiseClone();
}
