using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;

namespace WorkoutStorageBot.BusinessLogic.Extensions
{
    internal static class UserInformationExtensions
    {
        internal static bool IsAdmin(this UserContext userContext)
            => userContext.Roles == Roles.Admin;
    }
}