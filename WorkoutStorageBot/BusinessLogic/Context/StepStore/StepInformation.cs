#region using
using WorkoutStorageBot.BusinessLogic.Enums;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Context.StepStore
{
    internal class StepInformation
    {
        internal QueryFrom QueryFrom { get; private set; }
        internal string Message { get; private set; }
        internal ButtonsSet ButtonsSet { get; private set; }
        internal ButtonsSet BackButtonsSet { get; private set; }

        internal StepInformation(QueryFrom queryFrom, string message, ButtonsSet buttonsSet, ButtonsSet backButtonsSet)
        {
            QueryFrom = queryFrom;
            Message = message;
            ButtonsSet = buttonsSet;
            BackButtonsSet = backButtonsSet;
        }
    }
}