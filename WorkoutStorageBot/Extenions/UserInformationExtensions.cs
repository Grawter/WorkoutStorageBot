#region using

using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.SessionContext;

#endregion

namespace WorkoutStorageBot.Extenions
{
    internal static class UserInformationExtensions
    {
        internal static bool IsAdmin(this UserContext userContext)
            => userContext.Roles == Roles.Admin;
    }
}