using WorkoutStorageBot.Core.Abstraction;
using WorkoutStorageBot.Core.Manager;
using WorkoutStorageBot.Model.HandlerData;
using WorkoutStorageBot.Model.Logging;

namespace WorkoutStorageBot.BusinessLogic.CoreRepositories.Repositories
{
    internal class LogsRepository : CoreRepository
    {
        internal LogsRepository(CoreTools coreTools, CoreManager coreManager) : base(coreTools, coreManager, nameof(AdminRepository))
        {
        }

        internal LogsRepository(CoreTools coreTools) : base(coreTools, nameof(AdminRepository))
        {
        }

        internal IQueryable<Log> GetLogs(int count)
        {
            return this.CoreTools.Db.Logs.OrderByDescending(x => x.Id)
                                         .Take(count);
        }

        internal IQueryable<Log> GetLogs(string logLevel, int count)
        {
            IQueryable<Log> predResult = this.CoreTools.Db.Logs.Where(x => x.LogLevel == logLevel);

            return GetLogs(predResult, count);
        }

        internal IQueryable<Log> GetLogs(int eventID, int count)
        {
            IQueryable<Log> predResult = this.CoreTools.Db.Logs.Where(x => x.EventID == eventID);

            return GetLogs(predResult, count);
        }

        internal IQueryable<Log> GetLogsById(int id, int count)
        {
            IQueryable<Log> predResult = this.CoreTools.Db.Logs.Where(x => x.Id == id);

            return GetLogs(predResult, count);
        }

        private IQueryable<Log> GetLogs(IQueryable<Log> predResult, int count)
        {
            return predResult.OrderByDescending(x => x.Id)
                             .Take(count);
        }
    }
}