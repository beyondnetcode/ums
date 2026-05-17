namespace Ums.Domain.Approvals.AccessEnforcementPolicy;

public sealed class AccessEnforcementPolicy : AggregateRoot<AccessEnforcementPolicy, AccessEnforcementPolicyProps>
{
    private AccessEnforcementPolicy(AccessEnforcementPolicyProps props) : base(props)
    {
    }

    public TenantId TenantId => Props.TenantId;
    public ProfileId? ProfileId => Props.ProfileId;
    public RoleId? RoleId => Props.RoleId;
    public AccessEnforcementAction EnforcementAction => Props.EnforcementAction;
    public bool IsActive => Props.IsActive;

    public AccessEnforcementPolicyId GetId() => AccessEnforcementPolicyId.Load(Props.Id.GetValue());

    public static Result<AccessEnforcementPolicy> Create(
        TenantId tenantId,
        ProfileId? profileId,
        RoleId? roleId,
        AccessEnforcementAction enforcementAction,
        ActorId createdBy)
    {
        if (profileId is null && roleId is null)
        {
            return Result<AccessEnforcementPolicy>.Failure(DomainErrors.Approvals.PolicyRequiresProfileOrRole);
        }

        var props = new AccessEnforcementPolicyProps(IdValueObject.Create(), tenantId, profileId, roleId, enforcementAction, true, createdBy);
        var policy = new AccessEnforcementPolicy(props);

        if (!policy.IsValid())
        {
            return Result<AccessEnforcementPolicy>.Failure(policy.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<AccessEnforcementPolicy>.Success(policy);
    }

    public Result Deactivate(ActorId updatedBy)
    {
        if (!IsActive)
        {
            BrokenRules.Add(new BrokenRule(nameof(IsActive), DomainErrors.Approvals.PolicyAlreadyInactive));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.IsActive = false;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result UpdateAction(AccessEnforcementAction newAction, ActorId updatedBy)
    {
        Props.EnforcementAction = newAction;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }
}
