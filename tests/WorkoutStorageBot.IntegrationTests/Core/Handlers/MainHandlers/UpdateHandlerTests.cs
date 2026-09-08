using FluentAssertions;
using FluentAssertions.Specialized;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.BusinessLogic.Context.Global;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Extenions;
using WorkoutStorageBot.BusinessLogic.Extensions;
using WorkoutStorageBot.BusinessLogic.Handlers.MainHandlers;
using WorkoutStorageBot.BusinessLogic.Helpers.Updates;
using WorkoutStorageBot.Core.Logging;
using WorkoutStorageBot.Core.Repositories.Store;
using WorkoutStorageBot.Core.Sender;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.HandlerData.Results;
using WorkoutStorageBot.Model.DTO.HandlerData.Results.UpdateInfo;
using WorkoutStorageBot.UnitTests.Helpers;

namespace WorkoutStorageBot.IntegrationTests.Core.Handlers.MainHandlers
{
    public class UpdateHandlerTests : IDisposable
    {
        private EntityContextBuilder? builder;

        private EntityContext? entityContext;

        private Mock<IContextKeeper> contextKeeperMock;

        private readonly Mock<IBotResponseSender> botResponseSenderMock;

        private Mock<ICustomLoggerFactory> customLoggerFactoryMock;

        private readonly Mock<ILogger> loggerMock;

        private readonly CancellationTokenSource cts;

        public UpdateHandlerTests()
        {
            contextKeeperMock = new();
            botResponseSenderMock = new();
            customLoggerFactoryMock = new();
            loggerMock = new();

            cts = new CancellationTokenSource();

            customLoggerFactoryMock.Setup(x => x.CreateLogger<It.IsAnyType>()).Returns(loggerMock.Object);
        }
    
        [Theory]
        [InlineData("EditedMessageQuery")]
        [InlineData("MessageQuery")]
        [InlineData("CallbackQuery", "None", true)]
        [InlineData("CallbackQuery", "None", false)]
        [InlineData("CallbackQuery", "Workout", true)]
        [InlineData("CallbackQuery", "Workout", false)]
        [InlineData("CallbackQuery", "Settings", true)]
        [InlineData("CallbackQuery", "Settings", false)]
        [InlineData("CallbackQuery", "Admin", true)]
        [InlineData("CallbackQuery", "Admin", false)]
        public async Task Process_ByUpdateHandler_ShouldReturnExpectedHandlerResult(string updateType, string? subUpdateType = null, bool? checkingComplianceCallBackId = null)
        {
            // Arrange
            InitEntityContext();

            UpdateHandler updateHandler = GetUpdateHandler();

            int userID = 1;
            string firstName = "TestFirstName";
            string userName = "TestUsername";

            UserContext localUserContext = GetUserContext();

            Update update = GetUpdate(userID, firstName, userName, updateType, subUpdateType, checkingComplianceCallBackId, localUserContext.CallBackId);

            ShortUpdateInfo updateInfo = (ShortUpdateInfo)UpdatesHelper.GetUpdateInfo(update);

            UpdateHandlerResult startHandlerResult = new UpdateHandlerResult()
            {
                Update = update,
                CurrentUserContext = localUserContext,
                ShortUpdateInfo = updateInfo,
                HasAccess = true,
            };

            if (updateType == "EditedMessageQuery")
            {
                // Act
                Func<Task> func = async () => await updateHandler.Process(startHandlerResult);

                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                exceptionAssertions.WithMessage($"Неожиданный UpdateType: {updateInfo.UpdateType}");
            }
            else
            {
                // Act
                HandlerResult handlerResult = await updateHandler.Process(startHandlerResult);

                // Assert
                builder.Should().NotBeNull();

                handlerResult.IsNeedContinue.Should().BeFalse();

                handlerResult.Should().BeOfType<UpdateHandlerResult>();

                UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResult;

                updateHandlerResult.CurrentUserContext.Should().NotBeNull();
                updateHandlerResult.CurrentUserContext.UserInformation.Id = builder.TestUserInformation.Id;
                updateHandlerResult.CurrentUserContext.UserInformation.UserId = builder.TestUserInformation.UserId;
                updateHandlerResult.CurrentUserContext.UserInformation.Username = builder.TestUserInformation.Username;

                updateHandlerResult.InformationSet.Should().NotBeNull();

                updateHandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
                updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

                if (checkingComplianceCallBackId.HasValue && !checkingComplianceCallBackId.Value)
                {
                    updateHandlerResult.InformationSet.Message.Should().Contain(@$"Действие не может быть выполнено, т.к. информация устарела
======================

Для продолжения работы используйте действия, предложенные ниже
");
                    updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.Main, ButtonsSet.None));

                    updateHandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                }
                else if (updateType == "MessageQuery")
                {
                    updateHandlerResult.InformationSet.Message.Should().Contain($"Выберите интересующий раздел");
                    updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.Main, ButtonsSet.None));
                }
                else
                {
                    if (subUpdateType == "None")
                    {
                        updateHandlerResult.InformationSet.Message.Should().Contain($"Выберите интересующий раздел");
                        updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.Main, ButtonsSet.None));

