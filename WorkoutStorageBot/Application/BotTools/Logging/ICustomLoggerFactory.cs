using Microsoft.Extensions.Logging;

namespace WorkoutStorageBot.Application.BotTools.Logging
{
    public interface ICustomLoggerFactory
    {
        ILogger CreateLogger(string categoryName);

        ILogger CreateLogger<T>();
    }
}