using Microsoft.EntityFrameworkCore;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.BusinessLogic.Helpers.Export
{
    internal static class CommonExportHelper
    {
        internal static async Task<DateTime> GetFilterDateTime(int monthFilterPeriod, IQueryable<ResultExercise> fromDBData, CancellationToken cancellationToken)
        {
            DateTime filterDateTime = DateTime.MinValue;

            if (monthFilterPeriod > 0)
            {
                filterDateTime = await fromDBData.Select(r => r.DateTime).MaxAsync(cancellationToken);
                filterDateTime = filterDateTime.AddMonths(-monthFilterPeriod);
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