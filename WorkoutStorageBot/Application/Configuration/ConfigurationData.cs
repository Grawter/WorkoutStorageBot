

namespace WorkoutStorageBot.Application.Configuration
{
    public class ConfigurationData
    {
        public DbSettings DB { get; set; } = new DbSettings();
        public BotSettings Bot { get; set; } = new BotSettings();
        public NotificationsSettings Notifications { get; set; } = new NotificationsSettings();
        public LogSettings LogInfo { get; set; } = new LogSettings();
    }

    public class DbSettings
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string ConnectionString { 
            get
            {
                if (string.IsNullOrWhiteSpace(Database))
                    throw new InvalidDataException("Не удалось сформировать строку подключения");

                return Database;
            } 
        }
    }

    public class BotSettings
    {
        public string Token { get; set; }
        public bool WhiteListIsEnable { get; set; }
        public string[] OwnersChatIDs { get; set; } = [];
        public bool IsNeedCacheContext { get; set; }
        public bool IsNeedLimits { get; set; }
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
        public string[] DBLogLevels { get; set; } = [];
        public string[] ConsoleLogLevels { get; set; } = [];
    }

    public class CustomRuleLog : RuleLog
    {
        public string FullClassName { get; set; }
    }
}