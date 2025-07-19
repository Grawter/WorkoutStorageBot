#region using

using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.SessionContext;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class SettingCyclesBF : ButtonsFactory
    {
        public SettingCyclesBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Добавить новый цикл", "2|Add|Cycle");
            AddInlineButton("Настройка существующих циклов", "2|SettingExisting|Cycles");
            AddInlineButton("Вернуться к главному меню", "0|ToMain");
        }
    }
}