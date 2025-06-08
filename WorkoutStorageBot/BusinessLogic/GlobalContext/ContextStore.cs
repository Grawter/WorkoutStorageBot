#region using

using Microsoft.Extensions.Caching.Memory;
using WorkoutStorageBot.BusinessLogic.SessionContext;

#endregion

namespace WorkoutStorageBot.BusinessLogic.GlobalContext
{
    internal class ContextStore
    {
        internal ContextStore(bool isNeedCacheMode)
        {
            IsNeedCacheMode = isNeedCacheMode;

            memoryCacheContextStore = new MemoryCache(new MemoryCacheOptions());
            memoryCacheEntryOptions = new MemoryCacheEntryOptions()
                                            .SetSlidingExpiration(TimeSpan.FromHours(6));

            contextStore = new Dictionary<long, UserContext>();
        }

        internal bool IsNeedCacheMode { get; }

        private readonly MemoryCacheEntryOptions memoryCacheEntryOptions;
        private readonly MemoryCache memoryCacheContextStore;

        private readonly Dictionary<long, UserContext> contextStore;

        internal bool HasContext(long userID)
        {
            if (IsNeedCacheMode)
                return memoryCacheContextStore.TryGetValue(userID, out UserContext? result);
            else
               return contextStore.ContainsKey(userID);
        }

        internal UserContext? GetContext(long userID)
        {
            if (IsNeedCacheMode)
                return memoryCacheContextStore.Get<UserContext?>(userID);
            else
                return contextStore.GetValueOrDefault(userID);
        }

        internal void AddContext(long userID, UserContext userContext)
        {
            if (userID < 1 || userContext == null)
                throw new InvalidOperationException($"Аномалия: Попытка добавления некорректных данных в глобальный контекст!");

            if (IsNeedCacheMode)
                memoryCacheContextStore.Set(userID, userContext, memoryCacheEntryOptions);
            else
                contextStore.Add(userID, userContext);
        }

        internal void RemoveContext(long userID)
        {
            if (IsNeedCacheMode)
                memoryCacheContextStore.Remove(userID);
            else
                contextStore.Remove(userID);
        }
    }
}
