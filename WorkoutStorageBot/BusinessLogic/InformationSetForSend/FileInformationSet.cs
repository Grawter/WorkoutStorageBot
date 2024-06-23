using WorkoutStorageBot.BusinessLogic.Enums;

namespace WorkoutStorageBot.BusinessLogic.InformationSetForSend
{
    internal class FileInformationSet : IInformationSet
    {
        internal FileInformationSet(Stream stream, string fileName)
        {
            Stream = stream;
            FileName = fileName;
        }

        internal FileInformationSet(Stream stream,
                                    string fileName,
                                    string message,
                                    (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) buttonsSets,
                                    Dictionary<string, string> additionalParameters = default)
        {
            Stream = stream;
            FileName = fileName;
            Message = message;
            ButtonsSets = buttonsSets;
            AdditionalParameters = additionalParameters;
        }

        public string Message { get; }
        public (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) ButtonsSets { get; }
        public Dictionary<string, string> AdditionalParameters { get; }

        internal string FileName { get; }
        internal Stream Stream { get; }
    }
}