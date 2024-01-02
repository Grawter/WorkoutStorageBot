#region using
using System.Text;
using WorkoutStorageBot.Model;

#endregion

namespace WorkoutStorageBot.Helpers.UserMessageConverter
{
    internal class UserMessageConverter
    {
        internal UserMessageConverter(string data, bool withoutTrim = false)
        {
            if (withoutTrim)
                sb = new StringBuilder(data.Trim());
            else
                sb = new StringBuilder(data);
        }

        internal UserMessageConverter RemoveCompletely(int startIndex = 54)
        {
            if (sb.Length > startIndex && startIndex > 0)
                sb.Remove(startIndex, sb.Length - startIndex);

            return this;
        }

        internal UserMessageConverter WithoutServiceSymbol(char ch = '/')
        {
            var index = sb.ToString().IndexOf(ch);
            if (index != -1)
                sb.Remove(index, 1);

            return this;
        }

        internal UserMessageConverter WithoutServiceSymbols(char[] chars)
        {
            foreach (char c in chars)
                WithoutServiceSymbol(c);

            return this;
        }

        internal ResultExercise GetResultExercise()
        {
            var stringsResult = sb.ToString().Split(' ', 2);
            return new ResultExercise
            {
                Weight = float.Parse(stringsResult[0]),
                Count = float.Parse(stringsResult[1]),
                DateTime = DateTime.Now.ToShortDateString()
            };
        }

        internal string Convert()
        {
            return sb.ToString();
        }

        private StringBuilder sb;
    }
}