#region using

using WorkoutStorageBot.BusinessLogic.SessionContext;

#endregion 

namespace WorkoutStorageBot.BusinessLogic.GlobalContext
{
    internal class DictionaryAdapterContextKeeper : IContextKeeper
    {
        public DictionaryAdapterContextKeeper()
        {
            contextStore = new Dictionary<long, UserContext>();
        }

        Dictionary<long, UserContext> contextStore;

        bool IContextKeeper.HasContext(long userID)
            => contextStore.ContainsKey(userID);

        UserContext? IContextKeeper.GetContext(long userID)
            => contextStore.GetValueOrDefault(userID);

        void IContextKeeper.AddContext(long userID, UserContext userContext)
            => contextStore.Add(userID, userContext);

        void IContextKeeper.RemoveContext(long userID)
            => contextStore.Remove(userID);
    }
}