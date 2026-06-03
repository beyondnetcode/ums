namespace Ums.Domain.Authorization.AssignmentRule.Events;

using Ums.Domain.Events;

public sealed class TemplateAssignmentRuleDomainEventsManager : DomainEventsManager
{
    public TemplateAssignmentRuleDomainEventsManager(IAggregateRoot aggregateRoot) : base(aggregateRoot) { }

    private void Apply(AssignmentRuleCreatedEvent @event) { }
    private void Apply(AssignmentRuleDeactivatedEvent @event) { }
    private void Apply(AssignmentRuleReactivatedEvent @event) { }
}
