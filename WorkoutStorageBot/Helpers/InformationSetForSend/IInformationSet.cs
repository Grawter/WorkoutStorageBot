using WorkoutStorageBot.BusinessLogic.Enums;

namespace WorkoutStorageBot.Helpers.InformationSetForSend
{
    internal interface IInformationSet
    {
        string Message { get; }
        (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) ButtonsSets { get; }
        string[] AdditionalParameters { get; }
    }
}