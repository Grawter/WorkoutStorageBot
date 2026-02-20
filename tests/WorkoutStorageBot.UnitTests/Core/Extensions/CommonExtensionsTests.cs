using FluentAssertions;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Helpers.CallbackQueryParser;
using WorkoutStorageBot.Core.Extensions;

namespace WorkoutStorageBot.UnitTests.Core.Extensions
{
    public class CommonExtensionsTests
    {
        [Fact]
        public void ThrowIfNull_WithNullVariable_ShouldThrowArgumentNullExceptionWithParamNameInMessage()
        {
            // Arrange
            CallbackQueryParser? parser = null;

            // Act
            Action act = () => parser.ThrowIfNull();

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage($"Value cannot be null. (Parameter '{nameof(parser)}')");
        }

        [Fact]
        public void ThrowIfNullOrWhiteSpace_WithNullVariable_ShouldThrowArgumentNullExceptionWithParamNameInMessage()
        {
            // Arrange
            CallbackQueryParser? parser = null;

            // Act
            Action act = () => parser.ThrowIfNullOrWhiteSpace();

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage($"Value cannot be null. (Parameter '{nameof(parser)}')");
        }

        [Fact]
        public void ThrowIfNull_WithNotNullVariable_ShouldNotThrowException()
        {
            // Arrange
            CallbackQueryParser parser = new CallbackQueryParser("1|2");

            // Act
            Action act = () => parser.ThrowIfNull();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void ThrowIfNullOrWhiteSpace_WithNotNullVariable_ShouldNotThrowException()
        {
            // Arrange
            CallbackQueryParser parser = new CallbackQueryParser("1|2");

            // Act
            Action act = () => parser.ThrowIfNullOrWhiteSpace();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void ThrowIfNullOrWhiteSpace_WithNullStrVariable_ShouldThrowArgumentNullExceptionWithParamNameInMessage()
        {
            // Arrange
            string str = string.Empty;

            // Act
            Action act = () => str.ThrowIfNullOrWhiteSpace();

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage($"Value cannot be null. (Parameter '{nameof(str)}')");
        }

        [Fact]
        public void ThrowIfNullOrWhiteSpace_WithNotNullStrVariable_ShouldNotThrowException()
        {
            // Arrange
            string str = "1";

            // Act
            Action act = () => str.ThrowIfNullOrWhiteSpace();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void ThrowIfNull_WithNullableValueTypeVariable_ShouldThrowArgumentNullExceptionWithParamNameInMessage()
        {
            // Arrange
            int? num = null;

            // Act
            Action act = () => num.ThrowIfNull();

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage($"Value cannot be null. (Parameter '{nameof(num)}')");
        }

        [Fact]
        public void ThrowIfNull_WithNullableValueTypeVariable_ShouldNotThrowException()
        {
            // Arrange
            int? num = 0;

            // Act
            Action act = () => num.ThrowIfNull();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void HasItemsInCollection_WithNotEmptyCollection_ShouldReturnTrue()
        {
            // Arrange
            List<int> collection = new List<int>() { 0 };

            // Act
            bool result = collection.HasItemsInCollection();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void HasItemsInCollection_WithEmptyCollection_ShouldReturnFalse()
        {
            // Arrange
            List<int> collection = new List<int>();

            // Act
            bool result = collection.HasItemsInCollection();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void HasItemsInCollection_WithNotEmptyEnumerable_ShouldReturnTrue()
        {
            // Arrange
            IEnumerable<int> collection = new List<int>() { 0 };

            // Act
            bool result = collection.HasItemsInCollection();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void HasItemsInCollection_WithEmptyEnumerable_ShouldReturnFalse()
        {
            // Arrange
            IEnumerable<int> collection = new List<int>();

            // Act
            bool result = collection.HasItemsInCollection();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void TryParseToEnum_WithExistingEnumName_ShouldReturnExistingEnum()
        {
            // Arrange
            string buttonsSetStr = "Main1";

            // Act
            bool result = buttonsSetStr.TryParseToEnum(out ButtonsSet value);

            // Assert
            result.Should().BeFalse();
            value.Should().Be(ButtonsSet.None);
        }

        [Fact]
        public void TryParseToEnum_WithoutExistingEnumName_ShouldReturnDefaultEnum()
        {
            // Arrange
            string buttonsSetStr = "Main";

            // Act
            bool result = buttonsSetStr.TryParseToEnum(out ButtonsSet value);

            // Assert
            result.Should().BeTrue();
            value.Should().Be(ButtonsSet.Main);
        }
    }
}