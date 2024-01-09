#region using
using System.Text;
using WorkoutStorageBot.Model;

#endregion

namespace WorkoutStorageBot.Helpers.Converters
{
    public class UserMessageConverter : IStringConverter
    {
        public UserMessageConverter(string data, bool withoutTrim = false)
        {
            if (withoutTrim)
                sb = new StringBuilder(data.Trim());
            else
                sb = new StringBuilder(data);
        }

        public UserMessageConverter RemoveCompletely(int startIndex = 54)
        {
            if (sb.Length > startIndex && startIndex > 0)
                sb.Remove(startIndex, sb.Length - startIndex);

            return this;
        }

        public UserMessageConverter WithoutServiceSymbol(string simbol = "/")
        {
            sb.Replace(simbol, string.Empty);

            return this;
        }

        public UserMessageConverter WithoutServiceSymbols(string[] simbols)
        {
            foreach (var simbol in simbols)
                WithoutServiceSymbol(simbol);

            return this;
        }

        public ResultExercise? GetResultExercise()
        {
            var stringsResult = sb.ToString().Split(' ', 2);

            return stringsResult.Length == 2 ? new ResultExercise
            {
                Weight = float.Parse(stringsResult[0]),
                Count = float.Parse(stringsResult[1]),
                DateTime = DateTime.Now.ToShortDateString()
            }
            : null;
        }

        public string Convert()
        {
            return sb.ToString();
        }

        private StringBuilder sb;
    }
}