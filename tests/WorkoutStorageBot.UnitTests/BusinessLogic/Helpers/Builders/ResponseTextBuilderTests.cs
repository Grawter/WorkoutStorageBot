using FluentAssertions;
using WorkoutStorageBot.BusinessLogic.Helpers.Converters;

namespace WorkoutStorageBot.UnitTests.BusinessLogic.Helpers.Builders
{
    public class ResponseTextBuilderTests
    {
        private const string defaultSepartor = "======================";

        [Fact]
        public void Build_WhenOnlyTarget_ShouldReturnOnlyTarget()
        {
            // Arrange
            ResponseTextBuilder builder = new ResponseTextBuilder("TARGET");

            // Act
            string result = builder.Build();

            // Assert
            result.Should().Be("TARGET");
        }

        [Fact]
        public void ResetTitle_WhenOnlyTarget_ShouldThrow()
        {
            // Arrange
            ResponseTextBuilder builder = new ResponseTextBuilder("TARGET");

            // Act
            Action act = () => builder.ResetTitle("TITLE");

            // Assert
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("*onlyTarget*");
        }

        [Fact]
        public void ResetContent_WhenOnlyTarget_ShouldThrow()
        {
            // Arrange
            ResponseTextBuilder builder = new ResponseTextBuilder("TARGET");

            // Act
            Action act = () => builder.ResetContent("CONTENT");

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Build_WithContentAndTarget_ShouldUseDefaultSeparator()
        {
            // Arrange
            ResponseTextBuilder builder = new ResponseTextBuilder("CONTENT", "TARGET");

            // Act
            string result = builder.Build();

            // Assert
            result.Should().Be(
$@"CONTENT
{defaultSepartor}

TARGET
");
        }

        [Fact]
        public void Build_WithTitleContentAndTarget_ShouldBuildFullText()
        {
            // Arrange
            ResponseTextBuilder builder = new ResponseTextBuilder(
                title: "TITLE",
                content: "CONTENT",
                target: "TARGET"
            );

            // Act
            string result = builder.Build();

            // Assert
            result.Should().Be(
$@"TITLE
{defaultSepartor}
CONTENT
{defaultSepartor}

TARGET
");
        }

        [Fact]
        public void Build_WithCustomSeparator_ShouldUseIt()
        {
            // Arrange
            var builder = new ResponseTextBuilder(
                title: "TITLE",
                content: "CONTENT",
                target: "TARGET",
                separator: "---"
            );

            // Act
            string result = builder.Build();

            // Assert
            result.Should().Be(
@"TITLE
---
CONTENT
---

TARGET
");
        }

        [Fact]
        public void ResetTitle_ShouldChangeTitle()
        {
            // Arrange
            ResponseTextBuilder builder = new ResponseTextBuilder("TITLE", "CONTENT", "TARGET");

            // Act
            string result = builder
                .ResetTitle("NEW TITLE")
                .Build();

            // Assert
            result.Should().Be(
$@"NEW TITLE
{defaultSepartor}
CONTENT
{defaultSepartor}

TARGET
");
        }

        [Fact]
        public void ResetContent_ShouldChangeContent()
        {
            // Arrange
            ResponseTextBuilder builder = new ResponseTextBuilder("TITLE", "CONTENT", "TARGET");

            // Act
            string result = builder
                .ResetContent("NEW CONTENT")
                .Build();

            // Assert
            result.Should().Be(
$@"TITLE
{defaultSepartor}
NEW CONTENT
{defaultSepartor}

TARGET
");
        }

        [Fact]
        public void ResetTarget_ShouldChangeTarget()
        {
            // Arrange
            ResponseTextBuilder builder = new ResponseTextBuilder("CONTENT", "TARGET");

            // Act
            string result = builder
                .ResetTarget("NEW TARGET")
                .Build();

            // Assert
            result.Should().Be(
$@"CONTENT
{defaultSepartor}

NEW TARGET
");
        }

        [Fact]
        public void Build_WhenContentIsNull_ShouldResetTitle()
        {
            // Arrange
            ResponseTextBuilder builder = new ResponseTextBuilder("TITLE", "CONTENT", "TARGET");
                
            // Act
            string result = builder
                .ResetTitle(string.Empty)
                .Build();

            // Assert
            result.Should().Be(
$@"CONTENT
{defaultSepartor}

TARGET
");
        }

        [Fact]
        public void Build_WhenContentIsNull_ShouldResetContent()
        {
            // Arrange
            ResponseTextBuilder builder = new ResponseTextBuilder("TITLE", "CONTENT", "TARGET");

            // Act
            string result = builder
                .ResetContent(string.Empty)
                .Build();

            // Assert
            result.Should().Be(
$@"TITLE
{defaultSepartor}

TARGET
");
        }

        [Fact]
        public void Build_WhenContentIsNull_ShouldResetTarget()
        {
            // Arrange
            ResponseTextBuilder builder = new ResponseTextBuilder("TITLE", "CONTENT", "TARGET");

            // Act
            string result = builder
                .ResetTarget(string.Empty)
                .Build();

            // Assert
            result.Should().Be(
$@"TITLE
{defaultSepartor}
CONTENT
{defaultSepartor}
");
        }
    }
}