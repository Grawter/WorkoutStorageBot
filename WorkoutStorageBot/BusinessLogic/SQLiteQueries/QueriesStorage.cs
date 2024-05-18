#region using

using System.Text;
using WorkoutStorageBot.Model;

#endregion

namespace WorkoutStorageBot.BusinessLogic.SQLiteQueries
{
    internal static class QueriesStorage
    {
        internal static List<Exercise> GetExercisesWithDaysIds(IEnumerable<int> ids, Func<string, List<Exercise>> queryExecuter)
        {
            var args = FromIEnumerableIntToString(ids);

            string query = $@"
SELECT Exercises.Id, Exercises.Name, Exercises.DayId, Exercises.IsArchive FROM Exercises
WHERE Exercises.DayId IN ({args}) AND Exercises.IsArchive = FALSE;";

            return queryExecuter.Invoke(query);
        }

        internal static List<ResultExercise> GetLastResultsExercisesWithExercisesIds(IEnumerable<int> ids, Func<string, List<ResultExercise>> queryExecuter)
        {
            var args = FromIEnumerableIntToString(ids);

            string query = $@"
WITH LastResults (Id, Count, Weight, DateTime, ExerciseId)
AS
(
    SELECT * FROM ResultsExercises
    WHERE ResultsExercises.ExerciseId IN ({args})
)

SELECT * FROM LastResults
WHERE DateTime = (SELECT MAX(DateTime) FROM LastResults)";

            return queryExecuter.Invoke(query);
        }

        internal static List<ResultExercise> GetLastDateForExercises(IEnumerable<int> ids, Func<string, List<ResultExercise>> queryExecuter)
        {
            var args = FromIEnumerableIntToString(ids);

            string query = $@"
SELECT * FROM
    (
        SELECT ID, DateTime, ExerciseId, 0 AS Weight, 0 AS Count FROM ResultsExercises
        WHERE ExerciseId IN ({args})
        ORDER BY ExerciseId ASC, DateTime DESC
    )
GROUP BY ExerciseId;";

            return queryExecuter.Invoke(query);
        }

        internal static List<ResultExercise> GetLastResultsForExercisesAndDate(List<ResultExercise> lastDateForExercises, Func<string, List<ResultExercise>> queryExecuter)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < lastDateForExercises.Count; i++)
            {
                sb.AppendLine(@$"
SELECT * FROM ResultsExercises
WHERE ExerciseId = {lastDateForExercises[i].ExerciseId} AND DateTime LIKE ""{lastDateForExercises[i].DateTime.ToString("yyyy-MM-dd")}%""");

                if (i != lastDateForExercises.Count - 1)
                    sb.AppendLine("UNION");
            }

            return queryExecuter.Invoke(sb.ToString());
        }

        private static string FromIEnumerableIntToString(IEnumerable<int> arg)
        {
            return string.Join(", ", arg);
        }
    }
}