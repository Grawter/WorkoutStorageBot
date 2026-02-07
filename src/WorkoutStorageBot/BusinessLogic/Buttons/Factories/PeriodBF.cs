using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.Core.Extensions;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class PeriodBF : ButtonsFactory
    {
        internal PeriodBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            string actionName = (additionalParameters?["Act"]).ThrowIfNullOrWhiteSpace();

            AddInlineButton("Месяц", $"2|Period||{actionName}|1");
            AddInlineButton("Квартал", $"2|Period||{actionName}|3");
            AddInlineButton("Полгода", $"2|Period||{actionName}|6");
            AddInlineButton("Год", $"2|Period||{actionName}|12");
            AddInlineButton("За всё время", $"2|Period||{actionName}|0");
        }
    }
}