namespace Ums.Domain.Authorization.SystemSuite.Events;

public class SystemSuiteDomainEventsManager : DomainEventsManager
{
    public SystemSuiteDomainEventsManager(IAggregateRoot aggregateRoot) : base(aggregateRoot) { }

    private void Apply(SystemSuiteRegisteredEvent @event) { }
    private void Apply(SystemSuiteStatusChangedEvent @event) { }
    private void Apply(SystemSuiteModuleAddedEvent @event) { }
    private void Apply(SystemSuiteModuleRemovedEvent @event) { }
    private void Apply(SystemSuiteModuleStatusChangedEvent @event) { }
    private void Apply(SystemSuiteActionRegisteredEvent @event) { }
    private void Apply(SystemSuiteActionRemovedEvent @event) { }
}
