using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.BusinessLogic.Context.Global;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Extenions;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler.Context;
using WorkoutStorageBot.BusinessLogic.Handlers.MainHandlers;
using WorkoutStorageBot.BusinessLogic.Helpers.CallbackQueryParser;
using WorkoutStorageBot.Core.Logging;
using WorkoutStorageBot.Core.Repositories.Store;
using WorkoutStorageBot.Core.Sender;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.InformationSetForSend;
using WorkoutStorageBot.UnitTests.Helpers;

namespace WorkoutStorageBot.IntegrationTests.Core.Handlers.CallBackCommandHandler
{
    public class AdminCHTests : IDisposable
    {
        private readonly EntityContextBuilder builder;

        private readonly EntityContext entityContext;

        private Mock<IContextKeeper> contextKeeperMock;

        private readonly Mock<IBotResponseSender> botResponseSenderMock;

        private Mock<ICustomLoggerFactory> customLoggerFactoryMock;

        private readonly Mock<ILogger> loggerMock;

        private readonly CommandHandlerTools commandHandlerTools;

        private readonly CancellationTokenSource cts;

        public AdminCHTests()
        {
            builder = new EntityContextBuilder();
            entityContext = builder.Create()
                                   .WithUserInformation()
                                   .Build();

            contextKeeperMock = new();
            botResponseSenderMock = new();
            customLoggerFactoryMock = new();
            loggerMock = new();

            cts = new CancellationTokenSource();

            customLoggerFactoryMock.Setup(x => x.CreateLogger<It.IsAnyType>()).Returns(loggerMock.Object);

            CoreTools coreTools = new CoreTools()
            {
                ConfigurationData = new ConfigurationData() { DB = new DbSettings() { Database = "test"} },
                Db = entityContext,
                ContextKeeper = contextKeeperMock.Object,
                BotResponseSender = botResponseSenderMock.Object,
                LoggerFactory = customLoggerFactoryMock.Object,
                AppCTS = cts,
            };

            DTOUserInformation DTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();

            UserContext userContext = new UserContext(DTOCurrentUser, Roles.User | Roles.Admin);
            userContext.DataManager.CreateAndSetCurrentCycle("testCycle", true, DTOCurrentUser);

            commandHandlerTools = new CommandHandlerTools()
            {
                ParentHandler = new PrimaryUpdateHandler(coreTools, new RepositoriesStore(entityContext)),
                CurrentUserContext = userContext,
            };
        }

