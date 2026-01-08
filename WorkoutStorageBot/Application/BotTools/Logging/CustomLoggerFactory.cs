using Microsoft.Extensions.Logging;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.Helpers.Common;
using WorkoutStorageBot.Model.AppContext;

namespace WorkoutStorageBot.Application.BotTools.Logging
{
    internal class CustomLoggerFactory : ICustomLoggerFactory
    {
        private readonly EntityContext db;
        private readonly ConfigurationData configurationData;

        public CustomLoggerFactory(EntityContext entityContext, ConfigurationData configurationData)
        {
            this.db = CommonHelper.GetIfNotNull(entityContext);
            this.configurationData = CommonHelper.GetIfNotNull(configurationData);
        }

        public ILogger CreateLogger<T>()
            => new CustomLogger(typeof(T).FullName, this.db, this.configurationData);

        public ILogger CreateLogger(string categoryName)
            => new CustomLogger(categoryName, this.db, this.configurationData);

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}