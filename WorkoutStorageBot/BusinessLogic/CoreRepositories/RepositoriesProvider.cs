#region using

using Microsoft.Extensions.Logging;
using OfficeOpenXml.FormulaParsing.Logging;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.BusinessLogic.CoreRepositories.Repositories;
using WorkoutStorageBot.Core.Abstraction;
using WorkoutStorageBot.Core.Manager;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.HandlerData;

#endregion

namespace WorkoutStorageBot.BusinessLogic.CoreRepositories
{
    internal static class RepositoriesProvider
    {
        internal static List<CoreRepository> InitRepositories(EntityContext db, ILoggerFactory loggerFactory, ConfigurationData configurationData)
            => InitRepositories(null, db, loggerFactory, configurationData);

        internal static List<CoreRepository> InitRepositories(CoreManager? coreManager, EntityContext db, ILoggerFactory loggerFactory, ConfigurationData configurationData)
        {
            CoreTools coreTools = new CoreTools()
            {
                Db = db,
                LoggerFactory = loggerFactory,
                ConfigurationData = configurationData,
            };

            List<CoreRepository> repositories;

            if (coreManager != null)
            {
                repositories = new List<CoreRepository>()
                {
                    new AdminRepository(coreTools, coreManager),
                    new LogsRepository(coreTools, coreManager),
                };
            }
            else
            {
                repositories = new List<CoreRepository>()
                {
                    new AdminRepository(coreTools),
                    new LogsRepository(coreTools),
                };
            }

            return repositories;
        }
    }
}