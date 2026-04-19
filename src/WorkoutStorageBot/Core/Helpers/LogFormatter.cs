using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.Core.Consts;
using WorkoutStorageBot.Model.DTO.Log;
using WorkoutStorageModels.Entities.Core.Logging;

namespace WorkoutStorageBot.Core.Helpers
{
    internal static class LogFormatter
    {
        internal static string SimpleFormatter<TState>(TState state, Exception? exception)
        {
            if (state == null)
            {
                if (exception == null)
                    return string.Empty;
                else
                    return exception.ToString();
            }
            else if (state is string logMessage)
            {
                if (exception != null)
                {
                    if (!string.IsNullOrWhiteSpace(logMessage))
                        return $"Text: {logMessage} Ex: {exception.ToString()}";
                    else
                        return $"Ex: {exception.ToString()}";
                }
                else
                    return logMessage;
            }
            else if (state is LogData logData)
            {
                if (exception != null)
                {
                    if (!string.IsNullOrWhiteSpace(logData.Message))
                        return $"Text: {logData.Message} Ex: {exception.ToString()}";
                    else
                        return $"Ex: {exception.ToString()}";
                }
                else
                    return logData.Message ?? string.Empty;
            }
            else
                return @$"Тип {nameof(state)} - {typeof(TState).FullName} не поддеживается {nameof(SimpleFormatter)}
CallStack: {Environment.StackTrace}";
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
                maxLength = CoreConsts.Log.ShowLimit;

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