namespace Ums.Domain.Identity.Tenant.Events;

public class TenantDomainEventsManager : DomainEventsManager
{
    public TenantDomainEventsManager(IAggregateRoot aggregateRoot) : base(aggregateRoot) { }

    private void Apply(TenantCreatedEvent @event) { }
    private void Apply(TenantSuspendedEvent @event) { }
    private void Apply(TenantActivatedEvent @event) { }
    private void Apply(TenantArchivedEvent @event) { }
    private void Apply(BranchCreatedEvent @event) { }
    private void Apply(BranchRemovedEvent @event) { }
    private void Apply(BranchDeactivatedEvent @event) { }
    private void Apply(BranchReactivatedEvent @event) { }
    private void Apply(IdentityProviderRegisteredEvent @event) { }
    private void Apply(IdentityProviderActivatedEvent @event) { }
    private void Apply(IdentityProviderDeactivatedEvent @event) { }
    private void Apply(IdentityProviderRemovedEvent @event) { }
    private void Apply(BrandingCreatedEvent @event) { }
    private void Apply(BrandingUpdatedEvent @event) { }
    private void Apply(BrandingRemovedEvent @event) { }
    private void Apply(BrandingDnsVerifiedEvent @event) { }
    private void Apply(BrandingDnsFailedEvent @event) { }
}
