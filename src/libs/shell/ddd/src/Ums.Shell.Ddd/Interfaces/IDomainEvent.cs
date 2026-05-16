namespace Ums.Shell.Ddd.Interfaces
{
    /// <summary>
    /// Represents a domain event that implements the INotification interface.
    /// </summary>
    public interface IDomainEvent : INotification
    {
        /// <summary>
        /// Gets the metadata associated with the domain event.
        /// </summary>
        IMetadata Metadata { get; }

        /// <summary>
        /// Sets the metadata for the domain event.
        /// </summary>
        /// <param name="metadata">The metadata to set.</param>
        void SetMetadata(IMetadata metadata);
    }
}