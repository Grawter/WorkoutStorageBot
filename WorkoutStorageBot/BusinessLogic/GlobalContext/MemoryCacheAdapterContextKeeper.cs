﻿#region using

using Microsoft.Extensions.Caching.Memory;
using WorkoutStorageBot.BusinessLogic.SessionContext;

#endregion 

namespace WorkoutStorageBot.BusinessLogic.GlobalContext
{
    internal class MemoryCacheAdapterContextKeeper : IContextKeeper
    {
        public MemoryCacheAdapterContextKeeper()
        {
            memoryCacheContextStore = new MemoryCache(new MemoryCacheOptions());
            memoryCacheEntryOptions = new MemoryCacheEntryOptions()
                                            .SetSlidingExpiration(TimeSpan.FromHours(6));
        }

        private readonly MemoryCacheEntryOptions memoryCacheEntryOptions;
        private readonly MemoryCache memoryCacheContextStore;

        bool IContextKeeper.HasContext(long userID)
            => memoryCacheContextStore.TryGetValue(userID, out UserContext? result);

        UserContext? IContextKeeper.GetContext(long userID)
            => memoryCacheContextStore.Get<UserContext?>(userID);

        void IContextKeeper.AddContext(long userID, UserContext userContext)
            => memoryCacheContextStore.Set(userID, userContext, memoryCacheEntryOptions);

        void IContextKeeper.RemoveContext(long userID)
            => memoryCacheContextStore.Remove(userID);
    }
}