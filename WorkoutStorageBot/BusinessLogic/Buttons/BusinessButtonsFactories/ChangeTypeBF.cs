﻿#region using

using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.SessionContext;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class ChangeTypeBF : ButtonsFactory
    {
        public ChangeTypeBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Режим только повторения", "2|ChangedMode|Exercise|0");
            AddInlineButton("Режим повторения и вес", "2|ChangedMode|Exercise|1");
            AddInlineButton("Режим свободного формата результата", "2|ChangedMode|Exercise|2");
        }
    }
}