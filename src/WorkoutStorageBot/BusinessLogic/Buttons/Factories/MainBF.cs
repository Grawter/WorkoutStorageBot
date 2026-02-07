using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class MainBF : ButtonsFactory
    {
        internal MainBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton($"Начать тренировку", "1|Workout");
            AddInlineButton($"Настройки", "2|Settings");

            if (CurrentUserContext.Roles == Roles.Admin)
                AddInlineButton("Админка", "3|Admin");
        }
    }
}