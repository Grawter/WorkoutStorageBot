using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class DaysListWithLastWorkoutBF : ButtonsFactory
    {
        internal DaysListWithLastWorkoutBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Последняя тренировка", $"1|LastResults|{CommonConsts.DomainsAndEntities.Exercises}");
            AddInlineButton("Найти тренировку по дате", $"1|StartFindResultsByDate|{CommonConsts.DomainsAndEntities.Exercises}");
            GetDomainsInButtons(CurrentUserContext.ActiveCycle?.Days.Where(d => !d.IsArchive), "Selected");
        }
    }
}