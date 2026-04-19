using Microsoft.Extensions.Logging;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.Core.Consts;
using WorkoutStorageBot.Core.Extensions;
using WorkoutStorageBot.Core.Logging.OutputWriter;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.Log;
using WorkoutStorageModels.Entities.Core.Logging;

namespace WorkoutStorageBot.Core.Logging
{
    internal class CustomLogger : ILogger
    {
        private readonly string categoryName;
        private readonly EntityContext db;
        private readonly IOutputWriter outputWriter;
        private readonly LogSettings logSettings;

        public CustomLogger(string categoryName, EntityContext db, IOutputWriter outputWriter, LogSettings logSettings) 
        { 
            this.categoryName = categoryName;
            this.db = db;
            this.outputWriter = outputWriter;
            this.logSettings = logSettings;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
		{
			return Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance.BeginScope(state);
		}

        public bool IsEnabled(LogLevel logLevel)
        {
            RuleLog ruleLog = GetRuleLog();

            bool logLevelIsEnableByConsole = CurrentLogLevelIsEnable(logLevel, ruleLog.ConsoleLogLevels);

            bool logLevelIsEnableByDB = CurrentLogLevelIsEnable(logLevel, ruleLog.DBLogLevels);

            return logLevelIsEnableByConsole || logLevelIsEnableByDB;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            string logLevelStr = logLevel.ToString();

            RuleLog ruleLog = GetRuleLog();

            DateTime dateTime = DateTime.Now;

            LogData logData = GetLogData(state, exception, formatter);

            if (string.IsNullOrWhiteSpace(logData.Message) || logData.Message == "[null]")
                return;

            if (logData.Message.Length > CoreConsts.Log.SaveLimit)
                logData.Message = $"{logData.Message.Substring(0, CoreConsts.Log.SaveLimit - 3)}...";

            if (CurrentLogLevelIsEnable(logLevelStr, ruleLog.ConsoleLogLevels))
                TryWriteLogToConsoleWrapper(logLevelStr, eventId, dateTime, logData.Message, logData.TelegramUserId);

            if (CurrentLogLevelIsEnable(logLevelStr, ruleLog.DBLogLevels))
                TryWriteLogToDB(logLevelStr, eventId, dateTime, logData.Message, logData.TelegramUserId);
        }

        private RuleLog GetRuleLog()
        {
            CustomRuleLog? customRuleLog = GetCustomRuleLog();

            if (customRuleLog != null)
                return customRuleLog;
            else
                return logSettings.MainRuleLog;
        }

        private CustomRuleLog? GetCustomRuleLog()
        {
            if (!logSettings.CustomRulesLog.HasItemsInCollection())
                return default;

            CustomRuleLog? customRuleLog =
                logSettings.CustomRulesLog.FirstOrDefault(x => string.Equals(x.FullClassName, categoryName, StringComparison.InvariantCulture));

            return customRuleLog;
        }

        private LogData GetLogData<TState>(TState state, Exception? ex, Func<TState, Exception?, string> formatter)
        {
            if (formatter != null)
                return GetLogDataWithFormatter(state, ex, formatter);
            else
                return GetLogDataWithoutFormatter(state, ex);
        }

        private LogData GetLogDataWithFormatter<TState>(TState state, Exception? ex, Func<TState, Exception?, string> formatter)
        {
            LogData logData = new LogData();

            if (state is LogData logDataExternal)
            {
                logData.Message = formatter(state, ex);
                logData.TelegramUserId = logDataExternal.TelegramUserId;
            }
            // если это стандартный встроенный форматер, то он никак не обработает ex, поэтому обрабатываем ex самостоятельно
            else if (ex != null && formatter.Method.Name == "MessageFormatter" && formatter.Method.Module.Name == "Microsoft.Extensions.Logging.Abstractions.dll")
            {
                string stateValue = formatter(state, ex);

                if (stateValue == "[null]")
                    logData.Message = $"Ex: {ex.ToString()}";
                else
                    logData.Message = $"Text: {stateValue} Ex: {ex.ToString()}";
            }
            else
                logData.Message = formatter(state, ex);
            
            return logData;
        }

        private LogData GetLogDataWithoutFormatter<TState>(TState state, Exception? ex)
        {
            LogData logData = new LogData();

            if (state == null)
                return logData;
            else if (state is string logMessage)
            {
                if (ex != null)
                {
                    if (!string.IsNullOrWhiteSpace(logMessage))
                        logData.Message = $"Text: {logMessage} Ex: {ex.ToString()}";
                    else
                        logData.Message = $"Ex: {ex.ToString()}";
                }
                else
                    logData.Message = logMessage;
            }
            else if (state is LogData logDataExternal)
            {
                if (ex != null)
                {
                    if (!string.IsNullOrWhiteSpace(logDataExternal.Message))
                        logData.Message = $"Text: {logDataExternal.Message} Ex: {ex.ToString()}";
                    else
                        logData.Message = $"Ex: {ex.ToString()}";

                    logData.TelegramUserId = logDataExternal.TelegramUserId;
                }
                else
                    logData = logDataExternal;
            }
            else
                logData.Message = @$"Не удалось обработать {nameof(state)} с типом {typeof(TState).FullName}
CallStack: {Environment.StackTrace}";

            return logData;
        }

        private void TryWriteLogToConsoleWrapper(string currentLogLevelStr,
                                                 EventId eventId,
                                                 DateTime dateTime,
                                                 string message,
                                                 long? telegramUserId)
        {
            ConsoleColor originalConsoleColor = Console.ForegroundColor;

            SetConsoleColorText(currentLogLevelStr);

            TryWriteLogToConsole(currentLogLevelStr, eventId, dateTime, message, telegramUserId);

            Console.ForegroundColor = originalConsoleColor;
        }

        private void SetConsoleColorText(string logLevelStr)
        {
            Console.ForegroundColor = logLevelStr switch
            {
                "Information" => ConsoleColor.Cyan,
                "Warning" => ConsoleColor.Yellow,
                "Error" => ConsoleColor.Red,
                "Critical" => ConsoleColor.DarkRed,
                _ => ConsoleColor.Gray
            };
        }

        private void TryWriteLogToConsole(string currentLogLevelStr,
                                          EventId eventId,
                                          DateTime dateTime,
                                          string message,
                                          long? telegramUserId)
        {
            string logMessage;

            if (HasEventId(eventId, out string eventIdStr))
                logMessage = @$"[{currentLogLevelStr}] [{eventIdStr}] [{dateTime.ToString(CommonConsts.Common.DateTimeFormatDateFirst)}] {categoryName}:
{message}";
            else
                logMessage = @$"[{currentLogLevelStr}] [{dateTime.ToString(CommonConsts.Common.DateTimeFormatDateFirst)}] {categoryName}:
{message}";

            if (telegramUserId.HasValue)
                logMessage += $" |by {telegramUserId}";

            outputWriter.Write(logMessage);
        }

        private void TryWriteLogToDB(string currentLogLevelStr,
                                     EventId eventId,
                                     DateTime dateTime,
                                     string message,
                                     long? telegramUserId)
        {
            Log log = new Log()
            {
                LogLevel = currentLogLevelStr,
                EventID = eventId.Id > 0 ? eventId.Id : null,
                EventName = eventId.Name,
                DateTime = dateTime,
                Message = message,
                SourceContext = categoryName,
                TelegramUserId = telegramUserId,
            };

            db.Logs.Add(log);

            db.SaveChanges();
        }

        private bool HasEventId(EventId eventId, out string eventIdStr)
        {
            eventIdStr = string.Empty;

            if (eventId.Id < 1)
                return false;

            if (!string.IsNullOrWhiteSpace(eventId.Name))
                eventIdStr = $"{eventId.Id}|{eventId.Name}";
            else
                eventIdStr = eventId.Id.ToString();

            return true;
        }

        private bool CurrentLogLevelIsEnable(LogLevel logLevel, IEnumerable<string> permittedLogLevels)
            => CurrentLogLevelIsEnable(logLevel.ToString(), permittedLogLevels);

        private bool CurrentLogLevelIsEnable(string logLevelStr, IEnumerable<string> permittedLogLevels)
            => permittedLogLevels.Contains("All") || permittedLogLevels.Contains(logLevelStr);
    }
}