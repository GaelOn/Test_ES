using Domain.Mock.Implem.EventFromDomain.OfInput;
using FluentAssertions;
using NUnit.Framework;

namespace Domain.Base.Test
{
    [TestFixture]
    public class DomainEventTest
    {
        [Test]
        public void DomainEvent_Should_Have_Unique_Id()
        {
            // Arrange
            var evt = new InputAggregateCreated(1);
            // Act
            var expectedId = evt.EventId;
            // Assert
            evt.EventId.Should().Be(expectedId);
        }
    }
}
