using WorkoutStorageBot.Model.Entities.BusinessLogic;
using Microsoft.IO;

namespace WorkoutStorageBot.BusinessLogic.Helpers.Export
{
    internal static class CommonExportHelper
    {
        internal static RecyclableMemoryStreamManager RecyclableMSManager { get; } = new RecyclableMemoryStreamManager();

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

        internal static IQueryable<ResultExercise> GetResultExercisesByFilterDate (IQueryable<ResultExercise> sourceDBData, DateTime filterDate)
            => sourceDBData.Where(re =>
                                    filterDate > DateTime.MinValue
                                        ? re.DateTime >= filterDate
                                        : true);
        
    }
}