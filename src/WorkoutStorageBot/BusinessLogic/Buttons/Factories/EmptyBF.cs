using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class EmptyBF : ButtonsFactory
    {
        internal EmptyBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
        }
    }
}