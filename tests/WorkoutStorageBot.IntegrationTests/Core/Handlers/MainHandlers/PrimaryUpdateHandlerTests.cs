using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.BusinessLogic.Context.Global;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Extenions;
using WorkoutStorageBot.BusinessLogic.Handlers.MainHandlers;
using WorkoutStorageBot.Core.Extensions;
using WorkoutStorageBot.Core.Helpers;
using WorkoutStorageBot.Core.Logging;
using WorkoutStorageBot.Core.Repositories.Store;
using WorkoutStorageBot.Core.Sender;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.HandlerData.Results;
using WorkoutStorageBot.Model.DTO.Log;
using WorkoutStorageBot.UnitTests.Helpers;

namespace WorkoutStorageBot.IntegrationTests.Core.Handlers.MainHandlers
{
    public class PrimaryUpdateHandlerTests : IDisposable
    {
        private EntityContextBuilder? builder;

        private EntityContext? entityContext;

        private readonly Mock<IContextKeeper> contextKeeperMock;

        private readonly Mock<IBotResponseSender> botResponseSenderMock;

        private readonly Mock<ICustomLoggerFactory> customLoggerFactoryMock;

        private readonly Mock<ILogger> loggerMock;

        public PrimaryUpdateHandlerTests()
        {
            contextKeeperMock = new();
            botResponseSenderMock = new();
            customLoggerFactoryMock = new();
            loggerMock = new();

            customLoggerFactoryMock.Setup(x => x.CreateLogger<It.IsAnyType>()).Returns(loggerMock.Object);
        }

