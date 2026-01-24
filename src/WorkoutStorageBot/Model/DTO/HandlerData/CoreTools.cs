using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.Core.Logging;
using WorkoutStorageBot.Model.AppContext;

namespace WorkoutStorageBot.Model.DTO.HandlerData
{
    internal class CoreTools
    {
        internal required EntityContext Db { get; init; }
        internal required ICustomLoggerFactory LoggerFactory { get; init; }
        internal required ConfigurationData ConfigurationData { get; init; }
    }
}