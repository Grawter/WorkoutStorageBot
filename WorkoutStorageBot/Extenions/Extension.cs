

namespace WorkoutStorageBot.Extenions
{
    internal static class Extension
    {
        public static IEnumerable<T> GetEmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }
    }
}