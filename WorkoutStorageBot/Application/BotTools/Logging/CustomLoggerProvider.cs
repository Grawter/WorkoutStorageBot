#region using

using Microsoft.Extensions.Logging;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.Model.AppContext;

#endregion

namespace WorkoutStorageBot.Application.BotTools.Logging
{
    internal class CustomLoggerProvider : ILoggerProvider
    {
        private readonly EntityContext _db;

        private readonly ConfigurationData _configurationData;

        internal CustomLoggerProvider(EntityContext db, ConfigurationData configurationData)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(configurationData);

            _db = db;
            _configurationData = configurationData;
        }

        public ILogger CreateLogger(string categoryName)
            => new CustomLogger(categoryName, _db, _configurationData);

        public void Dispose()
        {
        }
    }
}