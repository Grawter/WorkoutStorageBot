#region using

using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class CycleListBF : ButtonsFactory
    {
        public CycleListBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            GetDomainsInButtons(CurrentUserContext.UserInformation.Cycles.Where(c => !c.IsArchive), "Selected");
        }
    }
}