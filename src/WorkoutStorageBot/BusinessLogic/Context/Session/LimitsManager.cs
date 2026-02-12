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

        internal bool HasBlockByTimeLimit(string limitName, out DateTime endLimitDateTime)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(limitName);

            endLimitDateTime = DateTime.MinValue;

            if (!IsEnableLimit)
                return false;

            if (!timeLimits.TryGetValue(limitName, out endLimitDateTime))
                return false;

            return endLimitDateTime >= DateTime.Now;
        }

        internal bool AddOrUpdateTimeLimit(string limitName, DateTime newEndLimitDateTime)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(limitName);

            if (!IsEnableLimit) 
                return false;

            timeLimits.AddOrUpdate(limitName, newEndLimitDateTime, (_, _) => newEndLimitDateTime);

            return true;
        }

        internal void ChangeLimitsMode(ILogger logger)
        {
            IsEnableLimit = !IsEnableLimit;

            logger.LogWarning($"Режим использования лимитов переключён в: {IsEnableLimit.ToString()}");
        }
    }
}