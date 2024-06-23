namespace WorkoutStorageBot.Logger
{
    internal interface ILogger
    {
        void WriteLog(string message, LogType logType = LogType.Information);
        void ClearLog();
    }
}