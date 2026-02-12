using FluentAssertions;
using WorkoutStorageBot.BusinessLogic.Context.StepStore;
using WorkoutStorageBot.BusinessLogic.Enums;

namespace WorkoutStorageBot.UnitTests.BusinessLogic.Context.StepStore
{
    public class StepStorageTests
    {
        [Fact]
        public void GetStep_WithSettingButtonsSet_ShouldReturnSettingButtonsSet()
        {
            // Arrange & Act
            StepInformation stepInformation = StepStorage.GetStep(ButtonsSet.Settings);

            // Assert
            stepInformation.ButtonsSet.Should().Be(ButtonsSet.Settings);
            stepInformation.BackButtonsSet.Should().Be(ButtonsSet.Main);
        }

        [Fact]
        public void GetStep_WithUnExistingButtonsSet_ShouldReturnMainButtonsSet()
        {
            // Arrange & Act
            StepInformation stepInformation = StepStorage.GetStep(ButtonsSet.HorizontalAndVerticalButtonsMoreThanExpectedTest);

            // Assert
            stepInformation.ButtonsSet.Should().Be(ButtonsSet.Main);
            stepInformation.BackButtonsSet.Should().Be(ButtonsSet.None);
        }

        [Fact]
        public void GetMainStep_ShouldReturnMainButtonsSet()
        {
            // Arrange & Act
            StepInformation stepInformation = StepStorage.GetMainStep();

            // Assert
            stepInformation.ButtonsSet.Should().Be(ButtonsSet.Main);
            stepInformation.BackButtonsSet.Should().Be(ButtonsSet.None);
        }
    }
}