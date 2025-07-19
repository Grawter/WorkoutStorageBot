#region using

using Microsoft.Extensions.Logging;

#endregion

namespace WorkoutStorageBot.Helpers.EventIDHelper
{
    internal class EventIDHelper
    {
        // Создание отдельного экземпляра для кажого потока, иначе при вызове Next() random может сломаться
        private static readonly ThreadLocal<Random> random = new ThreadLocal<Random>(() => new Random());

        internal static EventId GetNextEventIdThreadSave(string? name = null) 
            => new EventId(random.Value.Next(), name);

        internal static EventId GetNextEventId(string? name = null)
            => new EventId(new Random().Next(), name);
    }
}