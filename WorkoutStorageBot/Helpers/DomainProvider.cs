#region using
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.Model;
#endregion

namespace WorkoutStorageBot.Helpers
{
    internal class DomainProvider
    {
        private readonly UserContext currentUserContext;
        private readonly EntityContext db;

        internal DomainProvider(EntityContext db, UserContext userContext)
        {
            currentUserContext = userContext;
            this.db = db; ;
        }

        internal IDomain? GetDomainFromDataManager(string domainType)
        {
            return domainType switch
            {
                "Cycle"
                    => currentUserContext.DataManager.CurrentCycle,
                "Day"
                    => currentUserContext.DataManager.CurrentDay,
                "Exercise"
                     => currentUserContext.DataManager.CurrentExercise,
                _ => throw new NotImplementedException($"Неожиданный domainTyped: {domainType}")
            };
        }

        internal IDomain GetDomainWithId(int id, string domainType)
        {
            return domainType switch
            {
                "Cycle"
                    => currentUserContext.UserInformation.Cycles.First(c => c.Id == id),
                "Day"
                    => db.Days.First(d => d.Id == id),
                "Exercise"
                     => db.Exercises.First(e => e.Id == id),
                _ => throw new NotImplementedException($"Неожиданный domainTyped: {domainType}")
            };
        }
    }
}