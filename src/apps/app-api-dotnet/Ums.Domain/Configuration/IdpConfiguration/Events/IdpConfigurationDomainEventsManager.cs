namespace Ums.Domain.Configuration.IdpConfiguration.Events;

using Ums.Shell.Ddd.Services.Impl;

public class IdpConfigurationDomainEventsManager : DomainEventsManager
{
    public IdpConfigurationDomainEventsManager(object aggregateRoot) : base(aggregateRoot) { }

    public void Apply(IdpConfigRegisteredEvent @event) { }
    public void Apply(IdpConfigActivatedEvent @event) { }
    public void Apply(IdpConfigUpdatedEvent @event) { }
    public void Apply(IdpConfigDeactivatedEvent @event) { }
}
