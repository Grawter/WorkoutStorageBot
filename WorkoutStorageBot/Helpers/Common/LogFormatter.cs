using Telegram.Bot.Exceptions;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.Model.Entities.Logging;

namespace WorkoutStorageBot.Helpers.Common
{
    internal static class LogFormatter
    {
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

        internal static string ConvertLogToStr(Log log)
        {
            string[] partsOfContext = log.SourceContext.Split(['.'], StringSplitOptions.RemoveEmptyEntries);
            string lastContext = string.Empty; 

            if (partsOfContext.Length > 0)
                lastContext = partsOfContext[partsOfContext.Length - 1];

            string from = "Null";

            if (log?.TelegaramUserId > 0)
                from = log.TelegaramUserId.ToString()!;

            string logStr = 
                @$"[{log.Id}] [{log.LogLevel}] [{log.EventID}] [{log.DateTime.ToString(CommonConsts.Common.DateTimeFormatDateFirst)}] [{from}] [{lastContext}]:
[{log.Message}]";
            return logStr;
        }
    }
}