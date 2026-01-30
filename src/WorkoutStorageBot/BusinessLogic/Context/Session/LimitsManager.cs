using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace WorkoutStorageBot.BusinessLogic.Context.Session
{
    internal class LimitsManager
    {
        internal LimitsManager(bool isNeedLimits)
        {
            IsEnableLimit = isNeedLimits;

            timeLimits = new ConcurrentDictionary<string, DateTime>();
        }

        private readonly ConcurrentDictionary<string, DateTime> timeLimits;

        internal bool IsEnableLimit { get; set; }

        internal bool HasBlockByTimeLimit(string limitName, out DateTime limit)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(limitName);

            limit = DateTime.MinValue;

            if (!IsEnableLimit)
                return false;

            if (!timeLimits.TryGetValue(limitName, out limit))
                return false;

            return limit >= DateTime.Now;
        }

        internal bool AddOrUpdateTimeLimit(string limitName, DateTime newLimit)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(limitName);

            if (!IsEnableLimit) 
                return false;

            timeLimits.AddOrUpdate(limitName, newLimit, (_, _) => newLimit);

            return true;
        }

        internal void ChangeLimitsMode(ILogger logger)
        {
            IsEnableLimit = !IsEnableLimit;

            logger.LogWarning($"Режим использования лимитов переключён в: {IsEnableLimit.ToString()}");
        }
    }
}