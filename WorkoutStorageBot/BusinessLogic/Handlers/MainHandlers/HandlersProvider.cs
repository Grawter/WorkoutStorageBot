#region using

using Microsoft.Extensions.Logging;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.BusinessLogic.Handlers.MainHandlers.Handlers;
using WorkoutStorageBot.Core.Abstraction;
using WorkoutStorageBot.Core.Manager;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.HandlerData;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers.MainHandlers
{
    internal static class HandlersProvider
    {
        internal static List<CoreHandler> InitHandlers(EntityContext db, ILoggerFactory loggerFactory, ConfigurationData configurationData)
            => InitHandlers(null, db, loggerFactory, configurationData);
        
        internal static List<CoreHandler> InitHandlers(CoreManager? coreManager, EntityContext db, ILoggerFactory loggerFactory, ConfigurationData configurationData)
        {
            CoreTools coreTools = new CoreTools()
            {
                Db = db,
                LoggerFactory = loggerFactory,
                ConfigurationData = configurationData
            };

            List<CoreHandler> handlers;

            if (coreManager != null)
            {
                handlers = new List<CoreHandler>()
                {
                    new PrimaryUpdateHandler(coreTools, coreManager),
                    new UpdateHandler(coreTools, coreManager),
                };
            }
            else
            {
                handlers = new List<CoreHandler>()
                {
                    new PrimaryUpdateHandler(coreTools),
                    new UpdateHandler(coreTools),
                };
            }

            return handlers;
        }
    }
}