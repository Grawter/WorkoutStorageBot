using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class SettingDaysBF : ButtonsFactory
    {
        internal SettingDaysBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Добавить новый день в цикл", $"2|Add|{CommonConsts.DomainsAndEntities.Day}");
            AddInlineButton("Настройка существующих дней", $"2|SettingExisting|{CommonConsts.DomainsAndEntities.Days}");
            AddInlineButton("Вернуться к главному меню", "0|ToMain");
        }
    }
}