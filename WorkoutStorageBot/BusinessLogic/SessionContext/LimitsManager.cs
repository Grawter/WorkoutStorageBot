namespace WorkoutStorageBot.BusinessLogic.SessionContext
{
    internal class LimitsManager
    {
        internal LimitsManager(bool isNeedLimits)
        {
            IsEnableLimit = isNeedLimits;

            limits = new Dictionary<string, DateTime>();
        }

        private readonly Dictionary<string, DateTime> limits;

        internal bool IsEnableLimit { get; set; }

        internal bool HasBlockByLimit(string limitName, out DateTime limit)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(limitName);

            limit = DateTime.MinValue;

            if (!IsEnableLimit)
                return false;

            if (!limits.TryGetValue(limitName, out limit))
                return false;

            return limit >= DateTime.Now;
        }

        internal bool AddOrUpdateLimit(string limitName, DateTime newLimit)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(limitName);

            if (!IsEnableLimit) 
                return false;

            if (limits.ContainsKey(limitName))
                limits[limitName] = newLimit;
            else
                limits.Add(limitName, newLimit);

            return true;
        }

        internal void ChangeLimitsMode()
        {
            IsEnableLimit = !IsEnableLimit;
        }
    }
}