using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class ExercisesListWithLastWorkoutForDayBF : ButtonsFactory
    {
        internal ExercisesListWithLastWorkoutForDayBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Последние результаты выбранного дня", $"1|LastResults|{CommonConsts.DomainsAndEntities.Day}");
            AddInlineButton("Найти тренировку этого дня по дате", $"1|StartFindResultsByDate|{CommonConsts.DomainsAndEntities.Day}");
            GetDomainsInButtons(CurrentUserContext.DataManager.CurrentDay?.Exercises.Where(e => !e.IsArchive), "Selected");
        }
    }
}