using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class SaveResultsExerciseBF : ButtonsFactory
    {
        internal SaveResultsExerciseBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Сохранить результаты", "1|SaveResultsExercise");
            AddInlineButton("Сбросить результаты", "1|ResetResultsExercise");
        }
    }
}