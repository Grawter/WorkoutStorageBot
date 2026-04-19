using FluentAssertions;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.Core.Consts;
using WorkoutStorageBot.Core.Helpers;
using WorkoutStorageBot.Model.DTO.Log;
using WorkoutStorageModels.Entities.Core.Logging;

namespace WorkoutStorageBot.UnitTests.Core.Helpers
{
    public class LogFormatterTests
    {
        [Fact]
        public void SimpleFormatter_WithNullStateAndException_ShouldReturnEmptyStr()
        {
            // Act
            string resultMes = LogFormatter.SimpleFormatter<LogData?>(null, null);

            // Assert
            resultMes.Should().BeEmpty();
        }

        [Fact]
        public void SimpleFormatter_WithNullStateAndNotNullException_ShouldReturnExceptionStr()
        {
            // Arrange
            Exception ex = new Exception("TestEx");

            // Act
            string resultMes = LogFormatter.SimpleFormatter<LogData?>(null, ex);

            // Assert
            resultMes.Should().Be(ex.ToString());
        }

        [Fact]
        public void SimpleFormatter_WithStrStateAndNullException_ShouldReturnStateStr()
        {
            // Arrange
            string state = "123";

            // Act
            string resultMes = LogFormatter.SimpleFormatter<string>(state, null);

            // Assert
            resultMes.Should().Be(state);
        }

        [Fact]
        public void SimpleFormatter_WithStrStateAndNotNullException_ShouldReturnExceptionStr()
        {
            // Arrange
            Exception ex = new Exception("TestEx");

            // Act
            string resultMes = LogFormatter.SimpleFormatter<string>(string.Empty, ex);

            // Assert
            resultMes.Should().Be($"Ex: {ex.ToString()}");
        }

        [Fact]
        public void SimpleFormatter_WithEmptyStrStateAndNotNullException_ShouldReturnFormattedStr()
        {
            // Arrange
            string state = "123";
            Exception ex = new Exception("TestEx");

            // Act
            string resultMes = LogFormatter.SimpleFormatter<string>(state, ex);

            // Assert
            resultMes.Should().Be($"Text: {state} Ex: {ex.ToString()}");
        }

        [Fact]
        public void SimpleFormatter_WithEmptyLogDataStateAndNullException_ShouldReturnEmptyStr()
        {
            // Arrange
            LogData state = new LogData();

            // Act
            string resultMes = LogFormatter.SimpleFormatter<LogData>(state, null);

            // Assert
            resultMes.Should().BeEmpty();
        }

        [Fact]
        public void SimpleFormatter_WithLogDataStateAndNullException_ShouldReturnStateStr()
        {
            // Arrange
            LogData state = new LogData() { Message = "123" };

            // Act
            string resultMes = LogFormatter.SimpleFormatter<LogData>(state, null);

            // Assert
            resultMes.Should().Be(state.Message);
        }

        [Fact]
        public void SimpleFormatter_WithEmptyLogDataStateAndNotNullException_ShouldReturnExceptionStr()
        {
            // Arrange
            LogData state = new LogData();
            Exception ex = new Exception("TestEx");

            // Act
            string resultMes = LogFormatter.SimpleFormatter<LogData>(state, ex);

            // Assert
            resultMes.Should().Be($"Ex: {ex.ToString()}");
        }

        [Fact]
        public void SimpleFormatter_WithLogDataStateAndNotNullException_ShouldReturnFormattedStr()
        {
            // Arrange
            LogData state = new LogData() { Message = "123" };
            Exception ex = new Exception("TestEx");

            // Act
            string resultMes = LogFormatter.SimpleFormatter<LogData>(state, ex);

            // Assert
            resultMes.Should().Be($"Text: {state.Message} Ex: {ex.ToString()}");
        }

        [Fact]
        public void SimpleFormatter_WithUnexpectedState_ShouldReturnStrAboutUnexpectedState()
        {
            // Act
            string resultMes = LogFormatter.SimpleFormatter<DateTime>(DateTime.Now, null);

            // Assert
            resultMes.Should().Contain($"Тип state - {typeof(DateTime).FullName} не поддеживается SimpleFormatter{Environment.NewLine}CallStack:");
        }

