#region using

using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.Model.DTO.BusinessLogic;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class ReplaceToDayBF : ButtonsFactory
    {
        public ReplaceToDayBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            foreach (DTOCycle cycle in CurrentUserContext.UserInformation.Cycles.Where(c => !c.IsArchive))
            {
                GetDomainsInButtons(cycle.Days.Where(c => !c.IsArchive), "ReplaceTo");
            }
        }
    }
}