namespace Ums.Domain.Approvals.UserDocument.Events;

public class UserDocumentDomainEventsManager : DomainEventsManager
{
    public UserDocumentDomainEventsManager(IAggregateRoot aggregateRoot) : base(aggregateRoot) { }

    private void Apply(DocumentUploadedEvent @event) { }
    private void Apply(DocumentValidatedEvent @event) { }
    private void Apply(DocumentRejectedEvent @event) { }
    private void Apply(DocumentExpiredEvent @event) { }
    private void Apply(DocumentNearExpirationEvent @event) { }
    private void Apply(EnforcementExecutedEvent @event) { }
}
