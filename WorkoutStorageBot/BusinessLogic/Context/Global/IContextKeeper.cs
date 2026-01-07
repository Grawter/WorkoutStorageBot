#region using

using WorkoutStorageBot.BusinessLogic.Context.Session;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Context.Global
{
    public interface IContextKeeper
    {
        internal bool HasContext(long userID);

        internal UserContext? GetContext(long userID);

        internal void AddContext(long userID, UserContext userContext);

        internal void RemoveContext(long userID);
    }
}