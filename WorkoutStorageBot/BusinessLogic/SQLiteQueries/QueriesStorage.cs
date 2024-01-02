#region using
using WorkoutStorageBot.Model;

#endregion

namespace WorkoutStorageBot.BusinessLogic.SQLiteQueries
{
    internal static class QueriesStorage
    {
        internal static List<Exercise>? GetExercisesWithDaysIds(IEnumerable<int> ids, Func<string, List<Exercise>> quertExecuter)
        {
            var args = FromIEnumerableIntToString(ids);

            string query = $@"SELECT Exercises.Id, Exercises.NameExercise, Exercises.DayId FROM Exercises
							    WHERE Exercises.DayId IN ({args});";

            return quertExecuter.Invoke(query);
        }

        internal static List<ResultExercise>? GetLastResultsExercisesWithExercisesIds(IEnumerable<int> ids, Func<string, List<ResultExercise>> quertExecuter)
        {
            var args = FromIEnumerableIntToString(ids);

            string query = $@"WITH LastResults (Id, Count, Weight, DateTime, ExerciseId)
                                AS
                                (
                                    SELECT * FROM ResultsExercises
                                    WHERE ResultsExercises.ExerciseId IN ({args})
                                )

                                SELECT * FROM LastResults
                                WHERE DateTime = (SELECT MAX(DateTime) FROM LastResults)";

            return quertExecuter.Invoke(query);
        }

        internal static List<ResultExercise>? GetLastResultsForExercisesWithExercisesIds(IEnumerable<int> ids, Func<string, List<ResultExercise>> quertExecuter)
        {
            var args = FromIEnumerableIntToString(ids);

            string query = $@"WITH LastResults (DateTime, ExerciseId)
                                AS
                                ( 	SELECT * FROM 
		                                (
		                                SELECT DateTime, ExerciseId FROM ResultsExercises
		                                WHERE ExerciseId IN ({args})
		                                ORDER BY ExerciseId ASC, DateTime DESC
		                                )
	                                GROUP BY ExerciseId
                                )
			
                            SELECT * FROM ResultsExercises
                            WHERE ExerciseId IN (SELECT ExerciseId FROM LastResults) AND
	                            DateTime IN (SELECT DateTime FROM LastResults)";

            return quertExecuter.Invoke(query);
        }

        private static string FromIEnumerableIntToString(IEnumerable<int> arg)
        {
            return string.Join(", ", arg);
        }
    }
}