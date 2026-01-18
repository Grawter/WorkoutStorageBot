using Microsoft.Extensions.Configuration;
using System.Text.Json;
using WorkoutStorageBot.Core.Extensions;
using WorkoutStorageBot.Core.Helpers;

namespace WorkoutStorageBot.Application.Configuration
{
    internal class ConfigurationManager
    {
        internal static ConfigurationData GetConfiguration(string pathFileConfig)
        {
            if (!File.Exists(pathFileConfig))
                throw new Exception($"Указанный config-файл {pathFileConfig} не найден");

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();

            IConfigurationRoot configuration = configurationBuilder.AddJsonFile(pathFileConfig, optional: false, reloadOnChange: true)
                                                                   .Build();

            ConfigurationData configurationData = new ConfigurationData();

            FillAboutBotText(configurationData, configuration);

            FillDBSetting(configurationData.DB, configuration);

            FillBotSettings(configurationData.Bot, configuration);

            FillNotificationsSettings(configurationData.Notifications, configuration);

            FillLogSettings(configurationData.LogInfo, configuration);

            CreateVolumeDirectory(configurationData.DB.Database);

            return configurationData;
        }

        private static void FillAboutBotText(ConfigurationData configurationData, IConfigurationRoot configuration)
            => configurationData.AboutBotText = configuration.GetSection("AboutBot")?.Value;

        private static void FillDBSetting(DbSettings settings, IConfigurationRoot configuration)
        {
            IConfigurationSection DBSection = configuration.GetRequiredSection("DB");

            settings.EnsureCreated = bool.Parse(DBSection["EnsureCreated"] ?? "false");
            settings.Server = DBSection["Server"];
            settings.Database = GetRequiredValue(DBSection["Database"], "Не удалось получить название БД");
            settings.UserName = DBSection["UserName"];
            settings.Password = DBSection["Password"];
        }

        private static void FillBotSettings(BotSettings settings, IConfigurationRoot configuration)
        {
            IConfigurationSection botSection = configuration.GetRequiredSection("Bot");

            settings.Token = GetRequiredValue(botSection["Token"], "Не удалось получить токен телеграм бота");
            settings.WhiteListIsEnable = bool.Parse(GetRequiredValue(botSection["WhiteListIsEnable"], "Не удалось получить флаг белого списка"));
            settings.IsNeedCacheContext = bool.Parse(GetRequiredValue(botSection["IsNeedCacheContext"], "Не удалось получить флаг кэширования глобального контекста"));
            settings.IsNeedLimits = bool.Parse(GetValueOrDefault(botSection["IsNeedLimits"], "True"));

            IEnumerable<string> ownersChatIDs = GetChildrenCollection(botSection, "OwnersChatIDs");

            if (ownersChatIDs.HasItemsInCollection())
                settings.OwnersChatIDs = ownersChatIDs;
            else
                throw new InvalidOperationException("Не удалось получить идентификаторы владельцев");
        }

        private static void FillNotificationsSettings(NotificationsSettings settings, IConfigurationRoot configuration)
        {
            IConfigurationSection notifications = configuration.GetRequiredSection("Notifications");

            settings.NotifyOwnersAboutCriticalErrors = bool.Parse(
                GetRequiredValue(notifications["NotifyOwnersAboutCriticalErrors"], "Не удалось получить флаг уведомления о критических ошибках"));
            settings.NotifyOwnersAboutRuntimeErrors = bool.Parse(
                GetRequiredValue(notifications["NotifyOwnersAboutRuntimeErrors"], "Не удалось получить флаг уведомления о ошибках рантайма"));
        }

        private static void FillLogSettings(LogSettings settings, IConfigurationRoot configuration)
        {
            IConfigurationSection mainRuleLog = configuration.GetRequiredSection("LogInfo:MainRuleLog");

            settings.MainRuleLog.DBLogLevels = GetChildrenCollection(mainRuleLog, "DBLogLevels", false);
            settings.MainRuleLog.ConsoleLogLevels = GetChildrenCollection(mainRuleLog, "ConsoleLogLevels", false);

            IConfigurationSection customRulesLog = configuration.GetSection("LogInfo:CustomRulesLog");

            if (customRulesLog != null)
            {
                List<CustomRuleLog> CustomRulesLog = new List<CustomRuleLog>();

                foreach (IConfigurationSection customRuleSection in customRulesLog.GetChildren())
                {
                    CustomRuleLog customRuleLog = new CustomRuleLog()
                    {
                        FullClassName = GetRequiredValue(customRuleSection["FullClassName"], "Не указано название класса для кастомного правила логгирования"),
                        DBLogLevels = GetChildrenCollection(customRuleSection, "DBLogLevels", false),
                        ConsoleLogLevels = GetChildrenCollection(customRuleSection, "ConsoleLogLevels", false),
                    };

                    CustomRulesLog.Add(customRuleLog);
                }

                settings.CustomRulesLog = CustomRulesLog.ToArray();
            }
        }

        private static void CreateVolumeDirectory(string path)
        {
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        internal static void SetCensorToDBSettings(ConfigurationData configurationData)
        {
            configurationData.DB.UserName = "******";
            configurationData.DB.Password = "******";
        }

        internal static string GetSerializedSaveDeepCopy(ConfigurationData configurationData)
        {
            ConfigurationData saveDeepCopy = GetSaveDeepCopy(configurationData);

            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            return JsonSerializer.Serialize(saveDeepCopy, jsonSerializerOptions);
        }

        private static ConfigurationData GetSaveDeepCopy(ConfigurationData configurationData)
        {
            ConfigurationData deepCopy = GetDeepCopy(configurationData);
            SetCensorToConfigurationData(deepCopy);

            return deepCopy;
        }

        private static ConfigurationData GetDeepCopy(ConfigurationData configurationData)
        {
            return JsonSerializer.Deserialize<ConfigurationData>(
                JsonSerializer.Serialize(configurationData))
                    ?? throw new InvalidOperationException("Десериализация вернула null");
        }

        private static void SetCensorToConfigurationData(ConfigurationData configurationData)
        {
            SetCensorToDBSettings(configurationData);

            configurationData.Bot.Token = CommonHelper.GetCensorValue(configurationData.Bot.Token, 3);
            configurationData.Bot.OwnersChatIDs = configurationData.Bot.OwnersChatIDs.Where(x => !string.IsNullOrWhiteSpace(x))
                                                                                     .Select(x => CommonHelper.GetCensorValue(x, 3))
                                                                                     .ToArray();
        }

        private static IEnumerable<string> GetChildrenCollection(IConfigurationSection configurationSection, string sectionPath, bool isRequiredSection = true)
        {
            IConfigurationSection targerConfigurationSection = isRequiredSection 
                                                               ? configurationSection.GetRequiredSection(sectionPath)
                                                               : configurationSection.GetSection(sectionPath);

           return targerConfigurationSection.GetChildren()
                                            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                                            .Select(c => c.Value!)
                                            .ToList();
        }

        private static string GetRequiredValue(string? value, string emptyOrNullErrorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(emptyOrNullErrorMessage);

            return value;
        }

        private static string GetValueOrDefault(string? value, string defaultValue)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            return value;
        }
    }
}