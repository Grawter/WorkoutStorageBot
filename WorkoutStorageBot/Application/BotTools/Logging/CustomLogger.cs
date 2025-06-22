#region using

using Microsoft.Extensions.Logging;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.Extenions;
using WorkoutStorageBot.Helpers.Common;
using WorkoutStorageBot.Model.AppContext;

#endregion

namespace WorkoutStorageBot.Application.BotTools.Logging
{
    internal class CustomLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly EntityContext _db;
        private readonly ConfigurationData _configurationData;
        private const string _dateTimeFormat = CommonConsts.Common.DateTimeFormatDateFirst;

        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public CustomLogger(string categoryName)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(categoryName);

            _categoryName = categoryName;
        }

        public CustomLogger(string categoryName, EntityContext db) : this(categoryName)
        {
            ArgumentNullException.ThrowIfNull(db);

            _db = db;
        }

        public CustomLogger(string categoryName, EntityContext db, ConfigurationData configurationData) : this(categoryName, db)
        {
            ArgumentNullException.ThrowIfNull(configurationData);

            _configurationData = configurationData;
        }

        public IDisposable BeginScope<TState>(TState state) => default!;

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

            Dictionary<string, object> logData = GetLogData(state, exception, formatter);

            DateTime dateTime = DateTime.Now;

            ConsoleColor originalConsoleColor = Console.ForegroundColor;

            SetConsoleColorText(logLevel);

            TryWriteLogToConsole(ruleLog.ConsoleLogLevels, logLevelStr, eventId, dateTime, logData);
            TryWriteLogToDB(ruleLog.DBLogLevels, logLevelStr, eventId, dateTime, logData);

            Console.ForegroundColor = originalConsoleColor;
        }

        private RuleLog GetRuleLog()
        {
            CustomRuleLog? customRuleLog = GetCustomRuleLog();

            if (customRuleLog != null)
                return customRuleLog;
            else
                return _configurationData.LogInfo.MainRuleLog;
        }

        private CustomRuleLog? GetCustomRuleLog()
        {
            if (!_configurationData.LogInfo.CustomRulesLog.HasItemsInCollection())
                return default;

            CustomRuleLog? customRuleLog =
                _configurationData.LogInfo.CustomRulesLog.FirstOrDefault(x => string.Equals(x.FullClassName, _categoryName, StringComparison.InvariantCulture));

            return customRuleLog;
        }

        private Dictionary<string, object> GetLogData<TState>(TState state, Exception? ex, Func<TState, Exception?, string> formatter)
        {
            if (ex != null)
                return GetLogDataByEx(state, ex, formatter);
            else
                return GetLogDataByTState(state);
        }

        private Dictionary<string, object> GetLogDataByEx<TState>(TState state, Exception ex, Func<TState, Exception, string> formatter)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            string formatterMessage = string.Empty;

            if (formatter != null)
                formatterMessage = formatter(state, ex);

            if (!string.IsNullOrWhiteSpace(formatterMessage))
                result.Add("Message", formatterMessage);
            else
                result.Add("Message", ex.ToString());

            Dictionary<string, object> TStateLogData = GetLogDataByTState(state);

            if (TStateLogData.Count > 0)
                UnionLogData(result, TStateLogData);

            return result;
        }

        private Dictionary<string, object> GetLogDataByTState<TState>(TState state, Dictionary<string, object>? startedCollection = null)
        {
            Dictionary<string, object> result = startedCollection 
                ?? new Dictionary<string, object>();

            if (state is IReadOnlyList<KeyValuePair<string, object>> logReadOnlyListData)
            {
                foreach (KeyValuePair<string, object> keyValuePair in logReadOnlyListData)
                {
                    if (keyValuePair.Key == "{OriginalFormat}")
                        result.Add("Message", keyValuePair.Value);
                    else
                        result.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
            else if (state is Dictionary<string, object> logDictionaryResult)
                return logDictionaryResult;
            else if (state is string logMessage)
                result.Add("Message", logMessage);
            else
                result.Add("Message", @$"Не удалось обработать {nameof(state)} с типом {typeof(TState).FullName}
CallStack: {Environment.StackTrace}");

            return result;
        }

        private Dictionary<string, object> UnionLogData(Dictionary<string, object> exLogData, Dictionary<string, object> stateLogData)
        {
            foreach (KeyValuePair<string, object> keyValuePair in stateLogData)
            {
                exLogData.TryAdd(keyValuePair.Key, keyValuePair.Value);
            }

            return exLogData;
        }

        private void SetConsoleColorText(LogLevel logLevel)
        {
            Console.ForegroundColor = logLevel switch
            {
                LogLevel.Information => ConsoleColor.Cyan,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Critical => ConsoleColor.DarkRed,
                _ => ConsoleColor.Gray
            };
        }

        private bool TryWriteLogToConsole(string[] permittedLogLevels,
                                     string currentLogLevelStr,
                                     EventId eventId,
                                     DateTime dateTime,
                                     Dictionary<string, object> logData)
        {
            ArgumentNullException.ThrowIfNull(permittedLogLevels);

            if (!CurrentLogLevelIsEnable(currentLogLevelStr, permittedLogLevels))
                return false;

            string logMessage = string.Empty;

            if (HasEventId(eventId, out string eventIdStr))
                logMessage = @$"[{currentLogLevelStr}] [{eventIdStr}] [{dateTime.ToString(_dateTimeFormat)}] {_categoryName}:
{logData["Message"]}";
            else
                logMessage = @$"[{currentLogLevelStr}] [{dateTime.ToString(_dateTimeFormat)}] {_categoryName}:
{logData["Message"]}";

            if (CommonHelper.TryConvertToLong(logData.GetValueOrDefault("TelegaramUserId"), out long? telegaramUserId))
                logMessage += $" |by|{telegaramUserId}|";

            Console.WriteLine(logMessage);

            return true;
        }

        private bool HasEventId(EventId eventId, out string eventIdStr)
        {
            eventIdStr = string.Empty;

            if (eventId.Id < 1)
                return false;

            if (eventId.Name != null)
                eventIdStr = $"{eventId.Id}|{eventId.Name}";
            else
                eventIdStr = eventId.Id.ToString();

            return true;
        }

        private async Task<bool> TryWriteLogToDB(string[] permittedLogLevels,
                                     string currentLogLevelStr,
                                     EventId eventId,
                                     DateTime dateTime,
                                     Dictionary<string, object> logData)
        {
            ArgumentNullException.ThrowIfNull(permittedLogLevels);

            if (!CurrentLogLevelIsEnable(currentLogLevelStr, permittedLogLevels))
                return false;

            string message = logData["Message"].ToString();
            CommonHelper.TryConvertToLong(logData.GetValueOrDefault("TelegaramUserId"), out long? telegaramUserId);

            Model.Logging.Log log = new Model.Logging.Log()
            {
                LogLevel = currentLogLevelStr,
                EventID = eventId.Id,
                EventName = eventId.Name,
                DateTime = dateTime,
                Message = message,
                SourceContext = _categoryName,
                TelegaramUserId = telegaramUserId,
            };

            await _semaphore.WaitAsync();

            try
            {
                _db.Logs.Add(log);

                _db.SaveChanges();
            }
            finally
            {
                _semaphore.Release();
            }

            return true;
        }

        private bool CurrentLogLevelIsEnable(LogLevel logLevel, string[] permittedLogLevels)
            => CurrentLogLevelIsEnable(logLevel.ToString(), permittedLogLevels);

        private bool CurrentLogLevelIsEnable(string logLevelStr, string[] permittedLogLevels)
            => permittedLogLevels.Contains("All") || permittedLogLevels.Contains(logLevelStr);
    }
}