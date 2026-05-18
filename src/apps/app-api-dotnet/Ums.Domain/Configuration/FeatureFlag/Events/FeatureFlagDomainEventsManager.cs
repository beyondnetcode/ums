namespace Ums.Domain.Configuration.FeatureFlag.Events;

using Ums.Shell.Ddd.Services.Impl;

public class FeatureFlagDomainEventsManager : DomainEventsManager
{
    public FeatureFlagDomainEventsManager(IAggregateRoot aggregateRoot) : base(aggregateRoot) { }

    private void Apply(FeatureFlagCreatedEvent @event) { }
    private void Apply(FeatureFlagActivatedEvent @event) { }
    private void Apply(FeatureFlagDeactivatedEvent @event) { }
    private void Apply(FeatureFlagArchivedEvent @event) { }
    private void Apply(FeatureFlagStateChangedEvent @event) { }
    private void Apply(FlagEvaluatedEvent @event) { }
}
