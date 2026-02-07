using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class SaveExercisesBF : ButtonsFactory
    {
        internal SaveExercisesBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton($"Сохранить упражнения", "2|SaveExercises");
            AddInlineButton($"Сбросить добавленные упражнения", $"2|ResetTempDomains|{CommonConsts.DomainsAndEntities.Exercise}");
        }
    }
}