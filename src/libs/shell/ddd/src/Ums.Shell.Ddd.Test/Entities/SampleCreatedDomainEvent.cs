using System.Text.Json;

namespace Ums.Shell.Ddd.Test.Entities
{
    public record SampleCreatedDomainEvent : DomainEvent
    {
        public Guid SampleEntityId { get; init; } = default!;
        public string Name { get; init; } = default!;

        public SampleCreatedDomainEvent(Guid sampleEntityId, string name)
        {
            SampleEntityId = sampleEntityId;
            Name = name;
        }

    }
}