        [Fact]
        public void ConvertLogToStr_FullDataWithoutTruncation_ShouldReturnFormattedString()
        {
            // Arrange
            int id = 1;
            string level = "Information";
            int? eventId = 10;
            DateTime dateTime = new DateTime(2020, 6, 1);
            long? telegramId = 12345;
            string context = "MyApp.Services.UserService";
            string message = "Short message";

            Log log = CreateLog(id,
                                level,
                                eventId,
                                dateTime,
                                telegramId,
                                context,
                                message);

            int maxLength = 100;

            // Act
            string result = LogFormatter.ConvertLogToStr(log, maxLength);

            // Assert
            result.Should().Be($"[{id}] [{level}] [{eventId}] [{dateTime.ToString(CommonConsts.Common.DateTimeFormatDateFirst)}] [{telegramId}] [UserService]:{Environment.NewLine}{message}");
        }

        [Fact]
        public void ConvertLogToStr_WithNullUserId_ShouldReturnExpectedFormattedString()
        {
            // Arrange
            int id = 1;
            string level = "Information";
            int? eventId = 10;
            DateTime dateTime = new DateTime(2020, 6, 1);
            long? telegramId = null;
            string context = "MyApp.Services.UserService";
            string message = "Short message";

            Log log = CreateLog(id,
                                level,
                                eventId,
                                dateTime,
                                telegramId,
                                context,
                                message);

            int maxLength = 50;

            // Act
            string result = LogFormatter.ConvertLogToStr(log, maxLength);

            // Assert
            result.Should().Be($"[{id}] [{level}] [{eventId}] [{dateTime.ToString(CommonConsts.Common.DateTimeFormatDateFirst)}] [Null] [UserService]:{Environment.NewLine}{message}");
        }

        [Fact]
        public void ConvertLogToStr_WithZeroUserId_ShouldReturnExpectedFormattedString()
        {
            // Arrange
            int id = 1;
            string level = "Information";
            int? eventId = 10;
            DateTime dateTime = new DateTime(2020, 6, 1);
            long? telegramId = 0;
            string context = "MyApp.Services.UserService";
            string message = "Short message";

            Log log = CreateLog(id,
                                level,
                                eventId,
                                dateTime,
                                telegramId,
                                context,
                                message);


            int maxLength = 50;

            // Act
            string result = LogFormatter.ConvertLogToStr(log, maxLength);

            // Assert
            result.Should().Be($"[{id}] [{level}] [{eventId}] [{dateTime.ToString(CommonConsts.Common.DateTimeFormatDateFirst)}] [Null] [UserService]:{Environment.NewLine}{message}");
        }

        [Fact]
        public void ConvertLogToStr_MessageLongerThanMaxLength_ShouldReturnTruncatesMessage()
        {
            // Arrange
            int id = 1;
            string level = "Information";
            int? eventId = 10;
            DateTime dateTime = new DateTime(2020, 6, 1);
            long? telegramId = 0;
            string context = "MyApp.Services.UserService";
            string message = "This is a very long message that should be truncated";

            Log log = CreateLog(id,
                                level,
                                eventId,
                                dateTime,
                                telegramId,
                                context,
                                message);

            int maxLength = 10;

            // Act
            string result = LogFormatter.ConvertLogToStr(log, maxLength);

            // Assert
            result.Should().Be($"[{id}] [{level}] [{eventId}] [{dateTime.ToString(CommonConsts.Common.DateTimeFormatDateFirst)}] [Null] [UserService]:{Environment.NewLine}This is a ...");
        }

        [Fact]
        public void ConvertLogToStr_MessageEqualToMaxLength_ShouldReturnMessageWithoutTruncate()
        {
            // Arrange
            int id = 1;
            string level = "Information";
            int? eventId = 10;
            DateTime dateTime = new DateTime(2020, 6, 1);
            long? telegramId = 0;
            string context = "MyApp.Services.UserService";
            string message = new string('A', 20);

            Log log = CreateLog(id,
                                level,
                                eventId,
                                dateTime,
                                telegramId,
                                context,
                                message);

            int maxLength = 20;

            // Act
            string result = LogFormatter.ConvertLogToStr(log, maxLength);

            // Assert
            result.Should().Be($"[{id}] [{level}] [{eventId}] [{dateTime.ToString(CommonConsts.Common.DateTimeFormatDateFirst)}] [Null] [UserService]:{Environment.NewLine}{message}");
        }