        [Theory]
        [InlineData("UnknownUpdateInfo", "", false)]
        [InlineData("UnknownUpdateInfo", "", true)]
        [InlineData("ShortUpdateInfo", "UserContextIsNullAndUserNotExistInDb", true)]
        [InlineData("ShortUpdateInfo", "UserContextIsNullButUserExistInDb", true)]
        [InlineData("ShortUpdateInfo", "ProcessUnexpectedUpdateType", false)]
        [InlineData("ShortUpdateInfo", "ProcessUnexpectedUpdateType", true)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithoutContextWithoutActiveCycle", false)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithoutContextWithoutActiveCycle", true)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithoutContextWithActiveCycle", false)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithoutContextWithActiveCycle", true)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithContext", false)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithContext", true)]
        public async Task Process_ByPrimaryUpdateHandlerWithNotWhiteListUser_ShouldReturnExpectedHandlerResult(string scenario, string subScenario, bool whiteListEnable)
        {
            // Arrange
            InitEntityContext(scenario, subScenario, false, false);

            PrimaryUpdateHandler primaryUpdateHandler = GetPrimaryUpdateHandler(whiteListEnable);

            int userID = 1;
            string firstName = "TestFirstName";
            string userName = "TestUsername";

            Update update = GetUpdate(userID, firstName, userName, scenario, subScenario);

            if (subScenario == "ProcessExpectedUpdateTypeWithContext")
                SetupContext();

            HandlerResult startHandlerResult = new HandlerResult()
            {
                Update = update,
                HasAccess = true,
            };

            // Act
            HandlerResult handlerResult = await primaryUpdateHandler.Process(startHandlerResult, TestContext.Current.CancellationToken);

            // Assert
            if(scenario == "UnknownUpdateInfo")
                AssertUnknownUpdateInfoScenario(handlerResult);
            else if (scenario == "ShortUpdateInfo")
            {
                if (whiteListEnable)
                {
                    if (subScenario == "UserContextIsNullAndUserNotExistInDb")
                        AssertUserContextIsNullSubScenario(handlerResult, $"Не удалось создать нового пользователя {userID} ({userName}), т.к. включен режим белого списка");
                    else
                        AssertUserContextIsNullSubScenario(handlerResult);
                }
                else if (subScenario == "ProcessUnexpectedUpdateType")
                    AssertProcessUnexpectedUpdateTypeSubScenario(handlerResult);
                else if (subScenario == "ProcessExpectedUpdateTypeWithoutContextWithoutActiveCycle")
                    AssertProcessExpectedUpdateTypeWithoutContextWithoutActiveCycleSubScenario(handlerResult);
                else if (subScenario == "ProcessExpectedUpdateTypeWithoutContextWithActiveCycle")
                    AssertProcessExpectedUpdateTypeWithoutContextWithActiveCycleSubScenario(handlerResult);
                else if (subScenario == "ProcessExpectedUpdateTypeWithContext")
                    AssertProcessExpectedUpdateTypeWithContextSubScenario(handlerResult);
            }
        }

        [Theory]
        [InlineData("UnknownUpdateInfo", "", false)]
        [InlineData("UnknownUpdateInfo", "", true)]
        [InlineData("ShortUpdateInfo", "UserContextIsNullAndUserNotExistInDb", true)]
        //[InlineData("ShortUpdateInfo", "UserContextIsNullButUserExistInDb", true)] // такого кейса быть не может, кроме как поставить BlackList = true, но это отдельный метод для кейсов
        [InlineData("ShortUpdateInfo", "ProcessUnexpectedUpdateType", false)]
        [InlineData("ShortUpdateInfo", "ProcessUnexpectedUpdateType", true)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithoutContextWithoutActiveCycle", false)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithoutContextWithoutActiveCycle", true)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithoutContextWithActiveCycle", false)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithoutContextWithActiveCycle", true)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithContext", false)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithContext", true)]
        public async Task Process_ByPrimaryUpdateHandlerWithWhiteListUser_ShouldReturnExpectedHandlerResult(string scenario, string subScenario, bool whiteListEnable)
        {
            // Arrange
            InitEntityContext(scenario, subScenario, false, true);

            PrimaryUpdateHandler primaryUpdateHandler = GetPrimaryUpdateHandler(whiteListEnable);

            int userID = 1;
            string firstName = "TestFirstName";
            string userName = "TestUsername";

            Update update = GetUpdate(userID, firstName, userName, scenario, subScenario);

            if (subScenario == "ProcessExpectedUpdateTypeWithContext")
                SetupContext();

            HandlerResult startHandlerResult = new HandlerResult()
            {
                Update = update,
                HasAccess = true,
            };

            // Act
            HandlerResult handlerResult = await primaryUpdateHandler.Process(startHandlerResult, TestContext.Current.CancellationToken);

            // Assert
            if (scenario == "UnknownUpdateInfo")
                AssertUnknownUpdateInfoScenario(handlerResult);
            else if (scenario == "ShortUpdateInfo")
            {
                if (subScenario == "ProcessUnexpectedUpdateType")
                    AssertProcessUnexpectedUpdateTypeSubScenario(handlerResult);
                else if (subScenario == "ProcessExpectedUpdateTypeWithoutContextWithoutActiveCycle")
                    AssertProcessExpectedUpdateTypeWithoutContextWithoutActiveCycleSubScenario(handlerResult);
                else if (subScenario == "ProcessExpectedUpdateTypeWithoutContextWithActiveCycle")
                    AssertProcessExpectedUpdateTypeWithoutContextWithActiveCycleSubScenario(handlerResult);
                else if (subScenario == "ProcessExpectedUpdateTypeWithContext")
                    AssertProcessExpectedUpdateTypeWithContextSubScenario(handlerResult);
            }
        }

        [Theory]
        [InlineData("UnknownUpdateInfo", "", false)]
        [InlineData("UnknownUpdateInfo", "", true)]
        [InlineData("ShortUpdateInfo", "UserContextIsNullAndUserNotExistInDb", true)]
        [InlineData("ShortUpdateInfo", "UserContextIsNullButUserExistInDb", true)]
        [InlineData("ShortUpdateInfo", "ProcessUnexpectedUpdateType", false)]
        [InlineData("ShortUpdateInfo", "ProcessUnexpectedUpdateType", true)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithoutContextWithoutActiveCycle", false)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithoutContextWithoutActiveCycle", true)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithoutContextWithActiveCycle", false)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithoutContextWithActiveCycle", true)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithContext", false)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithContext", true)]
        public async Task Process_ByPrimaryUpdateHandlerWithVariableBlackListUser_ShouldReturnExpectedHandlerResult(string scenario, string subScenario, bool userInBlackList)
        {
            // Arrange
            InitEntityContext(scenario, subScenario, userInBlackList, false);

            PrimaryUpdateHandler primaryUpdateHandler = GetPrimaryUpdateHandler(false);

            int userID = 1;
            string firstName = "TestFirstName";
            string userName = "TestUsername";

            Update update = GetUpdate(userID, firstName, userName, scenario, subScenario);

            if (subScenario == "ProcessExpectedUpdateTypeWithContext")
                SetupContext();

            HandlerResult startHandlerResult = new HandlerResult()
            {
                Update = update,
                HasAccess = true,
            };

            // Act
            HandlerResult handlerResult = await primaryUpdateHandler.Process(startHandlerResult, TestContext.Current.CancellationToken);

            // Assert
            if (scenario == "UnknownUpdateInfo")
                AssertUnknownUpdateInfoScenario(handlerResult);
            else if (scenario == "ShortUpdateInfo")
            {
                if (userInBlackList && subScenario != "UserContextIsNullAndUserNotExistInDb")
                    AssertUserContextIsNullSubScenario(handlerResult);
                else if (subScenario == "ProcessUnexpectedUpdateType")
                    AssertProcessUnexpectedUpdateTypeSubScenario(handlerResult);
                else if (subScenario == "UserContextIsNullAndUserNotExistInDb")
                    AssertProcessExpectedUpdateTypeWithoutContextWithoutActiveCycleSubScenario(handlerResult, true);
                else if (subScenario == "ProcessExpectedUpdateTypeWithoutContextWithoutActiveCycle")
                    AssertProcessExpectedUpdateTypeWithoutContextWithoutActiveCycleSubScenario(handlerResult);
                else if (subScenario == "ProcessExpectedUpdateTypeWithoutContextWithActiveCycle")
                    AssertProcessExpectedUpdateTypeWithoutContextWithActiveCycleSubScenario(handlerResult);
                else if (subScenario == "ProcessExpectedUpdateTypeWithContext")
                    AssertProcessExpectedUpdateTypeWithContextSubScenario(handlerResult);
            }
        }

        [Theory]
        [InlineData("UnknownUpdateInfo", "", false)]
        [InlineData("UnknownUpdateInfo", "", true)]
        [InlineData("ShortUpdateInfo", "UserContextIsNullAndUserNotExistInDb", true)]
        [InlineData("ShortUpdateInfo", "UserContextIsNullButUserExistInDb", true)]
        [InlineData("ShortUpdateInfo", "ProcessUnexpectedUpdateType", false)]
        [InlineData("ShortUpdateInfo", "ProcessUnexpectedUpdateType", true)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithoutContextWithoutActiveCycle", false)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithoutContextWithoutActiveCycle", true)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithoutContextWithActiveCycle", false)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithoutContextWithActiveCycle", true)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithContext", false)]
        [InlineData("ShortUpdateInfo", "ProcessExpectedUpdateTypeWithContext", true)]
        public async Task Process_ByPrimaryUpdateHandlerWithBlackAndWhiteListUserButBlackHasPriority_ShouldReturnExpectedHandlerResult(string scenario, string subScenario, bool whiteListEnable)
        {
            // Arrange
            InitEntityContext(scenario, subScenario, true, true);

            PrimaryUpdateHandler primaryUpdateHandler = GetPrimaryUpdateHandler(whiteListEnable);

            int userID = 1;
            string firstName = "TestFirstName";
            string userName = "TestUsername";

            Update update = GetUpdate(userID, firstName, userName, scenario, subScenario);

            if (subScenario == "ProcessExpectedUpdateTypeWithContext")
                SetupContext();

            HandlerResult startHandlerResult = new HandlerResult()
            {
                Update = update,
                HasAccess = true,
            };

            // Act
            HandlerResult handlerResult = await primaryUpdateHandler.Process(startHandlerResult, TestContext.Current.CancellationToken);

            // Assert
            if (scenario == "UnknownUpdateInfo")
                AssertUnknownUpdateInfoScenario(handlerResult);
            else if (scenario == "ShortUpdateInfo")
            {
                if (subScenario == "UserContextIsNullAndUserNotExistInDb")
                    AssertUserContextIsNullSubScenario(handlerResult, $"Не удалось создать нового пользователя {userID} ({userName}), т.к. включен режим белого списка");
                else
                    AssertUserContextIsNullSubScenario(handlerResult);
            }
        }

        private void InitEntityContext(string scenario, string subScenario, bool userBlackList, bool userWhiteList)
        {
            builder = new EntityContextBuilder();

            if (subScenario != "UserContextIsNullAndUserNotExistInDb")
            {
                entityContext = builder.Create()
                                       .WithUserInformation(blackList: userBlackList, whiteList: userWhiteList)
                                       .Build();

                if (subScenario == "ProcessExpectedUpdateTypeWithoutContextWithActiveCycle")
                    builder.WithCycle(isActive: true);
            }
            else
            {
                entityContext = builder.Create()
                                       .Build();
            }
        }

        private PrimaryUpdateHandler GetPrimaryUpdateHandler(bool whiteListEnable)
        {
            ConfigurationData configurationData = new ConfigurationData()
            {
                DB = new DbSettings() { Database = "test" },
                Bot = new BotSettings() { Token = "1", WhiteListIsEnable = whiteListEnable }
            };

            CoreTools coreTools = new CoreTools()
            {
                ConfigurationData = configurationData,
                Db = entityContext ?? throw new InvalidOperationException($"entityContext was not initialized"),
                ContextKeeper = contextKeeperMock.Object,
                BotResponseSender = botResponseSenderMock.Object,
                LoggerFactory = customLoggerFactoryMock.Object,
            };

            PrimaryUpdateHandler primaryUpdateHandler = new PrimaryUpdateHandler(coreTools, new RepositoriesStore(entityContext));

            return primaryUpdateHandler;
        }

        private Update GetUpdate(int userID, string firstName, string userName, string scenario, string subScenario)
        {
            Update update = new Update();
            Chat chat = new Chat()
            {
                Id = 1,
            };
            User user = new User()
            {
                Id = userID,
                Username = userName,
                FirstName = firstName,
            };

            if (scenario == "ShortUpdateInfo")
            {
                if (subScenario == "ProcessUnexpectedUpdateType")
                {
                    update.EditedMessage = new Message()
                    {
                        Chat = chat,
                        Text = "Test",
                        From = user,

                    };
                }
                else
                {
                    update.Message = new Message()
                    {
                        Chat = chat,
                        Text = "Test",
                        From = user,
                    };
                }
            }
            else
                update = new Update();

            return update;
        }

        private void SetupContext()
        {
            entityContext.ThrowIfNull();

            DTOUserInformation localDTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();
            UserContext localUserContext = new UserContext(localDTOCurrentUser, Roles.User | Roles.Admin, false);

            contextKeeperMock.Setup(x => x.HasContext(localDTOCurrentUser.UserId)).Returns(true);
            contextKeeperMock.Setup(x => x.GetContext(localDTOCurrentUser.UserId)).Returns(localUserContext);
        }
        private void AssertUnknownUpdateInfoScenario(HandlerResult handlerResult)
        {
            loggerMock.Verify(x => x.Log(
                                    LogLevel.Warning,
                                    It.IsAny<EventId>(),
                                    It.Is<It.IsAnyType>((v, t) =>
                                    v != null && v.ToString() == "Получен неизвестный update c типом Unknown"),
                                    It.IsAny<Exception>(),
                                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()
                                ),
                                Times.Once);

            handlerResult.IsNeedContinue.Should().BeFalse();

            handlerResult.Should().NotBeOfType<PrimaryHandlerResult>();
        }

        private void AssertUserContextIsNullSubScenario(HandlerResult handlerResult, string? checkingLogMessage = null)
        {
            handlerResult.IsNeedContinue.Should().BeFalse();

            if (!string.IsNullOrWhiteSpace(checkingLogMessage))
            {
                loggerMock.Verify(x => x.Log(
                                    LogLevel.Warning,
                                    It.IsAny<EventId>(),
                                    It.Is<It.IsAnyType>((v, t) =>
                                    v != null && v.ToString() == checkingLogMessage),
                                    It.IsAny<Exception>(),
                                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()
                                ),
                                Times.Once);
            }

            handlerResult.Should().NotBeOfType<PrimaryHandlerResult>();
        }

        private void AssertProcessUnexpectedUpdateTypeSubScenario(HandlerResult handlerResult)
        {
            builder.Should().NotBeNull();

            handlerResult.IsNeedContinue.Should().BeFalse();

            handlerResult.Should().BeOfType<PrimaryHandlerResult>();

            PrimaryHandlerResult primaryHandlerResult = (PrimaryHandlerResult)handlerResult;
            primaryHandlerResult.IsNewContext.Should().BeTrue();

            primaryHandlerResult.CurrentUserContext.Should().NotBeNull();
            primaryHandlerResult.CurrentUserContext.UserInformation.Id = builder.TestUserInformation.Id;
            primaryHandlerResult.CurrentUserContext.UserInformation.UserId = builder.TestUserInformation.UserId;
            primaryHandlerResult.CurrentUserContext.UserInformation.Username = builder.TestUserInformation.Username;

            primaryHandlerResult.InformationSet.Should().NotBeNull();
            primaryHandlerResult.InformationSet.Message.Should().Contain($"Неподдерживаемый тип сообщения: EditedMessage | EventId:");
            primaryHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));

            primaryHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            primaryHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            loggerMock.Verify(x => x.Log(
                            LogLevel.Information,
                            It.IsAny<EventId>(),
                            It.IsAny<LogData>(),
                            null,
                            LogFormatter.SimpleFormatter
                        ),
                        Times.Once);
        }

        private void AssertProcessExpectedUpdateTypeWithoutContextWithoutActiveCycleSubScenario(HandlerResult handlerResult, bool withoutDB = false)
        {
            builder.Should().NotBeNull();

            int id;
            long userID;
            string firstName;
            string userName;

            if (withoutDB)
            {
                id = 1;
                userID = 1;
                firstName = "TestFirstName";
                userName = "@TestUsername";
            }
            else
            {
                id = builder.TestUserInformation.Id;
                userID = builder.TestUserInformation.UserId;
                firstName = builder.TestUserInformation.FirstName;
                userName = builder.TestUserInformation.Username;
            }

            loggerMock.Verify(x => x.Log(
                                LogLevel.Information,
                                It.IsAny<EventId>(),
                                It.IsAny<LogData>(),
                                null,
                                LogFormatter.SimpleFormatter
                            ),
                            Times.Once);

            handlerResult.IsNeedContinue.Should().BeFalse();

            PrimaryHandlerResult primaryHandlerResult = (PrimaryHandlerResult)handlerResult;
            primaryHandlerResult.IsNewContext.Should().BeTrue();

            primaryHandlerResult.CurrentUserContext.Should().NotBeNull();

            primaryHandlerResult.CurrentUserContext.UserInformation.Id.Should().Be(id);
            primaryHandlerResult.CurrentUserContext.UserInformation.UserId.Should().Be(userID);
            primaryHandlerResult.CurrentUserContext.UserInformation.Username.Should().Be(userName);
            primaryHandlerResult.CurrentUserContext.UserInformation.FirstName.Should().Be(firstName);

            primaryHandlerResult.InformationSet.Should().NotBeNull();
            primaryHandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.Start);
            primaryHandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            primaryHandlerResult.InformationSet.Message.Should().Contain(@$"Начнём");
            primaryHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.AddCycle, ButtonsSet.None));

            primaryHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            primaryHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();
        }

        private void AssertProcessExpectedUpdateTypeWithoutContextWithActiveCycleSubScenario(HandlerResult handlerResult)
        {
            builder.Should().NotBeNull();

            loggerMock.Verify(x => x.Log(
                                LogLevel.Information,
                                It.IsAny<EventId>(),
                                It.IsAny<LogData>(),
                                null,
                                LogFormatter.SimpleFormatter
                            ),
                            Times.Once);

            handlerResult.IsNeedContinue.Should().BeFalse();

            PrimaryHandlerResult primaryHandlerResult = (PrimaryHandlerResult)handlerResult;
            primaryHandlerResult.IsNewContext.Should().BeTrue();

            primaryHandlerResult.CurrentUserContext.Should().NotBeNull();
            primaryHandlerResult.CurrentUserContext.UserInformation.Id.Should().Be(builder.TestUserInformation.Id);
            primaryHandlerResult.CurrentUserContext.UserInformation.UserId.Should().Be(builder.TestUserInformation.UserId);
            primaryHandlerResult.CurrentUserContext.UserInformation.Username.Should().Be(builder.TestUserInformation.Username);
            primaryHandlerResult.CurrentUserContext.UserInformation.FirstName.Should().Be(builder.TestUserInformation.FirstName);

            primaryHandlerResult.InformationSet.Should().NotBeNull();
            primaryHandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            primaryHandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            primaryHandlerResult.InformationSet.Message.Should().Contain(@$"Информация о предыдущей сессии не была найдена
======================

Выберите интересующий раздел
");
            primaryHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.Main, ButtonsSet.None));

            primaryHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            primaryHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();
        }

        private void AssertProcessExpectedUpdateTypeWithContextSubScenario(HandlerResult handlerResult)
        {
            builder.Should().NotBeNull();

            loggerMock.Verify(x => x.Log(
                                LogLevel.Information,
                                It.IsAny<EventId>(),
                                It.IsAny<LogData>(),
                                null,
                                LogFormatter.SimpleFormatter
                            ),
                            Times.Once);

            PrimaryHandlerResult primaryHandlerResult = (PrimaryHandlerResult)handlerResult;
            primaryHandlerResult.IsNewContext.Should().BeFalse();
            handlerResult.IsNeedContinue.Should().BeTrue();

            primaryHandlerResult.CurrentUserContext.Should().NotBeNull();
            primaryHandlerResult.CurrentUserContext.UserInformation.Id.Should().Be(builder.TestUserInformation.Id);
            primaryHandlerResult.CurrentUserContext.UserInformation.UserId.Should().Be(builder.TestUserInformation.UserId);
            primaryHandlerResult.CurrentUserContext.UserInformation.Username.Should().Be(builder.TestUserInformation.Username);
            primaryHandlerResult.CurrentUserContext.UserInformation.FirstName.Should().Be(builder.TestUserInformation.FirstName);

            primaryHandlerResult.InformationSet.Should().BeNull();
        }

        public void Dispose()
        {
            builder?.Dispose();
        }
    }
}