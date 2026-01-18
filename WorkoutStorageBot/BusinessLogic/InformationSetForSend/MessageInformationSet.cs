using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Core.Extensions;

namespace WorkoutStorageBot.BusinessLogic.InformationSetForSend
{
    internal class MessageInformationSet : IInformationSet
    {
        internal MessageInformationSet(string message)
        {
            Message = message.ThrowIfNullOrWhiteSpace();
        }

        internal MessageInformationSet(string message, (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) buttonsSets) : this(message)
        {
            ButtonsSets = buttonsSets;
        }

        internal MessageInformationSet(string message,
                                      (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) buttonsSets,
                                      Dictionary<string, string> additionalParameters) : this(message, buttonsSets)
        {
            AdditionalParameters = additionalParameters;
        }

        public string Message { get; }
        public (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) ButtonsSets { get; }
        public Dictionary<string, string>? AdditionalParameters { get; }
    }
}