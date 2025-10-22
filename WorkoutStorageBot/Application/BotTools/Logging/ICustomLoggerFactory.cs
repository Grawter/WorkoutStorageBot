#region using

using Microsoft.Extensions.Logging;

#endregion

namespace WorkoutStorageBot.Application.BotTools.Logging
{
    public interface ICustomLoggerFactory
    {
        ILogger CreateLogger(string categoryName);

        ILogger CreateLogger<T>();
    }
}