using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class AdminBF : ButtonsFactory
    {
        public AdminBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            if (CurrentUserContext.Roles == Roles.Admin)
            {
                AddInlineButton("Логи", "3|Logs");
                AddInlineButton("Показать стартовую настройку", "3|ShowStartConfiguration");
                AddInlineButton("Сменить режим использования лимитов", "3|ChangeLimitsMods");
                AddInlineButton("Сменить режим белого списка", "3|ChangeWhiteListMode");
                AddInlineButton("Сменить white/black list у пользователя", "3|ChangeUserState");
                AddInlineButton("Удалить пользователя", "3|RemoveUser");
                AddInlineButton("Выключить бота", "3|DisableBot");
            }
        }
    }
}