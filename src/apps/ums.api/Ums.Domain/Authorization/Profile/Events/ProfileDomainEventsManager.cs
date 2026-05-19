namespace Ums.Domain.Authorization.Profile;

public class ProfileDomainEventsManager : DomainEventsManager
{
    public ProfileDomainEventsManager(IAggregateRoot aggregateRoot) : base(aggregateRoot) { }

    private void Apply(ProfileCreatedEvent @event) { }
    private void Apply(TemplateLinkedToProfileEvent @event) { }
    private void Apply(PermissionOverriddenEvent @event) { }
    private void Apply(PermissionStatusChangedEvent @event) { }
    private void Apply(ProfileDeactivatedEvent @event) { }
    private void Apply(ProfileActivatedEvent @event) { }
}
