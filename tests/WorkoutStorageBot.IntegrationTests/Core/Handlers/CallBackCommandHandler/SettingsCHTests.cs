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
using WorkoutStorageBot.Model.Interfaces;
using WorkoutStorageBot.UnitTests.Helpers;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.IntegrationTests.Core.Handlers.CallBackCommandHandler
{
    public class SettingsCHTests : IDisposable
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

        public SettingsCHTests()
        {
            builder = new EntityContextBuilder();
            entityContext = builder.Create()
                                   .WithUserInformation()
                                   .WithCycle(isArchive: true)
                                   .WithCycle(isArchive: false, isActive: true)
                                   .WithDay(isArchive: true)
                                   .WithDay(isArchive: false)
                                   .WithExercise(isArchive: true)
                                   .WithExercise(isArchive: false)
                                   .WithExercise(isArchive: false, mode: ExercisesMods.Timer)
                                   .Build();

            contextKeeperMock = new();
            botResponseSenderMock = new();
            customLoggerFactoryMock = new();
            loggerMock = new();

            customLoggerFactoryMock.Setup(x => x.CreateLogger<It.IsAnyType>()).Returns(loggerMock.Object);

            CoreTools coreTools = new CoreTools()
            {
                ConfigurationData = new ConfigurationData() { DB = new DbSettings() { Database = "test" } },
                Db = entityContext,
                ContextKeeper = contextKeeperMock.Object,
                BotResponseSender = botResponseSenderMock.Object,
                LoggerFactory = customLoggerFactoryMock.Object,
            };

            DTOUserInformation DTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();

            UserContext userContext = new UserContext(DTOCurrentUser, Roles.User | Roles.Admin, false);
            DTOCycle DTOCycle = builder.TestCycle.ToDTOCycle(DTOCurrentUser);
            userContext.DataManager.SetCurrentDomain(DTOCycle);
            DTODay DTODay = builder.TestDay.ToDTODay(DTOCycle);
            userContext.DataManager.SetCurrentDomain(DTODay);
            DTOExercise DTOExercise = builder.TestExercise.ToDTOExercise(DTODay);
            userContext.DataManager.SetCurrentDomain(DTOExercise);

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
        public async Task GetInformationSet_WithSettingsSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            string subDirection = "Settings";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|DomainType|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            // Act
            await settingsCH.Process(TestContext.Current.CancellationToken);

            // Assert
            CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

            InformationSet.Message.Should().Be("Выберите интересующие настройки");
            InformationSet.ButtonsSets.Should().Be((ButtonsSet.Settings, ButtonsSet.Main));
            InformationSet.ParseMode.Should().Be(ParseMode.Html);
            InformationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithArchiveStoreSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            string subDirection = "ArchiveStore";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|DomainType|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            // Act
            await settingsCH.Process(TestContext.Current.CancellationToken);

            // Assert
            CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

            InformationSet.Message.Should().Be("Выберите интересующий архив для разархивирования");
            InformationSet.ButtonsSets.Should().Be((ButtonsSet.ArchiveList, ButtonsSet.Settings));
            InformationSet.ParseMode.Should().Be(ParseMode.Html);
            InformationSet.AdditionalParameters.Should().BeNull();
        }

        [Theory]
        [InlineData("Cycles")]
        [InlineData("Days")]
        [InlineData("Exercises")]
        [InlineData("Other")]
        public async Task GetInformationSet_WithArchiveSubDirection_ShouldReturnExpectedIInformationSet(string domainType)
        {
            // Arrange
            string subDirection = "Archive";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            // Assert
            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.Process(TestContext.Current.CancellationToken);
               
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.DomainType: {callbackQueryParser.DomainType}");
            }
            else
            {
                // Act
                await settingsCH.Process(TestContext.Current.CancellationToken);

                if (domainType == "Cycles")
                {
                    InformationSet.Message.Should().Be("Выберите архивный цикл для разархивирования");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.ArchiveCyclesList, ButtonsSet.ArchiveList));
                }
                else if (domainType == "Days")
                {
                    InformationSet.Message.Should().Be("Выберите архивный день для разархивирования");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.ArchiveDaysList, ButtonsSet.ArchiveList));
                }
                else if (domainType == "Exercises")
                {
                    InformationSet.Message.Should().Be("Выберите архивное упражнение для разархивирования");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.ArchiveExercisesList, ButtonsSet.ArchiveList));
                }

                // Assert
                CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                InformationSet.ParseMode.Should().Be(ParseMode.Html);
                InformationSet.AdditionalParameters.Should().BeNull();
            }
        }

        [Theory]
        [InlineData("Cycle", 1)]
        [InlineData("Cycle", 0)]
        [InlineData("Day", 1)]
        [InlineData("Day", 0)]
        [InlineData("Exercise", 1)]
        [InlineData("Exercise", 0)]
        [InlineData("Other", 0)]
        public async Task GetInformationSet_WithUnArchiveSubDirection_ShouldReturnExpectedIInformationSet(string domainType, int domainIDforUnArchive)
        {
            // Arrange
            string subDirection = "UnArchive";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|{domainType}|{domainIDforUnArchive}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            bool hasDTODomain = domainIDforUnArchive > 0;

            // Assert
            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.Process(TestContext.Current.CancellationToken);

                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.DomainType: {callbackQueryParser.DomainType}");
            }
            else
            {
                if (hasDTODomain)
                {
                    // Act
                    await settingsCH.Process(TestContext.Current.CancellationToken);

                    IDTODomain? DTODomain = null;

                    if (domainType == "Cycle")
                    {
                        DTODomain = CurrentUserContext.UserInformation.Cycles.First(x => x.Id == domainIDforUnArchive);
                    }
                    else if (domainType == "Day")
                    {
                        IEnumerable<DTODay> allDays = CurrentUserContext.UserInformation.Cycles.SelectMany(x => x.Days);

                        DTODomain = allDays.First(x => x.Id == domainIDforUnArchive);
                    }
                    else if (domainType == "Exercise")
                    {
                        IEnumerable<DTOExercise> allExercise = CurrentUserContext.UserInformation.Cycles.SelectMany(x => x.Days)
                                                                                                                            .SelectMany(x => x.Exercises);

                        DTODomain = allExercise.FirstOrDefault(x => x.Id == domainIDforUnArchive);
                    }

                    DTODomain?.IsArchive.Should().BeFalse();

                    // Assert
                    InformationSet.Message.Should().Contain("разархивирован!");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.ArchiveList, ButtonsSet.Settings));
                    CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                    CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                    commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                    InformationSet.ParseMode.Should().Be(ParseMode.Html);
                    InformationSet.AdditionalParameters.Should().BeNull();
                }
                else
                {
                    Func<Task> func = async () => await settingsCH.Process(TestContext.Current.CancellationToken);

                    ExceptionAssertions<InvalidOperationException> exceptionAssertions = await func.Should().ThrowAsync<InvalidOperationException>();
                    exceptionAssertions.WithMessage($"Not found archive {domainType.ToLower()} for unarchiving with ID = {domainIDforUnArchive}");
                }
            }
        }

        [Fact]
        public async Task GetInformationSet_WithExportSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            string subDirection = "Export";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|DomainType|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            // Act
            await settingsCH.Process(TestContext.Current.CancellationToken);

            // Assert
            CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

            InformationSet.Message.Should().Be("Выберите формат в котором экспортировать данные о ваших тренировках");
            InformationSet.ButtonsSets.Should().Be((ButtonsSet.Export, ButtonsSet.Settings));
            InformationSet.ParseMode.Should().Be(ParseMode.Html);
            InformationSet.AdditionalParameters.Should().BeNull();
        }

        [Theory]
        [InlineData("Excel")]
        [InlineData("JSON")]
        [InlineData("Other")]

        public async Task GetInformationSet_WithExportToSubDirection_ShouldReturnExpectedIInformationSet(string exportFormat)
        {
            // Arrange
            string subDirection = "ExportTo";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|DomainType|{exportFormat}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (exportFormat == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.Process(TestContext.Current.CancellationToken);

                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                exceptionAssertions.WithMessage($"Неожиданный exportFormat: {exportFormat}");
            }
            else
            {
                // Act
                await settingsCH.Process(TestContext.Current.CancellationToken);

                if (exportFormat == "Excel")
                {
                    InformationSet.Message.Should().Be($"Выберите временной промежуток формирования данных от последней тренировки для экспорта в {"Excel".AddBold()}");
                    InformationSet.AdditionalParameters.Should().BeEquivalentTo(new Dictionary<string, string>() { { "Act", "Export/Excel" } });
                }
                else if (exportFormat == "JSON")
                {
                    InformationSet.Message.Should().Be($"Выберите временной промежуток формирования данных от последней тренировки для экспорта в {"JSON".AddBold()}");
                    InformationSet.AdditionalParameters.Should().BeEquivalentTo(new Dictionary<string, string>() { { "Act", "Export/JSON" } });
                }

                // Assert
                CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                InformationSet.ButtonsSets.Should().Be((ButtonsSet.Period, ButtonsSet.Export));
                InformationSet.ParseMode.Should().Be(ParseMode.Html);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetInformationSet_WithAboutBotSubDirection_ShouldReturnExpectedIInformationSet(bool withTextAboutBot)
        {
            // Arrange
            string subDirection = "AboutBot";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|DomainType|CallBackId");

            SettingsCH? settingsCH = null;

            if (withTextAboutBot)
            {
                // поднимаем зависимости локально для теста информации о боте

                // Arrange
                CoreTools localCoreTools = new CoreTools()
                {
                    ConfigurationData = new ConfigurationData() { DB = new DbSettings() { Database = "test" }, AboutBotText = "test1" },
                    Db = entityContext,
                    ContextKeeper = contextKeeperMock.Object,
                    BotResponseSender = botResponseSenderMock.Object,
                    LoggerFactory = customLoggerFactoryMock.Object,
                };

                DTOUserInformation localDTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();

                UserContext localUserContext = new UserContext(localDTOCurrentUser);
                localUserContext.DataManager.CreateAndSetCurrentCycle("testCycle", true, localDTOCurrentUser);

                Update update = new Update();

                CommandHandlerTools localCommandHandlerTools = new CommandHandlerTools()
                {
                    CoreTools = localCoreTools,
                    HandlerResult = new UpdateHandlerResult()
                    {
                        CurrentUserContext = localUserContext,
                        HasAccess = true,
                        Update = update,
                        ShortUpdateInfo = new ShortUpdateInfo(update, new User(), 1, "...", UpdateType.CallbackQuery, true)
                    },
                    RepositoriesStore = new RepositoriesStore(entityContext),
                };

                settingsCH = new SettingsCH(localCommandHandlerTools, callbackQueryParser);

                // Act
                await settingsCH.Process(TestContext.Current.CancellationToken);

                // Assert
                localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                localCommandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                IInformationSet? informationSet = localCommandHandlerTools.HandlerResult.InformationSet;
                informationSet.Should().NotBeNull();

                informationSet.Message.Should().Be("test1");

                informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.Settings));
                informationSet.ParseMode.Should().Be(ParseMode.Html);
                informationSet.AdditionalParameters.Should().BeNull();
            }
            else
            {
                settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

                // Act
                await settingsCH.Process(TestContext.Current.CancellationToken);

                // Assert
                CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                InformationSet.Message.Should().Be("Информации о боте не указано");

                InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.Settings));
                InformationSet.ParseMode.Should().Be(ParseMode.Html);
                InformationSet.AdditionalParameters.Should().BeNull();
            }
        }

        [Theory]
        [InlineData("Cycles")]
        [InlineData("Days")]
        [InlineData("Exercises")]
        [InlineData("Other")]
        public async Task GetInformationSet_WithSettingSubDirection_ShouldReturnExpectedIInformationSet(string domainType)
        {
            // Arrange
            string subDirection = "Setting";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.Process(TestContext.Current.CancellationToken);

                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }
            else
            {
                // Act
                await settingsCH.Process(TestContext.Current.CancellationToken);

                if (domainType == "Cycles")
                {
                    InformationSet.Message.Should().Be("Выберите интересующие настройки для циклов");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingCycles, ButtonsSet.Settings));
                }
                else if (domainType == "Days")
                {
                    InformationSet.Message.Should().Be($"Выберите интересующие настройки для дней из цикла {CurrentUserContext.DataManager.CurrentCycle?.Name.AddBoldAndQuotes()}");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingDays, ButtonsSet.SettingCycle));
                }
                else if (domainType == "Exercises")
                {
                    InformationSet.Message.Should().Be($"Выберите интересующие настройки для упражнений из дня {CurrentUserContext.DataManager.CurrentDay?.Name.AddBoldAndQuotes()} ({CurrentUserContext.DataManager.CurrentCycle?.Name.AddBold()})");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingExercises, ButtonsSet.SettingDay));
                }

                // Assert
                CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                InformationSet.ParseMode.Should().Be(ParseMode.Html);
                InformationSet.AdditionalParameters.Should().BeNull();
            }
        }

        [Theory]
        [InlineData("Cycle", QueryFrom.Start)]
        [InlineData("Cycle", QueryFrom.Settings)]
        [InlineData("Cycle", QueryFrom.NoMatter)]
        [InlineData("Day", QueryFrom.Start)]
        [InlineData("Day", QueryFrom.Settings)]
        [InlineData("Day", QueryFrom.NoMatter)]
        [InlineData("Exercise", QueryFrom.Start)]
        [InlineData("Exercise", QueryFrom.Settings)]
        [InlineData("Exercise", QueryFrom.NoMatter)]
        [InlineData("Other", QueryFrom.NoMatter)]
        public async Task GetInformationSet_WithAddSubDirection_ShouldReturnExpectedIInformationSet(string domainType, QueryFrom queryFrom)
        {
            // Arrange
            string subDirection = "Add";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            CurrentUserContext.Navigation.SetQueryFrom(queryFrom);

            // Assert
            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.Process(TestContext.Current.CancellationToken);

                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.DomainType: {callbackQueryParser.DomainType}");
            }
            else
            {
                if (domainType == "Cycle")
                {
                    if (queryFrom == QueryFrom.Start || queryFrom == QueryFrom.Settings)
                    {
                        // Act
                        await settingsCH.Process(TestContext.Current.CancellationToken);

                        // Assert
                        InformationSet.Message.Should().Be("Введите название тренировочного цикла");
                        CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddCycle);

                        commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                        InformationSet.ParseMode.Should().Be(ParseMode.Html);
                        InformationSet.AdditionalParameters.Should().BeNull();

                        if (queryFrom == QueryFrom.Start)
                        {
                            InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));
                        }
                        else
                        {
                            InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingCycles));
                        }
                    }
                }
                else if (domainType == "Day")
                {
                    if (queryFrom == QueryFrom.Start || queryFrom == QueryFrom.Settings)
                    {
                        // Act
                        await settingsCH.Process(TestContext.Current.CancellationToken);

                        // Assert
                        InformationSet.Message.Should().Be($"Введите название тренирочного дня для цикла {CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}");
                        CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddDays);

                        commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                        InformationSet.ParseMode.Should().Be(ParseMode.Html);
                        InformationSet.AdditionalParameters.Should().BeNull();

                        if (queryFrom == QueryFrom.Start)
                        {
                            InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));
                        }
                        else if (queryFrom == QueryFrom.Settings)
                        {
                            InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingDays));
                        }
                    }
                }
                else if (domainType == "Exercise")
                {
                    if (queryFrom == QueryFrom.Start || queryFrom == QueryFrom.Settings)
                    {
                        // Act
                        await settingsCH.Process(TestContext.Current.CancellationToken);

                        // Assert
                        InformationSet.Message.Should().Be(@$"Введите название(я) и тип(ы) упражнения(й) для дня {CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()}
======================
Доступные типы упражений:
<b>0</b> - только кол-во повторений (например, подтягивания)
<b>1</b> - вес и кол-во повторений (например, жим лёжа)
<b>2</b> - таймер (например, бег)
<b>3</b> - свободный формат результата (например, отработка на груше)

(Тип упражнения всегда можно поменять в <b>настройках</b>)
======================

Формат общего ввода: [название]-[тип]. 
Пример единичного ввода: Жим лёжа-0
Пример множественного ввода: Жим лёжа-0;Становая тяга-0;Прыжки на скакалке-2;...
");
                        CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddExercises);

                        commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                        InformationSet.ParseMode.Should().Be(ParseMode.Html);
                        InformationSet.AdditionalParameters.Should().BeNull();

                        if (queryFrom == QueryFrom.Start)
                        {
                            InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));
                        }
                        else if (queryFrom == QueryFrom.Settings)
                        {
                            InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingExercises));
                        }
                    }
                }
                else
                {
                    Func<Task> func = async () => await settingsCH.Process(TestContext.Current.CancellationToken);

                    ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                    exceptionAssertions.WithMessage($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {queryFrom}");
                }
            }
        }


        [Theory]
        [InlineData("Other")]
        [InlineData("Exercise")]
        public async Task GetInformationSet_WithResetTempDomainsSubDirection_ShouldReturnExpectedIInformationSet(string domainType)
        {
            // Arrange
            string subDirection = "ResetTempDomains";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            List<DTOExercise> dTOExercises = new List<DTOExercise>() { new DTOExercise() { Mode = ExercisesMods.Count, Name = "TestDTOExercise01" } };
            CurrentUserContext.DataManager.TryAddTempExercises(dTOExercises, out string? error);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.Process(TestContext.Current.CancellationToken);

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.DomainType: {domainType}");
            }
            else
            {
                // Act
                await settingsCH.Process(TestContext.Current.CancellationToken);

                // Assert
                CurrentUserContext.DataManager.TempExercises.Should().BeNull();

                CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                InformationSet.Message.Should().Be(@"Упражнения для сохранения сброшены!
======================

Выберите интересующую настройку
");
                InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingExercises, ButtonsSet.None));
                InformationSet.ParseMode.Should().Be(ParseMode.Html);
                InformationSet.AdditionalParameters.Should().BeNull();
            }
        }

        [Theory]
        [InlineData(QueryFrom.NoMatter)]
        [InlineData(QueryFrom.Start)]
        [InlineData(QueryFrom.Settings)]
        public async Task GetInformationSet_WithSaveExercisesSubDirection_ShouldReturnExpectedIInformationSet(QueryFrom queryFrom)
        {
            // Arrange
            string subDirection = "SaveExercises";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|domainType|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            CurrentUserContext.Navigation.SetQueryFrom(queryFrom);
            List<DTOExercise> dTOExercises = new List<DTOExercise>() { new DTOExercise() { Mode = ExercisesMods.Count, Name = "TestDTOExercise01" } };
            CurrentUserContext.DataManager.TryAddTempExercises(dTOExercises, out string? error);

            if (queryFrom == QueryFrom.NoMatter)
            {
                // Act
                Func<Task> func = async () => await settingsCH.Process(TestContext.Current.CancellationToken);

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                exceptionAssertions.WithMessage($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {queryFrom}");
            }
            else
            {
                // Act
                await settingsCH.Process(TestContext.Current.CancellationToken);

                if (queryFrom == QueryFrom.Start)
                {
                    // Assert
                    CurrentUserContext.DataManager.TempExercises.Should().BeNull();

                    CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.Start);

                    InformationSet.Message.Should().Be(@"Упражнения сохранены!
======================

Выберите дальнейшее действие
");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.RedirectAfterSaveExercise, ButtonsSet.None));

                }
                else if (queryFrom == QueryFrom.Settings)
                {
                    // Assert
                    CurrentUserContext.DataManager.TempExercises.Should().BeNull();

                    CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);

                    InformationSet.Message.Should().Be($@"Упражнения сохранены!
======================

Выберите интересующие настройки для упражнений из дня ""<b>{CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name}</b>"" (<b>{CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name}</b>)
");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingExercises, ButtonsSet.SettingDays));
                }
                
                CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                InformationSet.ParseMode.Should().Be(ParseMode.Html);
                InformationSet.AdditionalParameters.Should().BeNull();
            }
        }

        [Theory]
        [InlineData("Cycles")]
        [InlineData("Days")]
        [InlineData("Exercises")]
        [InlineData("Other")]
        public async Task GetInformationSet_WithSettingExistingSubDirection_ShouldReturnExpectedIInformationSet(string domainType)
        {
            // Arrange
            string subDirection = "SettingExisting";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.Process(TestContext.Current.CancellationToken);

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.DomainType: {domainType}");
            }
            else
            {
                // Act
                await settingsCH.Process(TestContext.Current.CancellationToken);

                if (domainType == "Cycles")
                {
                    // Assert
                    InformationSet.Message.Should().Be("Выберите интересующий цикл");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.CycleList, ButtonsSet.SettingCycles));

                }
                else if (domainType == "Days")
                {
                    // Assert
                    InformationSet.Message.Should().Be($"Выберите интересующий день из цикла {CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.DaysList, ButtonsSet.SettingDays));
                }
                else if (domainType == "Exercises")
                {
                    // Assert
                    InformationSet.Message.Should().Be($"Выберите интересующее упражнение из дня {CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()} ({CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBold()})");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesList, ButtonsSet.SettingExercises));
                }

                CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                InformationSet.ParseMode.Should().Be(ParseMode.Html);
                InformationSet.AdditionalParameters.Should().BeNull();
            }
        }

        [Theory]
        [InlineData("Other", QueryFrom.NoMatter, 0)]
        [InlineData("Cycle", QueryFrom.NoMatter, 1)]
        [InlineData("Day", QueryFrom.NoMatter, 1)]
        [InlineData("Day", QueryFrom.Settings, 1)]
        [InlineData("Exercise", QueryFrom.NoMatter, 1)]
        [InlineData("Exercise", QueryFrom.Settings, 2)]
        [InlineData("Exercise", QueryFrom.Settings, 1)]
        public async Task GetInformationSet_WithSelectedSubDirection_ShouldReturnExpectedIInformationSet(string domainType, QueryFrom queryFrom, int domainID)
        {
            // Arrange
            string subDirection = "Selected";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|{domainType}|{domainID}|CallBackId");

            CurrentUserContext.Navigation.SetQueryFrom(queryFrom);

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Id.Should().Be(2);
            CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Id.Should().Be(2);

            if (domainType == "Other" || ((domainType == "Day" || domainType == "Exercise") && queryFrom == QueryFrom.Start))
            {
                // Act
                Func<Task> func = async () => await settingsCH.Process(TestContext.Current.CancellationToken);

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                
                if (domainType == "Other")
                    exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.DomainType: {domainType}");

                if (domainType == "Day" || domainType == "Exercise")
                    exceptionAssertions.WithMessage($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {CurrentUserContext.Navigation.QueryFrom}");
            }
            else
            {
                // Act
                await settingsCH.Process(TestContext.Current.CancellationToken);

                if (domainType == "Cycle")
                {
                    // Assert
                    CurrentUserContext.DataManager.CurrentCycle.Id.Should().Be(domainID);

                    InformationSet.Message.Should().Be($"Выберите интересующую настройку для цикла {CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingCycle, ButtonsSet.CycleList));
                }
                else if (domainType == "Day")
                {
                    // Assert
                    CurrentUserContext.DataManager.CurrentDay.Id.Should().Be(domainID);

                    switch (CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.NoMatter:

                            InformationSet.Message.Should().Be($"Выберите упражнение из дня {CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()} ({CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBold()})");
                            InformationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout));

                            CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                            break;

                        case QueryFrom.Settings:

                            InformationSet.Message.Should().Be($"Выберите интересующую настройку для дня {CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()} ({CurrentUserContext.DataManager.CurrentCycle.Name.AddBold()})");
                            InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingDay, ButtonsSet.DaysList));

                            CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
                            break;

                    }

                    CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                }
                else if (domainType == "Exercise")
                {
                    // Assert
                    DTOExercise currentExercise = CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull();

                    currentExercise.Id.Should().Be(domainID);

                    string currentExerciseNameBoldAndQuotes = currentExercise.Name.AddBoldAndQuotes();

                    bool isNeedCheckMessageNavigationTarget = true;

                    switch (CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.NoMatter:

                            if (currentExercise.Mode == ExercisesMods.Timer)
                            {
                                InformationSet.Message.Should().Be($"Включение таймера для упражнения {currentExerciseNameBoldAndQuotes}");
                                InformationSet.ButtonsSets.Should().Be((ButtonsSet.EnableExerciseTimer, ButtonsSet.ExercisesListWithLastWorkoutForDay));

                                CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
                            }
                            else
                            {
                                InformationSet.Message.Should().Be(@$"Фиксирование результатов упражнения {currentExerciseNameBoldAndQuotes}
======================
Формат общего ввода: [кол-во повторений].
Пример единичного ввода: 25
Пример множественного ввода: 25 10 5
======================

Введите результат(ы) подхода(ов)
");
                                InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay));

                                CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                                CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddResultForExercise);

                                isNeedCheckMessageNavigationTarget = false;
                            }

                            break;

                        case QueryFrom.Settings:

                            InformationSet.Message.Should().Be($"Выберите интересующую настройку для упражнения {currentExerciseNameBoldAndQuotes} ({CurrentUserContext.DataManager.CurrentDay.Name.AddBold()}-{CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBold()})");
                            InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingExercise, ButtonsSet.ExercisesList));

                            CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
                            break;

                    }

                    if (isNeedCheckMessageNavigationTarget)
                        CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);
                }

                commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                InformationSet.ParseMode.Should().Be(ParseMode.Html);
                InformationSet.AdditionalParameters.Should().BeNull();
            }
        }

        [Theory]
        [InlineData("Other", false)]
        [InlineData("Cycle", true)]
        [InlineData("Cycle", false)]
        public async Task GetInformationSet_WithChangeActiveSubDirection_ShouldReturnExpectedIInformationSet(string domainType, bool currentCycleIsActive)
        {
            // Arrange
            string subDirection = "ChangeActive";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.Process(TestContext.Current.CancellationToken);

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();

                exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.DomainType: {domainType}");
            }
            else
            {
                if (currentCycleIsActive)
                {
                    DTOCycle currentActiveCycle = CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull();
                    currentActiveCycle.IsActive.Should().BeTrue();

                    // Act
                    await settingsCH.Process(TestContext.Current.CancellationToken);

                    // Assert
                    currentActiveCycle.IsActive.Should().BeTrue();

                    InformationSet.Message.Should().Be(@$"Выбранный цикл {CurrentUserContext.ActiveCycle.ThrowIfNull().Name.AddBoldAndQuotes()} уже является активным!
======================

Выберите интересующую настройку для цикла {CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}
");
                }
                else
                {
                    DTOCycle currentActiveCycle = CurrentUserContext.ActiveCycle.ThrowIfNull();
                    currentActiveCycle.IsActive.Should().BeTrue();

                    DTOCycle newSelectedCycle = entityContext.Cycles.First().ToDTOCycle();
                    newSelectedCycle.IsActive.Should().BeFalse();

                    CurrentUserContext.DataManager.SetCurrentDomain(newSelectedCycle);

                    // Act
                    await settingsCH.Process(TestContext.Current.CancellationToken);

                    // Assert
                    currentActiveCycle.IsActive.Should().BeFalse();
                    newSelectedCycle.IsActive.Should().BeTrue();

                    InformationSet.Message.Should().Be(@$"Активный цикл изменён на {CurrentUserContext.ActiveCycle.Name.AddBoldAndQuotes()}
======================

Выберите интересующую настройку для цикла {CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}
");
                }

                InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingCycle, ButtonsSet.SettingCycles));

                CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                InformationSet.ParseMode.Should().Be(ParseMode.Html);
                InformationSet.AdditionalParameters.Should().BeNull();
            }
        }

        [Theory]
        [InlineData("Other", false)]
        [InlineData("Cycle", true)]
        [InlineData("Cycle", false)]
        [InlineData("Day", false)]
        [InlineData("Exercise", false)]
        public async Task GetInformationSet_WithArchivingSubDirection_ShouldReturnExpectedIInformationSet(string domainType, bool currentCycleIsActive)
        {
            // Arrange
            string subDirection = "Archiving";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.Process(TestContext.Current.CancellationToken);

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();

                exceptionAssertions.WithMessage($"Неожиданный domainTyped: {domainType}");
            }
            else
            {
                if (domainType == "Cycle")
                {
                    if (currentCycleIsActive)
                    {
                        DTOCycle currentActiveCycle = CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull();
                        currentActiveCycle.IsActive.Should().BeTrue();

                        // Act
                        await settingsCH.Process(TestContext.Current.CancellationToken);

                        // Assert
                        currentActiveCycle.IsActive.Should().BeTrue();

                        currentActiveCycle.IsArchive.Should().BeFalse();

                        InformationSet.Message.Should().Be(@$"Ошибка при архивации!
======================
Нельзя архивировать цикл {currentActiveCycle.Name.AddBoldAndQuotes()}, т.к. он является активным!
======================

Выберите интересующую настройку для цикла {currentActiveCycle.Name.AddBoldAndQuotes()}
");
                        InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingCycle, ButtonsSet.CycleList));
                    }
                    else
                    {
                        DTOCycle currentActiveCycle = CurrentUserContext.ActiveCycle.ThrowIfNull();
                        currentActiveCycle.IsActive.Should().BeTrue();

                        DTOCycle newSelectedCycle = CurrentUserContext.UserInformation.Cycles.First();
                        newSelectedCycle.IsActive.Should().BeFalse();

                        CurrentUserContext.DataManager.SetCurrentDomain(newSelectedCycle);

                        // Act
                        await settingsCH.Process(TestContext.Current.CancellationToken);

                        newSelectedCycle.IsArchive.Should().BeTrue();

                        InformationSet.Message.Should().Be(@$"Цикл {newSelectedCycle.Name.AddBoldAndQuotes()} был добавлен в архив
======================

Выберите интересующий цикл
");

                        InformationSet.ButtonsSets.Should().Be((ButtonsSet.CycleList, ButtonsSet.SettingCycles));
                    }
                }
                else if (domainType == "Day")
                {
                    DTODay currentDay = CurrentUserContext.DataManager.CurrentDay.ThrowIfNull();

                    // Act
                    await settingsCH.Process(TestContext.Current.CancellationToken);

                    // Assert
                    currentDay.IsArchive.Should().BeTrue();

                    InformationSet.Message.Should().Be(@$"День {currentDay.Name.AddBoldAndQuotes()} был добавлен в архив
======================

Выберите интересующий день из цикла {CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}
");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.DaysList, ButtonsSet.SettingDays));
                }
                else
                {
                    DTOExercise currentExercise = CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull();

                    // Act
                    await settingsCH.Process(TestContext.Current.CancellationToken);

                    // Assert
                    currentExercise.IsArchive.Should().BeTrue();

                    InformationSet.Message.Should().Be(@$"Упражнение {currentExercise.Name.AddBoldAndQuotes()} было добавлено в архив
======================

Выберите интересующее упражнение из дня {CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()} ({CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBold()})
");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesList, ButtonsSet.SettingExercises));
                }

                CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                InformationSet.ParseMode.Should().Be(ParseMode.Html);
                InformationSet.AdditionalParameters.Should().BeNull();
            }
        }

        [Theory]
        [InlineData("Other")]
        [InlineData("Day")]
        [InlineData("Exercise")]
        public async Task GetInformationSet_WithReplaceSubDirection_ShouldReturnExpectedIInformationSet(string domainType)
        {
            // Arrange
            string subDirection = "Replace";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.Process(TestContext.Current.CancellationToken);

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();

                exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.DomainType: {domainType}");
            }
            else
            {
                // Act
                await settingsCH.Process(TestContext.Current.CancellationToken);

                if (domainType == "Day")
                {
                    // Assert
                    InformationSet.Message.Should().Be(@$"Выберите цикл, в который хотите перенести день {CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.ReplaceToCycle, ButtonsSet.SettingDay));
                }
                else
                {
                    // Assert
                    InformationSet.Message.Should().Be(@$"Выберите день, в который хотите перенести упражнение {CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.ReplaceToDay, ButtonsSet.SettingExercise));
                }

                CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                InformationSet.ParseMode.Should().Be(ParseMode.Html);
                InformationSet.AdditionalParameters.Should().BeNull();
            }
        }

        [Theory]
        [InlineData("Other", 0)]
        [InlineData("Cycle", 1)]
        [InlineData("Cycle", 2)]
        [InlineData("Day", 1)]
        [InlineData("Day", 2)]
        public async Task GetInformationSet_WithReplaceToSubDirection_ShouldReturnExpectedIInformationSet(string domainType, int targetDomainID)
        {
            // Arrange
            string subDirection = "ReplaceTo";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|{domainType}|{targetDomainID}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.Process(TestContext.Current.CancellationToken);

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();

                exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.DomainType: {domainType}");
            }
            else
            {
                if (domainType == "Cycle")
                {
                    if (CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Id == targetDomainID)
                    {
                        // Act
                        await settingsCH.Process(TestContext.Current.CancellationToken);

                        // Assert
                        InformationSet.Message.Should().Be(@$"Ошибка при переносе дня!
======================
Нельзя перенести день в тот же самый цикл
======================

Выберите цикл, в который хотите перенести день {CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()}
");
                        InformationSet.ButtonsSets.Should().Be((ButtonsSet.ReplaceToCycle, ButtonsSet.SettingDay));
                    }
                    else
                    {
                        DTODay dayForReplacement = CurrentUserContext.DataManager.CurrentDay.ThrowIfNull();

                        DTOCycle destinationCycle = CurrentUserContext.UserInformation.Cycles.First(x => x.Id == targetDomainID);

                        destinationCycle.Days.Should().BeEmpty();
                        CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Days.Should().Contain(x => x.Id == dayForReplacement.Id && x.Name == dayForReplacement.Name);

                        // Act
                        await settingsCH.Process(TestContext.Current.CancellationToken);

                        destinationCycle.Days.Should().Contain(dayForReplacement);
                        CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Days.Should().NotContain(x => x.Id == dayForReplacement.Id && x.Name == dayForReplacement.Name);

                        // Assert
                        InformationSet.Message.Should().Be(@$"День {dayForReplacement.Name.AddBoldAndQuotes()}, перенесён в цикл {destinationCycle.Name.AddBoldAndQuotes()}
======================

Выберите интересующий цикл
");
                        InformationSet.ButtonsSets.Should().Be((ButtonsSet.CycleList, ButtonsSet.SettingCycles));
                    }
                }
                else
                {
                    if (CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Id == targetDomainID)
                    {
                        // Act
                        await settingsCH.Process(TestContext.Current.CancellationToken);

                        // Assert
                        InformationSet.Message.Should().Be(@$"Ошибка при переносе упражнения!
======================
Нельзя перенести упражнение в тот же самый день
======================

Выберите день, в который хотите перенести упражнение {CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().Name.AddBoldAndQuotes()}
");
                        InformationSet.ButtonsSets.Should().Be((ButtonsSet.ReplaceToDay, ButtonsSet.SettingExercise));
                    }
                    else 
                    {
                        DTOExercise exerciseForReplacement = CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull();

                        DTODay destinationDay = CurrentUserContext.UserInformation.Cycles.SelectMany(x => x.Days).First(x => x.Id == targetDomainID);

                        destinationDay.Exercises.Should().BeEmpty();
                        CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Exercises.Should().Contain(x => x.Id == exerciseForReplacement.Id && x.Name == exerciseForReplacement.Name);

                        // Act
                        await settingsCH.Process(TestContext.Current.CancellationToken);

                        destinationDay.Exercises.Should().Contain(exerciseForReplacement);
                        CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Exercises.Should().NotContain(x => x.Id == exerciseForReplacement.Id && x.Name == exerciseForReplacement.Name);

                        // Assert
                        InformationSet.Message.Should().Be(@$"Упражнение {exerciseForReplacement.Name.AddBoldAndQuotes()}, перенесёно в день {destinationDay.Name.AddBoldAndQuotes()}
======================

Выберите интересующий цикл
");
                        InformationSet.ButtonsSets.Should().Be((ButtonsSet.CycleList, ButtonsSet.SettingCycles));
                    }
                }

                CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                InformationSet.ParseMode.Should().Be(ParseMode.Html);
                InformationSet.AdditionalParameters.Should().BeNull();
            }
        }

        [Theory]
        [InlineData("Other")]
        [InlineData("Cycle")]
        [InlineData("Day")]
        [InlineData("Exercise")]
        public async Task GetInformationSet_WithChangeNameSubDirection_ShouldReturnExpectedIInformationSet(string domainType)
        {
            // Arrange
            string subDirection = "ChangeName";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.Process(TestContext.Current.CancellationToken);

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();

                exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.DomainType: {domainType}");
            }
            else
            {
                // Act
                await settingsCH.Process(TestContext.Current.CancellationToken);

                if (domainType == "Cycle")
                {
                    // Assert
                    InformationSet.Message.Should().Be(@$"Введите новоё название для цикла {CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingCycle));

                    CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.ChangeNameCycle);
                }
                else if (domainType == "Day")
                {
                    // Assert
                    InformationSet.Message.Should().Be(@$"Введите новоё название для дня {CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingDay));

                    CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.ChangeNameDay);
                }
                else
                {
                    // Assert
                    InformationSet.Message.Should().Be(@$"Введите новоё название для упражнения {CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingExercise));

                    CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.ChangeNameExercise);
                }

                CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);

                commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                InformationSet.ParseMode.Should().Be(ParseMode.Html);
                InformationSet.AdditionalParameters.Should().BeNull();
            }
        }

        [Theory]
        [InlineData("Other")]
        [InlineData("Exercise")]
        public async Task GetInformationSet_WithChangeModeSubDirection_ShouldReturnExpectedIInformationSet(string domainType)
        {
            // Arrange
            string subDirection = "ChangeMode";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.Process(TestContext.Current.CancellationToken);

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();

                exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.DomainType: {domainType}");
            }
            else
            {
                // Act
                await settingsCH.Process(TestContext.Current.CancellationToken);

                // Assert
                InformationSet.Message.Should().Be(@$"Выберите новый тип для упражнения {CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().Name.AddBoldAndQuotes()}");
                InformationSet.ButtonsSets.Should().Be((ButtonsSet.ChangeType, ButtonsSet.SettingExercise));

                CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                InformationSet.ParseMode.Should().Be(ParseMode.Html);
                InformationSet.AdditionalParameters.Should().BeNull();
            }
        }

        [Theory]
        [InlineData("Other", ExercisesMods.Count)]
        [InlineData("Exercise", ExercisesMods.FreeResult)]
        public async Task GetInformationSet_WithChangedModeSubDirection_ShouldReturnExpectedIInformationSet(string domainType, ExercisesMods newExercisesMods)
        {
            // Arrange
            string subDirection = "ChangedMode";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|{domainType}|{(int)newExercisesMods}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.Process(TestContext.Current.CancellationToken);

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();

                exceptionAssertions.WithMessage($"Неожиданный domainTyped: {domainType}");
            }
            else
            {
                DTOExercise currentExercise = CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull();
                currentExercise.Mode.Should().Be(ExercisesMods.Timer);

                // Act
                await settingsCH.Process(TestContext.Current.CancellationToken);

                // Assert
                currentExercise.Mode.Should().Be(newExercisesMods);

                InformationSet.Message.Should().Be(@$"Режим для упражнения {currentExercise.Name.AddBoldAndQuotes()} изменён на {newExercisesMods.ToString().AddBoldAndQuotes()}
======================

Выберите интересующую настройку для упражнения {currentExercise.Name.AddBoldAndQuotes()}
");
                InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingExercise, ButtonsSet.ExercisesList));

                CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                InformationSet.ParseMode.Should().Be(ParseMode.Html);
                InformationSet.AdditionalParameters.Should().BeNull();
            }
        }

        [Theory]
        [InlineData("Other", false)]
        [InlineData("Export/Excel", false)]
        [InlineData("Export/Excel", true)]
        [InlineData("Export/JSON", false)]
        [InlineData("Export/JSON", true)]
        public async Task GetInformationSet_WithPeriodSubDirection_ShouldReturnExpectedIInformationSet(string operation, bool withResultSExercise)
        {
            // Arrange
            string subDirection = "Period";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}||{operation}|0|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (operation == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.Process(TestContext.Current.CancellationToken);

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();

                exceptionAssertions.WithMessage($"Неожиданный operation: {operation}");
            }
            else
            {
                if (withResultSExercise)
                {
                    builder.WithResultExercise();

                    // Act
                    await settingsCH.Process(TestContext.Current.CancellationToken);

                    InformationSet.Message.Should().Be(@$"Тренировки успешно экспортированы!");
                }
                else
                {
                    // Act
                    await settingsCH.Process(TestContext.Current.CancellationToken);

                    InformationSet.Message.Should().Be(@$"Отсутствуют результаты для экспорта
======================

Выберите интересующую настройку
");
                }

                // Assert
                InformationSet.ButtonsSets.Should().Be((ButtonsSet.Settings, ButtonsSet.Main));

                CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                InformationSet.ParseMode.Should().Be(ParseMode.Html);
                InformationSet.AdditionalParameters.Should().BeNull();

                if (InformationSet is FileInformationSet fileInformationSet)
                {
                    fileInformationSet.Stream.Length.Should().NotBe(0);
                    fileInformationSet.Stream.Position.Should().Be(0);
                }
            }
        }

        [Theory]
        [InlineData("Other")]
        [InlineData("Account")]
        [InlineData("Cycle")]
        [InlineData("Day")]
        [InlineData("Exercise")]
        public async Task GetInformationSet_WithDeleteSubDirection_ShouldReturnExpectedIInformationSet(string domainType)
        {
            // Arrange
            string subDirection = "Delete";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.Process(TestContext.Current.CancellationToken);

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();

                exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.DomainType: {domainType}");
            }
            else
            {
                // Act
                await settingsCH.Process(TestContext.Current.CancellationToken); ;

                InformationSet.AdditionalParameters.Should().HaveCount(2);
                InformationSet.AdditionalParameters.First().Key.Should().Be("DomainType");
                InformationSet.AdditionalParameters.First().Value.Should().Be($"{domainType}");

                KeyValuePair<string, string> secondParameter = InformationSet.AdditionalParameters.Skip(1).First();

                if (domainType == "Account")
                {
                    // Assert
                    InformationSet.Message.Should().Be(@$"Вы уверены?
======================

{"Удаление аккаунта приведёт к полной и безвозвратной потере информации о ваших тренировках".AddBold()}
");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.ConfirmDelete, ButtonsSet.Settings));

                    CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                    secondParameter.Key.Should().Be("Name");
                    secondParameter.Value.Should().Be("аккаунт");
                }
                else if (domainType == "Cycle")
                {
                    // Assert
                    InformationSet.Message.Should().Be(@$"Вы уверены?
======================

{"Удаление цикла приведёт к полной и безвозвратной потере информации о ваших тренировках в этом цикле".AddBold()}
");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.ConfirmDelete, ButtonsSet.SettingCycle));

                    CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                    secondParameter.Key.Should().Be("Name");
                    secondParameter.Value.Should().Be("цикл");
                }
                else if (domainType == "Day")
                {
                    // Assert
                    InformationSet.Message.Should().Be(@$"Вы уверены?
======================

{"Удаление дня приведёт к полной и безвозвратной потере информации о ваших тренировках в этом дне".AddBold()}
");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.ConfirmDelete, ButtonsSet.SettingDay));

                    CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                    secondParameter.Key.Should().Be("Name");
                    secondParameter.Value.Should().Be("день");
                }
                else if (domainType == "Exercise")
                {
                    // Assert
                    InformationSet.Message.Should().Be(@$"Вы уверены?
======================

{"Удаление упражнения приведёт к полной и безвозвратной потере информации о ваших тренировках с этим упражнением".AddBold()}
");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.ConfirmDelete, ButtonsSet.SettingExercise));

                    CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                    secondParameter.Key.Should().Be("Name");
                    secondParameter.Value.Should().Be("упражнение");
                }
                else
                {
                    // Assert
                    InformationSet.Message.Should().Be(@$"Вы уверены?
======================

{"Удаление подходов приведёт к полной и безвозвратной потере информации".AddBold()}
");
                    InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingExercise));

                    CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.DeleteResultsExercise);

                    secondParameter.Key.Should().Be("Name");
                    secondParameter.Value.Should().Be("упражнение");
                }

                CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);

                commandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                InformationSet.ParseMode.Should().Be(ParseMode.Html);
            }
        }

        [Theory]
        [InlineData("Other", false)]
        [InlineData("Account", false)]
        [InlineData("Cycle", false)]
        [InlineData("Cycle", true)]
        [InlineData("Day", false)]
        [InlineData("Exercise", false)]
        public async Task GetInformationSet_WithConfirmDeleteSubDirection_ShouldReturnExpectedIInformationSet(string domainType, bool currentCycleIsActive)
        {
            // поднимаем зависимости локально для теста т.к. для удаления сущностей нужны detached entity

            // Arrange
            EntityContextBuilder localBuilder = new EntityContextBuilder();
            EntityContext localEntityContext = localBuilder.Create()
                                   .WithUserInformation()
                                   .WithCycleDetached(isArchive: true)
                                   .WithCycleDetached(isArchive: false, isActive: true)
                                   .WithDayDetached(isArchive: false)
                                   .WithExerciseDetached(isArchive: false)
                                   .Build();
            
            CoreTools localCoreTools = new CoreTools()
            {
                ConfigurationData = new ConfigurationData() { DB = new DbSettings() { Database = "test" }, AboutBotText = "test1" },
                Db = localEntityContext,
                ContextKeeper = contextKeeperMock.Object,
                BotResponseSender = botResponseSenderMock.Object,
                LoggerFactory = customLoggerFactoryMock.Object,
            };

            DTOUserInformation localDTOCurrentUser = localEntityContext.UsersInformation.First().ToDTOUserInformation();

            UserContext localUserContext = new UserContext(localDTOCurrentUser);
            DTOCycle DTOCycle = localBuilder.TestCycle.ToDTOCycle(localDTOCurrentUser);
            localUserContext.DataManager.SetCurrentDomain(DTOCycle);
            DTODay DTODay = localBuilder.TestDay.ToDTODay(DTOCycle);
            localUserContext.DataManager.SetCurrentDomain(DTODay);
            DTOExercise DTOExercise = localBuilder.TestExercise.ToDTOExercise(DTODay);
            localUserContext.DataManager.SetCurrentDomain(DTOExercise);

            Update update = new Update();

            CommandHandlerTools localCommandHandlerTools = new CommandHandlerTools()
            {
                CoreTools = localCoreTools,
                HandlerResult = new UpdateHandlerResult()
                {
                    CurrentUserContext = localUserContext,
                    HasAccess = true,
                    Update = update,
                    ShortUpdateInfo = new ShortUpdateInfo(update, new User(), 1, "...", UpdateType.CallbackQuery, true)
                },
                RepositoriesStore = new RepositoriesStore(localEntityContext),
            };

            string subDirection = "ConfirmDelete";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(localCommandHandlerTools, callbackQueryParser);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.Process(TestContext.Current.CancellationToken);

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();

                exceptionAssertions.WithMessage($"Неожиданный domainTyped: {domainType}");
            }
            else
            {
                IInformationSet? informationSet;

                if (domainType == "Account")
                {
                    localEntityContext.UsersInformation.Any(x => x.Id == localCommandHandlerTools.HandlerResult.CurrentUserContext.UserInformation.Id).Should().BeTrue();

                    // Act
                    await settingsCH.Process(TestContext.Current.CancellationToken);

                    informationSet = localCommandHandlerTools.HandlerResult.InformationSet;
                    informationSet.Should().NotBeNull();

                    // Assert
                    localEntityContext.UsersInformation.Any(x => x.Id == localCommandHandlerTools.HandlerResult.CurrentUserContext.UserInformation.Id).Should().BeFalse();

                    informationSet.Message.Should().Be("Аккаунт успешно удалён");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));

                    contextKeeperMock.Verify(x => x.RemoveContext(It.IsAny<long>()), Times.Once);
                }
                else if (domainType == "Cycle")
                {
                    DTOCycle currentCycle = localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull();

                    localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.CurrentCycle.IsActive = currentCycleIsActive;

                    localEntityContext.Cycles.Any(x => x.Id == currentCycle.Id).Should().BeTrue();

                    // Act
                    await settingsCH.Process(TestContext.Current.CancellationToken);

                    informationSet = localCommandHandlerTools.HandlerResult.InformationSet;
                    informationSet.Should().NotBeNull();

                    if (currentCycleIsActive)
                    {
                        // Assert
                        localEntityContext.Cycles.Any(x => x.Id == currentCycle.Id).Should().BeTrue();

                        localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.CurrentCycle.Should().NotBeNull();

                        informationSet.Message.Should().Be(@$"Ошибка при удалении!
======================
Нельзя удалить активный цикл!
======================

Выберите интересующую настройку для цикла {localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}
");
                        informationSet.ButtonsSets.Should().Be((ButtonsSet.SettingCycle, ButtonsSet.CycleList));
                    }
                    else
                    {
                        // Assert
                        localEntityContext.Cycles.Any(x => x.Id == currentCycle.Id).Should().BeFalse();

                        localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.CurrentCycle.Should().BeNull();

                        informationSet.Message.Should().Be(@$"Цикл {currentCycle.Name.AddBoldAndQuotes()} удалён!
======================

Выберите интересующий цикл
");
                        informationSet.ButtonsSets.Should().Be((ButtonsSet.CycleList, ButtonsSet.SettingCycles));
                    }
                }
                else if (domainType == "Day")
                {
                    DTODay currentDay = localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull();

                    localEntityContext.Days.Any(x => x.Id == currentDay.Id).Should().BeTrue();

                    // Act
                    await settingsCH.Process(TestContext.Current.CancellationToken);

                    informationSet = localCommandHandlerTools.HandlerResult.InformationSet;
                    informationSet.Should().NotBeNull();

                    // Assert
                    localEntityContext.Days.Any(x => x.Id == currentDay.Id).Should().BeFalse();

                    localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.CurrentDay.Should().BeNull();

                    informationSet.Message.Should().Be(@$"День {currentDay.Name.AddBoldAndQuotes()} удалён!
======================

Выберите интересующий день из цикла {localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}
");

                    informationSet.ButtonsSets.Should().Be((ButtonsSet.DaysList, ButtonsSet.SettingDays));
                }
                else
                {
                    DTOExercise currentExercise = localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull();

                    localEntityContext.Exercises.Any(x => x.Id == currentExercise.Id).Should().BeTrue();

                    // Act
                    await settingsCH.Process(TestContext.Current.CancellationToken);

                    informationSet = localCommandHandlerTools.HandlerResult.InformationSet;
                    informationSet.Should().NotBeNull();

                    // Assert
                    localEntityContext.Exercises.Any(x => x.Id == currentExercise.Id).Should().BeFalse();

                    localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.CurrentExercise.Should().BeNull();

                    informationSet.Message.Should().Be(@$"Упражнение {currentExercise.Name.AddBoldAndQuotes()} удалёно!
======================

Выберите интересующее упражнение из дня {localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()} ({localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBold()})
");

                    informationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesList, ButtonsSet.SettingExercises));
                }

                localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);
                localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);

                localCommandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                informationSet.ParseMode.Should().Be(ParseMode.Html);
            }
        }

        public void Dispose()
        {
            builder.Dispose();
        }
    }
}