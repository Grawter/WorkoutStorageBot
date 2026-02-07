using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class ResetResultsExerciseBF : ButtonsFactory
    {
        internal ResetResultsExerciseBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Сбросить результаты", "1|ResetResultsExercise");
        }
    }
}