        [Fact]
        public void ConvertLogToStr_MaxLengthIsZero_ShouldUsesDefaultMaxCharactersCount()
        {
            // Arrange
            int id = 1;
            string level = "Information";
            int? eventId = 10;
            DateTime dateTime = new DateTime(2020, 6, 1);
            long? telegramId = 12345;
            string context = "MyApp.Services.UserService"; 
            // Сообщение длиной, меньше чем CoreConsts.LogLengthLimit.ConvertLimit
            string message = new string('B', 100);

            Log log = CreateLog(id,
                                level,
                                eventId,
                                dateTime,
                                telegramId,
                                context,
                                message);

            
            int maxLength = 0;

            // Act
            string result = LogFormatter.ConvertLogToStr(log, maxLength);

            // Assert
            result.Should().Be($"[{id}] [{level}] [{eventId}] [{dateTime.ToString(CommonConsts.Common.DateTimeFormatDateFirst)}] [{telegramId}] [UserService]:{Environment.NewLine}{message}");
        }

        [Fact]
        public void ConvertLogToStr_MessageMoreThanDefaultMaxCharactersCount_ShouldReturnTruncatesMessage()
        {
            // Arrange
            int id = 1;
            string level = "Information";
            int? eventId = 10;
            DateTime dateTime = new DateTime(2020, 6, 1);
            long? telegramId = 1;
            string context = "MyApp.Services.UserService";
            // Сообщение длиной, больше чем CoreConsts.Log.ConvertLimit (+ 5)
            string message = new string('C', CoreConsts.Log.ShowLimit + 5);

            Log log = CreateLog(id,
                                level,
                                eventId,
                                dateTime,
                                telegramId,
                                context,
                                message);

            int maxLength = 0;

            // Act
            string result = LogFormatter.ConvertLogToStr(log, maxLength);

            // Assert
            result.Should().Be($"[{id}] [{level}] [{eventId}] [{dateTime.ToString(CommonConsts.Common.DateTimeFormatDateFirst)}] [{telegramId}] [UserService]:{Environment.NewLine}{new string('C', CoreConsts.Log.ShowLimit)}...");
        }

        [Fact]
        public void ConvertLogToStr_ContextWithoutDots_ShouldReturnMessageWithFullContext()
        {
            // Arrange
            int id = 1;
            string level = "Information";
            int? eventId = 10;
            DateTime dateTime = new DateTime(2020, 6, 1);
            long? telegramId = 12345;
            string context = "NoDotsContext";
            // Сообщение длиной 100 символов, меньше чем CoreConsts.Log.ConvertLimit
            string message = "Msg";

            Log log = CreateLog(id,
                                level,
                                eventId,
                                dateTime,
                                telegramId,
                                context,
                                message);


            int maxLength = 50;

            // Act
            string result = LogFormatter.ConvertLogToStr(log, maxLength);

            // Assert
            result.Should().Be($"[{id}] [{level}] [{eventId}] [{dateTime.ToString(CommonConsts.Common.DateTimeFormatDateFirst)}] [{telegramId}] [NoDotsContext]:{Environment.NewLine}{message}");
        }

        [Fact]
        public void ConvertLogToStr_WithEmptyContext_ShouldReturnEmptyContext()
        {
            // Arrange
            int id = 1;
            string level = "Information";
            int? eventId = 10;
            DateTime dateTime = new DateTime(2020, 6, 1);
            long? telegramId = 0;
            string context = string.Empty;
            string message = "Short message";

            Log log = CreateLog(id,
                                level,
                                eventId,
                                dateTime,
                                telegramId,
                                context,
                                message);


            int maxLength = 50;

            // Act
            string result = LogFormatter.ConvertLogToStr(log, maxLength);

            // Assert
            result.Should().Be($"[{id}] [{level}] [{eventId}] [{dateTime.ToString(CommonConsts.Common.DateTimeFormatDateFirst)}] [Null] []:{Environment.NewLine}{message}");
        }

        private Log CreateLog(int id,
                              string level,
                              int? eventId,
                              DateTime dateTime,
                              long? telegramId,
                              string context,
                              string message)
        {
            Log log = new Log
            {
                Id = id,
                LogLevel = level,
                EventID = eventId,
                EventName = null,
                DateTime = dateTime,
                Message = message,
                SourceContext = context,
                TelegramUserId = telegramId
            };

            return log;
        }
    }
}