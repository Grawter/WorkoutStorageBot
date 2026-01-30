namespace WorkoutStorageBot.Application.Configuration
{
    public class ConfigurationData
    {
        public DbSettings DB { get; set; } = new DbSettings() { Database = string.Empty };
        public BotSettings Bot { get; set; } = new BotSettings() { Token = string.Empty };
        public NotificationsSettings Notifications { get; set; } = new NotificationsSettings();
        public LogSettings LogInfo { get; set; } = new LogSettings();

        public string? AboutBotText { get; set; }
    }

    public class DbSettings
    {
        public bool EnsureCreated { get; set; }
        public string? Server { get; set; }
        public required string Database { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }

        public string ConnectionString { 
            get
            {
                if (string.IsNullOrWhiteSpace(Database))
                    throw new InvalidDataException("Не удалось сформировать строку подключения");

                return $"Data Source= {Database}";
            } 
        }
    }

    public class BotSettings
    {
        public required string Token { get; set; }
        public bool WhiteListIsEnable { get; set; }
        public IEnumerable<string> OwnersChatIDs { get; set; } = [];
        public bool IsNeedCacheContext { get; set; }
        public bool IsNeedLimits { get; set; }
        public bool IsSupportOnlyKnownTypes { get; set; }
    }

    public class NotificationsSettings
    {
        public bool NotifyOwnersAboutCriticalErrors { get; set; }
        public bool NotifyOwnersAboutRuntimeErrors { get; set; }
    }

    public class LogSettings
    {
        public RuleLog MainRuleLog { get; set; } = new RuleLog();

        public CustomRuleLog[] CustomRulesLog { get; set; } = [];

    }

    public class RuleLog
    {
        public IEnumerable<string> DBLogLevels { get; set; } = [];
        public IEnumerable<string> ConsoleLogLevels { get; set; } = [];
    }

    public class CustomRuleLog : RuleLog
    {
        public required string FullClassName { get; set; }
    }
}