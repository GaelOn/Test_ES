using Domain.Base.Event.EventStore;
using Domain.Mock.Implem.EventFromDomain.OfInput;
using FluentAssertions;
using NUnit.Framework;

namespace Domain.Base.Test.EventTest
{
    [TestFixture]
    public class EventWrapperTest
    {
        [Test]
        public void EventWrapper_Should_Be_Equal_To_Himself()
        {
            // Arrange, Act
            var wrappedEvt = new EventWrapper<int>(new InputAggregateCreated(1));
            // Assert
            wrappedEvt.Equals(wrappedEvt).Should().BeTrue();
        }


        [Test]
        public void EventWrapper_Should_Be_Equal_To_The_Version_Of_Himself_Seen_As_Object()
        {
            // Arrange, Act
            var wrappedEvt1 = new EventWrapper<int>(new InputAggregateCreated(1));
            var wrappedEvt2 = wrappedEvt1 as object;
            // Assert
            (wrappedEvt1 == wrappedEvt2).Should().BeTrue();
            wrappedEvt1.Equals(wrappedEvt2).Should().BeTrue();
        }

        [Test]
        public void EventWrapper_Should_Be_Equal_betwween_Two_Instances_representing_The_Same_Thing()
        {
            // Arrange, Act
            var wrappedEvt1 = new EventWrapper<int>(new InputAggregateCreated(1));
            var wrappedEvt2 = new EventWrapper<int>(new InputAggregateCreated(1));
            // Assert
            (wrappedEvt1 == wrappedEvt2).Should().BeFalse();
            wrappedEvt1.Equals(wrappedEvt2).Should().BeFalse();
        }

        [Test]
        public void EventWrapper_Should_Not_Be_Equal_betwween_Two_Instances_representing_The_Other_Thing()
        {
            // Arrange, Act
            var wrappedEvt1 = new EventWrapper<int>(new InputAggregateCreated(1));
            var wrappedEvt2 = new EventWrapper<int>(new InputAggregateCreated(2));
            // Assert
            (wrappedEvt1 == wrappedEvt2).Should().BeFalse();
            wrappedEvt1.Equals(wrappedEvt2).Should().BeFalse();
            (wrappedEvt1.GetHashCode() == wrappedEvt2.GetHashCode()).Should().BeFalse();
        }

        [Test]
        public void EventWrapper_Should_Have_Key_Compose_With_StreamId_VersionId_InsertDate()
        {
            // Arrange
            var wrappedEvt = new EventWrapper<int>(new InputAggregateCreated(1));
            // Act
            var keyeqquals = wrappedEvt.Key == $"1:0:{wrappedEvt.InsertDate}";
            // Assert
            keyeqquals.Should().BeTrue();
        }

        [Test]
        public void EventWrapper_ToString_Should_Return_Concatatnation_Of_Key_And_EventId()
        {
            // Arrange
            var wrappedEvt = new EventWrapper<int>(new InputAggregateCreated(1));
            // Act
            var keyeqquals = wrappedEvt.ToString() == $"1:0:{wrappedEvt.InsertDate}:{wrappedEvt.DomainEvent.EventId}";
            // Assert
            keyeqquals.Should().BeTrue();
        }
    }
}
