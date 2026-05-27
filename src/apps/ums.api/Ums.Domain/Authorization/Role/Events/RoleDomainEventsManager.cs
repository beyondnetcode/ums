namespace Ums.Domain.Authorization.Role;

public sealed class RoleDomainEventsManager : DomainEventsManager
{
    public RoleDomainEventsManager(IAggregateRoot aggregateRoot) : base(aggregateRoot) { }

    private void Apply(RoleCreatedEvent @event) { }
    private void Apply(RoleUpdatedEvent @event) { }
    private void Apply(RoleActivatedEvent @event) { }
    private void Apply(RoleDeactivatedEvent @event) { }
}
