using Domain.Base.Event.EventStore;
using Domain.Mock.Implem.EventFromDomain.OfInput;
using FluentAssertions;
using NUnit.Framework;

namespace Domain.Base.Test
{
    [TestFixture]
    public class EventWrapperTest
    {
        [Test]
        public void EventWrapper_Should_Be_Equal_betwween_Two_Instances_representing_The_Same_Thing()
        {
            // Arrange, Act
            var wrappedEvt1 = new EventWrapper<int>(new InputAggregateCreated(1));
            var wrappedEvt2 = new EventWrapper<int>(new InputAggregateCreated(1));
            // Assert
            (wrappedEvt1 == wrappedEvt2).Should().BeFalse();
            wrappedEvt1.Equals(wrappedEvt2).Should().BeTrue();
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
        }
    }
}
