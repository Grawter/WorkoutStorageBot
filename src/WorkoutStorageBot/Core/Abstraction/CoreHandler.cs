using Microsoft.Extensions.Logging;
using WorkoutStorageBot.Core.Manager;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.HandlerData.Results;

namespace WorkoutStorageBot.Core.Abstraction
{
    internal abstract class CoreHandler : CoreData
    {
        internal string HandlerName { get; }

        protected virtual ILogger Logger { get; }

        protected CoreHandler(CoreTools coreTools, CoreManager coreManager, string handlerName) : base(coreTools, coreManager)
        {
            HandlerName = handlerName;
            Logger = CoreTools.LoggerFactory.CreateLogger<CoreHandler>();
        }

        internal abstract Task<HandlerResult> Process(HandlerResult handlerResult);
    }
}