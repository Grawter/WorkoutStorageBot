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
    public class CommonCHTests : IDisposable
    {
        private readonly EntityContextBuilder builder;

        private readonly EntityContext entityContext;

        private  Mock<IContextKeeper> contextKeeperMock;

        private readonly Mock<IBotResponseSender> botResponseSenderMock;

        private Mock<ICustomLoggerFactory> customLoggerFactoryMock;

        private readonly Mock<ILogger> loggerMock;

        private readonly CommandHandlerTools commandHandlerTools;

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
                AppCTS = new CancellationTokenSource(),
            };

            DTOUserInformation DTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();

            UserContext userContext = new UserContext(DTOCurrentUser);
            userContext.DataManager.CreateAndSetCurrentCycle("testCycle", true, DTOCurrentUser);

            commandHandlerTools = new CommandHandlerTools()
            {
                ParentHandler = new PrimaryUpdateHandler(coreTools, new RepositoriesStore(entityContext)),
                CurrentUserContext = userContext,
            };
        }

        [Fact]
        public async Task GetInformationSet_WithMainSubDirection_ShouldReturnExpectedIInformationSet()
        {
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|ToMain|DomainType|CallBackId");

            CommonCH commonCH = new CommonCH(commandHandlerTools, callbackQueryParser);

            IInformationSet informationSet = await commonCH.GetInformationSet();

            informationSet.Message.Should().Be("Выберите интересующий раздел");
            informationSet.ButtonsSets.Should().Be((ButtonsSet.Main, ButtonsSet.None));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithBackSubDirection_ShouldReturnExpectedIInformationSet()
        {
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|Back||DaysListWithLastWorkout|CallBackId");

            CommonCH commonCH = new CommonCH(commandHandlerTools, callbackQueryParser);

            IInformationSet informationSet = await commonCH.GetInformationSet();

            informationSet.Message.Should().Be("Выберите тренировочный день из цикла \"<b>testCycle</b>\"");
            informationSet.ButtonsSets.Should().Be((ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithUnknownSubDirection_ShouldThrowNotImplementedException()
        {
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|SomeUnknownSubDirection|DomainType|CallBackId");

            CommonCH commonCH = new CommonCH(commandHandlerTools, callbackQueryParser);

            Func<Task> task = async () => await commonCH.GetInformationSet();

            await task.Should().ThrowAsync<NotImplementedException>()
                .WithMessage("Неожиданный callbackQueryParser.SubDirection: SomeUnknownSubDirection");
        }

        public void Dispose()
        {
            builder.Dispose();
        }
    }
}