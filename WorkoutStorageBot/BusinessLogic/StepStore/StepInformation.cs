#region using
using WorkoutStorageBot.BusinessLogic.Enums;
#endregion

namespace WorkoutStorageBot.BusinessLogic.StepStore
{
    internal class StepInformation
    {
        internal NavigationType NavigationType { get; private set; }
        internal string Message { get; private set; }
        internal ButtonsSet ButtonsSet { get; private set; }
        internal ButtonsSet BackButtonsSet { get; private set; }

        internal StepInformation(NavigationType navigationType, string message, ButtonsSet buttonsSet, ButtonsSet backButtonsSet)
        {
            NavigationType = navigationType;
            Message = message;
            ButtonsSet = buttonsSet;
            BackButtonsSet = backButtonsSet;
        }
    }
}