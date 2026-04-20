using Microsoft.EntityFrameworkCore;
using WorkoutStorageBot.Core.Abstraction;
using WorkoutStorageBot.Core.Manager;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageModels.Entities.Core.Logging;

namespace WorkoutStorageBot.BusinessLogic.Repositories
{
    internal class LogsRepository : CoreRepository
    {
        internal LogsRepository(CoreTools coreTools, CoreManager coreManager) : base(coreTools, coreManager, nameof(LogsRepository))
        {}

        internal IQueryable<Log> GetLastLogs(int count)
            => CoreTools.Db.Logs.AsNoTracking()
                                .OrderByDescending(x => x.Id)
                                .Take(count);

        internal IQueryable<Log> GetLastLogs(string logLevel, int count)
            => CoreTools.Db.Logs.AsNoTracking()
                                .Where(x => x.LogLevel == logLevel)
                                .OrderByDescending(x => x.Id)
                                .Take(count);

        internal async Task<Log?> GetLogByEventId(int eventID)
           => await CoreTools.Db.Logs.AsNoTracking()
                                     .FirstOrDefaultAsync(x => x.EventID == eventID);

        internal async Task<Log?> GetLogById(int id)
            => await CoreTools.Db.Logs.AsNoTracking()
                                      .FirstOrDefaultAsync(x => x.Id == id);
    }
}