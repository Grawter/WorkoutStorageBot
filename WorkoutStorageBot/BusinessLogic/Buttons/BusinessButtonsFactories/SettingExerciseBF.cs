#region using

using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.SessionContext;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class SettingExerciseBF : ButtonsFactory
    {
        public SettingExerciseBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Сменить название", "2|ChangeName|Exercise");
            AddInlineButton("Сменить тип", "2|ChangeMode|Exercise");
            AddInlineButton("Перенести упражнение", "2|Replace|Exercise");
            AddInlineButton("Добавить в архив", $"2|Archiving|Exercise");
            AddInlineButton("Удалить", "2|Delete|Exercise");
            AddInlineButton("Вернуться к главному меню", "0|ToMain");
        }
    }
}