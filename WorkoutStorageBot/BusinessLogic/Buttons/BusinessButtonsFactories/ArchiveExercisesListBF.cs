#region using

using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.Model.DomainsAndEntities;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class ArchiveExercisesListBF : ButtonsFactory
    {
        public ArchiveExercisesListBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            foreach (Cycle cycle in CurrentUserContext.UserInformation.Cycles.Where(c => !c.IsArchive))
            {
                foreach (Day day in cycle.Days.Where(c => !c.IsArchive))
                {
                    GetDomainsInButtons(day.Exercises.Where(e => e.IsArchive), "UnArchive");
                }
            }
        }
    }
}