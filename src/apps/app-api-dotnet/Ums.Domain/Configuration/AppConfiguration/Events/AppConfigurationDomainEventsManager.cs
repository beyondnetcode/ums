namespace Ums.Domain.Configuration.AppConfiguration.Events;

using Ums.Shell.Ddd.Services.Impl;

public class AppConfigurationDomainEventsManager : DomainEventsManager
{
    public AppConfigurationDomainEventsManager(object aggregateRoot) : base(aggregateRoot) { }

    public void Apply(AppConfigCreatedEvent @event) { }
    public void Apply(AppConfigPublishedEvent @event) { }
    public void Apply(AppConfigArchivedEvent @event) { }
    public void Apply(AppConfigUpdatedEvent @event) { }
}
