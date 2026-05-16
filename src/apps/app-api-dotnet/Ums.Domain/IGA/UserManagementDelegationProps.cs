namespace Ums.Domain.IGA;

public class UserManagementDelegationProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.UserId DelegatorUserId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.UserId DelegateUserId { get; private set; }
    public DateTimeOffset EffectiveFrom { get; private set; }
    public DateTimeOffset? EffectiveTo { get; private set; }
    public StringValueObject Scope { get; private set; }
    public DelegationStatus Status { get; set; }
    public AuditValueObject Audit { get; private set; }

    public UserManagementDelegationProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Kernel.ValueObjects.UserId delegatorUserId, global::Ums.Domain.Kernel.ValueObjects.UserId delegateUserId, DateRange effectiveRange, StringValueObject scope)
    {
        Id = id;
        TenantId = tenantId;
        DelegatorUserId = delegatorUserId;
        DelegateUserId = delegateUserId;
        EffectiveFrom = effectiveRange.StartsAt;
        EffectiveTo = effectiveRange.EndsAt;
        Scope = scope;
        Status = DelegationStatus.Active;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
