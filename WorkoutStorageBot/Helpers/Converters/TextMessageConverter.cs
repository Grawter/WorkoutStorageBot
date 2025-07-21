#region using

using System.Text;
using WorkoutStorageBot.BusinessLogic.Exceptions;
using WorkoutStorageBot.Extenions;
using WorkoutStorageBot.Model.Domain;

#endregion

namespace WorkoutStorageBot.Helpers.Converters
{
    internal class TextMessageConverter : IStringConverter
    {
        internal TextMessageConverter(string data, bool withoutTrim = false)
        {
            if (withoutTrim)
                sb = new StringBuilder(data.Trim());
            else
                sb = new StringBuilder(data);
        }

        private StringBuilder sb;

        private StringSplitOptions stringSplitOptions = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

        internal TextMessageConverter RemoveCompletely(int startIndex = 54)
        {
            if (sb.Length > startIndex && startIndex > 0)
                sb.Remove(startIndex, sb.Length - startIndex);

            return this;
        }

        internal TextMessageConverter WithoutServiceSymbol(string symbol = "/")
        {
            sb.Replace(symbol, string.Empty);

            return this;
        }

        internal TextMessageConverter WithoutServiceSymbols(string[] symbols)
        {
            foreach (string simbol in symbols)
                WithoutServiceSymbol(simbol);

            return this;
        }

        internal List<Exercise> GetExercises()
        {
            string text = sb.ToString();

            List<Exercise> exercises = new List<Exercise>();

            foreach (string exerciseWithType in text.Split(';', stringSplitOptions))
            {
                string[] exerciseAndType = exerciseWithType.Split('-', stringSplitOptions);

                string? name = exerciseAndType.FirstOrDefault();

                if (string.IsNullOrWhiteSpace(name))
                    throw new CreateExerciseException("Не удалось получилось название упражнения.");

                if (!int.TryParse(exerciseAndType.Skip(1).FirstOrDefault(), out int type))
                    throw new CreateExerciseException($"Не удалось получить тип упражнения '{name}'");

                if (!Enum.IsDefined(typeof(ExercisesMods), type))
                    throw new CreateExerciseException($"Указанный тип упражнения ({type}) не относится к допустимым");

                Exercise exercise = new Exercise() { Name = name, Mode = (ExercisesMods)type };
                exercises.Add(exercise);
            }

            return exercises.Count > 0
                ? exercises
                : throw new CreateExerciseException("Не удалось получить ни одного упражнения");
        }

        internal List<ResultExercise> GetResultsExercise(ExercisesMods currentExerciseMode)
        {
            List<ResultExercise> results = new();

            StringSplitOptions stringSplitOptions = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

            string separator = currentExerciseMode switch
            {
                ExercisesMods.Count => " ",
                ExercisesMods.WeightCount => ";",
                _ => ""
            };

            string text = sb.ToString();

            foreach (string resultExerciseStr in text.Split(separator, stringSplitOptions))
            {
                ResultExercise resultExercise = GetResultExercise(resultExerciseStr.Trim(), currentExerciseMode);

                results.Add(resultExercise);
            }

            return results;
        }

        public string Convert()
        {
            return sb.ToString();
        }

        private ResultExercise GetResultExercise(string resultExerciseStr, ExercisesMods currentExerciseMode)
        {
            string currentType = currentExerciseMode.ToString().AddQuotes();

            switch (currentExerciseMode)
            {
                case ExercisesMods.Count:

                    if (!int.TryParse(resultExerciseStr, out int singleCount))
                        throw new CreateResultExerciseException("Указанное количество повторений не является цифрой");

                    return new ResultExercise()
                    {
                        Count = singleCount,
                        DateTime = DateTime.Now,
                    };

                case ExercisesMods.WeightCount:
                
                    string[] resultExerciseWeightCount = resultExerciseStr.Split(' ', stringSplitOptions);

                    string weightStr = resultExerciseWeightCount.FirstOrDefault()
                        ?? throw new CreateResultExerciseException($"Не удалось получить указанный вес подхода для упражнения с типом {currentType}");

                    if (!float.TryParse(weightStr, out float weight))
                        throw new CreateResultExerciseException("Указанное значение веса подхода не является цифрой");

                    string countStr = resultExerciseWeightCount.Skip(1).FirstOrDefault()
                        ?? throw new CreateResultExerciseException($"Не удалось получить указанное кол-во повторений для упражнения с типом {currentType}");

                    if (!int.TryParse(countStr, out int count))
                        throw new CreateResultExerciseException("Указанное количество повторений не является цифрой");

                    return new ResultExercise()
                    {
                        Weight = weight,
                        Count = count,
                        DateTime = DateTime.Now,
                    };
                
                case ExercisesMods.FreeResult:
                    return new ResultExercise()
                    {
                        FreeResult = resultExerciseStr,
                        DateTime = DateTime.Now,
                    };

                default:
                    throw new CreateResultExerciseException($"Неподдерживаемый тип упражнения: {currentType}");
                    
            }
        }
    }
}