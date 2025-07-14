#region using

using WorkoutStorageBot.BusinessLogic.SessionContext;

#endregion

namespace WorkoutStorageBot.BusinessLogic.GlobalContext
{
    internal class ContextStore
    {
        internal ContextStore(bool isNeedCacheMode)
        {
            contextKeeper = GetContextKeeper(isNeedCacheMode);
        }

        private readonly IContextKeeper contextKeeper;

        internal bool HasContext(long userID)
            => contextKeeper.HasContext(userID);

        internal UserContext? GetContext(long userID)
            => contextKeeper.GetContext(userID);

        internal void AddContext(long userID, UserContext userContext)
        {
            if (userID < 1 || userContext == null)
                throw new InvalidOperationException($"Аномалия: Попытка добавления некорректных данных в глобальный контекст!");

            contextKeeper.AddContext(userID, userContext);
        }

        internal void RemoveContext(long userID)
            => contextKeeper.RemoveContext(userID);

        private IContextKeeper GetContextKeeper(bool isNeedCacheMode)
        {
            if (isNeedCacheMode)
                return new MemoryCacheAdapterContextKeeper();
            else
                return new DictionaryAdapterContextKeeper();
        }
    }
}