

namespace WorkoutStorageBot.Helpers.CallbackQueryParser
{
    internal class CallbackQueryParser
    {
        internal CallbackQueryParser(string data)
        {
            AllParameters = data.Split(['|'], StringSplitOptions.RemoveEmptyEntries);

            if (AllParameters.Length == 0)
                throw new FormatException("Получен пустой CallBack");
        }

        internal int Direction { get => int.Parse(AllParameters[0]); }
        internal string SubDirection { get => AllParameters[1]; }
        internal string DomainType { get => AllParameters[2]; }
        internal List<string> AdditionalParameters { get => AllParameters.Skip(3).SkipLast(1).ToList(); }
        internal string CallBackId { get => AllParameters[AllParameters.Length - 1]; }

        private string[] AllParameters { get; }


        internal string GetRequiredParameter(int index)
            => AllParameters[index];

        internal string GetRequiredAdditionalParameter(int index)
            => AdditionalParameters[index];

        public bool TryGetAdditionalParameter(int index, out string? value)
        {
            if (index >= 0 && index < AdditionalParameters.Count)
            {
                value = AdditionalParameters[index];
                return true;
            }

            value = default;
            return false;
        }
    }
}