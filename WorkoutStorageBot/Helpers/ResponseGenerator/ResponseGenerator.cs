#region using
using System.Text;
using WorkoutStorageBot.Model;

#endregion

namespace WorkoutStorageBot.Helpers.ResponseGenerator
{
    internal class ResponseGenerator
    {
        private StringBuilder sb;
        private string? title;
        private string? content;
        private string? target;
        private string? separator;

        internal ResponseGenerator(string content, string target)
        {
            sb = new StringBuilder();

            this.content = content;

            this.target = target;

            separator = "======================";
        }

        internal ResponseGenerator(string title, string content, string target, string? separator = null)
        {
            sb = new StringBuilder();

            this.title = title;

            this.content = content;

            this.target = target;

            if (!string.IsNullOrEmpty(separator))
                this.separator = separator;
            else
                this.separator = "======================";
        }

        internal string Generate()
        {
            if (!string.IsNullOrEmpty(title))
            {
                sb.AppendLine(title);
                sb.AppendLine(separator);
            }

            if (!string.IsNullOrEmpty(content))
            {
                sb.AppendLine(content);
                sb.AppendLine(separator);
            }

            if (!string.IsNullOrEmpty(target))
            {
                sb.AppendLine();
                sb.AppendLine(target);
            }

            return sb.ToString();
        }

        internal static string GetInformationAboutLastWorkout(List<Exercise> exercises, List<ResultExercise> resultExercises)
        {
            var resultQuery = exercises
                        .Join(resultExercises,
                            e => e.Id,
                            rE => rE.ExerciseId,
                            (e, rE) => new { e.NameExercise, rE.Count, rE.Weight, rE.DateTime })
                        .GroupBy(rE => rE.NameExercise).ToArray();

            var sb = new StringBuilder();

            for (int i = 0; i < resultQuery.Count(); i++)
            {
                sb.AppendLine($"Упражнение: {resultQuery[i].Key}");

                foreach (var item2 in resultQuery[i])
                {
                    sb.AppendLine($"({item2.Count}) => ({item2.Weight})");
                }
            }

            return sb.ToString().Trim();
        }

        internal static string GetInformationAboutLastDay(List<Exercise> exercises, List<ResultExercise> resultExercises)
        {
            var resultQuery = exercises
                        .Join(resultExercises,
                            e => e.Id,
                            rE => rE.ExerciseId,
                            (e, rE) => new { e.NameExercise, rE.Count, rE.Weight, rE.DateTime })
                        .GroupBy(rE => rE.NameExercise).ToArray();
 
            var sb = new StringBuilder();

            for (int i = 0; i < resultQuery.Count(); i++)
            {
                sb.AppendLine($"Упражнение: {resultQuery[i].Key}");

                foreach (var item2 in resultQuery[i])
                {
                    sb.AppendLine($"({item2.Count}) => ({item2.Weight}) | Дата: {item2.DateTime}");
                }
            }

            return sb.ToString().Trim();
        } 
    }
}