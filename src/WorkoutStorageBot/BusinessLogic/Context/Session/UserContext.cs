using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Helpers.Crypto;
using WorkoutStorageBot.Model.DTO.BusinessLogic;

namespace WorkoutStorageBot.BusinessLogic.Context.Session
{
    internal class UserContext
    {
        internal DTOUserInformation UserInformation { get; }
        
        internal Roles Roles { get; }
        
        internal DTOCycle? ActiveCycle { get; private set; }

        internal DataManager DataManager { get; }

        internal Navigation Navigation { get; }

        internal LimitsManager LimitsManager { get; }

        internal string CallBackId { get; private set; } = string.Empty;

        internal UserContext(DTOUserInformation userInformation, Roles currentRoles = Roles.User, bool isEnableLimit = true)
        {
            UserInformation = userInformation;

            Roles = currentRoles;

            ActiveCycle = UserInformation.Cycles.FirstOrDefault(c => c.IsActive);

            DataManager = new();

            Navigation = new();

            LimitsManager = new(isEnableLimit);
        }

        internal void SetNewActiveCycleForce(DTOCycle cycle)
        {
            ActiveCycle = cycle;
            ActiveCycle.IsActive = true;
        }

        internal void GenerateNewCallBackId()
        {
            CallBackId = CryptographyHelper.CreateRandomCallBackQueryId();
        }
    }
}