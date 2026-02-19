using FluentAssertions;
using WorkoutStorageBot.BusinessLogic.Helpers.CallbackQueryParser;

namespace WorkoutStorageBot.UnitTests.BusinessLogic.Helpers.CallbackQueryPars
{
    public class CallbackQueryParserTests
    {
        [Fact]
        public void CallbackQueryParserCtor_WithEmptyInputString_ShouldThrowArgumentException()
        {
            // Arrange && Act
            Func<CallbackQueryParser> func = () => new CallbackQueryParser(string.Empty);

            // Assert
            func.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void DirectionProperty_WithIncorrectString__ShouldThrowFormatException()
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("A");

            // Act
            Func<int> func = () => callbackQueryParser.Direction;

            // Assert
            func.Should().Throw<FormatException>();
        }

        [Fact]
        public void AllOpenProperties_ShouldReturnsExpectedValues()
        {
            // Arrange && Act
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("1|Abc|Def|AD1|AD2||CallBackID");

            // Assert
            callbackQueryParser.Direction.Should().Be(1);
            callbackQueryParser.SubDirection.Should().Be("Abc");
            callbackQueryParser.DomainType.Should().Be("Def");
            callbackQueryParser.AdditionalParameters.First().Should().Be("AD1");
            callbackQueryParser.TryGetAdditionalParameter(1, out string? additionalValue1).Should().BeTrue();
            additionalValue1.Should().Be("AD2");
            callbackQueryParser.TryGetAdditionalParameter(2, out string? additionalValue2).Should().BeTrue();
            additionalValue2.Should().Be(string.Empty);
            callbackQueryParser.TryGetAdditionalParameter(3, out string? additionalValue3).Should().BeFalse();
            additionalValue3.Should().BeNull();
        }
    }
}