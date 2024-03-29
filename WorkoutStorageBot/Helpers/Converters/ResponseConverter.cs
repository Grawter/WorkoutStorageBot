﻿#region using
using System.Text;
using WorkoutStorageBot.Model;

#endregion

namespace WorkoutStorageBot.Helpers.Converters
{
    public class ResponseConverter : IStringConverter
    {
        private StringBuilder? sb;
        private string? title;
        private string? content;
        private string target;
        private string? separator;
        private bool onlyTarget;

        public ResponseConverter(string target)
        {
            this.target = target;

            onlyTarget = true;
        }

        public ResponseConverter(string content, string target)
        {
            sb = new StringBuilder();

            this.content = content;

            this.target = target;

            separator = "======================";
        }

        public ResponseConverter(string title, string content, string target, string? separator = null)
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

        public void ResetTitle(string title)
        {
            this.title = title;
        }

        public void ResetContent(string content)
        {
            this.content = content;
        }

        public void ResetTarget(string target)
        {
            this.target = target;
        }

        public string Convert()
        {
            if (onlyTarget)
                return target;

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

        public static string GetInformationAboutLastExercises(IEnumerable<Exercise>? exercises, IEnumerable<ResultExercise>? resultExercises)
        {
            if (!exercises.Any() || !resultExercises.Any())
                return "Нет информации для данного цикла";

            var resultQuery = exercises
                        .Join(resultExercises,
                            e => e.Id,
                            rE => rE.ExerciseId,
                            (e, rE) => new { e.Name, rE.Count, rE.Weight, rE.DateTime })
                        .GroupBy(rE => rE.Name).ToArray();


            var sb = new StringBuilder();

            sb.AppendLine($"Дата: {resultExercises.First().DateTime}");

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

        public static string GetInformationAboutLastDay(IEnumerable<Exercise> exercises, IEnumerable<ResultExercise>? resultExercises)
        {
            if (!exercises.Any() || !resultExercises.Any())
                return "Нет информации для данного цикла";

            var resultQuery = exercises
                        .Join(resultExercises,
                            e => e.Id,
                            rE => rE.ExerciseId,
                            (e, rE) => new { e.Name, rE.Count, rE.Weight, rE.DateTime })
                        .GroupBy(rE => rE.Name).ToArray();

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