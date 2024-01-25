#region using

using System.Text;
using WorkoutStorageBot.Model;

#endregion

namespace WorkoutStorageBot.Helpers.Converters
{
    public class TextMessageConverter : IStringConverter
    {
        public TextMessageConverter(string data, bool withoutTrim = false)
        {
            if (withoutTrim)
                sb = new StringBuilder(data.Trim());
            else
                sb = new StringBuilder(data);
        }

        public TextMessageConverter RemoveCompletely(int startIndex = 54)
        {
            if (sb.Length > startIndex && startIndex > 0)
                sb.Remove(startIndex, sb.Length - startIndex);

            return this;
        }

        public TextMessageConverter WithoutServiceSymbol(string simbol = "/")
        {
            sb.Replace(simbol, string.Empty);

            return this;
        }

        public TextMessageConverter WithoutServiceSymbols(string[] simbols)
        {
            foreach (var simbol in simbols)
                WithoutServiceSymbol(simbol);

            return this;
        }

        public string[] GetExercises()
        {
            List<Exercise> results = new();

            var text = sb.ToString();

            if (text.Contains(';'))
                return text.Split(';');
            else
                return [text];
        }

        public IEnumerable<ResultExercise> GetResultsExercise()
        {
            List<ResultExercise> results = new();

            var text = sb.ToString();

            if (text.Contains(';'))
            {
                string[] stringsResult = text.Split(';');

                foreach (var str in stringsResult)
                {
                    results.Add(GetResultExercise(str.Trim()));
                }
            }
            else
            {
                results.Add(GetResultExercise(text));
            }

            return results;
        }

        public string Convert()
        {
            return sb.ToString();
        }

        private StringBuilder sb;

        private ResultExercise GetResultExercise(string s)
        {
            var stringsResult = s.Split(' ', 2);
            return new ResultExercise
            {
                Weight = float.Parse(stringsResult[0]),
                Count = float.Parse(stringsResult[1]),
                DateTime = DateTime.Now.ToShortDateString()
            };
        }
    }
}