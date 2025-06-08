using Microsoft.Extensions.Logging;

namespace WorkoutStorageBot.Application.BotTools.Logging
{
    internal class EventIDHelper
    {
        // Создание отдельное экземпляра для кажого потока, иначе при вызове Next() random может сломаться
        private static readonly ThreadLocal<Random> random = new ThreadLocal<Random>(() => new Random());

        internal static EventId GetNextEventIdThreadSave(string? name = null) 
            => new EventId(random.Value.Next(), name);

        internal static EventId GetNextEventId(string? name = null)
            => new EventId(new Random().Next(), name);
    }
}