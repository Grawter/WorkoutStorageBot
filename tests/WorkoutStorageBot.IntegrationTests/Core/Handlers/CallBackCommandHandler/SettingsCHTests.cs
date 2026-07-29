using FluentAssertions;
using FluentAssertions.Specialized;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.BusinessLogic.Context.Global;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Extenions;
using WorkoutStorageBot.BusinessLogic.Extensions;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler.Context;
using WorkoutStorageBot.BusinessLogic.Handlers.MainHandlers;
using WorkoutStorageBot.BusinessLogic.Helpers.CallbackQueryParser;
using WorkoutStorageBot.Core.Extensions;
using WorkoutStorageBot.Core.Logging;
using WorkoutStorageBot.Core.Repositories.Store;
using WorkoutStorageBot.Core.Sender;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.DTO.HandlerData;
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

        private Mock<IContextKeeper> contextKeeperMock;

        private readonly Mock<IBotResponseSender> botResponseSenderMock;

        private Mock<ICustomLoggerFactory> customLoggerFactoryMock;

        private readonly Mock<ILogger> loggerMock;

        private readonly CommandHandlerTools commandHandlerTools;

        private readonly CancellationTokenSource cts;

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
                                   .WithExercise(isArchive: false, ExercisesMods.Timer)
                                   .Build();

            contextKeeperMock = new();
            botResponseSenderMock = new();
            customLoggerFactoryMock = new();
            loggerMock = new();

            cts = new CancellationTokenSource();

            customLoggerFactoryMock.Setup(x => x.CreateLogger<It.IsAnyType>()).Returns(loggerMock.Object);

            CoreTools coreTools = new CoreTools()
            {
                ConfigurationData = new ConfigurationData() { DB = new DbSettings() { Database = "test" } },
                Db = entityContext,
                ContextKeeper = contextKeeperMock.Object,
                BotResponseSender = botResponseSenderMock.Object,
                LoggerFactory = customLoggerFactoryMock.Object,
                AppCTS = cts,
            };

            DTOUserInformation DTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();

            UserContext userContext = new UserContext(DTOCurrentUser, Roles.User | Roles.Admin, false);
            DTOCycle DTOCycle = builder.TestCycle.ToDTOCycle(DTOCurrentUser);
            userContext.DataManager.SetCurrentDomain(DTOCycle);
            DTODay DTODay = builder.TestDay.ToDTODay(DTOCycle);
            userContext.DataManager.SetCurrentDomain(DTODay);
            DTOExercise DTOExercise = builder.TestExercise.ToDTOExercise(DTODay);
            userContext.DataManager.SetCurrentDomain(DTOExercise);

            commandHandlerTools = new CommandHandlerTools()
            {
                ParentHandler = new PrimaryUpdateHandler(coreTools, new RepositoriesStore(entityContext)),
                CurrentUserContext = userContext,
            };
        }

        [Fact]
        public async Task GetInformationSet_WithSettingsSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|Settings|DomainType|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            // Act
            IInformationSet informationSet = await settingsCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            informationSet.Message.Should().Be("Выберите интересующие настройки");
            informationSet.ButtonsSets.Should().Be((ButtonsSet.Settings, ButtonsSet.Main));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithArchiveStoreSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|ArchiveStore|DomainType|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            // Act
            IInformationSet informationSet = await settingsCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            informationSet.Message.Should().Be("Выберите интересующий архив для разархивирования");
            informationSet.ButtonsSets.Should().Be((ButtonsSet.ArchiveList, ButtonsSet.Settings));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Theory]
        [InlineData("Cycles")]
        [InlineData("Days")]
        [InlineData("Exercises")]
        [InlineData("Other")]
        public async Task GetInformationSet_WithArchiveSubDirection_ShouldReturnExpectedIInformationSet(string domainType)
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|Archive|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            // Assert
            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.GetInformationSet();
               
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.DomainType: {callbackQueryParser.DomainType}");
            }
            else
            {
                // Act
                IInformationSet informationSet = await settingsCH.GetInformationSet();

                if (domainType == "Cycles")
                {
                    informationSet.Message.Should().Be("Выберите архивный цикл для разархивирования");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.ArchiveCyclesList, ButtonsSet.ArchiveList));
                }
                else if (domainType == "Days")
                {
                    informationSet.Message.Should().Be("Выберите архивный день для разархивирования");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.ArchiveDaysList, ButtonsSet.ArchiveList));
                }
                else if (domainType == "Exercises")
                {
                    informationSet.Message.Should().Be("Выберите архивное упражнение для разархивирования");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.ArchiveExercisesList, ButtonsSet.ArchiveList));
                }

                // Assert
                commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                informationSet.ParseMode.Should().Be(ParseMode.Html);
                informationSet.AdditionalParameters.Should().BeNull();
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
            bool hasDTODomain = domainIDforUnArchive > 0;

            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|UnArchive|{domainType}|{domainIDforUnArchive}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            // Assert
            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.GetInformationSet();

                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.DomainType: {callbackQueryParser.DomainType}");
            }
            else
            {
                if (hasDTODomain)
                {
                    // Act
                    IInformationSet informationSet = await settingsCH.GetInformationSet();

                    IDTODomain? DTODomain = null;

                    if (domainType == "Cycle")
                    {
                        DTODomain = commandHandlerTools.CurrentUserContext.UserInformation.Cycles.First(x => x.Id == domainIDforUnArchive);
                    }
                    else if (domainType == "Day")
                    {
                        IEnumerable<DTODay> allDays = commandHandlerTools.CurrentUserContext.UserInformation.Cycles.SelectMany(x => x.Days);

                        DTODomain = allDays.First(x => x.Id == domainIDforUnArchive);
                    }
                    else if (domainType == "Exercise")
                    {
                        IEnumerable<DTOExercise> allExercise = commandHandlerTools.CurrentUserContext.UserInformation.Cycles.SelectMany(x => x.Days)
                                                                                                                            .SelectMany(x => x.Exercises);

                        DTODomain = allExercise.FirstOrDefault(x => x.Id == domainIDforUnArchive);
                    }

                    DTODomain?.IsArchive.Should().BeFalse();

                    // Assert
                    informationSet.Message.Should().Contain("разархивирован!");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.ArchiveList, ButtonsSet.Settings));
                    commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                    commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                    informationSet.ParseMode.Should().Be(ParseMode.Html);
                    informationSet.AdditionalParameters.Should().BeNull();
                }
                else
                {
                    Func<Task> func = async () => await settingsCH.GetInformationSet();

                    ExceptionAssertions<InvalidOperationException> exceptionAssertions = await func.Should().ThrowAsync<InvalidOperationException>();
                    exceptionAssertions.WithMessage($"Not found archive {domainType.ToLower()} for unarchiving with ID = {domainIDforUnArchive}");
                }
            }
        }

        [Fact]
        public async Task GetInformationSet_WithExportSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|Export|DomainType|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            // Act
            IInformationSet informationSet = await settingsCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            informationSet.Message.Should().Be("Выберите формат в котором экспортировать данные о ваших тренировках");
            informationSet.ButtonsSets.Should().Be((ButtonsSet.Export, ButtonsSet.Settings));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Theory]
        [InlineData("Excel")]
        [InlineData("JSON")]
        [InlineData("Other")]

        public async Task GetInformationSet_WithExportToSubDirection_ShouldReturnExpectedIInformationSet(string exportFormat)
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|ExportTo|DomainType|{exportFormat}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (exportFormat == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.GetInformationSet();

                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                exceptionAssertions.WithMessage($"Неожиданный exportFormat: {exportFormat}");
            }
            else
            {
                // Act
                IInformationSet informationSet = await settingsCH.GetInformationSet();

                if (exportFormat == "Excel")
                {
                    informationSet.Message.Should().Be($"Выберите временной промежуток формирования данных от последней тренировки для экспорта в {"Excel".AddBold()}");
                    informationSet.AdditionalParameters.Should().BeEquivalentTo(new Dictionary<string, string>() { { "Act", "Export/Excel" } });
                }
                else if (exportFormat == "JSON")
                {
                    informationSet.Message.Should().Be($"Выберите временной промежуток формирования данных от последней тренировки для экспорта в {"JSON".AddBold()}");
                    informationSet.AdditionalParameters.Should().BeEquivalentTo(new Dictionary<string, string>() { { "Act", "Export/JSON" } });
                }

                // Assert
                commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                informationSet.ButtonsSets.Should().Be((ButtonsSet.Period, ButtonsSet.Export));
                informationSet.ParseMode.Should().Be(ParseMode.Html);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetInformationSet_WithAboutBotSubDirection_ShouldReturnExpectedIInformationSet(bool withTextAboutBot)
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser("Direction|AboutBot|DomainType|CallBackId");

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
                    AppCTS = cts,
                };

                DTOUserInformation localDTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();

                UserContext localUserContext = new UserContext(localDTOCurrentUser);
                localUserContext.DataManager.CreateAndSetCurrentCycle("testCycle", true, localDTOCurrentUser);

                CommandHandlerTools localCommandHandlerTools = new CommandHandlerTools()
                {
                    ParentHandler = new PrimaryUpdateHandler(localCoreTools, new RepositoriesStore(entityContext)),
                    CurrentUserContext = localUserContext,
                };

                settingsCH = new SettingsCH(localCommandHandlerTools, callbackQueryParser);
            }
            else
            {
                settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);
            }

            // Act
            IInformationSet informationSet = await settingsCH.GetInformationSet();

            // Assert
            if (withTextAboutBot)
                informationSet.Message.Should().Be("test1");
            else
                informationSet.Message.Should().Be("Информации о боте не указано");

       
            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);
           
            informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.Settings));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Theory]
        [InlineData("Cycles")]
        [InlineData("Days")]
        [InlineData("Exercises")]
        [InlineData("Other")]
        public async Task GetInformationSet_WithSettingSubDirection_ShouldReturnExpectedIInformationSet(string domainType)
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|Setting|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.GetInformationSet();

                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }
            else
            {
                // Act
                IInformationSet informationSet = await settingsCH.GetInformationSet();

                if (domainType == "Cycles")
                {
                    informationSet.Message.Should().Be("Выберите интересующие настройки для циклов");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.SettingCycles, ButtonsSet.Settings));
                }
                else if (domainType == "Days")
                {
                    informationSet.Message.Should().Be($"Выберите интересующие настройки для дней из цикла {this.commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle?.Name.AddBoldAndQuotes()}");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.SettingDays, ButtonsSet.SettingCycle));
                }
                else if (domainType == "Exercises")
                {
                    informationSet.Message.Should().Be($"Выберите интересующие настройки для упражнений из дня {this.commandHandlerTools.CurrentUserContext.DataManager.CurrentDay?.Name.AddBoldAndQuotes()} ({this.commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle?.Name.AddBold()})");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.SettingExercises, ButtonsSet.SettingDay));
                }

                // Assert
                commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);
               
                informationSet.ParseMode.Should().Be(ParseMode.Html);
                informationSet.AdditionalParameters.Should().BeNull();
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
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|Add|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            commandHandlerTools.CurrentUserContext.Navigation.SetQueryFrom(queryFrom);

            // Assert
            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.GetInformationSet();

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
                        IInformationSet informationSet = await settingsCH.GetInformationSet();

                        // Assert
                        informationSet.Message.Should().Be("Введите название тренировочного цикла");
                        commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddCycle);

                        informationSet.ParseMode.Should().Be(ParseMode.Html);
                        informationSet.AdditionalParameters.Should().BeNull();

                        if (queryFrom == QueryFrom.Start)
                        {
                            informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));
                        }
                        else
                        {
                            informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingCycles));
                        }
                    }
                }
                else if (domainType == "Day")
                {
                    if (queryFrom == QueryFrom.Start || queryFrom == QueryFrom.Settings)
                    {
                        // Act
                        IInformationSet informationSet = await settingsCH.GetInformationSet();

                        // Assert
                        informationSet.Message.Should().Be($"Введите название тренирочного дня для цикла {commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}");
                        commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddDays);

                        informationSet.ParseMode.Should().Be(ParseMode.Html);
                        informationSet.AdditionalParameters.Should().BeNull();

                        if (queryFrom == QueryFrom.Start)
                        {
                            informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));
                        }
                        else if (queryFrom == QueryFrom.Settings)
                        {
                            informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingDays));
                        }
                    }
                }
                else if (domainType == "Exercise")
                {
                    if (queryFrom == QueryFrom.Start || queryFrom == QueryFrom.Settings)
                    {
                        // Act
                        IInformationSet informationSet = await settingsCH.GetInformationSet();

                        // Assert
                        informationSet.Message.Should().Be(@$"Введите название(я) и тип(ы) упражнения(й) для дня {commandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()}
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
                        commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddExercises);

                        informationSet.ParseMode.Should().Be(ParseMode.Html);
                        informationSet.AdditionalParameters.Should().BeNull();

                        if (queryFrom == QueryFrom.Start)
                        {
                            informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));
                        }
                        else if (queryFrom == QueryFrom.Settings)
                        {
                            informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingExercises));
                        }
                    }
                }
                else
                {
                    Func<Task> func = async () => await settingsCH.GetInformationSet();

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
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|ResetTempDomains|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            List<DTOExercise> dTOExercises = new List<DTOExercise>() { new DTOExercise() { Mode = ExercisesMods.Count, Name = "TestDTOExercise01" } };
            commandHandlerTools.CurrentUserContext.DataManager.TryAddTempExercises(dTOExercises, out string existingExerciseName);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.GetInformationSet();

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.DomainType: {domainType}");
            }
            else
            {
                // Act
                IInformationSet informationSet = await settingsCH.GetInformationSet();

                // Assert
                commandHandlerTools.CurrentUserContext.DataManager.TempExercises.Should().BeNull();

                commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                informationSet.Message.Should().Be(@"Упражнения для сохранения сброшены!
======================

Выберите интересующую настройку
");
                informationSet.ButtonsSets.Should().Be((ButtonsSet.SettingExercises, ButtonsSet.None));
                informationSet.ParseMode.Should().Be(ParseMode.Html);
                informationSet.AdditionalParameters.Should().BeNull();
            }
        }

        [Theory]
        [InlineData(QueryFrom.NoMatter)]
        [InlineData(QueryFrom.Start)]
        [InlineData(QueryFrom.Settings)]
        public async Task GetInformationSet_WithSaveExercisesSubDirection_ShouldReturnExpectedIInformationSet(QueryFrom queryFrom)
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|SaveExercises|domainType|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            commandHandlerTools.CurrentUserContext.Navigation.SetQueryFrom(queryFrom);
            List<DTOExercise> dTOExercises = new List<DTOExercise>() { new DTOExercise() { Mode = ExercisesMods.Count, Name = "TestDTOExercise01" } };
            commandHandlerTools.CurrentUserContext.DataManager.TryAddTempExercises(dTOExercises, out string existingExerciseName);

            if (queryFrom == QueryFrom.NoMatter)
            {
                // Act
                Func<Task> func = async () => await settingsCH.GetInformationSet();

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                exceptionAssertions.WithMessage($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {queryFrom}");
            }
            else
            {
                // Act
                IInformationSet informationSet = await settingsCH.GetInformationSet();

                if (queryFrom == QueryFrom.Start)
                {
                    // Assert
                    commandHandlerTools.CurrentUserContext.DataManager.TempExercises.Should().BeNull();

                    commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.Start);

                    informationSet.Message.Should().Be(@"Упражнения сохранены!
======================

Выберите дальнейшее действие
");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.RedirectAfterSaveExercise, ButtonsSet.None));

                }
                else if (queryFrom == QueryFrom.Settings)
                {
                    // Assert
                    commandHandlerTools.CurrentUserContext.DataManager.TempExercises.Should().BeNull();

                    commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);

                    informationSet.Message.Should().Be($@"Упражнения сохранены!
======================

Выберите интересующие настройки для упражнений из дня ""<b>{commandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name}</b>"" (<b>{commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name}</b>)
");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.SettingExercises, ButtonsSet.SettingDays));
                }
                
                commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                informationSet.ParseMode.Should().Be(ParseMode.Html);
                informationSet.AdditionalParameters.Should().BeNull();
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
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|SettingExisting|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.GetInformationSet();

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.DomainType: {domainType}");
            }
            else
            {
                // Act
                IInformationSet informationSet = await settingsCH.GetInformationSet();

                if (domainType == "Cycles")
                {
                    // Assert
                    informationSet.Message.Should().Be("Выберите интересующий цикл");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.CycleList, ButtonsSet.SettingCycles));

                }
                else if (domainType == "Days")
                {
                    // Assert
                    informationSet.Message.Should().Be($"Выберите интересующий день из цикла {commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.DaysList, ButtonsSet.SettingDays));
                }
                else if (domainType == "Exercises")
                {
                    // Assert
                    informationSet.Message.Should().Be($"Выберите интересующее упражнение из дня {commandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()} ({commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBold()})");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesList, ButtonsSet.SettingExercises));
                }

                commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                informationSet.ParseMode.Should().Be(ParseMode.Html);
                informationSet.AdditionalParameters.Should().BeNull();
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
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|Selected|{domainType}|{domainID}|CallBackId");

            commandHandlerTools.CurrentUserContext.Navigation.SetQueryFrom(queryFrom);

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Id.Should().Be(2);
            commandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Id.Should().Be(2);

            if (domainType == "Other" || ((domainType == "Day" || domainType == "Exercise") && queryFrom == QueryFrom.Start))
            {
                // Act
                Func<Task> func = async () => await settingsCH.GetInformationSet();

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                
                if (domainType == "Other")
                    exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.DomainType: {domainType}");

                if (domainType == "Day" || domainType == "Exercise")
                    exceptionAssertions.WithMessage($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {commandHandlerTools.CurrentUserContext.Navigation.QueryFrom}");
            }
            else
            {
                // Act
                IInformationSet informationSet = await settingsCH.GetInformationSet();

                if (domainType == "Cycle")
                {
                    // Assert
                    commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Id.Should().Be(domainID);

                    informationSet.Message.Should().Be($"Выберите интересующую настройку для цикла {commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.SettingCycle, ButtonsSet.CycleList));
                }
                else if (domainType == "Day")
                {
                    // Assert
                    commandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Id.Should().Be(domainID);

                    switch (commandHandlerTools.CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.NoMatter:

                            informationSet.Message.Should().Be($"Выберите упражнение из дня {commandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()} ({commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBold()})");
                            informationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout));

                            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                            break;

                        case QueryFrom.Settings:

                            informationSet.Message.Should().Be($"Выберите интересующую настройку для дня {commandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()} ({commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Name.AddBold()})");
                            informationSet.ButtonsSets.Should().Be((ButtonsSet.SettingDay, ButtonsSet.DaysList));

                            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
                            break;

                    }

                    commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                }
                else if (domainType == "Exercise")
                {
                    // Assert
                    DTOExercise currentExercise = commandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull();

                    currentExercise.Id.Should().Be(domainID);

                    string currentExerciseNameBoldAndQuotes = currentExercise.Name.AddBoldAndQuotes();

                    bool isNeedCheckMessageNavigationTarget = true;

                    switch (commandHandlerTools.CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.NoMatter:

                            if (currentExercise.Mode == ExercisesMods.Timer)
                            {
                                informationSet.Message.Should().Be($"Включение таймера для упражнения {currentExerciseNameBoldAndQuotes}");
                                informationSet.ButtonsSets.Should().Be((ButtonsSet.EnableExerciseTimer, ButtonsSet.ExercisesListWithLastWorkoutForDay));

                                commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
                            }
                            else
                            {
                                informationSet.Message.Should().Be(@$"Фиксирование результатов упражнения {currentExerciseNameBoldAndQuotes}
======================
Формат общего ввода: [кол-во повторений].
Пример единичного ввода: 25
Пример множественного ввода: 25 10 5
======================

Введите результат(ы) подхода(ов)
");
                                informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay));

                                commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                                commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddResultForExercise);

                                isNeedCheckMessageNavigationTarget = false;
                            }

                            break;

                        case QueryFrom.Settings:

                            informationSet.Message.Should().Be($"Выберите интересующую настройку для упражнения {currentExerciseNameBoldAndQuotes} ({commandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Name.AddBold()}-{commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBold()})");
                            informationSet.ButtonsSets.Should().Be((ButtonsSet.SettingExercise, ButtonsSet.ExercisesList));

                            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
                            break;

                    }

                    if (isNeedCheckMessageNavigationTarget)
                        commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);
                }

                informationSet.ParseMode.Should().Be(ParseMode.Html);
                informationSet.AdditionalParameters.Should().BeNull();
            }
        }

        [Theory]
        [InlineData("Other", false)]
        [InlineData("Cycle", true)]
        [InlineData("Cycle", false)]
        public async Task GetInformationSet_WithChangeActiveSubDirection_ShouldReturnExpectedIInformationSet(string domainType, bool currentCycleIsActive)
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|ChangeActive|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.GetInformationSet();

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();

                exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.DomainType: {domainType}");
            }
            else
            {
                IInformationSet informationSet;

                if (currentCycleIsActive)
                {
                    DTOCycle currentActiveCycle = commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull();
                    currentActiveCycle.IsActive.Should().BeTrue();

                    // Act
                    informationSet = await settingsCH.GetInformationSet();

                    // Assert
                    currentActiveCycle.IsActive.Should().BeTrue();

                    informationSet.Message.Should().Be(@$"Выбранный цикл {commandHandlerTools.CurrentUserContext.ActiveCycle.ThrowIfNull().Name.AddBoldAndQuotes()} уже является активным!
======================

Выберите интересующую настройку для цикла {commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}
");
                }
                else
                {
                    DTOCycle currentActiveCycle = commandHandlerTools.CurrentUserContext.ActiveCycle.ThrowIfNull();
                    currentActiveCycle.IsActive.Should().BeTrue();

                    DTOCycle newSelectedCycle = entityContext.Cycles.First().ToDTOCycle();
                    newSelectedCycle.IsActive.Should().BeFalse();

                    commandHandlerTools.CurrentUserContext.DataManager.SetCurrentDomain(newSelectedCycle);

                    // Act
                    informationSet = await settingsCH.GetInformationSet();

                    // Assert
                    currentActiveCycle.IsActive.Should().BeFalse();
                    newSelectedCycle.IsActive.Should().BeTrue();

                    informationSet.Message.Should().Be(@$"Активный цикл изменён на {commandHandlerTools.CurrentUserContext.ActiveCycle.Name.AddBoldAndQuotes()}
======================

Выберите интересующую настройку для цикла {commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}
");
                }

                informationSet.ButtonsSets.Should().Be((ButtonsSet.SettingCycle, ButtonsSet.SettingCycles));

                commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                informationSet.ParseMode.Should().Be(ParseMode.Html);
                informationSet.AdditionalParameters.Should().BeNull();
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
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|Archiving|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.GetInformationSet();

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();

                exceptionAssertions.WithMessage($"Неожиданный domainTyped: {domainType}");
            }
            else
            {
                IInformationSet informationSet;

                if (domainType == "Cycle")
                {
                    if (currentCycleIsActive)
                    {
                        DTOCycle currentActiveCycle = commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull();
                        currentActiveCycle.IsActive.Should().BeTrue();

                        // Act
                        informationSet = await settingsCH.GetInformationSet();

                        // Assert
                        currentActiveCycle.IsActive.Should().BeTrue();

                        currentActiveCycle.IsArchive.Should().BeFalse();

                        informationSet.Message.Should().Be(@$"Ошибка при архивации!
======================
Нельзя архивировать цикл {currentActiveCycle.Name.AddBoldAndQuotes()}, т.к. он является активным!
======================

Выберите интересующую настройку для цикла {currentActiveCycle.Name.AddBoldAndQuotes()}
");
                        informationSet.ButtonsSets.Should().Be((ButtonsSet.SettingCycle, ButtonsSet.CycleList));
                    }
                    else
                    {
                        DTOCycle currentActiveCycle = commandHandlerTools.CurrentUserContext.ActiveCycle.ThrowIfNull();
                        currentActiveCycle.IsActive.Should().BeTrue();

                        DTOCycle newSelectedCycle = commandHandlerTools.CurrentUserContext.UserInformation.Cycles.First();
                        newSelectedCycle.IsActive.Should().BeFalse();

                        commandHandlerTools.CurrentUserContext.DataManager.SetCurrentDomain(newSelectedCycle);

                        // Act
                        informationSet = await settingsCH.GetInformationSet();

                        newSelectedCycle.IsArchive.Should().BeTrue();

                        informationSet.Message.Should().Be(@$"Цикл {newSelectedCycle.Name.AddBoldAndQuotes()} был добавлен в архив
======================

Выберите интересующий цикл
");

                        informationSet.ButtonsSets.Should().Be((ButtonsSet.CycleList, ButtonsSet.SettingCycles));
                    }
                }
                else if (domainType == "Day")
                {
                    DTODay currentDay = commandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull();

                    // Act
                    informationSet = await settingsCH.GetInformationSet();

                    // Assert
                    currentDay.IsArchive.Should().BeTrue();

                    informationSet.Message.Should().Be(@$"День {currentDay.Name.AddBoldAndQuotes()} был добавлен в архив
======================

Выберите интересующий день из цикла {commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}
");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.DaysList, ButtonsSet.SettingDays));
                }
                else
                {
                    DTOExercise currentExercise = commandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull();

                    // Act
                    informationSet = await settingsCH.GetInformationSet();

                    // Assert
                    currentExercise.IsArchive.Should().BeTrue();

                    informationSet.Message.Should().Be(@$"Упражнение {currentExercise.Name.AddBoldAndQuotes()} было добавлено в архив
======================

Выберите интересующее упражнение из дня {commandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()} ({commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBold()})
");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesList, ButtonsSet.SettingExercises));
                }

                commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                informationSet.ParseMode.Should().Be(ParseMode.Html);
                informationSet.AdditionalParameters.Should().BeNull();
            }
        }

        [Theory]
        [InlineData("Other")]
        [InlineData("Day")]
        [InlineData("Exercise")]
        public async Task GetInformationSet_WithReplaceSubDirection_ShouldReturnExpectedIInformationSet(string domainType)
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|Replace|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.GetInformationSet();

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();

                exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.DomainType: {domainType}");
            }
            else
            {
                // Act
                IInformationSet informationSet = await settingsCH.GetInformationSet();

                if (domainType == "Day")
                {
                    // Assert
                    informationSet.Message.Should().Be(@$"Выберите цикл, в который хотите перенести день {commandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.ReplaceToCycle, ButtonsSet.SettingDay));
                }
                else
                {
                    // Assert
                    informationSet.Message.Should().Be(@$"Выберите день, в который хотите перенести упражнение {commandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.ReplaceToDay, ButtonsSet.SettingExercise));
                }

                commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                informationSet.ParseMode.Should().Be(ParseMode.Html);
                informationSet.AdditionalParameters.Should().BeNull();
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
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|ReplaceTo|{domainType}|{targetDomainID}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.GetInformationSet();

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();

                exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.DomainType: {domainType}");
            }
            else
            {
                IInformationSet informationSet;

                if (domainType == "Cycle")
                {
                    if (commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Id == targetDomainID)
                    {
                        // Act
                        informationSet = await settingsCH.GetInformationSet();

                        // Assert
                        informationSet.Message.Should().Be(@$"Ошибка при переносе дня!
======================
Нельзя перенести день в тот же самый цикл
======================

Выберите цикл, в который хотите перенести день {commandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()}
");
                        informationSet.ButtonsSets.Should().Be((ButtonsSet.ReplaceToCycle, ButtonsSet.SettingDay));
                    }
                    else
                    {
                        DTODay dayForReplacement = commandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull();

                        DTOCycle destinationCycle = commandHandlerTools.CurrentUserContext.UserInformation.Cycles.First(x => x.Id == targetDomainID);

                        destinationCycle.Days.Should().BeEmpty();
                        commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Days.Should().Contain(x => x.Id == dayForReplacement.Id && x.Name == dayForReplacement.Name);

                        // Act
                        informationSet = await settingsCH.GetInformationSet();

                        destinationCycle.Days.Should().Contain(dayForReplacement);
                        commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Days.Should().NotContain(x => x.Id == dayForReplacement.Id && x.Name == dayForReplacement.Name);

                        // Assert
                        informationSet.Message.Should().Be(@$"День {dayForReplacement.Name.AddBoldAndQuotes()}, перенесён в цикл {destinationCycle.Name.AddBoldAndQuotes()}
======================

Выберите интересующий цикл
");
                        informationSet.ButtonsSets.Should().Be((ButtonsSet.CycleList, ButtonsSet.SettingCycles));
                    }
                }
                else
                {
                    if (commandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Id == targetDomainID)
                    {
                        // Act
                        informationSet = await settingsCH.GetInformationSet();

                        // Assert
                        informationSet.Message.Should().Be(@$"Ошибка при переносе упражнения!
======================
Нельзя перенести упражнение в тот же самый день
======================

Выберите день, в который хотите перенести упражнение {commandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().Name.AddBoldAndQuotes()}
");
                        informationSet.ButtonsSets.Should().Be((ButtonsSet.ReplaceToDay, ButtonsSet.SettingExercise));
                    }
                    else 
                    {
                        DTOExercise exerciseForReplacement = commandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull();

                        DTODay destinationDay = commandHandlerTools.CurrentUserContext.UserInformation.Cycles.SelectMany(x => x.Days).First(x => x.Id == targetDomainID);

                        destinationDay.Exercises.Should().BeEmpty();
                        commandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Exercises.Should().Contain(x => x.Id == exerciseForReplacement.Id && x.Name == exerciseForReplacement.Name);

                        // Act
                        informationSet = await settingsCH.GetInformationSet();

                        destinationDay.Exercises.Should().Contain(exerciseForReplacement);
                        commandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Exercises.Should().NotContain(x => x.Id == exerciseForReplacement.Id && x.Name == exerciseForReplacement.Name);

                        // Assert
                        informationSet.Message.Should().Be(@$"Упражнение {exerciseForReplacement.Name.AddBoldAndQuotes()}, перенесёно в день {destinationDay.Name.AddBoldAndQuotes()}
======================

Выберите интересующий цикл
");
                        informationSet.ButtonsSets.Should().Be((ButtonsSet.CycleList, ButtonsSet.SettingCycles));
                    }
                }

                commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                informationSet.ParseMode.Should().Be(ParseMode.Html);
                informationSet.AdditionalParameters.Should().BeNull();
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
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|ChangeName|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.GetInformationSet();

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();

                exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.DomainType: {domainType}");
            }
            else
            {
                // Act
                IInformationSet informationSet = await settingsCH.GetInformationSet();

                if (domainType == "Cycle")
                {
                    // Assert
                    informationSet.Message.Should().Be(@$"Введите новоё название для цикла {commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingCycle));

                    commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.ChangeNameCycle);
                }
                else if (domainType == "Day")
                {
                    // Assert
                    informationSet.Message.Should().Be(@$"Введите новоё название для дня {commandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingDay));

                    commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.ChangeNameDay);
                }
                else
                {
                    // Assert
                    informationSet.Message.Should().Be(@$"Введите новоё название для упражнения {commandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingExercise));

                    commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.ChangeNameExercise);
                }

                commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                

                informationSet.ParseMode.Should().Be(ParseMode.Html);
                informationSet.AdditionalParameters.Should().BeNull();
            }
        }

        [Theory]
        [InlineData("Other")]
        [InlineData("Exercise")]
        public async Task GetInformationSet_WithChangeModeSubDirection_ShouldReturnExpectedIInformationSet(string domainType)
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|ChangeMode|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.GetInformationSet();

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();

                exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.DomainType: {domainType}");
            }
            else
            {
                // Act
                IInformationSet informationSet = await settingsCH.GetInformationSet();

                // Assert
                informationSet.Message.Should().Be(@$"Выберите новый тип для упражнения {commandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().Name.AddBoldAndQuotes()}");
                informationSet.ButtonsSets.Should().Be((ButtonsSet.ChangeType, ButtonsSet.SettingExercise));

                commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                informationSet.ParseMode.Should().Be(ParseMode.Html);
                informationSet.AdditionalParameters.Should().BeNull();
            }
        }

        [Theory]
        [InlineData("Other", ExercisesMods.Count)]
        [InlineData("Exercise", ExercisesMods.FreeResult)]
        public async Task GetInformationSet_WithCChangedModeSubDirection_ShouldReturnExpectedIInformationSet(string domainType, ExercisesMods newExercisesMods)
        {
            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|ChangedMode|{domainType}|{(int)newExercisesMods}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.GetInformationSet();

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();

                exceptionAssertions.WithMessage($"Неожиданный domainTyped: {domainType}");
            }
            else
            {
                DTOExercise currentExercise = commandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull();
                currentExercise.Mode.Should().Be(ExercisesMods.Timer);

                // Act
                IInformationSet informationSet = await settingsCH.GetInformationSet();

                // Assert
                currentExercise.Mode.Should().Be(newExercisesMods);

                informationSet.Message.Should().Be(@$"Режим для упражнения {currentExercise.Name.AddBoldAndQuotes()} изменён на {newExercisesMods.ToString().AddBoldAndQuotes()}
======================

Выберите интересующую настройку для упражнения {currentExercise.Name.AddBoldAndQuotes()}
");
                informationSet.ButtonsSets.Should().Be((ButtonsSet.SettingExercise, ButtonsSet.ExercisesList));

                commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                informationSet.ParseMode.Should().Be(ParseMode.Html);
                informationSet.AdditionalParameters.Should().BeNull();
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
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|Period||{operation}|0|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (operation == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.GetInformationSet();

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();

                exceptionAssertions.WithMessage($"Неожиданный operation: {operation}");
            }
            else
            {
                IInformationSet informationSet;

                if (withResultSExercise)
                {
                    builder.WithResultExercise();

                    // Act
                    informationSet = await settingsCH.GetInformationSet();

                    informationSet.Message.Should().Be(@$"Тренировки успешно экспортированы!");
                }
                else
                {
                    // Act
                    informationSet = await settingsCH.GetInformationSet();

                    informationSet.Message.Should().Be(@$"Отсутствуют результаты для экспорта
======================

Выберите интересующую настройку
");
                }

                // Assert
                informationSet.ButtonsSets.Should().Be((ButtonsSet.Settings, ButtonsSet.Main));

                commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                informationSet.ParseMode.Should().Be(ParseMode.Html);
                informationSet.AdditionalParameters.Should().BeNull();

                if (informationSet is FileInformationSet fileInformationSet)
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
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|Delete|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(commandHandlerTools, callbackQueryParser);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.GetInformationSet();

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();

                exceptionAssertions.WithMessage($"Неожиданный callbackQueryParser.DomainType: {domainType}");
            }
            else
            {
                // Act
                IInformationSet informationSet = await settingsCH.GetInformationSet();

                informationSet.AdditionalParameters.Should().HaveCount(2);
                informationSet.AdditionalParameters.First().Key.Should().Be("DomainType");
                informationSet.AdditionalParameters.First().Value.Should().Be($"{domainType}");

                KeyValuePair<string, string> secondParameter = informationSet.AdditionalParameters.Skip(1).First();

                if (domainType == "Account")
                {
                    // Assert
                    informationSet.Message.Should().Be(@$"Вы уверены?
======================

{"Удаление аккаунта приведёт к полной и безвозвратной потере информации о ваших тренировках".AddBold()}
");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.ConfirmDelete, ButtonsSet.Settings));

                    commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                    secondParameter.Key.Should().Be("Name");
                    secondParameter.Value.Should().Be("аккаунт");
                }
                else if (domainType == "Cycle")
                {
                    // Assert
                    informationSet.Message.Should().Be(@$"Вы уверены?
======================

{"Удаление цикла приведёт к полной и безвозвратной потере информации о ваших тренировках в этом цикле".AddBold()}
");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.ConfirmDelete, ButtonsSet.SettingCycle));

                    commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                    secondParameter.Key.Should().Be("Name");
                    secondParameter.Value.Should().Be("цикл");
                }
                else if (domainType == "Day")
                {
                    // Assert
                    informationSet.Message.Should().Be(@$"Вы уверены?
======================

{"Удаление дня приведёт к полной и безвозвратной потере информации о ваших тренировках в этом дне".AddBold()}
");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.ConfirmDelete, ButtonsSet.SettingDay));

                    commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                    secondParameter.Key.Should().Be("Name");
                    secondParameter.Value.Should().Be("день");
                }
                else if (domainType == "Exercise")
                {
                    // Assert
                    informationSet.Message.Should().Be(@$"Вы уверены?
======================

{"Удаление упражнения приведёт к полной и безвозвратной потере информации о ваших тренировках с этим упражнением".AddBold()}
");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.ConfirmDelete, ButtonsSet.SettingExercise));

                    commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                    secondParameter.Key.Should().Be("Name");
                    secondParameter.Value.Should().Be("упражнение");
                }
                else
                {
                    // Assert
                    informationSet.Message.Should().Be(@$"Вы уверены?
======================

{"Удаление подходов приведёт к полной и безвозвратной потере информации".AddBold()}
");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingExercise));

                    commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.DeleteResultsExercise);

                    secondParameter.Key.Should().Be("Name");
                    secondParameter.Value.Should().Be("упражнение");
                }

                commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);

                informationSet.ParseMode.Should().Be(ParseMode.Html);
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
                AppCTS = cts,
            };

            DTOUserInformation localDTOCurrentUser = localEntityContext.UsersInformation.First().ToDTOUserInformation();

            UserContext localUserContext = new UserContext(localDTOCurrentUser);
            DTOCycle DTOCycle = localBuilder.TestCycle.ToDTOCycle(localDTOCurrentUser);
            localUserContext.DataManager.SetCurrentDomain(DTOCycle);
            DTODay DTODay = localBuilder.TestDay.ToDTODay(DTOCycle);
            localUserContext.DataManager.SetCurrentDomain(DTODay);
            DTOExercise DTOExercise = localBuilder.TestExercise.ToDTOExercise(DTODay);
            localUserContext.DataManager.SetCurrentDomain(DTOExercise);

            CommandHandlerTools localCommandHandlerTools = new CommandHandlerTools()
            {
                ParentHandler = new PrimaryUpdateHandler(localCoreTools, new RepositoriesStore(localEntityContext)),
                CurrentUserContext = localUserContext,
            };

            // Arrange
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|ConfirmDelete|{domainType}|CallBackId");

            SettingsCH settingsCH = new SettingsCH(localCommandHandlerTools, callbackQueryParser);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await settingsCH.GetInformationSet();

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();

                exceptionAssertions.WithMessage($"Неожиданный domainTyped: {domainType}");
            }
            else
            {
                IInformationSet informationSet;

                if (domainType == "Account")
                {
                    localEntityContext.UsersInformation.Any(x => x.Id == localCommandHandlerTools.CurrentUserContext.UserInformation.Id).Should().BeTrue();

                    // Act
                    informationSet = await settingsCH.GetInformationSet();

                    // Assert
                    localEntityContext.UsersInformation.Any(x => x.Id == localCommandHandlerTools.CurrentUserContext.UserInformation.Id).Should().BeFalse();

                    informationSet.Message.Should().Be("Аккаунт успешно удалён");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));

                    contextKeeperMock.Verify(x => x.RemoveContext(It.IsAny<long>()), Times.Once);
                }
                else if (domainType == "Cycle")
                {
                    DTOCycle currentCycle = localCommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull();

                    localCommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.IsActive = currentCycleIsActive;

                    localEntityContext.Cycles.Any(x => x.Id == currentCycle.Id).Should().BeTrue();

                    // Act
                    informationSet = await settingsCH.GetInformationSet();

                    if (currentCycleIsActive)
                    {
                        // Assert
                        localEntityContext.Cycles.Any(x => x.Id == currentCycle.Id).Should().BeTrue();

                        localCommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Should().NotBeNull();

                        informationSet.Message.Should().Be(@$"Ошибка при удалении!
======================
Нельзя удалить активный цикл!
======================

Выберите интересующую настройку для цикла {localCommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}
");
                        informationSet.ButtonsSets.Should().Be((ButtonsSet.SettingCycle, ButtonsSet.CycleList));
                    }
                    else
                    {
                        // Assert
                        localEntityContext.Cycles.Any(x => x.Id == currentCycle.Id).Should().BeFalse();

                        localCommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Should().BeNull();

                        informationSet.Message.Should().Be(@$"Цикл {currentCycle.Name.AddBoldAndQuotes()} удалён!
======================

Выберите интересующий цикл
");
                        informationSet.ButtonsSets.Should().Be((ButtonsSet.CycleList, ButtonsSet.SettingCycles));
                    }
                }
                else if (domainType == "Day")
                {
                    DTODay currentDay = localCommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull();

                    localEntityContext.Days.Any(x => x.Id == currentDay.Id).Should().BeTrue();

                    // Act
                    informationSet = await settingsCH.GetInformationSet();

                    // Assert
                    localEntityContext.Days.Any(x => x.Id == currentDay.Id).Should().BeFalse();

                    localCommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Should().BeNull();

                    informationSet.Message.Should().Be(@$"День {currentDay.Name.AddBoldAndQuotes()} удалён!
======================

Выберите интересующий день из цикла {localCommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}
");

                    informationSet.ButtonsSets.Should().Be((ButtonsSet.DaysList, ButtonsSet.SettingDays));
                }
                else
                {
                    DTOExercise currentExercise = localCommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull();

                    localEntityContext.Exercises.Any(x => x.Id == currentExercise.Id).Should().BeTrue();

                    // Act
                    informationSet = await settingsCH.GetInformationSet();

                    // Assert
                    localEntityContext.Exercises.Any(x => x.Id == currentExercise.Id).Should().BeFalse();

                    localCommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.Should().BeNull();

                    informationSet.Message.Should().Be(@$"Упражнение {currentExercise.Name.AddBoldAndQuotes()} удалёно!
======================

Выберите интересующее упражнение из дня {localCommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()} ({localCommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBold()})
");

                    informationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesList, ButtonsSet.SettingExercises));
                }

                localCommandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);
                localCommandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);

                informationSet.ParseMode.Should().Be(ParseMode.Html);
            }
        }

        public void Dispose()
        {
            builder.Dispose();
        }
    }
}