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

        internal MessageInformationSet(string message, (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) buttonsSets, params string[] additionalParameters)
        {
            Message = message;
            ButtonsSets = buttonsSets;
            AdditionalParameters = additionalParameters;
        }

        public string Message { get; }
        public (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) ButtonsSets { get; }
        public string[] AdditionalParameters { get; }
    }
}