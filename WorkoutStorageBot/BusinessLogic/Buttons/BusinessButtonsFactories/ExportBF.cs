#region using

using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class ExportBF : ButtonsFactory
    {
        public ExportBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Экспортировать тренировки в Excel", "2|ExportTo||Excel");
            AddInlineButton("Экспортировать тренировки в JSON", "2|ExportTo||JSON");
        }
    }
}