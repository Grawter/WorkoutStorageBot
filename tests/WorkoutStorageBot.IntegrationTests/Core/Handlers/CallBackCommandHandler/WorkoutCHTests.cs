using FluentAssertions;
using FluentAssertions.Specialized;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.BusinessLogic.Consts;
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
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.IntegrationTests.Core.Handlers.CallBackCommandHandler
{
    public class WorkoutCHTests : IDisposable
    {
        private readonly EntityContextBuilder builder;

        private readonly EntityContext entityContext;

        private Mock<IContextKeeper> contextKeeperMock;

        private readonly Mock<IBotResponseSender> botResponseSenderMock;

        private Mock<ICustomLoggerFactory> customLoggerFactoryMock;

        private readonly Mock<ILogger> loggerMock;

        private readonly CommandHandlerTools commandHandlerTools;

        private readonly CancellationTokenSource cts;

        public WorkoutCHTests()
        {
            builder = new EntityContextBuilder();
            entityContext = builder.Create()
                                   .WithUserInformation()
                                   .WithCycle(true)
                                   .WithDay()
                                   .WithExercise()
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

            UserContext userContext = new UserContext(DTOCurrentUser, Roles.User | Roles.Admin);
            userContext.DataManager.SetCurrentDomain(builder.TestCycle.ToDTOCycle());

            commandHandlerTools = new CommandHandlerTools()
            {
                ParentHandler = new PrimaryUpdateHandler(coreTools, new RepositoriesStore(entityContext)),
                CurrentUserContext = userContext,
            };
        }

        [Fact]
        public async Task GetInformationSet_WithWorkoutSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            string subDirection = "Workout";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|DomainType|CallBackId");

            WorkoutCH commonCH = new WorkoutCH(commandHandlerTools, callbackQueryParser);

            // Act
            IInformationSet informationSet = await commonCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Should().NotBeNull();

            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            informationSet.Message.Should().Be("Выберите тренировочный день из цикла \"<b>TestCycle</b>\"");
            informationSet.ButtonsSets.Should().Be((ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Theory]
        [InlineData("Exercises", true)]
        [InlineData("Exercises", false)]
        [InlineData("Day", true)]
        [InlineData("Day", false)]
        [InlineData("Other", false)]
        public async Task GetInformationSet_WithLastResultsSubDirection_ShouldReturnExpectedIInformationSet(string domainType, bool withResultExercise)
        {
            // Arrange
            string subDirection = "LastResults";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|{domainType}|CallBackId");

            WorkoutCH commonCH = new WorkoutCH(commandHandlerTools, callbackQueryParser);
            ResultExercise addedResultExercise = new ResultExercise() { DateTime = new DateTime(2020, 10, 9), Count = 1, ExerciseId = builder.TestExercise.Id };

            if (withResultExercise)
            {
                await entityContext.ResultsExercises.AddAsync(addedResultExercise);
                await entityContext.SaveChangesAsync();
            }

            commandHandlerTools.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestDay.ToDTODay());

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await commonCH.GetInformationSet();

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                exceptionAssertions.WithMessage($"Неожиданный CallbackQueryParser.DomainType: {callbackQueryParser.DomainType}");
            }
            else
            {
                // Act
                IInformationSet informationSet = await commonCH.GetInformationSet();

                // Assert
                commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Should().NotBeNull();

                commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                switch (domainType)
                {
                    case "Exercises":
                        if (withResultExercise)
                        {
                            informationSet.Message.Should().Be(@$"Последняя тренировка:
======================
Дата: {addedResultExercise.DateTime.ToString(CommonConsts.Common.DateFormat)}
Упражнение: ""<b>{addedResultExercise?.Exercise?.Name}</b>""
Повторения: ({addedResultExercise?.Count})
======================

Выберите тренировочный день из цикла ""<b>{addedResultExercise?.Exercise?.Day?.Cycle?.Name}</b>""
");
                        }
                        else
                        {
                            informationSet.Message.Should().Be(@$"Последняя тренировка:
======================
Не удалось получить результаты последней тренировки
======================

Выберите тренировочный день из цикла ""<b>{builder.TestCycle.Name}</b>""
");
                        }

                        informationSet.ButtonsSets.Should().Be((ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main));

                        break;
                    case "Day":
                        if (withResultExercise)
                        {
                            informationSet.Message.Should().Be(@$"Последние результаты упражнений из этого дня:
======================
Дата: ""<b>{addedResultExercise.DateTime.ToString(CommonConsts.Common.DateFormat)}</b>""
Упражнение: ""<b>{addedResultExercise?.Exercise?.Name}</b>""
Повторения: ({addedResultExercise?.Count})
======================

Выберите упражнение из дня <b>{addedResultExercise?.Exercise?.Day?.Name}</b> (<b>{addedResultExercise?.Exercise?.Day?.Cycle?.Name}</b>)
");
                        }
                        else
                        {
                            informationSet.Message.Should().Be(@$"Последние результаты упражнений из этого дня:
======================
Нет информации для данного дня
======================

Выберите упражнение из дня <b>{builder.TestDay.Name}</b> (<b>{builder.TestCycle.Name}</b>)
");
                        }

                        informationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout));

                        break;
                }

                informationSet.ParseMode.Should().Be(ParseMode.Html);
                informationSet.AdditionalParameters.Should().BeNull();
            }
        }

        [Theory]
        [InlineData("Exercises")]
        [InlineData("Day")]
        [InlineData("Other")]
        public async Task GetInformationSet_WithStartFindResultsByDateSubDirection_ShouldReturnExpectedIInformationSet(string domainType)
        {
            // Arrange
            string subDirection = "StartFindResultsByDate";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|{domainType}|CallBackId");

            WorkoutCH commonCH = new WorkoutCH(commandHandlerTools, callbackQueryParser);

            if (domainType == "Other")
            {
                // Act
                Func<Task> func = async () => await commonCH.GetInformationSet();

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                exceptionAssertions.WithMessage($"Неожиданный CallbackQueryParser.DomainType: {callbackQueryParser.DomainType}");
            }
            else
            {
                // Act
                IInformationSet informationSet = await commonCH.GetInformationSet();

                // Assert
                commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Should().NotBeNull();

                commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);

                informationSet.Message.Should().Be(@$"Введите дату искомой тренировки
======================

{CommonConsts.Exercise.FindResultsByDateFormat}
");

                switch (domainType)
                {
                    case "Exercises":
                        commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.FindResultsByDate);
                        informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.DaysListWithLastWorkout));
                        break;
                    case "Day":
                        commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.FindResultsByDateInDay);
                        informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay));
                        break;
                    default:
                        throw new InvalidOperationException($"Неожиданный domainType - {domainType}");
                }

                informationSet.ParseMode.Should().Be(ParseMode.Html);
                informationSet.AdditionalParameters.Should().BeNull();
            }
        }

        [Theory]
        [InlineData("Exercise", "IncorrectDate")]
        [InlineData("Other", "IncorrectDate")]
        [InlineData("Exercise", "09.10.2020")]
        [InlineData("Other", "09.10.2020")]
        public async Task GetInformationSet_WithFindResultsByDateSubDirection_ShouldReturnExpectedIInformationSet(string domainType, string date)
        {
            // Arrange
            string subDirection = "FindResultsByDate";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|{domainType}|{date}|CallBackId");

            WorkoutCH commonCH = new WorkoutCH(commandHandlerTools, callbackQueryParser);

            commandHandlerTools.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestDay.ToDTODay());

            ResultExercise addedResultExercise = new ResultExercise() { DateTime = new DateTime(2020, 10, 9), Count = 1, ExerciseId = builder.TestExercise.Id };

            await entityContext.ResultsExercises.AddAsync(addedResultExercise);
            await entityContext.SaveChangesAsync();

            // Act
            IInformationSet informationSet = await commonCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Should().NotBeNull();
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);
            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);

            bool isNeedFindByCurrentDay = callbackQueryParser.DomainType == CommonConsts.DomainsAndEntities.Exercise;

            if (date == "IncorrectDate")
            {
                if (isNeedFindByCurrentDay)
                {
                    informationSet.Message.Should().Be($@"Не удалось найти данные, т.к. не удалось распарсить дату '{date}'
======================

Выберите упражнение из дня  ""<b>{builder.TestDay.Name}</b>"" (<b>{builder.TestCycle.Name}</b>)
");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay));
                }
                else
                {
                    informationSet.Message.Should().Be($@"Не удалось найти данные, т.к. не удалось распарсить дату '{date}'
======================

Выберите тренировочный день из цикла ""<b>{builder.TestCycle.Name}</b>""
");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.DaysListWithLastWorkout));
                }
            }
            else
            {
                if (isNeedFindByCurrentDay)
                {
                    informationSet.Message.Should().Be(@$"Найденная тренировка:
======================
Дата: {addedResultExercise.DateTime.ToString(CommonConsts.Common.DateFormat)}
Упражнение: ""<b>{addedResultExercise?.Exercise?.Name}</b>""
Повторения: ({addedResultExercise?.Count})
======================

Выберите упражнение из дня ""<b>{addedResultExercise?.Exercise?.Day?.Name}</b>"" (<b>{addedResultExercise?.Exercise?.Day?.Cycle?.Name}</b>)
");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout));
                }
                else
                {
                    informationSet.Message.Should().Be(@$"Найденная тренировка:
======================
Дата: {addedResultExercise.DateTime.ToString(CommonConsts.Common.DateFormat)}
Упражнение: ""<b>{addedResultExercise?.Exercise?.Name}</b>""
Повторения: ({addedResultExercise?.Count})
======================

Выберите тренировочный день из цикла ""<b>{addedResultExercise?.Exercise?.Day?.Cycle?.Name}</b>""
");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main));
                }
            }

            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithStartExerciseTimerSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            string subDirection = "StartExerciseTimer";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|DomainType|CallBackId");

            WorkoutCH commonCH = new WorkoutCH(commandHandlerTools, callbackQueryParser);

            // Act
            IInformationSet informationSet = await commonCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.DataManager.ExerciseTimer.Should().BeAfter(DateTime.MinValue);

            commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Should().NotBeNull();

            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            informationSet.Message.Should().Be("Таймер запущен");
            informationSet.ButtonsSets.Should().Be((ButtonsSet.FixExerciseTimer, ButtonsSet.None));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithStopExerciseTimerSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            string subDirection = "StopExerciseTimer";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|DomainType|CallBackId");

            WorkoutCH commonCH = new WorkoutCH(commandHandlerTools, callbackQueryParser);

            commandHandlerTools.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestExercise.ToDTOExercise());

            // Act
            IInformationSet informationSet = await commonCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Should().NotBeNull();

            commandHandlerTools.CurrentUserContext.DataManager.ExerciseTimer.Should().Be(DateTime.MinValue);
            commandHandlerTools.CurrentUserContext.DataManager.TempResultsExercise.Should().NotBeNull();

            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddCommentForExerciseTimer);

            // \A и \z гарантируют, что совпадение будет строгим от начала до конца строки
            string pattern = @"\AРезультат: <b>\d{2}:\d{2}:\d{2}</b>\r?\n======================\r?\n\r?\nЕсли требуется, введите комментарий к результату или выберите интересующее действие\r?\n?\z";
            informationSet.Message.Should().MatchRegex(pattern);

            informationSet.ButtonsSets.Should().Be((ButtonsSet.SaveResultsExercise, ButtonsSet.None));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithShowExerciseTimerSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            string subDirection = "ShowExerciseTimer";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|DomainType|CallBackId");

            WorkoutCH commonCH = new WorkoutCH(commandHandlerTools, callbackQueryParser);

            commandHandlerTools.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestExercise.ToDTOExercise());

            // Act
            IInformationSet informationSet = await commonCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Should().NotBeNull();

            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            // \A и \z гарантируют, что совпадение будет строгим от начала до конца строки
            string pattern = @"\AС момента запуска таймера прошло: <b>\d{2}:\d{2}:\d{2}</b>\r?\n======================\r?\n\r?\nВыберите интересующее действие\r?\n?\z";
            informationSet.Message.Should().MatchRegex(pattern);

            informationSet.ButtonsSets.Should().Be((ButtonsSet.FixExerciseTimer, ButtonsSet.None));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithResetResultsExerciseSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            string subDirection = "ResetResultsExercise";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|DomainType|CallBackId");

            WorkoutCH commonCH = new WorkoutCH(commandHandlerTools, callbackQueryParser);

            commandHandlerTools.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestDay.ToDTODay());
            commandHandlerTools.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestExercise.ToDTOExercise());

            // Act
            IInformationSet informationSet = await commonCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Should().NotBeNull();
            commandHandlerTools.CurrentUserContext.DataManager.TempResultsExercise.Should().BeNull();

            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            informationSet.Message.Should().Be($@"Результат упражнения '""<b>{builder.TestExercise.Name}</b>""' был сброшен
======================

Выберите упражнение из дня <b>{builder.TestDay.Name}</b> (<b>{builder.TestCycle.Name}</b>)
");

            informationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithSaveResultsExerciseSubDirection_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            string subDirection = "SaveResultsExercise";

            CallbackQueryParser callbackQueryParser = new CallbackQueryParser($"Direction|{subDirection}|DomainType|CallBackId");

            WorkoutCH commonCH = new WorkoutCH(commandHandlerTools, callbackQueryParser);

            commandHandlerTools.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestExercise.ToDTOExercise());

            commandHandlerTools.CurrentUserContext.DataManager.AddTempResultsExercise([new DTOResultExercise() { Count = 1, DateTime = DateTime.Now, ExerciseId = builder.TestExercise.Id }]);

            // Act
            IInformationSet informationSet = await commonCH.GetInformationSet();

            // Assert
            commandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Should().NotBeNull();

            entityContext.ResultsExercises.Should().NotBeNull().And.HaveCount(1);

            ResultExercise resultExercise = entityContext.ResultsExercises.First();
            resultExercise.Count.Should().Be(1);
            resultExercise.ExerciseId = builder.TestExercise.Id;
            resultExercise.DateTime.Should().BeAfter(DateTime.MinValue);

            commandHandlerTools.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            commandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            informationSet.Message.Should().Be($@"Введённые данные сохранены!
======================

Выберите упражнение
");

            informationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout));
            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        public void Dispose()
        {
            builder.Dispose();
        }
    }
}