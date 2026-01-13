using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.Core.Logging;
using WorkoutStorageBot.Model.AppContext;

namespace WorkoutStorageBot.Model.DTO.HandlerData
{
    internal class CoreTools
    {
        internal EntityContext Db { get; init; }
        internal ICustomLoggerFactory LoggerFactory { get; init; }
        internal ConfigurationData ConfigurationData { get; init; }
    }
}