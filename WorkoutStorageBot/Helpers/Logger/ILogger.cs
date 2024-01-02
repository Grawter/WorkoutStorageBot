

namespace WorkoutStorageBot.Helpers.Logger
{
    public interface ILogger
    {
        void WriteLog(string message, LogType logType = LogType.Information);
        void ClearLog();
    }
}