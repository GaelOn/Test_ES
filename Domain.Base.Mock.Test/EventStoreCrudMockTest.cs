using System;
using System.Linq;
using Domain.Base.Event;
using Domain.Base.Event.EventStore;
using FluentAssertions;
using NUnit.Framework;

namespace Domain.Base.Mock.Test
{
    [TestFixture]
    public class EventStoreCrudMockTest
    {
        private IEventStore<int> _store = new EventStoreCrudMock<int>();
        private readonly Func<IDomainEvent<int>, Func<IDomainEvent<int>, bool>> testUniqueKeyGenerator =
            (evtToBeAdded) => ((evt) => evtToBeAdded.StreamId == evt.StreamId && evtToBeAdded.EventVersion == evt.EventVersion);

        [SetUp]
        public void Initialize() => ((EventStoreCrudMock<int>)_store).Reset();

        [Test]
        public void Should_Not_throw_Error_When_Add_New_Event_With_Non_Already_Used_Version_Value()
        {
            // Arrange
            var evt = new TestEvent(1, 0, "first");
            // Act
            var actionTest = new Action(() => _store.AddEvent(evt));
            // Assert
            actionTest.Should().NotThrow();
        }

        [Test]
        public void Should_Not_throw_Error_When_TryAdd_New_Event_With_Non_lLready_Used_Version_Value()
        {
            // Arrange
            var evt = new TestEvent(1, 0, "first");
            // Act
            var addRes = _store.TryAddEventWithUniqueKey(evt, testUniqueKeyGenerator(evt));
            // Assert
            addRes.Should().BeTrue();
        }

        [Test]
        public void Should_Retrieve_All_Event_From_Store()
        {
            // Arrange
            StoreFiveEvent(1);
            // Act
            var result = _store.ReadEvents(1);
            // Assert
            result.Count().Should().Be(5);
        }

        [Test]
        public void Should_Retrieve_Event_From_Store_Begining_At_Version_Three()
        {
            // Arrange
            StoreFiveEvent(1);
            // Act
            var result = _store.ReadEventsFromVersion(1, 3);
            // Assert
            result.Count().Should().Be(2);
            result.Min(evt => evt.Version).Should().Be(3);
        }

        [Test]
        public void Should_Retrieve_The_Next_Expected_Version_Value()
        {
            // Arrange
            StoreFiveEvent(1);
            // Act
            var result = _store.GetNextExpectedVersion(1);
            // Assert
            result.ExpectedVersion.Should().Be(5);
        }

        [Test]
        public void Should_Throw_Error_When_Event_With_The_already_Used_Version_Value_To_Be_Added()
        {
            // Arrange
            StoreFiveEvent(1);
            // Act
            var actionTest = new Action(() => _store.AddEvent(new TestEvent(1, 3, "second")));
            // Assert
            actionTest.Should().Throw<StoredVersionDontMatchException>();
            _store.ReadEvents(1).Where(wrap => (wrap.DomainEvent as TestEvent).Value == "second").Should().BeEmpty();
        }

        [Test]
        public void Should_Throw_Error_When_Event_With_The_already_Used_Version_Value_Try_To_Be_Added()
        {
            // Arrange
            StoreFiveEvent(1);
            var evtToBeAdded = new TestEvent(1, 3, "second");
            // Act
            var addResult = _store.TryAddEventWithUniqueKey(evtToBeAdded, testUniqueKeyGenerator(evtToBeAdded));
            // Assert
            addResult.Should().BeFalse();
            _store.ReadEvents(1).Where(wrap => (wrap.DomainEvent as TestEvent).Value == "second").Should().BeEmpty();
        }

        [Test]
        public void Event_Must_Not_Be_Inserted_In_Version_Order()
        {
            // Arrange
            StoreFiveEvent(1);
            // Act
            var actionTest = new Action(() => _store.AddEvent(new TestEvent(10, 3, "second")));
            // Assert
            actionTest.Should().NotThrow();
        }

        private void StoreFiveEvent(int streamId)
        {
            _store.AddEvent(new TestEvent(streamId, 0, "first"));
            _store.AddEvent(new TestEvent(streamId, 1, "first"));
            _store.AddEvent(new TestEvent(streamId, 2, "first"));
            _store.AddEvent(new TestEvent(streamId, 3, "first"));
            _store.AddEvent(new TestEvent(streamId, 4, "first"));
        }
    }

    public class TestEvent : IDomainEvent<int>
    {
        public Guid EventId { get; }

        public int StreamId { get; }

        public long EventVersion { get; }

        public string Value { get; }

        public void OfAggregate(IEventSourced<int> aggregate)
        {
            throw new NotImplementedException();
        }
        public TestEvent(int streamId, long eventVersion, string value)
        {
            EventId      = Guid.NewGuid();
            StreamId     = streamId;
            EventVersion = eventVersion;
            Value        = value;
        }
    }
}
