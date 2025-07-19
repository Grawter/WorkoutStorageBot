#region using

using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.SessionContext;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class SettingCycleBF : ButtonsFactory
    {
        public SettingCycleBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Сделать активным", "2|ChangeActive|Cycle");
            AddInlineButton("Сменить название", "2|ChangeName|Cycle");
            AddInlineButton("Добавить в архив", "2|Archiving|Cycle");
            AddInlineButton("Удалить", "2|Delete|Cycle");
            AddInlineButton("Настройка дней", "2|Setting|Days");
            AddInlineButton("Вернуться к главному меню", "0|ToMain");
        }
    }
}