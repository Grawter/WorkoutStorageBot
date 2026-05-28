using Microsoft.Extensions.Logging;
using WorkoutStorageBot.Core.Repositories.Store;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.HandlerData.Results;

namespace WorkoutStorageBot.Core.Handlers.Abstraction
{
    internal abstract class CoreHandler : CoreData
    {
        internal RepositoriesStore RepositoriesStore { get; }

        internal string HandlerName { get; }

        protected virtual ILogger Logger { get; }

        protected CoreHandler(CoreTools coreTools, RepositoriesStore repositoriesStore, string handlerName) : base(coreTools)
        {
            RepositoriesStore = repositoriesStore;

            HandlerName = handlerName;
            
            Logger = CoreTools.LoggerFactory.CreateLogger<CoreHandler>();
        }

        internal abstract Task<HandlerResult> Process(HandlerResult handlerResult);

        internal void CloseApp(TimeSpan timeSpan)
        {
            if (timeSpan.TotalSeconds < 2)
                timeSpan = TimeSpan.FromSeconds(2);

            Logger.LogWarning("Инициировано отключение бота");

            this.CoreTools.AppCTS.CancelAfter(timeSpan);
        }
    }
}