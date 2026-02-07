using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories.TestFactories
{
    internal class VerticalButtonsMoreThanExpectedTestBF : ButtonsFactory
    {
        internal VerticalButtonsMoreThanExpectedTestBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("1", "1");
            AddInlineButton("2", "2");
            AddInlineButton("3", "3");
            AddInlineButton("4", "4");
            AddInlineButton("5", "5");
            AddInlineButton("6", "6");
            AddInlineButton("7", "7");
            AddInlineButton("8", "8");
            AddInlineButton("9", "9");
            AddInlineButton("10", "10");
            AddInlineButton("11", "11");
            AddInlineButton("12", "12");
            AddInlineButton("13", "13");
            AddInlineButton("14", "14");
            AddInlineButton("15", "15");
            AddInlineButton("16", "15");
        }
    }
}