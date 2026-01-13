using System.Diagnostics.CodeAnalysis;

namespace WorkoutStorageBot.Core.Extensions
{
    internal static class CommonExtensions
    {
        internal static IEnumerable<T> GetEmptyIfNull<T>(this IEnumerable<T> source)
            => source ?? Enumerable.Empty<T>();

        internal static bool HasItemsInCollection<T>([NotNullWhen(true)] this ICollection<T>? collection)
            => collection != null && collection.Count > 0;

        internal static bool HasItemsInCollection<T>([NotNullWhen(true)] this IEnumerable<T>? collection)
            => collection != null && collection.Any();
    }
}