using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class RedirectAfterSaveExerciseBF : ButtonsFactory
    {
        internal RedirectAfterSaveExerciseBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton($"Добавить новый день", $"2|Add|{CommonConsts.DomainsAndEntities.Day}");
            AddInlineButton($"Перейти в главное меню", "0|ToMain");
        }
    }
}