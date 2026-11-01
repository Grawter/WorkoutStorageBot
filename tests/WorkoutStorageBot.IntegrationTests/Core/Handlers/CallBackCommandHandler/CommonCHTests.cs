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
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler.Context;
using WorkoutStorageBot.BusinessLogic.Helpers.CallbackQueryParser;
using WorkoutStorageBot.Core.Extensions;
using WorkoutStorageBot.Core.Logging;
using WorkoutStorageBot.Core.Repositories.Store;
using WorkoutStorageBot.Core.Sender;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.HandlerData.Results;
using WorkoutStorageBot.Model.DTO.HandlerData.Results.UpdateInfo;
using WorkoutStorageBot.Model.DTO.InformationSetForSend;
using WorkoutStorageBot.UnitTests.Helpers;

namespace WorkoutStorageBot.IntegrationTests.Core.Handlers.CallBackCommandHandler
{
    public class CommonCHTests : IDisposable
    {
        private readonly EntityContextBuilder builder;

        private readonly EntityContext entityContext;

        private readonly Mock<IContextKeeper> contextKeeperMock;

        private readonly Mock<IBotResponseSender> botResponseSenderMock;

        private readonly Mock<ICustomLoggerFactory> customLoggerFactoryMock;

        private readonly Mock<ILogger> loggerMock;

        private readonly CommandHandlerTools commandHandlerTools;

        private UserContext CurrentUserContext => commandHandlerTools.HandlerResult.CurrentUserContext;
        private IInformationSet InformationSet => commandHandlerTools.HandlerResult.InformationSet.ThrowIfNull();

        public CommonCHTests() 
        {
            builder = new EntityContextBuilder();
            entityContext = builder.Create()
                                   .WithUserInformation()
                                   .Build();

            contextKeeperMock = new();
            botResponseSenderMock = new();
            customLoggerFactoryMock = new();
            loggerMock = new();

            customLoggerFactoryMock.Setup(x => x.CreateLogger<It.IsAnyType>()).Returns(loggerMock.Object);

            CoreTools coreTools = new CoreTools()
            {
                ConfigurationData = new ConfigurationData(),
                Db = entityContext,
                ContextKeeper = contextKeeperMock.Object,
                BotResponseSender = botResponseSenderMock.Object,
                LoggerFactory = customLoggerFactoryMock.Object,
            };

            DTOUserInformation DTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();

            UserContext userContext = new UserContext(DTOCurrentUser);
            userContext.DataManager.CreateAndSetCurrentCycle("testCycle", true, DTOCurrentUser);

            Update update = new Update();

            commandHandlerTools = new CommandHandlerTools()
            {
                CoreTools = coreTools,
                HandlerResult = new UpdateHandlerResult()
                {
                    CurrentUserContext = userContext,
                    HasAccess = true,
                    Update = update,
                    ShortUpdateInfo = new ShortUpdateInfo(update, new User(), 1, "...", UpdateType.CallbackQuery, true)
                },
                RepositoriesStore = new RepositoriesStore(entityContext),
            };
        }

        [Fact]
        public async Task GetInformationSet_WithToMainSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            string subDirection = "ToMain";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|DomainType|CallBackId");

            CommonCH commonCH = new CommonCH(commandHandlerTools, callbackQueryParser);

            // Act
            await commonCH.Process(TestContext.Current.CancellationToken);

            // Assert
            CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

            InformationSet.Message.Should().Be("Выберите интересующий раздел");
            InformationSet.ButtonsSets.Should().Be((ButtonsSet.Main, ButtonsSet.None));
            InformationSet.ParseMode.Should().Be(ParseMode.Html);
            InformationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithBackSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            string subDirection = "Back";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}||DaysListWithLastWorkout|CallBackId");

            CommonCH commonCH = new CommonCH(commandHandlerTools, callbackQueryParser);

            // Act
            await commonCH.Process(TestContext.Current.CancellationToken);

            // Assert
            CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

            InformationSet.Message.Should().Be("Выберите тренировочный день из цикла \"<b>testCycle</b>\"");
            InformationSet.ButtonsSets.Should().Be((ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main));
            InformationSet.ParseMode.Should().Be(ParseMode.Html);
            InformationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithUnknownSubDirection_ShouldThrowNotImplementedException()
        {
            // Arrange
            string subDirection = "SomeUnknownSubDirection";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|DomainType|CallBackId");

            CommonCH commonCH = new CommonCH(commandHandlerTools, callbackQueryParser);

            // Act
            Func<Task> task = async () => await commonCH.Process(TestContext.Current.CancellationToken);

            // Assert
            await task.Should().ThrowAsync<NotImplementedException>()
                .WithMessage("Неожиданный callbackQueryParser.SubDirection: SomeUnknownSubDirection");
        }

        public void Dispose()
        {
            builder.Dispose();
        }
    }
}