

namespace WorkoutStorageBot.Helpers.Logger
{
    public class ConsoleLogger : ILogger
    {
        void ILogger.WriteLog(string message, LogType logType) => Console.WriteLine($"{DateTime.Now} |{logType}| {message}");

        void ILogger.ClearLog() => Console.Clear();

    }
}