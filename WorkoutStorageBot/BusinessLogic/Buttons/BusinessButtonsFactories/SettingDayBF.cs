#region using

using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class SettingDayBF : ButtonsFactory
    {
        public SettingDayBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Сменить название", $"2|ChangeName|{CommonConsts.DomainsAndEntities.Day}");
            AddInlineButton("Перенести день", $"2|Replace|{CommonConsts.DomainsAndEntities.Day}");
            AddInlineButton("Добавить в архив", $"2|Archiving|{CommonConsts.DomainsAndEntities.Day}");
            AddInlineButton("Удалить", $"2|Delete|{CommonConsts.DomainsAndEntities.Day}");
            AddInlineButton("Настройка упражнений", $"2|Setting|{CommonConsts.DomainsAndEntities.Exercises}");
            AddInlineButton("Вернуться к главному меню", "0|ToMain");
        }
    }
}