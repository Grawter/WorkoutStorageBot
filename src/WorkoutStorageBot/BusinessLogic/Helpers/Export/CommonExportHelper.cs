using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.BusinessLogic.Helpers.Export
{
    internal static class CommonExportHelper
    {
        internal static DateTime GetFilterDateTime(int monthFilterPeriod, IQueryable<ResultExercise> fromDBData)
        {
            DateTime filterDateTime = DateTime.MinValue;

            if (monthFilterPeriod > 0)
            {
                filterDateTime = fromDBData.Select(r => r.DateTime)
                                           .Max()
                                           .AddMonths(-monthFilterPeriod);            
            }

            return filterDateTime;

        }

        internal static IQueryable<ResultExercise> GetResultExercisesByFilterDate (IQueryable<ResultExercise> sourceDBData, DateTime filterDate)
        {
            if (filterDate > DateTime.MinValue)
                return sourceDBData.Where(re => re.DateTime >= filterDate);
            else
                return sourceDBData;
        }
    }
}