using Ums.Shell.Ddd.Interfaces;

namespace Ums.Shell.Ddd.Services.Impl
{
    public interface IDomainEventsManager
    {
        int Version { get; set; }
        public IAggregateRoot AggregateRoot { get; }
        void ApplyChange(IDomainEvent domainEvent, bool isNew);
        IReadOnlyCollection<IDomainEvent> GetUncommittedChanges();
        void LoadFromHistory(IReadOnlyCollection<IDomainEvent> history);
        void MarkChangesAsCommitted();
        void ReplayEvents(IEnumerable<IDomainEvent> domainEvents);
    }
}