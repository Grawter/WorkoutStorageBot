using FluentAssertions;
using WorkoutStorageBot.BusinessLogic.Helpers.Converters;

namespace WorkoutStorageBot.UnitTests.BusinessLogic.Helpers.Builders
{
    public class MessageTextBuilderTests
    {
        [Fact]
        public void Ctor_WithTrimFalse_ShouldNotTrimInput()
        {
            // Arrange
            string input = "  test message  ";

            // Act
            MessageTextBuilder builder = new MessageTextBuilder(input, withTrim: false);
            string result = builder.Build();

            // Assert
            result.Should().Be("  test message  ");
        }

        [Fact]
        public void Ctor_WithTrimTrue_ShouldTrimInput()
        {
            // Arrange
            string input = "  test message  ";

            // Act
            MessageTextBuilder builder = new MessageTextBuilder(input, withTrim: true);
            string result = builder.Build();

            // Assert
            result.Should().Be("test message");
        }

        [Fact]
        public void RemoveCompletely_WhenLengthGreaterThanStartIndex_ShouldRemoveTail()
        {
            // Arrange
            string input = new string('a', 100);
            MessageTextBuilder builder = new MessageTextBuilder(input);

            // Act
            string result = builder.RemoveCompletely(54).Build();

            // Assert
            result.Length.Should().Be(54);
            result.Should().Be(new string('a', 54));
        }

        [Fact]
        public void RemoveCompletely_WhenLengthLessThanStartIndex_ShouldDoNothing()
        {
            // Arrange
            string input = "short text";
            MessageTextBuilder builder = new MessageTextBuilder(input);

            // Act
            string result = builder.RemoveCompletely(54).Build();

            // Assert
            result.Should().Be(input);
        }

        [Fact]
        public void RemoveCompletely_WhenStartIndexIsZero_ShouldDoNothing()
        {
            // Arrange
            string input = "some text";
            MessageTextBuilder builder = new MessageTextBuilder(input);

            // Act
            string result = builder.RemoveCompletely(0).Build();

            // Assert
            result.Should().Be(input);
        }

        [Fact]
        public void WithoutServiceSymbol_ShouldRemoveAllOccurrences()
        {
            // Arrange
            string input = "/test/message/";
            MessageTextBuilder builder = new MessageTextBuilder(input);

            // Act
            string result = builder.WithoutServiceSymbol("/").Build();

            // Assert
            result.Should().Be("testmessage");
        }

        [Fact]
        public void WithoutServiceSymbols_ShouldRemoveAllProvidedSymbols()
        {
            // Arrange
            string input = "/test-message_result/";
            string[] symbols = new[] { "/", "-", "_" };
            MessageTextBuilder builder = new MessageTextBuilder(input);

            // Act
            string result = builder.WithoutServiceSymbols(symbols).Build();

            // Assert
            result.Should().Be("testmessageresult");
        }

        [Fact]
        public void Build_ShouldReturnCurrentState()
        {
            // Arrange
            MessageTextBuilder builder = new MessageTextBuilder("abc");

            // Act
            string result = builder.Build();

            // Assert
            result.Should().Be("abc");
        }

        [Fact]
        public void Methods_ShouldBeChainable()
        {
            // Arrange
            string input = "/test message that is definitely longer than fifty four characters/";
            MessageTextBuilder builder = new MessageTextBuilder(input);

            // Act
            string result = builder.WithoutServiceSymbol("/")
                                   .RemoveCompletely(12)
                                   .Build();

            // Assert
            result.Should().Be("test message");
        }
    }
}