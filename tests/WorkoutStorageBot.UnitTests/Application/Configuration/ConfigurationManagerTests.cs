using FluentAssertions;
using WorkoutStorageBot.Application.Configuration;

namespace WorkoutStorageBot.UnitTests.Application.Configuration
{
    public class ConfigurationManagerTests
    {
        [Fact]
        public void GetConfiguration_WithTemplateAppSettings_ShouldSetSpecifiedPropertiesToConfigModel()
        {
            // Arrange
            string pathToTemplateAppSettings = "./Application/StartConfiguration/TemplateAppSettings.json";

            // Act
            ConfigurationData templateConfigurationData = ConfigurationManager.GetConfiguration(pathToTemplateAppSettings);

            // Assert
            templateConfigurationData.DB.EnsureCreated.Should().BeFalse();
            templateConfigurationData.DB.Server.Should().BeEmpty();
            templateConfigurationData.DB.Database.Should().Be("data/test.db");
            templateConfigurationData.DB.UserName.Should().BeEmpty();
            templateConfigurationData.DB.Password.Should().BeEmpty();

            templateConfigurationData.Bot.Token.Should().Be("<Your telegram bot token>");
            templateConfigurationData.Bot.WhiteListIsEnable.Should().BeFalse();
            templateConfigurationData.Bot.OwnersChatIDs.Single().Should().Be("000000000");
            templateConfigurationData.Bot.IsNeedCacheContext.Should().BeTrue();
            templateConfigurationData.Bot.WhiteListIsEnable.Should().BeFalse();
            templateConfigurationData.Bot.IsSupportOnlyKnownTypes.Should().BeTrue();

            templateConfigurationData.Notifications.NotifyOwnersAboutCriticalErrors.Should().BeTrue();
            templateConfigurationData.Notifications.NotifyOwnersAboutRuntimeErrors.Should().BeTrue();

            templateConfigurationData.LogInfo.MainRuleLog.DBLogLevels.Should().Contain(["Error", "Critical"]);
            templateConfigurationData.LogInfo.MainRuleLog.ConsoleLogLevels.Should().Contain("All");

            CustomRuleLog customRuleLog = templateConfigurationData.LogInfo.CustomRulesLog.Single();
            customRuleLog.FullClassName.Should().Be("WorkoutStorageBot.BusinessLogic.Handlers.MainHandlers.PrimaryUpdateHandler");
            customRuleLog.DBLogLevels.Should().Contain("Warning", "Error", "Critical");
            customRuleLog.ConsoleLogLevels.Should().Contain("Warning");
        }

        [Fact]
        public void GetSerializedSafeConfigurationData_WithTemplateAppSettings_ShouldCreateSerializedSafeConfigurationData()
        {
            // Arrange
            string pathToTemplateAppSettings = "./Application/StartConfiguration/TemplateAppSettings.json";

            ConfigurationData templateConfigurationData = ConfigurationManager.GetConfiguration(pathToTemplateAppSettings);
            
            // Act
            string serializedSafeConfigurationData = ConfigurationManager.GetSerializedSafeConfigurationData(templateConfigurationData);

            // Assert
            // т.к. в шаблоне пустые логин и пароль, проверка реализована через проверку ***, правильная реализация, две строки ниже
            //serializedSafeConfigurationData.Should().NotContain(templateConfigurationData.DB.UserName);
            //serializedSafeConfigurationData.Should().NotContain(templateConfigurationData.DB.Password);
            serializedSafeConfigurationData.Should().Contain("\"UserName\": \"******\"");
            serializedSafeConfigurationData.Should().Contain("\"Password\": \"******\"");
            
            serializedSafeConfigurationData.Should().NotContain(templateConfigurationData.Bot.Token);

            foreach (string ownerChatID in templateConfigurationData.Bot.OwnersChatIDs)
            {
                serializedSafeConfigurationData.Should().NotContain(ownerChatID);
            }

            serializedSafeConfigurationData.Should().Contain(templateConfigurationData.LogInfo.CustomRulesLog.Single().FullClassName);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetCensorToConfigurationData_WithTemplateAppSettings_ShouldCensorSensitiveInformation(bool isNeedCensorOwnersChatIDs)
        {
            // Arrange
            string pathToTemplateAppSettings = "./Application/StartConfiguration/TemplateAppSettings.json";
            
            ConfigurationData templateConfigurationData = ConfigurationManager.GetConfiguration(pathToTemplateAppSettings);

            string originalToken = templateConfigurationData.Bot.Token;

            List<string> originalOwnersChatIDs = templateConfigurationData.Bot.OwnersChatIDs.ToList();

            // Act
            ConfigurationManager.SetCensorToConfigurationData(templateConfigurationData, isNeedCensorOwnersChatIDs);

            // Assert
            templateConfigurationData.DB.UserName.Should().Be("******");
            templateConfigurationData.DB.Password.Should().Be("******");
            templateConfigurationData.Bot.Token.Should().NotBe(originalToken);

            if (isNeedCensorOwnersChatIDs)
                templateConfigurationData.Bot.OwnersChatIDs.Should().NotBeEquivalentTo(originalOwnersChatIDs);
            else
                templateConfigurationData.Bot.OwnersChatIDs.Should().BeEquivalentTo(originalOwnersChatIDs);

        }
    }
}