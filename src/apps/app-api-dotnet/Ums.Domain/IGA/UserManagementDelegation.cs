namespace Ums.Domain.IGA;

public sealed class UserManagementDelegation : AggregateRoot<UserManagementDelegation, UserManagementDelegationProps>
{
    private UserManagementDelegation(UserManagementDelegationProps props) : base(props)
    {
        if (TrackingState.IsNew)
        {
            DomainEvents.ApplyChange(new DelegationGrantedEvent(Props.TenantId.GetValue(), Props.Id.GetValue(), Props.DelegateUserId.GetValue()), true);
        }
    }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid DelegatorUserId => Props.DelegatorUserId.GetValue();
    public Guid DelegateUserId => Props.DelegateUserId.GetValue();
    public DateTimeOffset EffectiveFrom => Props.EffectiveFrom;
    public DateTimeOffset? EffectiveTo => Props.EffectiveTo;
    public string Scope => Props.Scope.GetValue();
    public DelegationStatus Status => Props.Status;

    public static Result<UserManagementDelegation> Grant(Guid tenantId, Guid delegatorUserId, Guid delegateUserId, DateRange effectiveRange, string scope, string createdBy)
    {
        var brokenRules = new BrokenRulesManager();
        if (tenantId == Guid.Empty || delegatorUserId == Guid.Empty || delegateUserId == Guid.Empty)
            brokenRules.Add(new BrokenRule("Identifiers", DomainErrors.Iga.DelegationIdentifiersRequired));

        if (delegatorUserId == delegateUserId)
            brokenRules.Add(new BrokenRule("Users", DomainErrors.Iga.DelegatorDelegateMustDiffer));

        if (string.IsNullOrWhiteSpace(scope))
            brokenRules.Add(new BrokenRule(nameof(scope), DomainErrors.Iga.DelegationScopeRequired));

        if (brokenRules.GetBrokenRules().Any())
            return Result<UserManagementDelegation>.Failure(brokenRules.GetBrokenRulesAsString());

        var props = new UserManagementDelegationProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            global::Ums.Domain.Kernel.ValueObjects.UserId.Load(delegatorUserId),
            global::Ums.Domain.Kernel.ValueObjects.UserId.Load(delegateUserId),
            effectiveRange,
            global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(scope.Trim()),
            createdBy);

        var delegation = new UserManagementDelegation(props);

        if (!delegation.IsValid())
        {
            return Result<UserManagementDelegation>.Failure(delegation.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<UserManagementDelegation>.Success(delegation);
    }

    public Result Revoke(string updatedBy)
    {
        Props.Status = DelegationStatus.Revoked;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy);
        return Result.Success();
    }
    
    public Guid GetId() => Props.Id.GetValue();
}
