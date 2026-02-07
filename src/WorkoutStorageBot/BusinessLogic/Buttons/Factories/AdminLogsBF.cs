using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class AdminLogsBF : ButtonsFactory
    {
        internal AdminLogsBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            if (CurrentUserContext.Roles == Roles.Admin)
            {
                AddInlineButton("Показать последний лог", "3|ShowLastLog");
                AddInlineButton("Показать последние логи ошибок", "3|ShowLastExceptionLogs");
                AddInlineButton("Найти лог по ID", "3|FindLogByID");
                AddInlineButton("Найти лог по eventID", "3|FindLogByEventID");
            }
        }
    }
}