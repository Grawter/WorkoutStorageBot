using Microsoft.EntityFrameworkCore;
using WorkoutStorageBot.Core.Repositories.Abstraction;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageModels.Entities.Core.Logging;

namespace WorkoutStorageBot.BusinessLogic.Repositories
{
    internal class LogsRepository : CoreRepository
    {
        internal LogsRepository(EntityContext db) : base(db, nameof(LogsRepository))
        {}

        internal IQueryable<Log> GetLastLogs(int count)
            => db.Logs.AsNoTracking()
                      .OrderByDescending(x => x.Id)
                      .Take(count);

        internal IQueryable<Log> GetLastLogs(string logLevel, int count)
            => db.Logs.AsNoTracking()
                      .Where(x => x.LogLevel == logLevel)
                      .OrderByDescending(x => x.Id)
                      .Take(count);

        internal async Task<Log?> GetLogByEventId(int eventID)
           => await db.Logs.AsNoTracking()
                           .FirstOrDefaultAsync(x => x.EventID == eventID);

        internal async Task<Log?> GetLogById(int id)
            => await db.Logs.AsNoTracking()
                            .FirstOrDefaultAsync(x => x.Id == id);
    }
}