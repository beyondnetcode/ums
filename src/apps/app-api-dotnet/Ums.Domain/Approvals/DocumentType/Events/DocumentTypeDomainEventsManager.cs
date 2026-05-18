namespace Ums.Domain.Approvals.DocumentType.Events;

public class DocumentTypeDomainEventsManager : DomainEventsManager
{
    public DocumentTypeDomainEventsManager(IAggregateRoot aggregateRoot) : base(aggregateRoot) { }

    private void Apply(DocumentTypeRegisteredEvent @event) { }
    private void Apply(NotificationRuleConfiguredEvent @event) { }
    private void Apply(NotificationRuleRemovedEvent @event) { }
    private void Apply(EnforcementPolicyDefinedEvent @event) { }
    private void Apply(EnforcementPolicyUpdatedEvent @event) { }
}
