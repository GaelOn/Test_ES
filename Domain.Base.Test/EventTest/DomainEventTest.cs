using Domain.Mock.Implem.EventFromDomain.OfInput;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace Domain.Base.Test.EventTest
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

        [Test]
        public void DomainEvent_Should_Be_Equal_To_Himself()
        {
            // Arrange
            var evt = new InputAggregateCreated(1);
            // Assert
            evt.Should().Be(evt);
        }

        [Test]
        public void DomainEvent_Should_Be_Equal_To_The_Version_Of_Himself_As_Object()
        {
            // Arrange
            var evt = new InputAggregateCreated(1);
            // Act
            var objEvt = evt as object;
            // Assert
            evt.Equals(objEvt).Should().BeTrue();
        }

        [Test]
        public void DomainEvent_Should_Not_Be_Equal_To_Other_Event_Of_Same_Type()
        {
            // Arrange
            var evt1 = new InputAggregateCreated(1);
            var evt2 = new InputAggregateCreated(1);
            // Assert
            evt1.Equals(evt2).Should().BeFalse();
        }

        [Test]
        public void Two_DomainEvent_Create_In_The8Same_Way_Should_Have_Different_HashCode()
        {
            // Arrange
            var evt1 = new InputAggregateCreated(1);
            var evt2 = new InputAggregateCreated(1);
            // Assert
            evt1.GetHashCode().Equals(evt2.GetHashCode()).Should().BeFalse();
        }

        [Test]
        public void DomainEvent_Should_Be_Created_Using_Version_Value_Initialisation()
        {
            // Arrange
            InputAggregateCreated evt1;
            Action toBeTested = () => evt1 = new InputAggregateCreated(1, 0);
            // Assert
            toBeTested.Should().NotThrow();
        }
    }
}
