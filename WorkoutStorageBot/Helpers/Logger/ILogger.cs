

namespace WorkoutStorageBot.Helpers.Logger
{
    internal interface ILogger
    {
        void WriteLog(string message, LogType logType = LogType.Information);
        void ClearLog();
    }
}