                        updateHandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                    }
                    else if (subUpdateType == "Workout")
                    {
                        updateHandlerResult.InformationSet.Message.Should().Contain(@$"Выберите тренировочный день из цикла {builder.TestCycle.Name.AddBoldAndQuotes()}");
                        updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main));

                        updateHandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                    }
                    else if (subUpdateType == "Settings")
                    {
                        updateHandlerResult.InformationSet.Message.Should().Contain($"Выберите интересующие настройки");
                        updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.Settings, ButtonsSet.Main));

                        updateHandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
                    }
                    else if (subUpdateType == "Admin")
                    {
                        updateHandlerResult.InformationSet.Message.Should().Contain($"Выберите интересующее действие с админкой");
                        updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.Admin, ButtonsSet.Main));

                        updateHandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                    }
                }
            }
        }

        [Fact]
        public async Task CloseApp_ByUpdateHandler_ShouldReturnExpectedHandlerResult()
        {
            // Arrange
            InitEntityContext();

            UpdateHandler updateHandler = GetUpdateHandler();

            TimeSpan waiting = TimeSpan.FromSeconds(2);

            // Act
            updateHandler.CloseApp(waiting);

            await Task.Delay(waiting);

            // Assert
            cts.IsCancellationRequested.Should().BeTrue();

            loggerMock.Verify(x => x.Log(
                                   LogLevel.Warning,
                                   It.IsAny<EventId>(),
                                   It.Is<It.IsAnyType>((v, t) =>
                                   v.ToString() == "Инициировано отключение бота"),
                                   It.IsAny<Exception>(),
                                   (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()
                               ),
                               Times.Once);
        }

        private void InitEntityContext()
        {
            builder = new EntityContextBuilder();

            entityContext = builder.Create()
                                       .WithUserInformation()
                                       .WithCycle(isActive: true)
                                       .Build();
        }

        private UpdateHandler GetUpdateHandler()
        {
            entityContext.Should().NotBeNull();

            ConfigurationData configurationData = new ConfigurationData()
            {
                DB = new DbSettings() { Database = "test" },
                Bot = new BotSettings() { Token = "1" }
            };

            CoreTools coreTools = new CoreTools()
            {
                ConfigurationData = configurationData,
                Db = entityContext ?? throw new InvalidOperationException($"entityContext was not initialized"),
                ContextKeeper = contextKeeperMock.Object,
                BotResponseSender = botResponseSenderMock.Object,
                LoggerFactory = customLoggerFactoryMock.Object,
                AppCTS = cts,
            };

            UpdateHandler updateHandler = new UpdateHandler(coreTools, new RepositoriesStore(entityContext));

            return updateHandler;
        }

        private Update GetUpdate(int userID, string firstName, string userName, string updateType, string? subUpdateType, bool? checkingComplianceCallBackId, string callBackID)
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

            if (updateType == "EditedMessageQuery")
            {
                update.EditedMessage = new Message()
                {
                    Chat = chat,
                    Text = "/start",
                    From = user,
                };
            }
            else if (updateType == "MessageQuery")
            {
                update.Message = new Message()
                {
                    Chat = chat,
                    Text = "/start",
                    From = user,
                };
            }
            else
            {
                update.CallbackQuery = new CallbackQuery()
                {
                    Message = new Message()
                    {
                        Chat = chat
                    },
                    From = user,
                };

                if (subUpdateType == "None")
                {
                    update.CallbackQuery.Data = $"0|ToMain";
                }
                else if (subUpdateType == "Workout")
                {
                    update.CallbackQuery.Data = $"1|Workout";
                }
                else if (subUpdateType == "Settings")
                {
                    update.CallbackQuery.Data = $"2|Settings";
                }
                else if (subUpdateType == "Admin")
                {
                    update.CallbackQuery.Data = $"3|Admin";
                }

                if (checkingComplianceCallBackId.HasValue && checkingComplianceCallBackId.Value)
                    update.CallbackQuery.Data += $"|{callBackID}";
            }    

            return update;
        }

        private UserContext GetUserContext()
        {
            entityContext.Should().NotBeNull();

            DTOUserInformation localDTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();
            UserContext localUserContext = new UserContext(localDTOCurrentUser, Roles.User | Roles.Admin, false);
            localUserContext.GenerateNewCallBackId();
            
            return localUserContext;
        }

        public void Dispose()
        {
            builder?.Dispose();
            cts?.Dispose();
        }
    }
}