using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class AdminUsersBF : ButtonsFactory
    {
        internal AdminUsersBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            if (CurrentUserContext.Roles == Roles.Admin)
            {
                AddInlineButton("Показать кол-во активных сессий", "3|ShowCountActiveSessions");
                AddInlineButton("Отослать сообщение пользователю", "3|SendMessageToUser");
                AddInlineButton("Разослать сообщение по активным сессиям", "3|SendMessagesToActiveUsers");
                AddInlineButton("Разослать сообщение всем пользователям", "3|SendMessagesToAllUsers");
                AddInlineButton("Сменить white/black list у пользователя", "3|ChangeUserState");
                AddInlineButton("Удалить пользователя", "3|RemoveUser");
                AddInlineButton("Вернуться к главному меню", "0|ToMain");
            }
        }
    }
}