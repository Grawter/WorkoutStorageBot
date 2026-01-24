namespace WorkoutStorageBot.BusinessLogic.Extensions
{
    internal static class CommonExtensions
    {
        internal static string AddBold(this string text)
            => $"<b>{text}</b>";

        internal static string AddQuotes(this string text)
            => $"\"{text}\"";

        internal static string AddBoldAndQuotes(this string text)
            => text.AddBold().AddQuotes();
    }
}