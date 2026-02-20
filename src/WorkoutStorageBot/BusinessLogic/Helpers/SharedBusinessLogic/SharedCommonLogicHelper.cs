using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Helpers.Converters;
using WorkoutStorageBot.Model.DTO.InformationSetForSend;

namespace WorkoutStorageBot.BusinessLogic.Helpers.SharedBusinessLogic
{
    /// <summary>
    /// Повторяющаяся бизнес-логика, которая может быть применима в разных CommandHandler
    /// </summary>
    internal static class SharedCommonLogicHelper
    {
        internal static MessageInformationSet GetAccessDeniedMessageInformationSet()
        {
            ResponseTextBuilder responseConverter = new ResponseTextBuilder("Отказано в действии");

            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Main, ButtonsSet.None);

            return new MessageInformationSet(responseConverter.Build(), buttonsSets);
        }
    }
}