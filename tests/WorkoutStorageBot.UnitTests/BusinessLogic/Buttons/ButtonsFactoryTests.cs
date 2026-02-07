using FluentAssertions;
using Telegram.Bot.Types.ReplyMarkups;
using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Buttons.Factories;
using WorkoutStorageBot.BusinessLogic.Buttons.Factories.TestFactories;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.UnitTests.Helpers;

namespace WorkoutStorageBot.UnitTests.BusinessLogic.Buttons
{
    public class ButtonsFactoryTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateButtons_WithEmptyBF_ShouldCreateButtonsCollectionsAndDisplayedAllButtons(bool hasBackButton)
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();
            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;
            UserContext userContext = new UserContext(DTOUserInformation, WorkoutStorageBot.BusinessLogic.Enums.Roles.User, false);

            ButtonsFactory emptyBF = new EmptyBF(userContext);

            // Act
            IEnumerable<IEnumerable<InlineKeyboardButton>> buttonsCollections = hasBackButton 
                ? emptyBF.CreateButtons(WorkoutStorageBot.BusinessLogic.Enums.ButtonsSet.Main)
                : emptyBF.CreateButtons();

            // Assert
            emptyBF.AllHorizontalButtonsDisplayed.Should().BeTrue();
            emptyBF.AllVerticalButtonsDisplayed.Should().BeTrue();

            if (hasBackButton)
                buttonsCollections.Should().HaveCount(1);
            else
                buttonsCollections.Should().HaveCount(0);
        }

        [Fact]
        public void CreateButtons_WithHorizontalButtonsMoreThanExpectedBF_ShouldNotDisplayedAllHorizontalButtons()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();
            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;
            UserContext userContext = new UserContext(DTOUserInformation, WorkoutStorageBot.BusinessLogic.Enums.Roles.User, false);

            ButtonsFactory horizontalButtonsMoreThanExpectedBF = new HorizontalButtonsMoreThanExpectedTestBF(userContext);

            // Act
            IEnumerable<IEnumerable<InlineKeyboardButton>> buttonsCollections = horizontalButtonsMoreThanExpectedBF.CreateButtons();

            // Assert
            horizontalButtonsMoreThanExpectedBF.AllHorizontalButtonsDisplayed.Should().BeFalse();
            buttonsCollections.Single().Should().HaveCount(CommonConsts.Buttons.MaxHorizontalButtonsCount);
            horizontalButtonsMoreThanExpectedBF.AllVerticalButtonsDisplayed.Should().BeTrue();
        }

        [Fact]
        public void CreateButtons_WithVerticalButtonsMoreThanExpectedBF_ShouldNotDisplayedAllVerticalButtons()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();
            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;
            UserContext userContext = new UserContext(DTOUserInformation, WorkoutStorageBot.BusinessLogic.Enums.Roles.User, false);

            ButtonsFactory verticalButtonsMoreThanExpectedBF = new VerticalButtonsMoreThanExpectedTestBF(userContext);
            
            // Act
            IEnumerable<IEnumerable<InlineKeyboardButton>> buttonsCollections = verticalButtonsMoreThanExpectedBF.CreateButtons();

            // Assert
            verticalButtonsMoreThanExpectedBF.AllHorizontalButtonsDisplayed.Should().BeTrue();
            buttonsCollections.Should().HaveCount(CommonConsts.Buttons.MaxVerticalButtonsCount);
            verticalButtonsMoreThanExpectedBF.AllVerticalButtonsDisplayed.Should().BeFalse();
        }

        [Fact]
        public void CreateButtons_WithHorizontalAndVerticalButtonsMoreThanExpectedBF_ShouldNotDisplayedHorizontalAndVerticalButtons()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();
            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;
            UserContext userContext = new UserContext(DTOUserInformation, WorkoutStorageBot.BusinessLogic.Enums.Roles.User, false);

            ButtonsFactory horizontalAndVerticalButtonsMoreThanExpectedBF = new HorizontalAndVerticalButtonsMoreThanExpectedTestBF(userContext);

            // Act
            IEnumerable<IEnumerable<InlineKeyboardButton>> buttonsCollections = horizontalAndVerticalButtonsMoreThanExpectedBF.CreateButtons();

            // Assert
            horizontalAndVerticalButtonsMoreThanExpectedBF.AllHorizontalButtonsDisplayed.Should().BeFalse();
            buttonsCollections.First().Should().HaveCount(CommonConsts.Buttons.MaxHorizontalButtonsCount);
            horizontalAndVerticalButtonsMoreThanExpectedBF.AllVerticalButtonsDisplayed.Should().BeFalse();
            buttonsCollections.Should().HaveCount(CommonConsts.Buttons.MaxVerticalButtonsCount);
        }
    }
}