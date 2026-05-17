namespace Ums.Domain.Authorization.PermissionTemplate;

public class PermissionTemplateDomainEventsManager : DomainEventsManager
{
    public PermissionTemplateDomainEventsManager(IAggregateRoot aggregateRoot) : base(aggregateRoot) { }

    private void Apply(PermissionTemplateCreatedEvent @event) { }
    private void Apply(PermissionTemplatePublishedEvent @event) { }
    private void Apply(PermissionTemplateMutatedEvent @event) { }
    private void Apply(PermissionTemplateDeprecatedEvent @event) { }
}
