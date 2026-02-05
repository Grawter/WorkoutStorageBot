using Microsoft.Extensions.Configuration;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using WorkoutStorageBot.Core.Extensions;
using WorkoutStorageBot.Core.Helpers;

namespace WorkoutStorageBot.Application.Configuration
{
    internal class ConfigurationManager
    {
        private static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true, // форматированная сериализация с табуляцией
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All), // Чтобы кириллические (или возможные другие) символы не экранировались
        };

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

            settings.EnsureCreated = bool.Parse(GetValueOrDefault(DBSection["EnsureCreated"], "False"));
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
            settings.IsSupportOnlyKnownTypes = bool.Parse(GetValueOrDefault(botSection["IsSupportOnlyKnownTypes"], "True"));

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

        internal static void SetCensorToConfigurationData(ConfigurationData configurationData, bool isNeedCensorOwnersChatIDs)
        {
            SetCensorToDBSettings(configurationData.DB);
            SetCensorToBotSettings(configurationData.Bot, isNeedCensorOwnersChatIDs);
        }

        internal static string GetSerializedSafeConfigurationData(ConfigurationData configurationData)
        {
            ConfigurationData safeDeepCopy = GetSafeDeepCopy(configurationData);

            return JsonSerializer.Serialize(safeDeepCopy, jsonSerializerOptions);
        }

        private static ConfigurationData GetSafeDeepCopy(ConfigurationData configurationData)
        {
            ConfigurationData deepCopy = GetDeepCopy(configurationData);
            SetCensorToConfigurationData(deepCopy, true);

            deepCopy.AboutBotText = RemoveTags(deepCopy.AboutBotText);

            return deepCopy;
        }

        private static ConfigurationData GetDeepCopy(ConfigurationData configurationData)
        {
            return JsonSerializer.Deserialize<ConfigurationData>(JsonSerializer.Serialize(configurationData))
                    ?? throw new InvalidOperationException("Десериализация configurationData вернула null");
        }

        private static void SetCensorToDBSettings(DbSettings dbSettings)
        {
            dbSettings.UserName = "******";
            dbSettings.Password = "******";
        }

        private static void SetCensorToBotSettings(BotSettings botSettings, bool isNeedCensorOwnersChatIDs)
        {
            botSettings.Token = CommonHelper.GetCensorValue(botSettings.Token, 3);

            if (isNeedCensorOwnersChatIDs)
                botSettings.OwnersChatIDs = botSettings.OwnersChatIDs.Where(x => !string.IsNullOrWhiteSpace(x))
                                                                                     .Select(x => CommonHelper.GetCensorValue(x, 3))
                                                                                     .ToArray();
        }

        private static string RemoveTags(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Удаляем любые теги вида <...>, включая переносы строк внутри тега
            return Regex.Replace(input, @"<[^>]*>", string.Empty, RegexOptions.Compiled);
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