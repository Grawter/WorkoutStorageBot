using Microsoft.Extensions.Logging;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.Core.Helpers;
using WorkoutStorageBot.Model.AppContext;

namespace WorkoutStorageBot.Core.Logging
{
    internal class CustomLoggerFactory : ICustomLoggerFactory
    {
        private readonly EntityContext db;
        private readonly ConfigurationData configurationData;

        public CustomLoggerFactory(EntityContext entityContext, ConfigurationData configurationData)
        {
            db = CommonHelper.GetIfNotNull(entityContext);
            this.configurationData = CommonHelper.GetIfNotNull(configurationData);
        }

        public ILogger CreateLogger<T>()
            => new CustomLogger(typeof(T).FullName, db, configurationData);

        public ILogger CreateLogger(string categoryName)
            => new CustomLogger(categoryName, db, configurationData);

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}