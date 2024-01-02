

namespace WorkoutStorageBot.Helpers.CallbackQueryParser
{
    internal static class CallbackQueryParser
    {
        internal static bool TryParse(string data)
        {
            Args = data.Split('|');

            if (Args.Length == 0)
                return false;

            return true;
        }

        internal static string Direction { get => Args[0]; }
        internal static string ObjectName { get => Args[1]; }
        internal static string ObjectId { get => Args[2]; }
        internal static string CallBackSetId { get => Args[Args.Length - 1]; }

        internal static string[] Args { get; private set; }
    }
}