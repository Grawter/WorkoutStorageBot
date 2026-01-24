using System.Collections.Concurrent;
using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Context.Global
{
    internal class DictionaryAdapterContextKeeper : IContextKeeper
    {
        public DictionaryAdapterContextKeeper()
        {
            contextStore = new ConcurrentDictionary<long, UserContext>();
        }

        ConcurrentDictionary<long, UserContext> contextStore;

        bool IContextKeeper.HasContext(long userID)
            => contextStore.ContainsKey(userID);

        UserContext? IContextKeeper.GetContext(long userID)
            => contextStore.GetValueOrDefault(userID);

        void IContextKeeper.SetContext(long userID, UserContext userContext)
        {
            if (userID < 1 || userContext == null)
                throw new InvalidOperationException($"Аномалия: Попытка добавления некорректных данных в глобальный контекст!");

            contextStore.AddOrUpdate(userID, userContext, (_, _) => userContext);
        }

        void IContextKeeper.RemoveContext(long userID)
            => contextStore.Remove(userID, out UserContext? removedContext);

        int IContextKeeper.Count => contextStore.Count;

        IEnumerable<long> IContextKeeper.GetAllKeys()
            => contextStore.Keys;
    }
}