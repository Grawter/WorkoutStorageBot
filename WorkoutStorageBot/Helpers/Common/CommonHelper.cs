

using System.Diagnostics.CodeAnalysis;

namespace WorkoutStorageBot.Helpers.Common
{
    public class CommonHelper
    {
        [return: NotNull]
        public static T GetIfNotNull<T>(T source)
        {
            ArgumentNullException.ThrowIfNull(source);

            return source;
        }

        [return: NotNull]
        public static T GetIfNotNullOrWhiteSpace<T>(T source)
        {
            if (source is string sourceString)
                ArgumentNullException.ThrowIfNullOrWhiteSpace(sourceString);
            else
                ArgumentNullException.ThrowIfNull(source);

            return source;
        }

        internal static string GetCensorValue(string text, int countShowLastSymbols)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length <= countShowLastSymbols)
                return "******";

            // Берём только последние 3 символа
            string lastThreeSymbols = text.Substring(text.Length - countShowLastSymbols);

            // Заменяем остальные символы на *
            string masked = $"{new string('*', text.Length - countShowLastSymbols)}{lastThreeSymbols}";

            return masked;
        }

        internal static bool TryConvertToLong(object obj, out long? result)
        {
            result = null;

            if (obj == null)
                return false;

            try
            {
                result = Convert.ToInt64(obj);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}