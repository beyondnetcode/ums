namespace Ums.Shell.Ddd.Interfaces
{
    /// <summary>
    /// Interface representing metadata for events and aggregates.
    /// </summary>
    public interface IMetadata
    {
        /// <summary>
        /// Gets the unique identifier for the event.
        /// </summary>
        Guid EventId { get; }

        /// <summary>
        /// Gets the name of the event.
        /// </summary>
        string EventName { get; }

        /// <summary>
        /// Gets the unique identifier for the aggregate.
        /// </summary>
        Guid AggregateId { get; }

        /// <summary>
        /// Gets the name of the aggregate.
        /// </summary>
        string AggregateName { get; }

        /// <summary>
        /// Sets the metadata for the event and aggregate.
        /// </summary>
        /// <param name="eventId">The unique identifier for the event.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="aggregateId">The unique identifier for the aggregate.</param>
        /// <param name="aggregateName">The name of the aggregate.</param>
        void SetMetadata(Guid eventId, string eventName, Guid aggregateId, string aggregateName);
    }
}
