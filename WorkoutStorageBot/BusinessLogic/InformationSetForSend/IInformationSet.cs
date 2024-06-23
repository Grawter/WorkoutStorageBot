using WorkoutStorageBot.BusinessLogic.Enums;

namespace WorkoutStorageBot.BusinessLogic.InformationSetForSend
{
    internal interface IInformationSet
    {
        string Message { get; }
        (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) ButtonsSets { get; }
        Dictionary<string, string> AdditionalParameters { get; }
    }
}