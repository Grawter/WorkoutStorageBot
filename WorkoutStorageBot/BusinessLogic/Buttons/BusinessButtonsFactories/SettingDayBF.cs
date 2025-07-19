#region using

using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.SessionContext;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class SettingDayBF : ButtonsFactory
    {
        public SettingDayBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Сменить название", "2|ChangeName|Day");
            AddInlineButton("Перенести день", "2|Replace|Day");
            AddInlineButton("Добавить в архив", "2|Archiving|Day");
            AddInlineButton("Удалить", "2|Delete|Day");
            AddInlineButton("Настройка упражнений", "2|Setting|Exercises");
            AddInlineButton("Вернуться к главному меню", "0|ToMain");
        }
    }
}