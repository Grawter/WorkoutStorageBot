using FluentAssertions;
using WorkoutStorageBot.BusinessLogic.Extensions;

namespace WorkoutStorageBot.UnitTests.BusinessLogic.Exceptions
{
    public class CommonExtensionsTests
    {
        [Fact]
        public void AddBold_ShouldWrapTextWithBoldTags()
        {
            // Arrange
            string input = "Hello";
            string expected = "<b>Hello</b>";

            // Act
            string result = input.AddBold();

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void AddBold_ShouldHandleEmptyString()
        {
            // Arrange
            string input = string.Empty;
            string expected = "<b></b>";

            // Act
            string result = input.AddBold();

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void AddQuotes_ShouldWrapTextWithDoubleQuotes()
        {
            // Arrange
            string input = "World";
            string expected = "\"World\"";

            // Act
            string result = input.AddQuotes();

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void AddQuotes_ShouldHandleEmptyString()
        {
            // Arrange
            string input = string.Empty;
            string expected = "\"\"";

            // Act
            string result = input.AddQuotes();

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void AddBoldAndQuotes_ShouldApplyBothFormattingInCorrectOrder()
        {
            // Arrange
            string input = "Test";
            string expected = "\"<b>Test</b>\"";

            // Act
            string result = input.AddBoldAndQuotes();

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void AddBoldAndQuotes_ShouldHandleEmptyString()
        {
            // Arrange
            string input = string.Empty;
            string expected = "\"<b></b>\"";

            // Act
            string result = input.AddBoldAndQuotes();

            // Assert
            result.Should().Be(expected);
        }
    }
}