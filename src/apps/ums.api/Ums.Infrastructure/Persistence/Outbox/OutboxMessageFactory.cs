using System.Text.Json;
using Ums.Infrastructure.Persistence.Reflection;
using Ums.Shell.Ddd.Interfaces;

namespace Ums.Infrastructure.Persistence.Outbox;

internal static class OutboxMessageFactory
{
    public static IReadOnlyCollection<OutboxMessage> CreateFromAggregate(IAggregateRoot aggregateRoot)
    {
        return aggregateRoot.DomainEvents
            .GetUncommittedChanges()
            .Select(domainEvent => new OutboxMessage
            {
                Id = domainEvent.Metadata.EventId,
                AggregateId = domainEvent.Metadata.AggregateId,
                AggregateName = domainEvent.Metadata.AggregateName,
                EventName = domainEvent.Metadata.EventName,
                EventType = domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
                Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                TenantId = ExtractTenantId(domainEvent),
                OccurredOnUtc = (DateTime?)domainEvent.GetType().GetProperty("CreatedAt")?.GetValue(domainEvent) ?? DateTime.UtcNow,
            })
            .ToList();
    }

    private static Guid? ExtractTenantId(IDomainEvent domainEvent)
    {
        var property = domainEvent.GetType().GetProperty("TenantId");
        if (property is null) return null;

        // FIX-11: domain events carry TenantId as Guid? (nullable), not Guid.
        // The original check (== typeof(Guid)) always returned false for nullable types,
        // so TenantId was never populated in outbox messages.
        if (property.PropertyType == typeof(Guid))
            return (Guid)property.GetValue(domainEvent)!;

        if (property.PropertyType == typeof(Guid?))
            return (Guid?)property.GetValue(domainEvent);

        return null;
    }
}
