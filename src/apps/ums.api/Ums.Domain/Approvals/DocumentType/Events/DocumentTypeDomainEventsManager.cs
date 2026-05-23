namespace Ums.Domain.Approvals.DocumentType.Events;

public class DocumentTypeDomainEventsManager : DomainEventsManager
{
    public DocumentTypeDomainEventsManager(IAggregateRoot aggregateRoot) : base(aggregateRoot) { }

    private void Apply(DocumentTypeRegisteredEvent @event) { }
}
