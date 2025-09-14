#region using

using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.SessionContext;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class DaysListWithLastWorkoutBF : ButtonsFactory
    {
        public DaysListWithLastWorkoutBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Последняя тренировка", $"1|LastResults|{CommonConsts.DomainsAndEntities.Exercises}");
            GetDomainsInButtons(CurrentUserContext.ActiveCycle.Days.Where(d => !d.IsArchive), "Selected");
        }
    }
}