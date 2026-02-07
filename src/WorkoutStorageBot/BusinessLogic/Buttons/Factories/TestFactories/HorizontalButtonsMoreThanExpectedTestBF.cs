using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories.TestFactories
{
    internal class HorizontalButtonsMoreThanExpectedTestBF : ButtonsFactory
    {
        internal HorizontalButtonsMoreThanExpectedTestBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("1", "1", false);
            AddInlineButton("2", "2", false);
            AddInlineButton("3", "3", false);
            AddInlineButton("4", "4", false);
            SaveHorizontalButtons();
        }
    }
}