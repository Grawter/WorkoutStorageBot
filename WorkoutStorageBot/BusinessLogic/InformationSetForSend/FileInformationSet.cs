using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Core.Extensions;

namespace WorkoutStorageBot.BusinessLogic.InformationSetForSend
{
    internal class FileInformationSet : IInformationSet
    {
        internal FileInformationSet(Stream stream, string fileName, string message, ParseMode parseMode = ParseMode.Html)
        {
            Stream = stream;
            FileName = fileName;
            Message = message.ThrowIfNullOrWhiteSpace();
            ParseMode = parseMode;
        }

        internal FileInformationSet(Stream stream, string fileName, string message, (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) buttonsSets, ParseMode parseMode = ParseMode.Html) 
            : this(stream, fileName, message, parseMode)
        {
            ButtonsSets = buttonsSets;
        }

        internal FileInformationSet(Stream stream,
                                    string fileName,
                                    string message,
                                    (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) buttonsSets,
                                    Dictionary<string, string> additionalParameters, ParseMode parseMode = ParseMode.Html) : this(stream, fileName, message, buttonsSets, parseMode)
        {
            AdditionalParameters = additionalParameters;
        }

        public string Message { get; }
        public ParseMode ParseMode { get; }
        public (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) ButtonsSets { get; }
        public Dictionary<string, string>? AdditionalParameters { get; }

        internal string FileName { get; }
        internal Stream Stream { get; }
    }
}