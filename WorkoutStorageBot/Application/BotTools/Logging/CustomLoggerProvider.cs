#region using

using Microsoft.Extensions.Logging;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.Model.AppContext;

#endregion

namespace WorkoutStorageBot.Application.BotTools.Logging
{
    internal class CustomLoggerProvider : ILoggerProvider
    {
        private readonly EntityContext db;

        private readonly ConfigurationData configurationData;

        internal CustomLoggerProvider(EntityContext db, ConfigurationData configurationData)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(configurationData);

            this.db = db;
            this.configurationData = configurationData;
        }

        public ILogger CreateLogger(string categoryName)
            => new CustomLogger(categoryName, db, configurationData);

        public void Dispose()
        {
        }
    }
}