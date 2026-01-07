#region using

using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class PeriodBF : ButtonsFactory
    {
        public PeriodBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            string actionName = additionalParameters["Act"];

            AddInlineButton("Месяц", $"2|Period||{actionName}|1");
            AddInlineButton("Квартал", $"2|Period||{actionName}|3");
            AddInlineButton("Полгода", $"2|Period||{actionName}|6");
            AddInlineButton("Год", $"2|Period||{actionName}|12");
            AddInlineButton("За всё время", $"2|Period||{actionName}|0");
        }
    }
}