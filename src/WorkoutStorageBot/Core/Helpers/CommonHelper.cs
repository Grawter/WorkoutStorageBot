using System.Diagnostics.CodeAnalysis;

namespace WorkoutStorageBot.Core.Helpers
{
    public class CommonHelper
    {
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

        internal static bool TryConvertToLong(object? obj, [NotNullWhen(true)] out long? result)
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