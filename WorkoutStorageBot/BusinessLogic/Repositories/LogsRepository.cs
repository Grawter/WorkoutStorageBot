using Microsoft.EntityFrameworkCore;
using WorkoutStorageBot.Core.Abstraction;
using WorkoutStorageBot.Core.Manager;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.Entities.Logging;

namespace WorkoutStorageBot.BusinessLogic.Repositories
{
    internal class LogsRepository : CoreRepository
    {
        internal LogsRepository(CoreTools coreTools, CoreManager coreManager) : base(coreTools, coreManager, nameof(AdminRepository))
        {}

        internal IQueryable<Log> GetLogs(int count)
            => CoreTools.Db.Logs.AsNoTracking()
                                .OrderByDescending(x => x.Id)
                                .Take(count);

        internal IQueryable<Log> GetLogs(string logLevel, int count)
            => CoreTools.Db.Logs.AsNoTracking()
                                .Where(x => x.LogLevel == logLevel)
                                .OrderByDescending(x => x.Id)
                                .Take(count);

        internal IQueryable<Log> GetLogs(int eventID, int count)
            => CoreTools.Db.Logs.AsNoTracking()
                                .Where(x => x.EventID == eventID)
                                .OrderByDescending(x => x.Id)
                                .Take(count);

        internal async Task<Log?> GetLogById(int id, int count)
            => await CoreTools.Db.Logs.AsNoTracking()
                                      .FirstOrDefaultAsync();
    }
}