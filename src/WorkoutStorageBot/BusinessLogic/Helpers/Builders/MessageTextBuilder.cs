using System.Text;

namespace WorkoutStorageBot.BusinessLogic.Helpers.Converters
{
    internal class MessageTextBuilder
    {
        internal MessageTextBuilder(string data, bool withTrim = false)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(data);

            sb = new StringBuilder(withTrim ? data.Trim() : data);
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
            if (!string.IsNullOrWhiteSpace(symbol))
                sb.Replace(symbol, string.Empty);

            return this;
        }

        internal MessageTextBuilder WithoutServiceSymbols(IEnumerable<string> symbols)
        {
            foreach (string symbol in symbols)
                WithoutServiceSymbol(symbol);

            return this;
        }

        internal string Build()
            => sb.ToString();
    }
}