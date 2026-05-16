using Ums.Shell.Ddd.Services.Impl;

namespace Ums.Shell.Ddd.Test.Services
{
    [TestClass]
    public class DomainEventsManagerTest
    {
        private class TestAggregateRoot : IAggregateRoot
        {
            public int Version { get; set; }
            public IdValueObject Id { get; } = IdValueObject.Create();
            public DomainEventsManager DomainEvents { get; } = null!;
        }

        private class TestMetadata : IMetadata
        {
            public Guid EventId { get; private set; }
            public string EventName { get; private set; } = string.Empty;
            public Guid AggregateId { get; private set; }
            public string AggregateName { get; private set; } = string.Empty;

            public void SetMetadata(Guid eventId, string eventName, Guid aggregateId, string aggregateName)
            {
                EventId = eventId;
                EventName = eventName;
                AggregateId = aggregateId;
                AggregateName = aggregateName;
            }
        }

        private class TestDomainEvent : IDomainEvent
        {
            public IMetadata Metadata { get; private set; } = new TestMetadata();
            public void SetMetadata(IMetadata metadata) => Metadata = metadata;
        }

        private class TestDomainEventsManager : DomainEventsManager
        {
            public TestDomainEventsManager(IAggregateRoot aggregateRoot) : base(aggregateRoot) { }
            // Simulate the Apply method for TestDomainEvent
            private void Apply(TestDomainEvent @event) { /* no-op for test */ }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Throws_When_AggregateRoot_Is_Null()
        {
            _ = new DomainEventsManager(null!);
        }

        [TestMethod]
        public void RaiseEvent_AddsEventToUncommittedChanges()
        {
            var aggregate = new TestAggregateRoot();
            var manager = new TestDomainEventsManager(aggregate);
            var domainEvent = new TestDomainEvent();

            manager.RaiseEvent(domainEvent);

            var changes = manager.GetUncommittedChanges();
            Assert.AreEqual(1, changes.Count);
            Assert.AreSame(domainEvent, new List<IDomainEvent>(changes)[0]);
        }

        [TestMethod]
        public void MarkChangesAsCommitted_ClearsUncommittedChanges()
        {
            var aggregate = new TestAggregateRoot();
            var manager = new TestDomainEventsManager(aggregate);
            manager.RaiseEvent(new TestDomainEvent());

            manager.MarkChangesAsCommitted();

            Assert.AreEqual(0, manager.GetUncommittedChanges().Count);
        }

        [TestMethod]
        public void LoadFromHistory_AppliesEventsWithoutAddingToUncommitted()
        {
            var aggregate = new TestAggregateRoot();
            var manager = new TestDomainEventsManager(aggregate);
            var history = new List<IDomainEvent> { new TestDomainEvent(), new TestDomainEvent() };

            manager.LoadFromHistory(history);

            Assert.AreEqual(0, manager.GetUncommittedChanges().Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoadFromHistory_Throws_When_History_Is_Null()
        {
            var aggregate = new TestAggregateRoot();
            var manager = new TestDomainEventsManager(aggregate);

            manager.LoadFromHistory(null!);
        }
    }
}
