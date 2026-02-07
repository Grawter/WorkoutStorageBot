using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class CycleListBF : ButtonsFactory
    {
        internal CycleListBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            GetDomainsInButtons(CurrentUserContext.UserInformation.Cycles.Where(c => !c.IsArchive), "Selected");
        }
    }
}