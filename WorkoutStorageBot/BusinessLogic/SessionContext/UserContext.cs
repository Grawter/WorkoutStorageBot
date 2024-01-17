#region using
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
    }
}