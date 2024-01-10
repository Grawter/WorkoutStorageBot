

namespace WorkoutStorageBot.Helpers.CallbackQueryParser
{
    internal class CallbackQueryParser
    {
        internal bool TryParse(string data)
        {
            Args = data.Split(new char[] {'|'});

            if (Args.Length == 0)
                return false;

            return true;
        }

        internal int Direction { get => int.Parse(Args[0]); }
        internal string SubDirection { get => Args[1]; }
        internal int ObjectId { get => int.Parse(Args[2]); }
        internal string ObjectType { get => Args[3]; }
        internal string CallBackId { get => Args[Args.Length - 1]; }

        internal string[] Args { get; private set; }
    }
}