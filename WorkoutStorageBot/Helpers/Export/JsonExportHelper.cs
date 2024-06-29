#region using
using Newtonsoft.Json;
using System.Text;
using WorkoutStorageBot.Model;
#endregion

namespace WorkoutStorageBot.Helpers.Export
{
    internal static class JsonExportHelper
    {
        internal static string GetJSONFile(List<Cycle> cycles, IQueryable<ResultExercise> resultsExercises, int monthFilterPeriod)
        {
            DateTime filterDateTime = CommonExportHelper.GetFilterDateTime(monthFilterPeriod, resultsExercises);
            CommonExportHelper.LoadDBDataToDBContextForFilterDate(resultsExercises, filterDateTime);

            return RemoveAdminInfo(JsonConvert.SerializeObject(cycles, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        private static string RemoveAdminInfo(string json)
        {
            int startIndexDelete = json.IndexOf(",\"UserInformationId\"");
            int lengthToDelete = json.Length - startIndexDelete - 2;

            var sb = new StringBuilder(json);

            sb.Remove(startIndexDelete, lengthToDelete);
            return sb.ToString();
        }
    }
}