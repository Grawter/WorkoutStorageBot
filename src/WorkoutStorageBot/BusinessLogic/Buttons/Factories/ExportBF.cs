using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class ExportBF : ButtonsFactory
    {
        internal ExportBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Экспортировать тренировки в Excel", "2|ExportTo||Excel");
            AddInlineButton("Экспортировать тренировки в JSON", "2|ExportTo||JSON");
            AddInlineButton("Вернуться к главному меню", "0|ToMain");
        }
    }
}