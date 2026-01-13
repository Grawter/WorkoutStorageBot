using Microsoft.Extensions.Logging;

namespace WorkoutStorageBot.Core.Logging
{
    public interface ICustomLoggerFactory
    {
        ILogger CreateLogger(string categoryName);

        ILogger CreateLogger<T>();
    }
}