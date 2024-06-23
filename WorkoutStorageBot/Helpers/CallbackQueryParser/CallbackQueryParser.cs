namespace WorkoutStorageBot.Helpers.CallbackQueryParser
{
    internal class CallbackQueryParser
    {
        internal CallbackQueryParser(string data)
        {
            Args = data.Split('|');

            if (Args.Length == 0)
                throw new FormatException("Получен пустой CallBack");
        }

        internal int Direction { get => int.Parse(Args[0]); }
        internal string SubDirection { get => Args[1]; }
        internal string DomainType { get => Args[2]; }
        internal int DomainId { get => int.Parse(Args[3]); }
        internal string DomainName { get => Args[4]; }
        internal IEnumerable<string> AdditionalParameters { get => Args.Skip(5).SkipLast(1); }
        internal string CallBackId { get => Args[Args.Length - 1]; }

        internal string[] Args { get; private set; }
    }
}