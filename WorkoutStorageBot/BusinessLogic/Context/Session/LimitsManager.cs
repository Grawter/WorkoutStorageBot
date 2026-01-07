namespace WorkoutStorageBot.BusinessLogic.Context.Session
{
    internal class LimitsManager
    {
        internal LimitsManager(bool isNeedLimits)
        {
            IsEnableLimit = isNeedLimits;

            timeLimits = new Dictionary<string, DateTime>();
        }

        private readonly Dictionary<string, DateTime> timeLimits;

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

            if (timeLimits.ContainsKey(limitName))
                timeLimits[limitName] = newLimit;
            else
                timeLimits.Add(limitName, newLimit);

            return true;
        }

        internal void ChangeLimitsMode()
        {
            IsEnableLimit = !IsEnableLimit;
        }
    }
}