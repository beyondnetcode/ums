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

    public static Result<UserManagementDelegation> Grant(Guid tenantId, Guid delegatorUserId, Guid delegateUserId, DateRange effectiveRange, string scope)
    {
        if (tenantId == Guid.Empty || delegatorUserId == Guid.Empty || delegateUserId == Guid.Empty)
            return Result<UserManagementDelegation>.Failure("Tenant, delegator, and delegate identifiers are required.");

        if (delegatorUserId == delegateUserId)
            return Result<UserManagementDelegation>.Failure("Delegator and delegate must be different.");

        if (string.IsNullOrWhiteSpace(scope))
            return Result<UserManagementDelegation>.Failure("Delegation scope is required.");

        var props = new UserManagementDelegationProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            global::Ums.Domain.Kernel.ValueObjects.UserId.Load(delegatorUserId),
            global::Ums.Domain.Kernel.ValueObjects.UserId.Load(delegateUserId),
            effectiveRange,
            global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(scope.Trim()));

        var delegation = new UserManagementDelegation(props);
        return Result<UserManagementDelegation>.Success(delegation);
    }

    public void Revoke()
    {
        Props.Status = DelegationStatus.Revoked;
        Props.Audit.Update("system");
    }
    
    public Guid GetId() => Props.Id.GetValue();
}
