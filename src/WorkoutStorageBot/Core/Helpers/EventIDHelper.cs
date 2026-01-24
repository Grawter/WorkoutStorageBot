using Microsoft.Extensions.Logging;

namespace WorkoutStorageBot.Core.Helpers
{
    internal class EventIDHelper
    {
        internal static EventId GetNextEventIdThreadSafe(string? name = null) 
            => new EventId(Random.Shared.Next(), name);

        internal static EventId GetNextEventId(string? name = null)
            => new EventId(new Random().Next(), name);
    }
}