        [Fact]
        public async Task GetInformationSet_WithAdminSubDirectionWithoutAdminRoles_ShouldReturnExpectedIInformationSet()
        {
            // поднимаем зависимости локально для единичного теста без админ прав

            // Arrange
            CoreTools localCoreTools = new CoreTools()
            {
                ConfigurationData = new ConfigurationData(),
                Db = entityContext,
                ContextKeeper = contextKeeperMock.Object,
                BotResponseSender = botResponseSenderMock.Object,
                LoggerFactory = customLoggerFactoryMock.Object,
                AppCTS = cts,
            };

            DTOUserInformation localDTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();

            UserContext userContext = new UserContext(localDTOCurrentUser);
            userContext.DataManager.CreateAndSetCurrentCycle("testCycle", true, localDTOCurrentUser);

            CommandHandlerTools localCommandHandlerTools = new CommandHandlerTools()
            {
                ParentHandler = new PrimaryUpdateHandler(localCoreTools, new RepositoriesStore(entityContext)),
                CurrentUserContext = userContext,
            };

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|Admin|DomainType|CallBackId");

            AdminCH adminCH = new AdminCH(localCommandHandlerTools, callbackQueryParser);

            // Act
            IInformationSet informationSet = await adminCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            informationSet.Message.Should().Be("Отказано в действии");
            informationSet.ButtonsSets.Should().Be((ButtonsSet.Main, ButtonsSet.None));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithAdminSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|Admin|DomainType|CallBackId");

            AdminCH adminCH = new AdminCH(commandHandlerTools, callbackQueryParser);

            // Act
            IInformationSet informationSet = await adminCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            informationSet.Message.Should().Be("Выберите интересующее действие с админкой");
            informationSet.ButtonsSets.Should().Be((ButtonsSet.Admin, ButtonsSet.Main));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithLogsSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|Logs|DomainType|CallBackId");

            AdminCH adminCH = new AdminCH(commandHandlerTools, callbackQueryParser);

            // Act
            IInformationSet informationSet = await adminCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            informationSet.Message.Should().Be("Выберите интересующее действие с логами");
            informationSet.ButtonsSets.Should().Be((ButtonsSet.AdminLogs, ButtonsSet.Admin));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task GetInformationSet_WithShowLastLogSubDirection_ShouldReturnExpectedIInformationSet(bool withLogs)
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|ShowLastLog|DomainType|CallBackId");

            AdminCH adminCH = new AdminCH(commandHandlerTools, callbackQueryParser);

            if (withLogs)
            {
                await entityContext.Logs.AddAsync(new WorkoutStorageModels.Entities.Core.Logging.Log() { LogLevel = LogLevel.Warning.ToString(), Message = "TestLog", SourceContext = "Abc", });
                await entityContext.SaveChangesAsync();
            }

            // Act
            IInformationSet informationSet = await adminCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            if (withLogs)
                informationSet.Message.Should().Be(@"Последний лог:
======================
[1] [Warning] [] [01.01.0001 00:00:00] [Null] [Abc]:
TestLog
======================

Выберите интересующее действие с логами
");
            else
                informationSet.Message.Should().Be(@"Логов не найдено
======================

Выберите интересующее действие с логами
");

            informationSet.ButtonsSets.Should().Be((ButtonsSet.AdminLogs, ButtonsSet.Admin));
            informationSet.ParseMode.Should().Be(ParseMode.None);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task GetInformationSet_WithShowLastExceptionLogsSubDirection_ShouldReturnExpectedIInformationSet(bool withLogs)
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|ShowLastExceptionLogs|DomainType|CallBackId");

            AdminCH adminCH = new AdminCH(commandHandlerTools, callbackQueryParser);

            if (withLogs)
            {
                await entityContext.Logs.AddAsync(new WorkoutStorageModels.Entities.Core.Logging.Log() { LogLevel = LogLevel.Error.ToString(), Message = "TestLog1", SourceContext = "Abc", });
                await entityContext.Logs.AddAsync(new WorkoutStorageModels.Entities.Core.Logging.Log() { LogLevel = LogLevel.Critical.ToString(), Message = "TestLog2", SourceContext = "Abc", });
                await entityContext.SaveChangesAsync();
            }

            // Act
            IInformationSet informationSet = await adminCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            if (withLogs)
                informationSet.Message.Should().Be(@"Последние ошибочные логи:
======================
[1] [Error] [] [01.01.0001 00:00:00] [Null] [Abc]:
TestLog1
======================
[2] [Critical] [] [01.01.0001 00:00:00] [Null] [Abc]:
TestLog2

======================

Выберите интересующее действие с логами
");
            else
                informationSet.Message.Should().Be(@"Последние ошибочные логи:
======================
Логов не найдено

======================

Выберите интересующее действие с логами
");

            informationSet.ButtonsSets.Should().Be((ButtonsSet.AdminLogs, ButtonsSet.Admin));
            informationSet.ParseMode.Should().Be(ParseMode.None);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithFindLogByIDSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|FindLogByID|DomainType|CallBackId");

            AdminCH adminCH = new AdminCH(commandHandlerTools, callbackQueryParser);
            
            // Act
            IInformationSet informationSet = await adminCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.FindLogByID);
            
            informationSet.Message.Should().Be(@"Введите Id интересующего лога:");
            informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.AdminLogs));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithFindLogByEventIDSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|FindLogByEventID|DomainType|CallBackId");

            AdminCH adminCH = new AdminCH(commandHandlerTools, callbackQueryParser);
            
            // Act
            IInformationSet informationSet = await adminCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.FindLogByEventID);

            informationSet.Message.Should().Be(@"Введите eventId интересующего лога:");
            informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.AdminLogs));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithAdminUsersSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|AdminUsers|DomainType|CallBackId");

            AdminCH adminCH = new AdminCH(commandHandlerTools, callbackQueryParser);
            
            // Act
            IInformationSet informationSet = await adminCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            informationSet.Message.Should().Be("Выберите интересующее действие с пользователями");
            informationSet.ButtonsSets.Should().Be((ButtonsSet.AdminUsers, ButtonsSet.Admin));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithShowCountActiveSessionsSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|ShowCountActiveSessions|DomainType|CallBackId");

            int countActiveSessions = 2;
            contextKeeperMock.Setup(x => x.Count).Returns(countActiveSessions);

            AdminCH adminCH = new AdminCH(commandHandlerTools, callbackQueryParser);
            
            // Act
            IInformationSet informationSet = await adminCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            informationSet.Message.Should().Contain($"<b>{countActiveSessions}</b>").And.Contain("Выберите интересующее действие с пользователями");
            informationSet.ButtonsSets.Should().Be((ButtonsSet.AdminUsers, ButtonsSet.Admin));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithSendMessageToUserSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|SendMessageToUser|DomainType|CallBackId");

            AdminCH adminCH = new AdminCH(commandHandlerTools, callbackQueryParser);
            
