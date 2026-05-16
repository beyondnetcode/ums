using Ums.Shell.Ddd.Interfaces;

namespace Ums.Shell.Ddd
{
    /// <summary>
    /// Represents a domain event.
    /// </summary>
    public abstract record DomainEvent : IDomainEvent
    {
        public IMetadata Metadata { get; set; } = default!;
        public DateTime CreatedAt { get; }

        protected DomainEvent()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public void SetMetadata(IMetadata metadata)
        {
            Metadata = metadata;
        }
    }
}
