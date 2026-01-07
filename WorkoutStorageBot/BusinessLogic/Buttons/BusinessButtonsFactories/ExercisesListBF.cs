#region using

using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class ExercisesListBF : ButtonsFactory
    {
        public ExercisesListBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            GetDomainsInButtons(CurrentUserContext.DataManager.CurrentDay.Exercises.Where(e => !e.IsArchive), "Selected");
        }
    }
}