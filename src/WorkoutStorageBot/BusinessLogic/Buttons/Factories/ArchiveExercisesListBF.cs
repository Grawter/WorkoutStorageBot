using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.Model.DTO.BusinessLogic;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class ArchiveExercisesListBF : ButtonsFactory
    {
        internal ArchiveExercisesListBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            foreach (DTOCycle cycle in CurrentUserContext.UserInformation.Cycles.Where(c => !c.IsArchive))
            {
                foreach (DTODay day in cycle.Days.Where(c => !c.IsArchive))
                {
                    GetDomainsInButtons(day.Exercises.Where(e => e.IsArchive), "UnArchive", true);
                }
            }
        }
    }
}