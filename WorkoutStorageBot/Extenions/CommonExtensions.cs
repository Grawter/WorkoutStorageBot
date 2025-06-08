namespace WorkoutStorageBot.Extenions
{
    internal static class CommonExtensions
    {
        internal static IEnumerable<T> GetEmptyIfNull<T>(this IEnumerable<T> source)
            => source ?? Enumerable.Empty<T>();

        internal static bool HasItemsInCollection<T>(this ICollection<T>? collection)
            => collection != null && collection.Count > 0;

    }
}