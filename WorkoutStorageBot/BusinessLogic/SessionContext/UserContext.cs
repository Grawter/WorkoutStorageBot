#region using
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Helpers.Crypto;
using WorkoutStorageBot.Model;

#endregion

namespace WorkoutStorageBot.BusinessLogic.SessionContext
{
    internal class UserContext
    {
        internal UserInformation UserInformation { get; }
        internal Cycle? Cycle { get; private set; }

        internal DataManager DataManager { get; private set; }

        internal NavigationType NavigationType { get; set; }

        internal string CallBackSetId { get; set; }

        internal UserContext(UserInformation userInformation)
        {
            UserInformation = userInformation;
            Cycle = UserInformation.Cycles.FirstOrDefault(c => c.IsActive);

            DataManager = new();

            CallBackSetId = Convert.ToBase64String(Cryptography.CreateRandomByteArray());
        }

        internal void RefleshCycleForce(Cycle cycle)
        {
            Cycle = cycle;
        }

        internal void RefleshDataManagerForce(DataManager dataManager)
        {
            DataManager = dataManager;
        }
    }
}