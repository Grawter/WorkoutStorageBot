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

        internal void UdpateActiveCycleForce(Cycle cycle)
        {
            ActiveCycle = cycle;
            ActiveCycle.IsActive = true;
        }
    }
}