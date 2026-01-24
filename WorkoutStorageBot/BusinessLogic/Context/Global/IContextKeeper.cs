using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Context.Global
{
    public interface IContextKeeper
    {
        internal bool HasContext(long userID);

        internal UserContext? GetContext(long userID);

        internal void SetContext(long userID, UserContext userContext);

        internal void RemoveContext(long userID);

        internal int Count { get; }

        internal IEnumerable<long> GetAllKeys();
    }
}