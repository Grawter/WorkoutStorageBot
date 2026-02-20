using FluentAssertions;
using Telegram.Bot.Exceptions;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.Core.Helpers;
using WorkoutStorageModels.Entities.Core.Logging;

namespace WorkoutStorageBot.UnitTests.Core.Helpers
{
    public class LogFormatterTests
    {
        [Fact]
        public void EmptyFormatter_WithNullException_ShouldReturnEmptyStr()
        {
            Exception? ex = null;

            string emptyStr = LogFormatter.EmptyFormatter<string>("123", ex);

            emptyStr.Should().BeEmpty();

        }

        [Fact]
        public void EmptyFormatter_WithException_ShouldReturnEmptyStr()
        {
            string emptyStr = LogFormatter.EmptyFormatter<string>("123", new Exception("TestEx"));

            emptyStr.Should().BeEmpty();
        }


        [Fact]
        public void CriticalExBotFormatter_WithNullException_ShouldReturnTextAboutEmptyEx()
        {
            Exception? ex = null;

            string emptyStr = LogFormatter.CriticalExBotFormatter<string>("123", ex);

            emptyStr.Should().Be("Получено пустое исключение");
        }

        [Fact]
        public void CriticalExBotFormatter_WithException_ShouldReturnTextEx()
        {
            Exception ex = new Exception("TestEx");

            string emptyStr = LogFormatter.CriticalExBotFormatter<string>("123", new Exception("TestEx"));

            emptyStr.Should().Be(ex.ToString());
        }

        [Fact]
        public void CriticalExBotFormatter_WithApiRequestException_ShouldReturnFormattedTextEx()
        {
            ApiRequestException apiEx = new ApiRequestException("TestEx", 11);

            string emptyStr = LogFormatter.CriticalExBotFormatter<string>("123", apiEx);

            emptyStr.Should().Be($"Telegram API Error:{Environment.NewLine}[{apiEx.ErrorCode}]{Environment.NewLine}{apiEx.Message}");
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
            // Сообщение длиной 100 символов, меньше чем MaxCharactersCount (1500)
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
            // Сообщение длиной 1600 символов, больше чем MaxCharactersCount (+ 5)
            string message = new string('C', LogFormatter.MaxCharactersCount + 5);

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
            result.Should().Be($"[{id}] [{level}] [{eventId}] [{dateTime.ToString(CommonConsts.Common.DateTimeFormatDateFirst)}] [{telegramId}] [UserService]:{Environment.NewLine}{new string('C', LogFormatter.MaxCharactersCount)}...");
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
            // Сообщение длиной 100 символов, меньше чем MaxCharactersCount (1500)
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