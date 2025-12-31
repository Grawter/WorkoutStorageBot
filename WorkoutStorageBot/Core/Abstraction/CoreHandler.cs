#region using

using Microsoft.Extensions.Logging;
using WorkoutStorageBot.Core.Manager;
using WorkoutStorageBot.Helpers.Common;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.HandlerData.Results;

#endregion

namespace WorkoutStorageBot.Core.Abstraction
{
    internal abstract class CoreHandler : CoreData
    {
        internal string HandlerName { get; }

        protected virtual ILogger Logger { get; }

        protected CoreHandler(CoreTools coreTools, CoreManager coreManager, string handlerName) : base(coreTools, coreManager)
        {
            HandlerName = CommonHelper.GetIfNotNullOrWhiteSpace(handlerName);
            Logger = CoreTools.LoggerFactory.CreateLogger<CoreHandler>();
        }

        internal abstract HandlerResult Process(HandlerResult handlerResult);

        protected abstract HandlerResult InitHandlerResult(HandlerResult handlerResult);

        protected virtual void ValidateHandlerResult(HandlerResult handlerResult)
        {
            ArgumentNullException.ThrowIfNull(handlerResult);

            if (handlerResult.Update == null)
                throw new InvalidOperationException($"Получен пустой {nameof(handlerResult.Update)}");
        }
    }
}