using FluentAssertions;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Helpers.SharedBusinessLogic;
using WorkoutStorageBot.Model.DTO.InformationSetForSend;

namespace WorkoutStorageBot.UnitTests.BusinessLogic.Helpers.SharedBusinessLogic
{
    public class SharedCommonLogicHelperTests
    {
        [Fact]
        public void GetAccessDeniedMessageInformationSet_ShouldReturnExpectedIInformationSet()
        {
            // Arrange && Act
            IInformationSet informationSet = SharedCommonLogicHelper.GetAccessDeniedMessageInformationSet();

            // Assert
            informationSet.Should().BeOfType<MessageInformationSet>();
            informationSet.Message.Should().Be("Отказано в действии");
            informationSet.ParseMode.Should().Be(Telegram.Bot.Types.Enums.ParseMode.Html);
            informationSet.ButtonsSets.Should().Be((ButtonsSet.Main, ButtonsSet.None));
            informationSet.AdditionalParameters.Should().BeNull();
        }
    }
}