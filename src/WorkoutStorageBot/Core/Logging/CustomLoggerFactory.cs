using Microsoft.Extensions.Logging;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.Core.Extensions;
using WorkoutStorageBot.Core.Logging.OutputWriter;
using WorkoutStorageBot.Model.AppContext;

namespace WorkoutStorageBot.Core.Logging
{
    internal class CustomLoggerFactory : ICustomLoggerFactory
    {
        private readonly EntityContext db;
        private readonly IOutputWriter outputWriter;
        private readonly ConfigurationData configurationData;

        public CustomLoggerFactory(EntityContext entityContext, ConfigurationData configurationData)
        {
            this.db = entityContext;
            this.configurationData = configurationData;
            this.outputWriter = new ConsoleWriter();
        }

        public CustomLoggerFactory(EntityContext entityContext, IOutputWriter outputWriter, ConfigurationData configurationData)
        {
            this.db = entityContext;
            this.configurationData = configurationData;
            this.outputWriter = outputWriter;
        }

        public ILogger CreateLogger<T>()
            => new CustomLogger(typeof(T).FullName.ThrowIfNullOrWhiteSpace(), db, outputWriter, configurationData.LogInfo);

        public ILogger CreateLogger(string categoryName)
            => new CustomLogger(categoryName, db, outputWriter, configurationData.LogInfo);

        public void Dispose()
            => throw new NotImplementedException();
    }
}