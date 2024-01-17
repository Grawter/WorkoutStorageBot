#region using
using WorkoutStorageBot.BusinessLogic.Enums;
#endregion

namespace WorkoutStorageBot.Helpers.InformationSetForSend
{
    internal class MessageInformationSet : IInformationSet
    {
        internal MessageInformationSet(string message)
        {
            Message = message;
        }

        internal MessageInformationSet(string message, (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) buttonsSets)
        {
            Message = message;
            ButtonsSets = buttonsSets;
        }

        internal string Message { get; set; }
        internal (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) ButtonsSets { get; set; }
    }
}