using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class MainBF : ButtonsFactory
    {
        public MainBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton($"Начать тренировку", "1|Workout");
            AddInlineButton($"Настройки", "2|Settings");

            if (CurrentUserContext.Roles == Roles.Admin)
                AddInlineButton("Админка", "3|Admin");
        }
    }
}