            // Act
            IInformationSet informationSet = await adminCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.SendMessageToUser);

            informationSet.Message.Should().Be(@"Введите [UserId/@userLogin]-[Сообщение] для отправки сообщения пользователю
======================

Пример: @TestUser-Тест
");
            informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.AdminUsers));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithSendMessagesToActiveUsersSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|SendMessagesToActiveUsers|DomainType|CallBackId");

            AdminCH adminCH = new AdminCH(commandHandlerTools, callbackQueryParser);
            
            // Act
            IInformationSet informationSet = await adminCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.SendMessagesToActiveUsers);

            informationSet.Message.Should().Be($"Введите сообщение для отправки его всем пользователям с активной сессией");
            informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.AdminUsers));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithSendMessagesToAllUsersSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|SendMessagesToAllUsers|DomainType|CallBackId");

            AdminCH adminCH = new AdminCH(commandHandlerTools, callbackQueryParser);
            
            // Act
            IInformationSet informationSet = await adminCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.SendMessagesToAllUsers);

            informationSet.Message.Should().Be($"Введите сообщение для отправки его всем пользователям из БД");
            informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.AdminUsers));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithChangeUserStateSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|ChangeUserState|DomainType|CallBackId");

            AdminCH adminCH = new AdminCH(commandHandlerTools, callbackQueryParser);
            
            // Act
            IInformationSet informationSet = await adminCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.ChangeUserState);

            informationSet.Message.Should().Be(@$"Изменение black/white list у пользователя:
======================

Введите [UserID/@TestUser] [bl/wl]
");
            informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.AdminUsers));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithDeleteUserSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|DeleteUser|DomainType|CallBackId");

            AdminCH adminCH = new AdminCH(commandHandlerTools, callbackQueryParser);
            
            // Act
            IInformationSet informationSet = await adminCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.DeleteUser);

            informationSet.Message.Should().Be(@$"Удаление пользователя:
======================
<b>Внимание, пользователь будет удалён без возможности восстановления!</b>
======================

Введите [UserID/@TestUser]
");
            informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.AdminUsers));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithShowStartConfiguration_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|ShowStartConfiguration|DomainType|CallBackId");

            AdminCH adminCH = new AdminCH(commandHandlerTools, callbackQueryParser);
            
            // Act
            IInformationSet informationSet = await adminCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            informationSet.Message.Should().Be(@"Текущая настройка:
{
  ""DB"": {
    ""EnsureCreated"": false,
    ""Server"": null,
    ""Database"": ""test"",
    ""UserName"": ""******"",
    ""Password"": ""******"",
    ""ConnectionString"": ""Data Source= test""
  },
  ""Bot"": {
    ""Token"": ""******"",
    ""WhiteListIsEnable"": false,
    ""OwnersChatIDs"": [],
    ""IsNeedCacheContext"": false,
    ""IsNeedLimits"": false,
    ""IsSupportOnlyKnownTypes"": false
  },
  ""Notifications"": {
    ""NotifyOwnersAboutCriticalErrors"": false,
    ""NotifyOwnersAboutRuntimeErrors"": false
  },
  ""LogInfo"": {
    ""MainRuleLog"": {
      ""DBLogLevels"": [],
      ""ConsoleLogLevels"": []
    },
    ""CustomRulesLog"": []
  },
  ""AboutBotText"": """"
}
======================

Выберите интересующее действие
");
            informationSet.ButtonsSets.Should().Be((ButtonsSet.Admin, ButtonsSet.Main));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithChangeLimitsModsSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|ChangeLimitsMods|DomainType|CallBackId");

            AdminCH adminCH = new AdminCH(commandHandlerTools, callbackQueryParser);

            // Act
            IInformationSet informationSet = await adminCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            informationSet.Message.Should().Be(@$"Режим использования лимитов переключён в: <b>False</b>
======================

Выберите интересующее действие
");
            commandHandlerTools.CurrentUserContext.LimitsManager.IsEnableLimit.Should().BeFalse();

            informationSet.ButtonsSets.Should().Be((ButtonsSet.Admin, ButtonsSet.Main));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(2000)]
        public async Task GetInformationSet_WithDisableBotSubDirection_ShouldReturnExpectedIInformationSet(int ms)
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|DisableBot|DomainType|CallBackId");

            AdminCH adminCH = new AdminCH(commandHandlerTools, callbackQueryParser);

            // Act
            IInformationSet informationSet = await adminCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            await Task.Delay(ms);

            if (ms >= 2000)
                cts.IsCancellationRequested.Should().BeTrue();
            else
                cts.IsCancellationRequested.Should().BeFalse();

            informationSet.Message.Should().Be(@$"Бот отключён");

            informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        public void Dispose()
        {
            builder.Dispose();
        }
    }
}