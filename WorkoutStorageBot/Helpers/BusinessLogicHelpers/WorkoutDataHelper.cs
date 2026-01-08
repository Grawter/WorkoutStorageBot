#region using
using System.Text;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.Extenions;
using WorkoutStorageBot.Model.Entities.BusinessLogic;
#endregion

namespace WorkoutStorageBot.Helpers.BusinessLogicHelpers
{
    internal static class WorkoutDataHelper
    {
        internal static string GetInformationAboutLastExercises(DateTime filterDateTime, IEnumerable<ResultExercise>? resultsExercises)
        {
            if (!resultsExercises.HasItemsInCollection())
                return "Нет информации для данного цикла";

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Дата: {filterDateTime.ToString(CommonConsts.Common.DateFormat)}");

            IEnumerable<IGrouping<int, ResultExercise>> groupsResultsExercise = resultsExercises.GroupBy(x => x.ExerciseId);

            foreach (IGrouping<int, ResultExercise> groupResultExercise in groupsResultsExercise)
            {
                ResultExercise firstGroupResultExercise = groupResultExercise.First();

                sb.AppendLine($"Упражнение: {firstGroupResultExercise.Exercise.Name.AddBoldAndQuotes()}");

                foreach (ResultExercise resultExercise in groupResultExercise)
                {
                    string resultExerciseStr = ConvertResultExerciseToString(resultExercise);

                    sb.AppendLine(resultExerciseStr);
                }
            }

            return sb.ToString().Trim();
        }

        internal static string GetInformationAboutLastDay(IEnumerable<ResultExercise>? resultsExercises)
        {
            if (!resultsExercises.HasItemsInCollection())
                return "Нет информации для данного дня";

            StringBuilder sb = new StringBuilder();

            IEnumerable<IGrouping<int, ResultExercise>> groupsResultsExercise = resultsExercises.GroupBy(x => x.ExerciseId);

            foreach (IGrouping<int, ResultExercise> groupResultExercise in groupsResultsExercise)
            {
                ResultExercise firstResultExercise = groupResultExercise.First();

                sb.AppendLine($"Упражнение: {firstResultExercise.Exercise.Name.AddBoldAndQuotes()} | Дата: {firstResultExercise.DateTime.ToString(CommonConsts.Common.DateFormat).AddBoldAndQuotes()}");

                foreach (ResultExercise resultExercise in groupResultExercise)
                {
                    string resultExerciseStr = ConvertResultExerciseToString(resultExercise);

                    sb.AppendLine(resultExerciseStr);
                }
            }

            return sb.ToString().Trim();
        }

        internal static string ConvertResultExerciseToString(ResultExercise resultExercise)
        {
            if (!string.IsNullOrWhiteSpace(resultExercise.FreeResult))
                return $"=> {resultExercise.FreeResult}";
            else if (resultExercise.Count.HasValue)
            {
                if (resultExercise.Weight.HasValue)
                    return $"Повторения: ({resultExercise.Count}) => Вес: ({resultExercise.Weight})";
                else
                    return $"Повторения: ({resultExercise.Count})";
            }
            else
                throw new InvalidOperationException($"Не удалось отобразить данные для результата упражнения с ID: {resultExercise.Id}, ID упражнения: {resultExercise.ExerciseId}, тип упражнения: {resultExercise.Exercise.Mode.ToString().AddQuotes()}");
        }
    }
}