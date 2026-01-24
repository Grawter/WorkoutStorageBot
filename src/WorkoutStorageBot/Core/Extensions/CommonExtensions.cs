using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace WorkoutStorageBot.Core.Extensions
{
    internal static class CommonExtensions
    {
        internal static T ThrowIfNull<T>([NotNull] this T? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null) where T : class
        {
            if (value is null)
                throw new ArgumentNullException(parameterName);

            return value;
        }

        internal static T ThrowIfNullOrWhiteSpace<T>([NotNull] this T? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null) where T : class
        {
            if (value is null || value is string strValue && string.IsNullOrWhiteSpace(strValue))
                throw new ArgumentNullException(parameterName);

            return value;
        }

        internal static T ThrowIfNull<T>(this T? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null) where T : struct
        {
            if (!value.HasValue)
                throw new ArgumentNullException(parameterName);

            return value.Value;
        }

        internal static IEnumerable<T> GetEmptyIfNull<T>(this IEnumerable<T> source)
            => source ?? Enumerable.Empty<T>();

        internal static bool HasItemsInCollection<T>([NotNullWhen(true)] this ICollection<T>? collection)
            => collection != null && collection.Count > 0;

        internal static bool HasItemsInCollection<T>([NotNullWhen(true)] this IEnumerable<T>? collection)
            => collection != null && collection.Any();
    }
}