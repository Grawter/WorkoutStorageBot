

namespace WorkoutStorageBot.Helpers.Logger
{
    public class ConsoleLogger : ILogger
    {
        public void WriteLog(string message, LogType logType)
        {
            Console.WriteLine($"{DateTime.Now} |{logType}| {message}");
        }

        public void ClearLog()
        {
            Console.Clear();
        }
    }
}