﻿#region using

using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.SessionContext;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class SettingDaysBF : ButtonsFactory
    {
        public SettingDaysBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Добавить новый день в цикл", "2|Add|Day");
            AddInlineButton("Настройка существующих дней", "2|SettingExisting|Days");
            AddInlineButton("Вернуться к главному меню", "0|ToMain");
        }
    }
}