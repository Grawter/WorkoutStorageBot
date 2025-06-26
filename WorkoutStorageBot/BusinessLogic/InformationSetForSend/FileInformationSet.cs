#region using
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Helpers.Common;
#endregion

namespace WorkoutStorageBot.BusinessLogic.InformationSetForSend
{
    internal class FileInformationSet : IInformationSet
    {
        internal FileInformationSet(Stream stream, string fileName)
        {
            Stream = CommonHelper.GetIfNotNull(stream);
            FileName = CommonHelper.GetIfNotNullOrWhiteSpace(fileName);
        }

        internal FileInformationSet(Stream stream, string fileName, string message, (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) buttonsSets) 
            : this(stream, fileName)
        {
            Message = message;
            ButtonsSets = buttonsSets;
        }

        internal FileInformationSet(Stream stream,
                                    string fileName,
                                    string message,
                                    (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) buttonsSets,
                                    Dictionary<string, string> additionalParameters) : this(stream, fileName, message, buttonsSets)
        {
            AdditionalParameters = CommonHelper.GetIfNotNull(additionalParameters);
        }

        public string Message { get; }
        public (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) ButtonsSets { get; }
        public Dictionary<string, string> AdditionalParameters { get; } = new Dictionary<string, string>();

        internal string FileName { get; }
        internal Stream Stream { get; }
    }
}