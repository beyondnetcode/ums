namespace Ums.Domain.Approvals.DocumentType.EnforcementPolicy;

public class EnforcementPolicyProps : IProps
{
    public IdValueObject Id { get; set; }
    public AccessEnforcementAction ActionOnExpiration { get; set; }
    public int? GracePeriodDays { get; set; }
    public bool IsActive { get; set; }

    public EnforcementPolicyProps(
        IdValueObject id,
        AccessEnforcementAction actionOnExpiration,
        int? gracePeriodDays)
    {
        Id = id;
        ActionOnExpiration = actionOnExpiration;
        GracePeriodDays = gracePeriodDays;
        IsActive = true;
    }

    public object Clone() => MemberwiseClone();
}
