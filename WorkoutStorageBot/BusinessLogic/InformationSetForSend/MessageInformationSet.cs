using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Core.Extensions;

namespace WorkoutStorageBot.BusinessLogic.InformationSetForSend
{
    internal class MessageInformationSet : IInformationSet
    {
        internal MessageInformationSet(string message, ParseMode parseMode = ParseMode.Html)
        {
            Message = message.ThrowIfNullOrWhiteSpace();
        }

        internal MessageInformationSet(string message, (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) buttonsSets, ParseMode parseMode = ParseMode.Html) : this(message, parseMode)
        {
            ButtonsSets = buttonsSets;
        }

        internal MessageInformationSet(string message,
                                      (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) buttonsSets,
                                      Dictionary<string, string> additionalParameters, ParseMode parseMode = ParseMode.Html) : this(message, buttonsSets, parseMode)
        {
            AdditionalParameters = additionalParameters;
        }

        public string Message { get; }
        public ParseMode ParseMode { get; }
        public (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) ButtonsSets { get; }
        public Dictionary<string, string>? AdditionalParameters { get; }
    }
}