﻿#region using

using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.SessionContext;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class RedirectAfterSaveExerciseBF : ButtonsFactory
    {
        public RedirectAfterSaveExerciseBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton($"Добавить новый день", "2|Add|Day");
            AddInlineButton($"Перейти в главное меню", "0|ToMain");
        }
    }
}