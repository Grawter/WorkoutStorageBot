#region using

using WorkoutStorageBot.BusinessLogic.SessionContext;

#endregion

namespace WorkoutStorageBot.BusinessLogic.GlobalContext
{
    public interface IContextKeeper
    {
        internal bool HasContext(long userID);

        internal UserContext? GetContext(long userID);

        internal void AddContext(long userID, UserContext userContext);

        internal void RemoveContext(long userID);
    }
}