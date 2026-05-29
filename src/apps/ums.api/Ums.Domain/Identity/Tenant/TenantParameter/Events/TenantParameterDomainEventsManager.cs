namespace Ums.Domain.Identity.Tenant.TenantParameter.Events;

public class TenantParameterDomainEventsManager : DomainEventsManager
{
    public TenantParameterDomainEventsManager(IAggregateRoot aggregateRoot) : base(aggregateRoot) { }

    private void Apply(TenantParameterCreatedEvent @event) { }
    private void Apply(TenantParameterUpdatedEvent @event) { }
    private void Apply(TenantParameterDeactivatedEvent @event) { }
    private void Apply(TenantParameterReactivatedEvent @event) { }
}