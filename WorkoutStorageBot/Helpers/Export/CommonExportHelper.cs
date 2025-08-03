#region using

using WorkoutStorageBot.Model.DomainsAndEntities;

#endregion
namespace WorkoutStorageBot.Helpers.Export
{
    internal static class CommonExportHelper
    {
        internal static DateTime GetFilterDateTime(int monthFilterPeriod, IQueryable<ResultExercise> fromDBData)
        {
            DateTime filterDateTime = DateTime.MinValue;

            if (monthFilterPeriod > 0)
            {
                filterDateTime = fromDBData
                                        .Select(r => r.DateTime)
                                        .Max()
                                        .AddMonths(-monthFilterPeriod);            
            }

            return filterDateTime;

        }

        internal static void LoadDBDataToDBContextForFilterDate (IQueryable<ResultExercise> sourceDBData, DateTime filterDate)
        {
            sourceDBData.Where(re => 
                                filterDate > DateTime.MinValue
                                    ? re.DateTime >= filterDate
                                    : true)
                        .ToList();
        }
    }
}