using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.RegularExpressions;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.Core.Consts;
using WorkoutStorageBot.Core.Helpers;
using WorkoutStorageBot.Core.Logging;
using WorkoutStorageBot.Core.Logging.OutputWriter;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.Log;
using WorkoutStorageBot.UnitTests.Helpers;
using WorkoutStorageModels.Entities.Core.Logging;

namespace WorkoutStorageBot.IntegrationTests.Core.Logging
{
    public class CustomLoggerTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(11)]
        [InlineData(12)]
        [InlineData(13)]
        [InlineData(14)]
        [InlineData(15)]
        [InlineData(16)]

        public void Log_WithAllowedLogLevel_ShouldCorrectRecordLog(int logCase)
        {
            // Arrange
            using EntityContextBuilder builder = new EntityContextBuilder();

            EntityContext entityContext = builder.Create().Build();

            Mock<IOutputWriter> outputWriterMock = new Mock<IOutputWriter>();

            //Trace, Debug, Information, Warning, Error, Critical, None, All.
            LogSettings logSettings = new LogSettings()
            {
                MainRuleLog = new RuleLog()
                {
                    ConsoleLogLevels = ["Trace"],
                    DBLogLevels = ["Trace"]
                }
            };

            string categoryName = nameof(CustomLoggerTests);

            ILogger customLogger = new CustomLogger(categoryName, entityContext, outputWriterMock.Object, logSettings);

            string logStr = "TestLog";

            string regexStr;

            // Act
            if (logCase == 0)
            {
                customLogger.LogTrace(logStr);
                regexStr = $@"\[Trace\] \[.*?\] CustomLoggerTests:{Environment.NewLine}{logStr}";

                // Assert
                Log log = entityContext.Logs.First();

                log.LogLevel.Should().Be("Trace");
                log.Message.Should().Be(logStr);
                log.EventID.Should().BeNull();
                log.EventName.Should().BeNull();
                log.SourceContext.Should().Be(categoryName);
                log.TelegramUserId.Should().BeNull();
            }
            else if (logCase == 1)
            {
                EventId eventId = new EventId(1);
                customLogger.LogTrace(eventId, logStr);
                regexStr = $@"\[Trace\] \[{eventId.Id}\] \[.*?\] CustomLoggerTests:{Environment.NewLine}{logStr}";

                // Assert
                Log log = entityContext.Logs.First();

                log.LogLevel.Should().Be("Trace");
                log.Message.Should().Be(logStr);
                log.EventID.Should().Be(eventId.Id);
                log.EventName.Should().Be(eventId.Name);
                log.SourceContext.Should().Be(categoryName);
                log.TelegramUserId.Should().BeNull();
            }
            else if (logCase == 2)
            {
                Exception ex = new Exception("TestEx");
                customLogger.LogTrace(ex, logStr);
                regexStr = $@"\[Trace\] \[.*?\] CustomLoggerTests:{Environment.NewLine}Text: {logStr} Ex: {ex.ToString()}";
                logStr = $"Text: {logStr} Ex: {ex.ToString()}";

                // Assert
                Log log = entityContext.Logs.First();

                log.LogLevel.Should().Be("Trace");
                log.Message.Should().Be(logStr);
                log.EventID.Should().BeNull();
                log.EventName.Should().BeNull();
                log.SourceContext.Should().Be(categoryName);
                log.TelegramUserId.Should().BeNull();
            }
            else if (logCase == 3)
            {
                EventId eventId = new EventId(1);
                Exception ex = new Exception("TestEx");
                customLogger.LogTrace(new EventId(1), ex, logStr);
                regexStr = $@"\[Trace\] \[{eventId.Id}\] \[.*?\] CustomLoggerTests:{Environment.NewLine}Text: {logStr} Ex: {ex.ToString()}";
                logStr = $"Text: {logStr} Ex: {ex.ToString()}";

                // Assert
                Log log = entityContext.Logs.First();

                log.LogLevel.Should().Be("Trace");
                log.Message.Should().Be(logStr);
                log.EventID.Should().Be(eventId.Id);
                log.EventName.Should().Be(eventId.Name);
                log.SourceContext.Should().Be(categoryName);
                log.TelegramUserId.Should().BeNull();
            }
            else if (logCase == 4)
            {
                Exception ex = new Exception("TestEx");
                customLogger.LogTrace(ex, null);
                regexStr = $@"\[Trace\] \[.*?\] CustomLoggerTests:{Environment.NewLine}Ex: {ex.ToString()}";
                logStr = $"Ex: {ex.ToString()}";

                // Assert
                Log log = entityContext.Logs.First();

                log.LogLevel.Should().Be("Trace");
                log.Message.Should().Be(logStr);
                log.EventID.Should().BeNull();
                log.EventName.Should().BeNull();
                log.SourceContext.Should().Be(categoryName);
                log.TelegramUserId.Should().BeNull();
            }
            else if (logCase == 5)
            {
                EventId eventId = new EventId(1);
                Exception ex = new Exception("TestEx");
                customLogger.LogTrace(eventId, ex, null);
                regexStr = $@"\[Trace\] \[{eventId.Id}\] \[.*?\] CustomLoggerTests:{Environment.NewLine}Ex: {ex.ToString()}";
                logStr = $"Ex: {ex.ToString()}";

                // Assert
                Log log = entityContext.Logs.First();

                log.LogLevel.Should().Be("Trace");
                log.Message.Should().Be(logStr);
                log.EventID.Should().Be(eventId.Id);
                log.EventName.Should().Be(eventId.Name);
                log.SourceContext.Should().Be(categoryName);
                log.TelegramUserId.Should().BeNull();
            }
            else if (logCase == 6)
            {
                customLogger.Log(LogLevel.Trace, logStr);
                regexStr = $@"\[Trace\] \[.*?\] CustomLoggerTests:{Environment.NewLine}{logStr}";

                // Assert
                Log log = entityContext.Logs.First();

                log.LogLevel.Should().Be("Trace");
                log.Message.Should().Be(logStr);
                log.EventID.Should().BeNull();
                log.EventName.Should().BeNull();
                log.SourceContext.Should().Be(categoryName);
                log.TelegramUserId.Should().BeNull();
            }
            else if (logCase == 7)
            {
                EventId eventId = new EventId(1);
                customLogger.Log(LogLevel.Trace, eventId, logStr);
                regexStr = $@"\[Trace\] \[.*?\] CustomLoggerTests:{Environment.NewLine}{logStr}";

                // Assert
                Log log = entityContext.Logs.First();

                log.LogLevel.Should().Be("Trace");
                log.Message.Should().Be(logStr);
                log.EventID.Should().Be(eventId.Id);
                log.EventName.Should().Be(eventId.Name);
                log.SourceContext.Should().Be(categoryName);
                log.TelegramUserId.Should().BeNull();
            }
            else if (logCase == 8)
            {
                Exception ex = new Exception("TestEx");
                customLogger.Log(LogLevel.Trace, ex, logStr);
                regexStr = $@"\[Trace\] \[.*?\] CustomLoggerTests:{Environment.NewLine}Text: {logStr} Ex: {ex.ToString()}";
                logStr = $"Text: {logStr} Ex: {ex.ToString()}";

                // Assert
                Log log = entityContext.Logs.First();

                log.LogLevel.Should().Be("Trace");
                log.Message.Should().Be(logStr);
                log.EventID.Should().BeNull();
                log.EventName.Should().BeNull();
                log.SourceContext.Should().Be(categoryName);
                log.TelegramUserId.Should().BeNull();
            }
            else if (logCase == 9)
            {
                EventId eventId = new EventId(1);
                Exception ex = new Exception("TestEx");
                customLogger.Log(LogLevel.Trace, eventId, ex, logStr);
                regexStr = $@"\[Trace\] \[{eventId.Id}\] \[.*?\] CustomLoggerTests:{Environment.NewLine}Text: {logStr} Ex: {ex.ToString()}";
                logStr = $"Text: {logStr} Ex: {ex.ToString()}";

                // Assert
                Log log = entityContext.Logs.First();

                log.LogLevel.Should().Be("Trace");
                log.Message.Should().Be(logStr);
                log.EventID.Should().Be(eventId.Id);
                log.EventName.Should().Be(eventId.Name);
                log.SourceContext.Should().Be(categoryName);
                log.TelegramUserId.Should().BeNull();
            }
            else if (logCase == 10)
            {
                EventId eventId = new EventId(1);
                LogData logData = new LogData() { Message = logStr, TelegramUserId = 22 };
                customLogger.Log(LogLevel.Trace, eventId, logData, null, null!);
                regexStr = $@"\[Trace\] \[{eventId.Id}\] \[.*?\] CustomLoggerTests:{Environment.NewLine}{logStr} |by {logData.TelegramUserId}";

                // Assert
                Log log = entityContext.Logs.First();

                log.LogLevel.Should().Be("Trace");
                log.Message.Should().Be(logStr);
                log.EventID.Should().Be(eventId.Id);
                log.EventName.Should().Be(eventId.Name);
                log.SourceContext.Should().Be(categoryName);
                log.TelegramUserId.Should().Be(logData.TelegramUserId);
            }
            else if (logCase == 11)
            {
                EventId eventId = new EventId(1);
                LogData logData = new LogData() { Message = logStr, TelegramUserId = 22 };
                Exception ex = new Exception("TestEx");
                customLogger.Log(LogLevel.Trace, eventId, logData, ex, null!);
                regexStr = $@"\[Trace\] \[{eventId.Id}\] \[.*?\] CustomLoggerTests:{Environment.NewLine}Text: {logStr} Ex: {ex.ToString()} |by {logData.TelegramUserId}";
                logStr = $"Text: {logStr} Ex: {ex.ToString()}";

                // Assert
                Log log = entityContext.Logs.First();

                log.LogLevel.Should().Be("Trace");
                log.Message.Should().Be(logStr);
                log.EventID.Should().Be(eventId.Id);
                log.EventName.Should().Be(eventId.Name);
                log.SourceContext.Should().Be(categoryName);
                log.TelegramUserId.Should().Be(logData.TelegramUserId);
            }
            else if (logCase == 12)
            {
                EventId eventId = new EventId(1);
                LogData logData = new LogData() { Message = logStr, TelegramUserId = 22 };
                Exception ex = new Exception("TestEx");
                customLogger.Log(LogLevel.Trace, eventId, logData, ex, LogFormatter.SimpleFormatter);
                regexStr = $@"\[Trace\] \[{eventId.Id}\] \[.*?\] CustomLoggerTests:{Environment.NewLine}Text: {logStr} Ex: {ex.ToString()} |by {logData.TelegramUserId}";
                logStr = $"Text: {logStr} Ex: {ex.ToString()}";

                // Assert
                Log log = entityContext.Logs.First();

                log.LogLevel.Should().Be("Trace");
                log.Message.Should().Be(logStr);
                log.EventID.Should().Be(eventId.Id);
                log.EventName.Should().Be(eventId.Name);
                log.SourceContext.Should().Be(categoryName);
                log.TelegramUserId.Should().Be(logData.TelegramUserId);
            }
            else if (logCase == 13)
            {
                Exception ex = new Exception("TestEx");
                customLogger.Log(LogLevel.Trace, ex, null);
                regexStr = $@"\[Trace\] \[.*?\] CustomLoggerTests:{Environment.NewLine}Ex: {ex.ToString()}";
                logStr = $"Ex: {ex.ToString()}";

                // Assert
                Log log = entityContext.Logs.First();

                log.LogLevel.Should().Be("Trace");
                log.Message.Should().Be(logStr);
                log.EventID.Should().BeNull();
                log.EventName.Should().BeNull();
                log.SourceContext.Should().Be(categoryName);
                log.TelegramUserId.Should().BeNull();
            }
            else if (logCase == 14)
            {
                EventId eventId = new EventId(1);
                Exception ex = new Exception("TestEx");
                customLogger.Log(LogLevel.Trace, eventId, ex, null);
                regexStr = $@"\[Trace\] \[{eventId.Id}\] \[.*?\] CustomLoggerTests:{Environment.NewLine}Ex: {ex.ToString()}";
                logStr = $"Ex: {ex.ToString()}";

                // Assert
                Log log = entityContext.Logs.First();

                log.LogLevel.Should().Be("Trace");
                log.Message.Should().Be(logStr);
                log.EventID.Should().Be(eventId.Id);
                log.EventName.Should().Be(eventId.Name);
                log.SourceContext.Should().Be(categoryName);
                log.TelegramUserId.Should().BeNull();
            }
            else if (logCase == 15)
            {
                EventId eventId = new EventId(1);

                LogData logData = new LogData() { Message = logStr, TelegramUserId = 22 };
                customLogger.Log(LogLevel.Trace, eventId, logData, null, LogFormatter.SimpleFormatter);
                regexStr = $@"\[Trace\] \[{eventId.Id}\] \[.*?\] CustomLoggerTests:{Environment.NewLine}{logData.Message} |by {logData.TelegramUserId}";

                // Assert
                Log log = entityContext.Logs.First();

                log.LogLevel.Should().Be("Trace");
                log.Message.Should().Be(logData.Message);
                log.EventID.Should().Be(eventId.Id);
                log.EventName.Should().Be(eventId.Name);
                log.SourceContext.Should().Be(categoryName);
                log.TelegramUserId.Should().Be(logData.TelegramUserId);
            }
            else if (logCase == 16)
            {
                EventId eventId = new EventId(1);
                Exception ex = new Exception("TestEx");
                LogData logData = new LogData() { TelegramUserId = 22 };
                customLogger.Log(LogLevel.Trace, eventId, logData, ex, LogFormatter.SimpleFormatter);
                regexStr = $@"\[Trace\] \[{eventId.Id}\] \[.*?\] CustomLoggerTests:{Environment.NewLine}Ex: {ex.ToString()} |by {logData.TelegramUserId}";
                logStr = $"Ex: {ex.ToString()}";

                // Assert
                Log log = entityContext.Logs.First();

                log.LogLevel.Should().Be("Trace");
                log.Message.Should().Be(logStr);
                log.EventID.Should().Be(eventId.Id);
                log.EventName.Should().Be(eventId.Name);
                log.SourceContext.Should().Be(categoryName);
                log.TelegramUserId.Should().Be(logData.TelegramUserId);
            }
            else
                throw new InvalidOperationException($"Неожиданный {nameof(logCase)}");

            // какое сообщение было передано в консоль
            outputWriterMock.Verify(w =>
                                        w.Write(It.Is<string>(s => Regex.IsMatch(s, regexStr))),
                                        Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(11)]
        [InlineData(12)]
        [InlineData(13)]
        [InlineData(14)]
        [InlineData(15)]
        public void Log_WithNullOrEmptyString_ShouldNotRecordLog(int logCase)
        {
            // Arrange
            using EntityContextBuilder builder = new EntityContextBuilder();

            EntityContext entityContext = builder.Create().Build();

            Mock<IOutputWriter> outputWriterMock = new Mock<IOutputWriter>();

            //Trace, Debug, Information, Warning, Error, Critical, None, All.
            LogSettings logSettings = new LogSettings()
            {
                MainRuleLog = new RuleLog()
                {
                    ConsoleLogLevels = ["Trace"],
                    DBLogLevels = ["Trace"]
                }
            };

            ILogger customLogger = new CustomLogger(typeof(CustomLoggerTests).Name, entityContext, outputWriterMock.Object, logSettings);

            // Act
            if (logCase == 0)
                customLogger.LogTrace(null);
            else if (logCase == 1)
                customLogger.LogTrace(string.Empty);
            else if (logCase == 2)
                customLogger.LogTrace(new EventId(1), null);
            else if (logCase == 3)
                customLogger.LogTrace(new EventId(1), string.Empty);
            else if (logCase == 4)
                customLogger.LogTrace(exception: null, null);
            else if (logCase == 5)
                customLogger.LogTrace(exception: null, string.Empty);
            else if (logCase == 6)
                customLogger.LogTrace(new EventId(1), exception: null, null);
            else if (logCase == 7)
                customLogger.LogTrace(new EventId(1), exception: null, string.Empty);
            else if (logCase == 8)
                customLogger.Log(LogLevel.Trace, null);
            else if (logCase == 9)
                customLogger.Log(LogLevel.Trace, string.Empty);
            else if (logCase == 10)
                customLogger.Log(LogLevel.Trace, new EventId(1), null);
            else if (logCase == 11)
                customLogger.Log(LogLevel.Trace, new EventId(1), string.Empty);
            else if (logCase == 12)
                customLogger.Log(LogLevel.Trace, exception: null, null);
            else if (logCase == 13)
                customLogger.Log(LogLevel.Trace, exception: null, string.Empty);
            else if (logCase == 14)
                customLogger.Log(LogLevel.Trace, new EventId(1), exception: null, null);
            else if (logCase == 15)
                customLogger.Log(LogLevel.Trace, new EventId(1), null, string.Empty);
            else
                throw new InvalidOperationException($"Неожиданный {nameof(logCase)}");

            // Assert
            entityContext.Logs.Should().BeEmpty();

            outputWriterMock.Verify(w =>
                                    w.Write(It.IsAny<string>()),
                                    Times.Never);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        public void Log_WithNotAllowedLogLevel_ShouldNotRecordLog(int logCase)
        {
            // Arrange
            using EntityContextBuilder builder = new EntityContextBuilder();

            EntityContext entityContext = builder.Create().Build();

            Mock<IOutputWriter> outputWriterMock = new Mock<IOutputWriter>();

            //Trace, Debug, Information, Warning, Error, Critical, None, All.
            LogSettings logSettings = new LogSettings()
            {
                MainRuleLog = new RuleLog()
                {
                    ConsoleLogLevels = ["Debug"],
                    DBLogLevels = ["Debug"]
                }
            };

            ILogger customLogger = new CustomLogger(typeof(CustomLoggerTests).Name, entityContext, outputWriterMock.Object, logSettings);

            string logStr = "TestLog";

            // Act
            if (logCase == 0)
                customLogger.LogTrace(logStr);
            else if (logCase == 1)
                customLogger.LogTrace(new EventId(1), logStr);
            else if (logCase == 2)
                customLogger.LogTrace(new Exception("TestEx"), logStr);
            else if (logCase == 3)
                customLogger.LogTrace(new EventId(1), new Exception("TestEx"), logStr);
            else if (logCase == 4)
                customLogger.Log(LogLevel.Trace, logStr);
            else if (logCase == 5)
                customLogger.Log(LogLevel.Trace, new EventId(1), logStr);
            else if (logCase == 6)
                customLogger.Log(LogLevel.Trace, new Exception("TestEx"), logStr);
            else if (logCase == 7)
                customLogger.Log(LogLevel.Trace, new EventId(1), new Exception("TestEx"), logStr);
            else
                throw new InvalidOperationException($"Неожиданный {nameof(logCase)}");

            // Assert
            entityContext.Logs.Should().BeEmpty();

            outputWriterMock.Verify(w =>
                                    w.Write(It.IsAny<string>()),
                                    Times.Never);
        }

        [Fact]
        public void Log_WithMessageMoreThanDefaultMaxCharactersCount_ShouldTrimMessageRecord()
        {
            // Arrange
            using EntityContextBuilder builder = new EntityContextBuilder();

            EntityContext entityContext = builder.Create().Build();

            Mock<IOutputWriter> outputWriterMock = new Mock<IOutputWriter>();

            //Trace, Debug, Information, Warning, Error, Critical, None, All.
            LogSettings logSettings = new LogSettings()
            {
                MainRuleLog = new RuleLog()
                {
                    ConsoleLogLevels = ["Trace"],
                    DBLogLevels = ["Trace"]
                }
            };

            ILogger customLogger = new CustomLogger(typeof(CustomLoggerTests).Name, entityContext, outputWriterMock.Object, logSettings);

            // Act
            customLogger.LogTrace(new string('c', CoreConsts.Log.SaveLimit + 100));

            // Assert
            Log log = entityContext.Logs.First();

            log.Message.Length.Should().Be(CoreConsts.Log.SaveLimit);

            outputWriterMock.Verify(w =>
                                        w.Write(It.Is<string>(s => s.Length < CoreConsts.Log.SaveLimit + 100)),
                                        Times.Once);
        }
    }
}