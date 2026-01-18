using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Core.Extensions;

namespace WorkoutStorageBot.BusinessLogic.InformationSetForSend
{
    internal class FileInformationSet : IInformationSet
    {
        internal FileInformationSet(Stream stream, string fileName, string message)
        {
            Stream = stream;
            FileName = fileName;
            Message = message.ThrowIfNullOrWhiteSpace();
        }

        internal FileInformationSet(Stream stream, string fileName, string message, (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) buttonsSets) 
            : this(stream, fileName, message)
        {
            ButtonsSets = buttonsSets;
        }

        internal FileInformationSet(Stream stream,
                                    string fileName,
                                    string message,
                                    (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) buttonsSets,
                                    Dictionary<string, string> additionalParameters) : this(stream, fileName, message, buttonsSets)
        {
            AdditionalParameters = additionalParameters;
        }

        public string Message { get; }
        public (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) ButtonsSets { get; }
        public Dictionary<string, string>? AdditionalParameters { get; }

        internal string FileName { get; }
        internal Stream Stream { get; }
    }
}