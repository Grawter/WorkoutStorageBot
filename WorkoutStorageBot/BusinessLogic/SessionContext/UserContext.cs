#region using

using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Helpers.Common;
using WorkoutStorageBot.Helpers.Crypto;
using WorkoutStorageBot.Model.Domain;

#endregion

namespace WorkoutStorageBot.BusinessLogic.SessionContext
{
    internal class UserContext
    {
        internal UserInformation UserInformation { get; }
        
        internal Roles Roles { get; }
        
        internal Cycle? ActiveCycle { get; private set; }

        internal DataManager DataManager { get; }

        internal Navigation Navigation { get; }

        internal LimitsManager LimitsManager { get; }

        internal string CallBackId { get; set; }

        internal UserContext(UserInformation userInformation, Roles currentRoles = Roles.User, bool isEnableLimit = true)
        {
            UserInformation = CommonHelper.GetIfNotNull(userInformation);

            Roles = currentRoles;

            ActiveCycle = UserInformation.Cycles.FirstOrDefault(c => c.IsActive);

            DataManager = new();

            Navigation = new();

            LimitsManager = new(isEnableLimit);

            CallBackId = CryptographyHelper.CreateRandomCallBackQueryId();
        }

        internal void UdpateCycleForce(Cycle cycle)
        {
            ActiveCycle = cycle;
            ActiveCycle.IsActive = true;
        }

        internal IEnumerable<int> GetUserExercisesIds()
        {
            foreach (Cycle cycle in UserInformation.Cycles)
            {
                foreach (Day day in cycle.Days)
                {
                    foreach(Exercise exercise in day.Exercises)
                    {
                        yield return exercise.Id;
                    }
                }
            }
        }

        internal IDomain? GetCurrentDomainFromDataManager(DomainType domainType, bool throwEx = true)
            => GetCurrentDomainFromDataManager(domainType.ToString());

        internal IDomain? GetCurrentDomainFromDataManager(string domainType, bool throwEx = true)
        {
            return domainType switch
            {
                "Cycle"
                    => DataManager.CurrentCycle,
                "Day"
                    => DataManager.CurrentDay,
                "Exercise"
                     => DataManager.CurrentExercise,
                _ => throwEx ? throw new NotImplementedException($"Неожиданный domainTyped: {domainType}") : null,
            };
        }
    }
}