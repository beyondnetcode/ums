namespace Ums.Domain.Configuration.IdpConfiguration.Events;

using Ums.Shell.Ddd.Services.Impl;

public class IdpConfigurationDomainEventsManager : DomainEventsManager
{
    public IdpConfigurationDomainEventsManager(IAggregateRoot aggregateRoot) : base(aggregateRoot) { }

    private void Apply(IdpConfigRegisteredEvent @event) { }
    private void Apply(IdpConfigActivatedEvent @event) { }
    private void Apply(IdpConfigUpdatedEvent @event) { }
    private void Apply(IdpConfigDeactivatedEvent @event) { }
}
