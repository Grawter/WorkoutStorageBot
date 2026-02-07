using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class SettingExercisesBF : ButtonsFactory
    {
        internal SettingExercisesBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Добавить новые упражнения в день", $"2|Add|{CommonConsts.DomainsAndEntities.Exercise}");
            AddInlineButton("Настройка существующих упражнений", $"2|SettingExisting|{CommonConsts.DomainsAndEntities.Exercises}");
            AddInlineButton("Вернуться к главному меню", "0|ToMain");
        }
    }
}