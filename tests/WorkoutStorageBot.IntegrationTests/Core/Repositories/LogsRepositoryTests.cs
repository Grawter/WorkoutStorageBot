using FluentAssertions;
using WorkoutStorageBot.BusinessLogic.Repositories;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.UnitTests.Helpers;
using WorkoutStorageModels.Entities.Core.Logging;

namespace WorkoutStorageBot.IntegrationTests.Core.Repositories
{
    public class LogsRepositoryTests
    {
        [Fact]
        public void GetLastLogs_WithSpecifiedCount_ShouldReturnSpecifiedCountLastLogs()
        {
            // Arrange
            using EntityContextBuilder builder = new EntityContextBuilder();

            EntityContext entityContext = builder.Create().Build();

            AddTestLogs(entityContext);

            CoreTools coreTools = new CoreTools()
            {
                Db = entityContext,
                ConfigurationData = null,
                LoggerFactory = null,
            };

            LogsRepository logsRepository = new LogsRepository(coreTools, null);

            // Act
            List<Log> logs = logsRepository.GetLastLogs(2).ToList();

            // Assert
            logs.Count.Should().Be(2);
            logs.Count(x => x.Id > 1).Should().Be(2);
        }

        [Fact]
        public void GetLastLogs_WithSpecifiedCountAndLogLevel_ShouldReturnSpecifiedCountLastLogs()
        {
            // Arrange
            using EntityContextBuilder builder = new EntityContextBuilder();

            EntityContext entityContext = builder.Create().Build();

            AddTestLogs(entityContext);

            CoreTools coreTools = new CoreTools()
            {
                Db = entityContext,
                ConfigurationData = null,
                LoggerFactory = null,
            };

            LogsRepository logsRepository = new LogsRepository(coreTools, null);

            // Act
            List<Log> logs = logsRepository.GetLastLogs("Trace", 2).ToList();

            // Assert
            logs.Count.Should().Be(2);
            logs.Count(x => x.Id > 1 && x.LogLevel == "Trace").Should().Be(2);
        }

        [Fact]
        public async Task GetLogByEventId_WithSpecifiedEventId_ShouldReturnLogBySpecifiedEventId()
        {
            // Arrange
            using EntityContextBuilder builder = new EntityContextBuilder();

            EntityContext entityContext = builder.Create().Build();

            AddTestLogs(entityContext);

            CoreTools coreTools = new CoreTools()
            {
                Db = entityContext,
                ConfigurationData = null,
                LoggerFactory = null,
            };

            LogsRepository logsRepository = new LogsRepository(coreTools, null);

            // Act
            Log? log = await logsRepository.GetLogByEventId(200);

            // Assert
            log.Should().NotBeNull();
            log.EventID.Should().Be(200);
        }

        [Fact]
        public async Task GetLogById_WithSpecifiedId_ShouldReturnLogBySpecifiedId()
        {
            // Arrange
            using EntityContextBuilder builder = new EntityContextBuilder();

            EntityContext entityContext = builder.Create().Build();

            AddTestLogs(entityContext);

            CoreTools coreTools = new CoreTools()
            {
                Db = entityContext,
                ConfigurationData = null,
                LoggerFactory = null,
            };

            LogsRepository logsRepository = new LogsRepository(coreTools, null);

            // Act
            Log? log = await logsRepository.GetLogById(3);

            // Assert
            log.Should().NotBeNull();
            log.Id.Should().Be(3);
        }

        private void AddTestLogs(EntityContext entityContext)
        {
            entityContext.Logs.AddRange([
                new Log() 
                { 
                    DateTime = DateTime.Now,
                    LogLevel = "Debug",
                    EventID = 100,
                    Message = "test1",
                    SourceContext = "SourceContext"
                },
                new Log()
                {
                    DateTime = DateTime.Now,
                    LogLevel = "Trace",
                    EventID = 200,
                    Message = "test2",
                    SourceContext = "SourceContext2"
                },
                new Log()
                {
                    DateTime = DateTime.Now,
                    LogLevel = "Trace",
                    EventID = 300,
                    Message = "test3",
                    SourceContext = "SourceContext3"
                },
            ]);

            entityContext.SaveChanges();
        }
    }
}