using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class ChangeTypeBF : ButtonsFactory
    {
        internal ChangeTypeBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Режим: только повторения", "2|ChangedMode|Exercise|0");
            AddInlineButton("Режим: повторения и вес", "2|ChangedMode|Exercise|1");
            AddInlineButton("Режим: таймер", "2|ChangedMode|Exercise|2");
            AddInlineButton("Режим: свободного формата результата", "2|ChangedMode|Exercise|3");
        }
    }
}