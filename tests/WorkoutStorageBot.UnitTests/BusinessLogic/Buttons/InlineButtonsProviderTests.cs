using FluentAssertions;
using Telegram.Bot.Types.ReplyMarkups;
using WorkoutStorageBot.BusinessLogic.Buttons.Provider;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.UnitTests.Helpers;
using WorkoutStorageBot.BusinessLogic.Enums;


namespace WorkoutStorageBot.UnitTests.BusinessLogic.Buttons
{
    public class InlineButtonsProviderTests
    {
        [Fact]
        public void GetInlineButtons_WithNoneButtonsSet_ShouldReturnExistingReplyMarkup() 
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();
            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;
            UserContext userContext = new UserContext(DTOUserInformation, Roles.User, false);

            InlineButtonsProvider inlineButtonsProvider = new InlineButtonsProvider(userContext);

            // Act
            InlineKeyboardMarkup buttons = (InlineKeyboardMarkup)inlineButtonsProvider.GetInlineButtons(ButtonsSet.None);

            // Assert
            buttons.InlineKeyboard.Should().HaveCount(0);
            inlineButtonsProvider.AllHorizontalButtonsDisplayed.Should().BeTrue();
            inlineButtonsProvider.AllVerticalButtonsDisplayed.Should().BeTrue();
        }

        [Fact]
        public void GetInlineButtons_WithNoneButtonsSetAndMainBackButtonSet_ShouldReturnExistingReplyMarkup()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();
            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;
            UserContext userContext = new UserContext(DTOUserInformation, Roles.User, false);
            
            InlineButtonsProvider inlineButtonsProvider = new InlineButtonsProvider(userContext);

            // Act
            InlineKeyboardMarkup buttons = (InlineKeyboardMarkup)inlineButtonsProvider.GetInlineButtons((ButtonsSet.None, ButtonsSet.Main));

            // Assert
            buttons.InlineKeyboard.Should().HaveCount(1);
            inlineButtonsProvider.AllHorizontalButtonsDisplayed.Should().BeTrue();
            inlineButtonsProvider.AllVerticalButtonsDisplayed.Should().BeTrue();
        }

        [Fact]
        public void GetInlineButtons_WithHorizontalButtonsMoreThanExpectedTestButtonsSet_ShouldReturnExistingReplyMarkup()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();
            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;
            UserContext userContext = new UserContext(DTOUserInformation, Roles.User, false);
            
            InlineButtonsProvider inlineButtonsProvider = new InlineButtonsProvider(userContext);

            // Act
            InlineKeyboardMarkup buttons = (InlineKeyboardMarkup)inlineButtonsProvider.GetInlineButtons(ButtonsSet.HorizontalButtonsMoreThanExpectedTest);

            // Assert
            inlineButtonsProvider.AllHorizontalButtonsDisplayed.Should().BeFalse();
            buttons.InlineKeyboard.Single().Should().HaveCount(CommonConsts.Buttons.MaxHorizontalButtonsCount);
            inlineButtonsProvider.AllVerticalButtonsDisplayed.Should().BeTrue();
        }

        [Fact]
        public void GetInlineButtons_WithVerticalButtonsMoreThanExpectedTestButtonsSet_ShouldReturnExistingReplyMarkup()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();
            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;
            UserContext userContext = new UserContext(DTOUserInformation, Roles.User, false);

            InlineButtonsProvider inlineButtonsProvider = new InlineButtonsProvider(userContext);

            // Act
            InlineKeyboardMarkup buttons = (InlineKeyboardMarkup)inlineButtonsProvider.GetInlineButtons(ButtonsSet.VerticalButtonsMoreThanExpectedTest);

            // Assert
            inlineButtonsProvider.AllHorizontalButtonsDisplayed.Should().BeTrue();
            buttons.InlineKeyboard.Should().HaveCount(CommonConsts.Buttons.MaxVerticalButtonsCount);
            inlineButtonsProvider.AllVerticalButtonsDisplayed.Should().BeFalse();
        }

        [Fact]
        public void GetInlineButtons_WithHorizontalAndVerticalButtonsMoreThanExpectedButtonsSet_ShouldReturnExistingReplyMarkup()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();
            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;
            UserContext userContext = new UserContext(DTOUserInformation, Roles.User, false);

            InlineButtonsProvider inlineButtonsProvider = new InlineButtonsProvider(userContext);

            // Act
            InlineKeyboardMarkup buttons = (InlineKeyboardMarkup)inlineButtonsProvider.GetInlineButtons(ButtonsSet.HorizontalAndVerticalButtonsMoreThanExpectedTest);

            // Assert
            inlineButtonsProvider.AllHorizontalButtonsDisplayed.Should().BeFalse();
            buttons.InlineKeyboard.First().Should().HaveCount(CommonConsts.Buttons.MaxHorizontalButtonsCount);
            inlineButtonsProvider.AllVerticalButtonsDisplayed.Should().BeFalse();
            buttons.InlineKeyboard.Should().HaveCount(CommonConsts.Buttons.MaxVerticalButtonsCount);
        }

        [Fact]
        public void GetInlineButtons_WithPeriodButtonsSetAndCorrectAdditionalParameters_ShouldReturnExistingReplyMarkup()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();
            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;
            UserContext userContext = new UserContext(DTOUserInformation, Roles.User, false);

            InlineButtonsProvider inlineButtonsProvider = new InlineButtonsProvider(userContext);

            Dictionary<string, string> additionalParameters = new Dictionary<string, string>() { { "Act", "SomeValue" }, };

            // Act
            InlineKeyboardMarkup buttons = (InlineKeyboardMarkup)inlineButtonsProvider.GetInlineButtons(ButtonsSet.Period, additionalParameters);

            // Assert
            buttons.InlineKeyboard.Should().HaveCount(5);
        }

        [Fact]
        public void GetInlineButtons_WithPeriodButtonsSetAndIncorrectAdditionalParameters_ShouldThrowException()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();
            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;
            UserContext userContext = new UserContext(DTOUserInformation, Roles.User, false);

            InlineButtonsProvider inlineButtonsProvider = new InlineButtonsProvider(userContext);

            Dictionary<string, string> additionalParameters = new Dictionary<string, string>() { { "IncorrectKeyParam", "SomeValue" }, };

            // Act
            Action act = () => inlineButtonsProvider.GetInlineButtons(ButtonsSet.Period);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetInlineButtons_WithPeriodButtonsSet_ShouldThrowException()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();
            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;
            UserContext userContext = new UserContext(DTOUserInformation, Roles.User, false);

            InlineButtonsProvider inlineButtonsProvider = new InlineButtonsProvider(userContext);

            // Act
            Action act = () => inlineButtonsProvider.GetInlineButtons(ButtonsSet.Period);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}