using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.BusinessLogic.Context.Global;
using WorkoutStorageBot.Core.Logging;
using WorkoutStorageBot.Core.Sender;
using WorkoutStorageBot.Model.AppContext;

namespace WorkoutStorageBot.Model.DTO.HandlerData
{
    internal class CoreTools
    {
        internal required ConfigurationData ConfigurationData { get; init; }
        internal required EntityContext Db { get; init; }
        internal required IContextKeeper ContextKeeper { get; init; }
        internal required IBotResponseSender BotResponseSender { get; init; }
        internal required ICustomLoggerFactory LoggerFactory { get; init; }
        internal required CancellationTokenSource AppCTS { get; init; }
    }
}