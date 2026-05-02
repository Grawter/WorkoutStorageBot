using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Context.Global
{
    public interface IContextKeeper
    {
        bool HasContext(long userID);

        UserContext? GetContext(long userID);

        void SetContext(long userID, UserContext userContext);

        void RemoveContext(long userID);

        int Count { get; }

        IEnumerable<long> GetAllKeys();
    }
}