using FluentAssertions;
using Microsoft.Extensions.Logging;
using WorkoutStorageBot.Core.Helpers;

namespace WorkoutStorageBot.UnitTests.Core.Helpers
{
    public class EventIDHelperTests
    {
        [Fact]
        public void GetNextEventId_WithoutName_ReturnsEventIdWithNullName()
        {
            // Arrange & Act
            EventId eventId = EventIDHelper.GetNextEventId();

            // Assert
            eventId.Name.Should().BeNull();
            eventId.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public void GetNextEventId_WithName_ReturnsEventIdWithProvidedName()
        {
            // Arrange
            string eventName = "CustomEvent";

            // Act
            EventId eventId = EventIDHelper.GetNextEventId(eventName);

            // Assert
            eventId.Name.Should().Be(eventName);
            eventId.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public void GetNextEventIdThreadSafe_WithoutName_ReturnsEventIdWithNullName()
        {
            // Arrange & Act
            EventId eventId = EventIDHelper.GetNextEventIdThreadSafe();

            // Assert
            eventId.Name.Should().BeNull();
            eventId.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public void GetNextEventIdThreadSafe_WithName_ReturnsEventIdWithProvidedName()
        {
            // Arrange
            string eventName = "CustomThreadSafeEvent";

            // Act
            EventId eventId = EventIDHelper.GetNextEventIdThreadSafe(eventName);

            // Assert
            eventId.Name.Should().Be(eventName);
            eventId.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public void GetNextEventId_WithMultipleCalls_ReturnsDifferentIds()
        {
            // Arrange & Act
            EventId firstEventId = EventIDHelper.GetNextEventId();
            EventId secondEventId = EventIDHelper.GetNextEventId();

            // Assert
            firstEventId.Id.Should().NotBe(secondEventId.Id);
        }

        [Fact]
        public void GetNextEventIdThreadSafe_WithMultipleCalls_ReturnsDifferentIds()
        {
            // Arrange & Act
            EventId firstEventId = EventIDHelper.GetNextEventIdThreadSafe();
            EventId secondEventId = EventIDHelper.GetNextEventIdThreadSafe();

            // Assert
            firstEventId.Id.Should().NotBe(secondEventId.Id);
        }
    }
}