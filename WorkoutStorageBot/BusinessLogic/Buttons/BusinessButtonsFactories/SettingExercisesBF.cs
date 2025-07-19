#region using

using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.SessionContext;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class SettingExercisesBF : ButtonsFactory
    {
        public SettingExercisesBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Добавить новые упражнения в день", "2|Add|Exercise");
            AddInlineButton("Настройка существующих упражнений", "2|SettingExisting|Exercises");
            AddInlineButton("Вернуться к главному меню", "0|ToMain");
        }
    }
}