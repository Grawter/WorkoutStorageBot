#region using

using System;
using System.Diagnostics;
using Telegram.Bot.Exceptions;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Helpers.Crypto;
using WorkoutStorageBot.Model;

#endregion

namespace WorkoutStorageBot.BusinessLogic.SessionContext
{
    internal class UserContext
    {
        internal UserInformation UserInformation { get; }
        internal Cycle? ActiveCycle { get; private set; }

        internal DataManager DataManager { get; private set; }

        internal Navigation Navigation { get; private set; }

        internal string CallBackId { get; set; }

        internal UserContext(UserInformation userInformation)
        {
            UserInformation = userInformation;
            ActiveCycle = UserInformation.Cycles.FirstOrDefault(c => c.IsActive);

            DataManager = new();

            Navigation = new();

            CallBackId = Cryptography.CreateRandomCallBackQueryId();
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

        internal IDomain? GetCurrentDomainFromDataManager(DomainType domainType)
            => GetCurrentDomainFromDataManager(domainType.ToString());

        internal IDomain? GetCurrentDomainFromDataManager(string domainType)
        {
            return domainType switch
            {
                "Cycle"
                    => DataManager.CurrentCycle,
                "Day"
                    => DataManager.CurrentDay,
                "Exercise"
                     => DataManager.CurrentExercise,
                _ => throw new NotImplementedException($"Неожиданный domainTyped: {domainType}")
            };
        }
    }
}