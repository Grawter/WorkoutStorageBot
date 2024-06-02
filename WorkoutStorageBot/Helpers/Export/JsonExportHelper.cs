using Newtonsoft.Json;
using WorkoutStorageBot.Model;

namespace WorkoutStorageBot.Helpers.Export
{
    internal static class JsonExportHelper
    {
        internal static string GetJSONFile(List<Cycle> cycles, IQueryable<ResultExercise> resultsExercises, int monthFilterPeriod)
        {
            DateTime filterDateTime = CommonExportHelper.GetFilterDateTime(monthFilterPeriod, resultsExercises);
            CommonExportHelper.LoadDBDataToDBContextForFilterDate(resultsExercises, filterDateTime);

            return JsonConvert.SerializeObject(cycles, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
        }
    }
}