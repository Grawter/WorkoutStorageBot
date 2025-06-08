#region using

using Microsoft.Extensions.Logging;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.Model.AppContext;

#endregion

namespace WorkoutStorageBot.Model.HandlerData
{
    internal class CoreTools
    {
        internal EntityContext Db { get; init; }
        internal ILoggerFactory LoggerFactory { get; init; }
        internal ConfigurationData ConfigurationData { get; init; }
    }
}