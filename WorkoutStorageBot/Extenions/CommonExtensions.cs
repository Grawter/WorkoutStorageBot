namespace WorkoutStorageBot.Extenions
{
    internal static class CommonExtensions
    {
        internal static IEnumerable<T> GetEmptyIfNull<T>(this IEnumerable<T> source)
            => source ?? Enumerable.Empty<T>();

        internal static bool HasItemsInCollection<T>(this ICollection<T>? collection)
            => collection != null && collection.Count > 0;

        internal static bool HasItemsInCollection<T>(this IEnumerable<T>? collection)
            => collection != null && collection.Count() > 0;

        internal static string AddBold(this string text)
            => $"<b>{text}</b>";

        internal static string AddQuotes(this string text)
            => $"\"{text}\"";

        internal static string AddBoldQuotes(this string text)
            => text.AddBold().AddQuotes();
    }
}