using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class SettingsBF : ButtonsFactory
    {
        internal SettingsBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Настройка тренировочных циклов", $"2|Setting|{CommonConsts.DomainsAndEntities.Cycles}");
            AddInlineButton("Архив", "2|ArchiveStore");
            AddInlineButton("Экспорт тренировок", "2|Export");
            AddInlineButton("Удалить свой аккаунт", $"2|Delete|{CommonConsts.DomainsAndEntities.Account}");
            AddInlineButton("О боте", "2|AboutBot");
        }
    }
}