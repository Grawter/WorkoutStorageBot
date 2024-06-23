#region using
using WorkoutStorageBot.BusinessLogic.Enums;
#endregion

namespace WorkoutStorageBot.BusinessLogic.InformationSetForSend
{
    internal class MessageInformationSet : IInformationSet
    {
        internal MessageInformationSet(string message)
        {
            Message = message;
        }

        internal MessageInformationSet(string message, (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) buttonsSets, Dictionary<string, string> additionalParameters = default)
        {
            Message = message;
            ButtonsSets = buttonsSets;
            AdditionalParameters = additionalParameters;
        }

        public string Message { get; }
        public (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) ButtonsSets { get; }
        public Dictionary<string, string> AdditionalParameters { get; }
    }
}