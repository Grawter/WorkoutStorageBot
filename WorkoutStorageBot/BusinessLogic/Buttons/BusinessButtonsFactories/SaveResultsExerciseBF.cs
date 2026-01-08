using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class SaveResultsExerciseBF : ButtonsFactory
    {
        public SaveResultsExerciseBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Сохранить результаты", "1|SaveResultsExercise");
            AddInlineButton("Сбросить результаты", "1|ResetResultsExercise");
        }
    }
}