namespace Ums.Domain.Configuration.FeatureFlag;

public class FeatureFlagProps : IProps
{
    public IdValueObject Id { get; set; }
    public IdValueObject SystemSuiteId { get; set; }
    public IdValueObject? TenantId { get; set; }
    public string FlagCode { get; set; }
    public FlagType FlagType { get; set; }
    public string FlagTargets { get; set; }
    public FlagStatus Status { get; set; }
    public LinkedResourceType? LinkedResourceType { get; set; }
    public IdValueObject? LinkedResourceId { get; set; }
    public int? RolloutPercentage { get; set; }
    public AuditValueObject Audit { get; private set; }

    public FeatureFlagProps(
        IdValueObject id,
        IdValueObject systemSuiteId,
        IdValueObject? tenantId,
        string flagCode,
        FlagType flagType,
        string flagTargets,
        LinkedResourceType? linkedResourceType,
        IdValueObject? linkedResourceId,
        int? rolloutPercentage,
        ActorId createdBy)
    {
        Id = id;
        SystemSuiteId = systemSuiteId;
        TenantId = tenantId;
        FlagCode = flagCode;
        FlagType = flagType;
        FlagTargets = flagTargets;
        Status = FlagStatus.Inactive;
        LinkedResourceType = linkedResourceType;
        LinkedResourceId = linkedResourceId;
        RolloutPercentage = rolloutPercentage;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public object Clone() => MemberwiseClone();
}
