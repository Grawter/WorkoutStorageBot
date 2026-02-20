using Telegram.Bot.Exceptions;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageModels.Entities.Core.Logging;

namespace WorkoutStorageBot.Core.Helpers
{
    internal static class LogFormatter
    {
        internal const int MaxCharactersCount = 1500;

        internal static string EmptyFormatter<TState>(TState state, Exception? exception)
            => string.Empty;

        internal static string CriticalExBotFormatter<TState>(TState state, Exception? exception)
        {
            string errorMessage = string.Empty;

            if (exception == null)
                return "Получено пустое исключение";

            errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:{Environment.NewLine}[{apiRequestException.ErrorCode}]{Environment.NewLine}{apiRequestException.Message}",
                _ => exception.ToString()
            };

            return errorMessage;
        }

        internal static string ConvertLogToStr(Log log, int maxLength = 0)
        {
            string[] partsOfContext = log.SourceContext.Split(['.'], StringSplitOptions.RemoveEmptyEntries);
            string lastContext = string.Empty; 

            if (partsOfContext.Length > 0)
                lastContext = partsOfContext[partsOfContext.Length - 1];

            string from = "Null";

            if (log.TelegramUserId > 0)
                from = log.TelegramUserId.ToString()!;

            if (maxLength == 0)
                maxLength = MaxCharactersCount;

            string logMessage = log.Message.Length > maxLength 
                ? $"{log.Message.Substring(0, maxLength)}..." 
                : log.Message;

            string logStr = 
                @$"[{log.Id}] [{log.LogLevel}] [{log.EventID}] [{log.DateTime.ToString(CommonConsts.Common.DateTimeFormatDateFirst)}] [{from}] [{lastContext}]:
{logMessage}";
            return logStr;
        }
    }
}