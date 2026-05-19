namespace Ums.Domain.Approvals.DocumentType.EnforcementPolicy;

public sealed class EnforcementPolicy : Entity<EnforcementPolicy, EnforcementPolicyProps>
{
    private EnforcementPolicy(EnforcementPolicyProps props) : base(props) { }

    public AccessEnforcementAction ActionOnExpiration => Props.ActionOnExpiration;
    public int? GracePeriodDays => Props.GracePeriodDays;
    public bool IsActive => Props.IsActive;

    public static Result<EnforcementPolicy> Create(
        AccessEnforcementAction actionOnExpiration,
        int? gracePeriodDays)
    {
        var props = new EnforcementPolicyProps(IdValueObject.Create(), actionOnExpiration, gracePeriodDays);
        var policy = new EnforcementPolicy(props);

        if (!policy.IsValid())
        {
            return Result<EnforcementPolicy>.Failure(policy.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<EnforcementPolicy>.Success(policy);
    }

    public Result Update(AccessEnforcementAction newAction, int? gracePeriodDays)
    {
        Props.ActionOnExpiration = newAction;
        Props.GracePeriodDays = gracePeriodDays;
        TrackingState.MarkAsDirty();
        return Result.Success();
    }
}
