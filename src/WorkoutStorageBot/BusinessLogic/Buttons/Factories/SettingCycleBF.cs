using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class SettingCycleBF : ButtonsFactory
    {
        internal SettingCycleBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Сделать активным", $"2|ChangeActive|{CommonConsts.DomainsAndEntities.Cycle}");
            AddInlineButton("Сменить название", $"2|ChangeName|{CommonConsts.DomainsAndEntities.Cycle}");
            AddInlineButton("Добавить в архив", $"2|Archiving|{CommonConsts.DomainsAndEntities.Cycle}");
            AddInlineButton("Удалить", $"2|Delete|{CommonConsts.DomainsAndEntities.Cycle}");
            AddInlineButton("Настройка дней", $"2|Setting|{CommonConsts.DomainsAndEntities.Days}");
            AddInlineButton("Вернуться к главному меню", "0|ToMain");
        }
    }
}