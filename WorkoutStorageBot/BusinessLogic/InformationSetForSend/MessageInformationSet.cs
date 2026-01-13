using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Core.Helpers;

namespace WorkoutStorageBot.BusinessLogic.InformationSetForSend
{
    internal class MessageInformationSet : IInformationSet
    {
        internal MessageInformationSet(string message)
        {
            Message = message;
        }

        internal MessageInformationSet(string message, (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) buttonsSets) : this(message)
        {
            ButtonsSets = buttonsSets;
        }

        internal MessageInformationSet(string message,
                                      (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) buttonsSets,
                                      Dictionary<string, string> additionalParameters) : this(message, buttonsSets)
        {
            AdditionalParameters = CommonHelper.GetIfNotNull(additionalParameters);
        }

        public string Message { get; }
        public (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) ButtonsSets { get; }
        public Dictionary<string, string> AdditionalParameters { get; } = new Dictionary<string, string>();
    }
}