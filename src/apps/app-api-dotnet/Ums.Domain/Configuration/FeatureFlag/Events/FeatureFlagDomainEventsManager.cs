namespace Ums.Domain.Configuration.FeatureFlag.Events;

using Ums.Shell.Ddd.Services.Impl;

public class FeatureFlagDomainEventsManager : DomainEventsManager
{
    public FeatureFlagDomainEventsManager(object aggregateRoot) : base(aggregateRoot) { }

    public void Apply(FeatureFlagCreatedEvent @event) { }
    public void Apply(FeatureFlagActivatedEvent @event) { }
    public void Apply(FeatureFlagDeactivatedEvent @event) { }
    public void Apply(FeatureFlagArchivedEvent @event) { }
    public void Apply(FeatureFlagStateChangedEvent @event) { }
    public void Apply(FlagEvaluatedEvent @event) { }
}
