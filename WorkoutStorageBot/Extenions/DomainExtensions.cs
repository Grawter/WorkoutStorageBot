#region using

using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.Domain;

#endregion

namespace WorkoutStorageBot.Extenions
{
    internal static class DomainExtensions
    {
        internal static IDomain GetDomainWithId(this EntityContext db, int id, DomainType domainType)
            => db.GetDomainWithId(id, domainType.ToString());


        internal static IDomain GetDomainWithId(this EntityContext db, int id, string domainType)
        {
            return domainType switch
            {
                CommonConsts.DomainsAndEntities.Cycle
                    => db.Cycles.First(c => c.Id == id),
                CommonConsts.DomainsAndEntities.Day
                    => db.Days.First(d => d.Id == id),
                CommonConsts.DomainsAndEntities.Exercise
                     => db.Exercises.First(e => e.Id == id),
                _ => throw new NotImplementedException($"Неожиданный domainTyped: {domainType}")
            };
        }
    }
}