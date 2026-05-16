using Ums.Shell.Ddd.Interfaces;

namespace Ums.Shell.Ddd.Services.Impl
{
    /// <summary>
    /// Manages domain events, including tracking uncommitted changes and applying events.
    /// </summary>
    public class DomainEventsManager : IDomainEventsManager
    {
        /// <summary>
        /// Gets the aggregate root associated with this manager.
        /// </summary>
        public IAggregateRoot AggregateRoot { get; }

        private readonly List<IDomainEvent> _domainEvents = new();

        /// <summary>
        /// Gets or sets the version of the domain events manager.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEventsManager"/> class.
        /// </summary>
        /// <param name="aggregateRoot">The aggregate root associated with this manager.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregateRoot"/> is null.</exception>
        public DomainEventsManager(IAggregateRoot aggregateRoot)
        {
            AggregateRoot = aggregateRoot ?? throw new ArgumentNullException(nameof(aggregateRoot));
            Version = -1;
        }

        /// <summary>
        /// Gets the uncommitted changes.
        /// </summary>
        /// <returns>A read-only collection of uncommitted domain events.</returns>
        public IReadOnlyCollection<IDomainEvent> GetUncommittedChanges() => _domainEvents.AsReadOnly();

        /// <summary>
        /// Marks all changes as committed.
        /// </summary>
        public void MarkChangesAsCommitted() => _domainEvents.Clear();

        /// <summary>
        /// Loads the domain events from history.
        /// </summary>
        /// <param name="history">The history of domain events.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="history"/> is null.</exception>
        public void LoadFromHistory(IReadOnlyCollection<IDomainEvent> history)
        {
            if (history is null)
                throw new ArgumentNullException(nameof(history));

            foreach (var e in history)
            {
                ApplyChange(e, false);
            }
        }

        /// <summary>
        /// Applies a domain event.
        /// </summary>
        /// <param name="domainEvent">The domain event to apply.</param>
        /// <param name="isNew">Indicates whether the event is new.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="domainEvent"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the Apply method is missing for the domain event type.</exception>
        public void ApplyChange(IDomainEvent domainEvent, bool isNew)
        {
            ArgumentNullException.ThrowIfNull(domainEvent, nameof(domainEvent));    

            var method = GetType().GetMethod("Apply", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { domainEvent.GetType() }, null);

            if (method == null)
                throw new InvalidOperationException($"Missing Apply method for {domainEvent.GetType()}");

            method.Invoke(this, new object[] { domainEvent });

            if (isNew)
            {
                _domainEvents.Add(domainEvent);
            }
        }

        /// <summary>
        /// Raises a domain event.
        /// </summary>
        /// <param name="domainEvent">The domain event to raise.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="domainEvent"/> is null.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1030:Use events where appropriate", Justification = "<Pending>")]
        public void RaiseEvent(IDomainEvent domainEvent)
        {
            ArgumentNullException.ThrowIfNull(domainEvent, nameof(domainEvent));

            if (domainEvent is IMetadata metadata)
            {
                metadata.SetMetadata(Guid.NewGuid(),
                    domainEvent.GetType().Name,
                    AggregateRoot.Id.GetValue(),
                    AggregateRoot.GetType().Name);
            }

            ApplyChange(domainEvent, true);
        }

        /// <summary>
        /// Applies a domain event.
        /// </summary>
        /// <param name="domainEvent">The domain event to apply.</param>
        public void ApplyChange(IDomainEvent domainEvent) => ApplyChange(domainEvent, true);

        /// <summary>
        /// Replays a collection of domain events.
        /// </summary>
        /// <param name="domainEvents">The domain events to replay.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="domainEvents"/> is null.</exception>
        public void ReplayEvents(IEnumerable<IDomainEvent> domainEvents)
        {
            ArgumentNullException.ThrowIfNull(domainEvents, nameof(domainEvents));

            foreach (var domainEvent in domainEvents)
            {
                ApplyChange(domainEvent, false);
            }
        }
    }
}
