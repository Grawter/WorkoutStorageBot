using System.Text;

namespace WorkoutStorageBot.BusinessLogic.Helpers.Converters
{
    internal class MessageTextBuilder : IBuilder
    {
        internal MessageTextBuilder(string data, bool withoutTrim = false)
        {
            if (withoutTrim)
                sb = new StringBuilder(data.Trim());
            else
                sb = new StringBuilder(data);
        }

        private StringBuilder sb;

        internal MessageTextBuilder RemoveCompletely(int startIndex = 54)
        {
            if (sb.Length > startIndex && startIndex > 0)
                sb.Remove(startIndex, sb.Length - startIndex);

            return this;
        }

        internal MessageTextBuilder WithoutServiceSymbol(string symbol = "/")
        {
            sb.Replace(symbol, string.Empty);

            return this;
        }

        internal MessageTextBuilder WithoutServiceSymbols(string[] symbols)
        {
            foreach (string simbol in symbols)
                WithoutServiceSymbol(simbol);

            return this;
        }

        string IBuilder.Build()
            => Build();

        internal string Build()
            => sb.ToString();
    }
}