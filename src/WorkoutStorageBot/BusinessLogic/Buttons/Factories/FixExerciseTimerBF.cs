using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class FixExerciseTimerBF : ButtonsFactory
    {
        internal FixExerciseTimerBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Остановить таймер", $"1|StopExerciseTimer");
            AddInlineButton("Показать прошедшее время с момента запуска", $"1|ShowExerciseTimer");
        }
    }
}