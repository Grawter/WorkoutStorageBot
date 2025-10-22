#region using

using WorkoutStorageBot.Application.BotTools.Logging;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.Model.AppContext;

#endregion

namespace WorkoutStorageBot.Model.HandlerData
{
    internal class CoreTools
    {
        internal EntityContext Db { get; init; }
        internal ICustomLoggerFactory LoggerFactory { get; init; }
        internal ConfigurationData ConfigurationData { get; init; }
    }
}