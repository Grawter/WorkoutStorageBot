using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class SettingCyclesBF : ButtonsFactory
    {
        internal SettingCyclesBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Добавить новый цикл", $"2|Add|{CommonConsts.DomainsAndEntities.Cycle}");
            AddInlineButton("Настройка существующих циклов", $"2|SettingExisting|{CommonConsts.DomainsAndEntities.Cycles}");
            AddInlineButton("Вернуться к главному меню", "0|ToMain");
        }
    }
}