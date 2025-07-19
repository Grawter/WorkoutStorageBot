#region using

using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.SessionContext;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class AdminLogsBF : ButtonsFactory
    {
        public AdminLogsBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
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