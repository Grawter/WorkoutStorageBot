using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Global;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Handlers.MainHandlers;
using WorkoutStorageBot.Core.Handlers.Abstraction;
using WorkoutStorageBot.Core.Logging;
using WorkoutStorageBot.Core.Repositories.Store;
using WorkoutStorageBot.Core.Sender;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.HandlerData.Results;
using WorkoutStorageBot.Model.DTO.InformationSetForSend;

namespace WorkoutStorageBot.E2ETests.Scenarios.Scenario1
{
    public class TestsScenario1 : IDisposable
    {
        private readonly string userName;
        private readonly long userID;
        private readonly string firstName;
        private readonly int userInformationID;

        private readonly string firstCycleName;
        private readonly string secondCycleName;
        private readonly string thirdCycleName;

        private readonly string firstDayNameInFirstCycle;
        private readonly string secondDayNameInFirstCycle;
        private readonly string firstDayNameInSecondCycle;
        private readonly string secondDayNameInSecondCycle;
        private readonly string firstDayNameInThirdCycle;

        private readonly string firstExerciseNameOnFirstDayInFirstCycle;
        private readonly string secondExerciseNameOnFirstDayInFirstCycle;
        private readonly string firstExerciseNameOnSecondDayInFirstCycle;
        private readonly string secondExerciseNameOnSecondDayInFirstCycle;
        private readonly string firstExerciseNameOnFirstDayInSecondCycle;
        private readonly string secondExerciseNameOnFirstDayInSecondCycle;
        private readonly string firstExerciseNameOnSecondDayInSecondCycle;
        private readonly string secondExerciseNameOnSecondDayInSecondCycle;
        private readonly string firstExerciseNameOnFirstDayInThirdCycle;

        private readonly SqliteConnection dbConnection;

        private readonly EntityContext entityContext;

        private readonly Mock<IBotResponseSender> botResponseSenderMock;

        private readonly Mock<ICustomLoggerFactory> customLoggerFactoryMock;

        private readonly Mock<ILogger> loggerMock;

        private readonly IContextKeeper contextKeeper;

        private readonly List<CoreHandler> handlers;

        private readonly CancellationTokenSource cts;

        private readonly CommonTestsHelper commonTestsHelper;

        private IEnumerable<InlineKeyboardButton> lastSuggestedButtons;

        private string lastSuggestedButtonsStr;

        public TestsScenario1()
        {
            userName = "@TestUser";
            userID = 1;
            firstName = "TestUserFirstName";
            userInformationID = 1;

            firstCycleName = "1";
            secondCycleName = "2";
            thirdCycleName = "3";

            firstDayNameInFirstCycle = "11";
            secondDayNameInFirstCycle = "12";
            firstDayNameInSecondCycle = "21";
            secondDayNameInSecondCycle = "22";
            firstDayNameInThirdCycle = "31";

            firstExerciseNameOnFirstDayInFirstCycle = "111";
            secondExerciseNameOnFirstDayInFirstCycle = "112";
            firstExerciseNameOnSecondDayInFirstCycle = "121";
            secondExerciseNameOnSecondDayInFirstCycle = "122";
            firstExerciseNameOnFirstDayInSecondCycle = "211";
            secondExerciseNameOnFirstDayInSecondCycle = "212";
            firstExerciseNameOnSecondDayInSecondCycle = "221";
            secondExerciseNameOnSecondDayInSecondCycle = "222";
            firstExerciseNameOnFirstDayInThirdCycle = "311";

            dbConnection = new SqliteConnection("Data Source=:memory:");
            dbConnection.Open();

            DbContextOptions<EntityContext> options = new DbContextOptionsBuilder<EntityContext>()
                //.EnableSensitiveDataLogging()
                //.LogTo(message => Debug.WriteLine(message), LogLevel.Information)
                .UseSqlite(dbConnection)
                .Options;

            entityContext = new EntityContext(options);
            entityContext.Database.OpenConnection();
            entityContext.Database.EnsureCreated();

            ConfigurationData configurationData = new ConfigurationData()
            {
                DB = new DbSettings() { Database = "test" },
                Bot = new BotSettings() { Token = "1" }
            };

            contextKeeper = new DictionaryAdapterContextKeeper();

            botResponseSenderMock = new();

            customLoggerFactoryMock = new();

            loggerMock = new();

            customLoggerFactoryMock.Setup(x => x.CreateLogger<It.IsAnyType>()).Returns(loggerMock.Object);

            cts = new CancellationTokenSource();

            CoreTools coreTools = new CoreTools()
            {
                ConfigurationData = configurationData,
                Db = entityContext,
                ContextKeeper = contextKeeper,
                BotResponseSender = botResponseSenderMock.Object,
                LoggerFactory = customLoggerFactoryMock.Object,
                AppCTS = cts,
            };

            RepositoriesStore repositoriesStore = new RepositoriesStore(entityContext);

            handlers = new List<CoreHandler>()
            {
                new PrimaryUpdateHandler(coreTools, repositoriesStore),
                new UpdateHandler(coreTools, repositoriesStore),
            };

            commonTestsHelper = new CommonTestsHelper(botResponseSenderMock.Object, handlers, userName, userID, firstName);

            lastSuggestedButtons = new List<InlineKeyboardButton>();

            lastSuggestedButtonsStr = string.Empty;
        }
        
        [Fact]
        public async Task TestScenario1()
        {
            entityContext.UsersInformation.Should().BeEmpty();

            await ExecuteAndAssertBusinessAct1();
            await ExecuteAndAssertBusinessAct2();
            await ExecuteAndAssertBusinessAct3();
            await ExecuteAndAssertBusinessAct4();
            await ExecuteAndAssertBusinessAct5();
            await ExecuteAndAssertBusinessAct6();
            await ExecuteAndAssertBusinessAct7();
            await ExecuteAndAssertBusinessAct8();
            await ExecuteAndAssertBusinessAct9();
        }

        /// <summary>
        /// Бизнес действие1: Регистрация юзера @TestUser с циклом (1), двумя днями(11 12), четырьмя упражнениями(111 112 121 122).
        /// Проверяем регистрацию и заполнение цикла, дней и упражнений при регистрации
        /// </summary>
        private async Task ExecuteAndAssertBusinessAct1()
        {
            Scenario1BusinessAct1AssertsTestsHelper scenario1BusinessAct1AssertTestsHelper = new Scenario1BusinessAct1AssertsTestsHelper(contextKeeper,
                                                                                                                                         entityContext,
                                                                                                                                         userName,
                                                                                                                                         userID,
                                                                                                                                         firstName,
                                                                                                                                         userInformationID,

                                                                                                                                         firstCycleName,

                                                                                                                                         firstDayNameInFirstCycle,
                                                                                                                                         secondDayNameInFirstCycle,

                                                                                                                                         firstExerciseNameOnFirstDayInFirstCycle,
                                                                                                                                         secondExerciseNameOnFirstDayInFirstCycle,
                                                                                                                                         firstExerciseNameOnSecondDayInFirstCycle,
                                                                                                                                         secondExerciseNameOnSecondDayInFirstCycle);

            // Первое обращение к боту
            await CreateMessageUpdateThenStartProcessThenAssertResult("/Start", scenario1BusinessAct1AssertTestsHelper.AssertAct1);

            // Нажатие на кнопку "Добавить цикл"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct1AssertTestsHelper.AssertAct2);

            // Ввод названия цикла
            await CreateMessageUpdateThenStartProcessThenAssertResult(firstCycleName, scenario1BusinessAct1AssertTestsHelper.AssertAct3);

            // Ввод названия дня
            await CreateMessageUpdateThenStartProcessThenAssertResult(firstDayNameInFirstCycle, scenario1BusinessAct1AssertTestsHelper.AssertAct4);

            // Ввод названия первого упражнения и его типа
            await CreateMessageUpdateThenStartProcessThenAssertResult($"{firstExerciseNameOnFirstDayInFirstCycle}-0", scenario1BusinessAct1AssertTestsHelper.AssertAct5);

            // Ввод названия второго упражнения и его типа
            await CreateMessageUpdateThenStartProcessThenAssertResult($"{secondExerciseNameOnFirstDayInFirstCycle}-1", scenario1BusinessAct1AssertTestsHelper.AssertAct6);

            // Нажатие на кнопку "Сохранить упражнения"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct1AssertTestsHelper.AssertAct7);

            // Нажатие на кнопку "Добавить новый день"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct1AssertTestsHelper.AssertAct8);

            // Ввод названия второго дня
            await CreateMessageUpdateThenStartProcessThenAssertResult(secondDayNameInFirstCycle, scenario1BusinessAct1AssertTestsHelper.AssertAct9);

            // Ввод названия третьего и четвёртого упражнений с их типами
            await CreateMessageUpdateThenStartProcessThenAssertResult($"{firstExerciseNameOnSecondDayInFirstCycle}-3;{secondExerciseNameOnSecondDayInFirstCycle}-3", 
                scenario1BusinessAct1AssertTestsHelper.AssertAct10);

            // Нажатие на кнопку "Сохранить упражнения"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct1AssertTestsHelper.AssertAct11);

            // Нажатие на кнопку "Перейти в главное меню"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct1AssertTestsHelper.AssertAct12);

            // Нажатие на кнопку "Начать тренировку"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct1AssertTestsHelper.AssertAct13);

            // Нажатие на кнопку "Последняя тренировка". Будет пусто, т.к. ни для одного упражнения нет результатов
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct1AssertTestsHelper.AssertAct14);

            // Нажатие на кнопку "Найти тренировку по дате"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct1AssertTestsHelper.AssertAct15);

            // Ввод рандомной даты. Будет пусто, т.к. ни для одного упражнения нет результатов
            await CreateMessageUpdateThenStartProcessThenAssertResult("13.12.2000", scenario1BusinessAct1AssertTestsHelper.AssertAct16);

            // Нажатие на кнопку "Назад"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct1AssertTestsHelper.AssertAct17);

            // Нажатие на кнопку "Назад"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct1AssertTestsHelper.AssertAct18);
        }

        /// <summary>
        /// Бизнес действие2: Заполнение упражнений (111 112 121 122)
        /// Проверяем заполнение результатов тренировки сразу после регистрации
        /// </summary>
        private async Task ExecuteAndAssertBusinessAct2()
        {
            Scenario1BusinessAct2AssertsTestsHelper scenario1BusinessAct2AssertTestsHelper = new Scenario1BusinessAct2AssertsTestsHelper(contextKeeper,
                                                                                                                                         entityContext,
                                                                                                                                         userName,
                                                                                                                                         userID,
                                                                                                                                         firstName,
                                                                                                                                         userInformationID,

                                                                                                                                         firstCycleName,

                                                                                                                                         firstDayNameInFirstCycle,
                                                                                                                                         secondDayNameInFirstCycle,

                                                                                                                                         firstExerciseNameOnFirstDayInFirstCycle,
                                                                                                                                         secondExerciseNameOnFirstDayInFirstCycle,
                                                                                                                                         firstExerciseNameOnSecondDayInFirstCycle,
                                                                                                                                         secondExerciseNameOnSecondDayInFirstCycle);

            string dateStr = DateTime.Now.ToString(CommonConsts.Common.DateFormat);

            // Нажатие на кнопку "Начать тренировку"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct2AssertTestsHelper.AssertAct1);

            // Нажатие на кнопку "11". Выбор дня
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(2, scenario1BusinessAct2AssertTestsHelper.AssertAct2);

            // Нажатие на кнопку "111". Выбор упражнения
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(2, scenario1BusinessAct2AssertTestsHelper.AssertAct3);

            // Ввод результата для упраженения "111"
            await CreateMessageUpdateThenStartProcessThenAssertResult("10", scenario1BusinessAct2AssertTestsHelper.AssertAct4);

            // Нажатие на кнопку "Сохранить результаты"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct2AssertTestsHelper.AssertAct5);

            // Нажатие на кнопку "Последние результаты выбранного дня"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct2AssertTestsHelper.AssertAct6);

            // Нажатие на кнопку "112"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(3, scenario1BusinessAct2AssertTestsHelper.AssertAct7);

            // Ввод результата для упраженения "112"
            await CreateMessageUpdateThenStartProcessThenAssertResult("10 20", scenario1BusinessAct2AssertTestsHelper.AssertAct8);

            // Нажатие на кнопку "Сохранить результаты"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct2AssertTestsHelper.AssertAct9);

            // Нажатие на кнопку "Найти тренировку этого дня по дате"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct2AssertTestsHelper.AssertAct10);

            // Ввод даты поиска тренировки дня по дате
            await CreateMessageUpdateThenStartProcessThenAssertResult(dateStr, scenario1BusinessAct2AssertTestsHelper.AssertAct11);

            // Нажатие на кнопку "Назад"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct2AssertTestsHelper.AssertAct12);

            // Нажатие на кнопку "12"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(3, scenario1BusinessAct2AssertTestsHelper.AssertAct13);

            // Нажатие на кнопку "121"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(2, scenario1BusinessAct2AssertTestsHelper.AssertAct14);

            // Ввод результата для упраженения "121"
            await CreateMessageUpdateThenStartProcessThenAssertResult("TestValue121", scenario1BusinessAct2AssertTestsHelper.AssertAct15);

            // Нажатие на кнопку "Сохранить результаты"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct2AssertTestsHelper.AssertAct16);

            // Нажатие на кнопку "Последние результаты выбранного дня"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct2AssertTestsHelper.AssertAct17);

            // Нажатие на кнопку "122"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(3, scenario1BusinessAct2AssertTestsHelper.AssertAct18);

            // Ввод результата для упраженения "122"
            await CreateMessageUpdateThenStartProcessThenAssertResult("TestValue122", scenario1BusinessAct2AssertTestsHelper.AssertAct19);

            // Нажатие на кнопку "Сохранить результаты"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct2AssertTestsHelper.AssertAct20);

            // Нажатие на кнопку "Найти тренировку этого дня по дате"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct2AssertTestsHelper.AssertAct21);

            // Ввод даты поиска тренировки дня по дате
            await CreateMessageUpdateThenStartProcessThenAssertResult(dateStr, scenario1BusinessAct2AssertTestsHelper.AssertAct22);

            // Нажатие на кнопку "Назад"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct2AssertTestsHelper.AssertAct23);

            // Нажатие на кнопку "Последняя тренировка"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct2AssertTestsHelper.AssertAct24);

            // Нажатие на кнопку "Найти тренировку по дате"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct2AssertTestsHelper.AssertAct25);

            // Ввод даты поиска тренировки среди дней по дате
            await CreateMessageUpdateThenStartProcessThenAssertResult(dateStr, scenario1BusinessAct2AssertTestsHelper.AssertAct26);

            // Нажатие на кнопку "Назад"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct2AssertTestsHelper.AssertAct27);
        }

        /// <summary>
        /// Бизнес действие3: Добавление цикла (2), двумя днями(21 22), четырьмя упражнениями(211 212 221 222)
        /// Проверяем заполнение цикла, дней и упражнений из настройки 
        /// </summary>
        private async Task ExecuteAndAssertBusinessAct3()
        {
            Scenario1BusinessAct3AssertsTestsHelper scenario1BusinessAct3AssertTestsHelper = new Scenario1BusinessAct3AssertsTestsHelper(contextKeeper,
                                                                                                                                         entityContext,
                                                                                                                                         userName,
                                                                                                                                         userID,
                                                                                                                                         firstName,
                                                                                                                                         userInformationID,

                                                                                                                                         secondCycleName,

                                                                                                                                         firstDayNameInSecondCycle,
                                                                                                                                         secondDayNameInSecondCycle,

                                                                                                                                         firstExerciseNameOnFirstDayInSecondCycle,
                                                                                                                                         secondExerciseNameOnFirstDayInSecondCycle,
                                                                                                                                         firstExerciseNameOnSecondDayInSecondCycle,
                                                                                                                                         secondExerciseNameOnSecondDayInSecondCycle);

            // Нажатие на кнопку "Настройки"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct3AssertTestsHelper.AssertAct1);

            // Нажатие на кнопку "Настройка тренировочных циклов"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct3AssertTestsHelper.AssertAct2);

            // Нажатие на кнопку "Добавить новый цикл"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct3AssertTestsHelper.AssertAct3);

            // Ввод названия второго цикла
            await CreateMessageUpdateThenStartProcessThenAssertResult(secondCycleName, scenario1BusinessAct3AssertTestsHelper.AssertAct4);
            
            // Нажатие на кнопку "Добавить дни"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct3AssertTestsHelper.AssertAct5);

            // Ввод названия дня
            await CreateMessageUpdateThenStartProcessThenAssertResult(firstDayNameInSecondCycle, scenario1BusinessAct3AssertTestsHelper.AssertAct6);

            // Нажатие на кнопку "Добавить упражнения"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct3AssertTestsHelper.AssertAct7);

            // Ввод названия первого упражнения и его типа
            await CreateMessageUpdateThenStartProcessThenAssertResult($"{firstExerciseNameOnFirstDayInSecondCycle}-0", scenario1BusinessAct3AssertTestsHelper.AssertAct8);

            // Нажатие на кнопку "Сохранить упражнения"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct3AssertTestsHelper.AssertAct9);

            // Нажатие на кнопку "Добавить новые упражнения в день"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct3AssertTestsHelper.AssertAct10);

            // Ввод названия второго упражнения и его типа
            await CreateMessageUpdateThenStartProcessThenAssertResult($"{secondExerciseNameOnFirstDayInSecondCycle}-3", scenario1BusinessAct3AssertTestsHelper.AssertAct11);

            // Нажатие на кнопку "Сохранить упражнения"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct3AssertTestsHelper.AssertAct12);

            // Нажатие на кнопку "Назад"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct3AssertTestsHelper.AssertAct13);

            // Нажатие на кнопку "Добавить новый день в цикл"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct3AssertTestsHelper.AssertAct14);

            // Ввод названия дня
            await CreateMessageUpdateThenStartProcessThenAssertResult(secondDayNameInSecondCycle, scenario1BusinessAct3AssertTestsHelper.AssertAct15);

            // Нажатие на кнопку "Добавить упражнения"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct3AssertTestsHelper.AssertAct16);

            // Ввод названия третьего и четвёртого упражнений с их типами
            await CreateMessageUpdateThenStartProcessThenAssertResult($"{firstExerciseNameOnSecondDayInSecondCycle}-1;{secondExerciseNameOnSecondDayInSecondCycle}-3",
                scenario1BusinessAct3AssertTestsHelper.AssertAct17);

            // Нажатие на кнопку "Сохранить упражнения"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct3AssertTestsHelper.AssertAct18);

            // Нажатие на кнопку "Вернуться к главному меню"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(2, scenario1BusinessAct3AssertTestsHelper.AssertAct19);
        }

        /// <summary>
        /// Бизнес действие4: Заполнение упражнений (211 212 221 222)
        /// Проверяем заполнение результатов тренировки по упражнениям, добавленным из настройки
        /// </summary>
        private async Task ExecuteAndAssertBusinessAct4()
        {
            Scenario1BusinessAct4AssertsTestsHelper scenario1BusinessAct4AssertTestsHelper = new Scenario1BusinessAct4AssertsTestsHelper(contextKeeper,
                                                                                                                                         entityContext,
                                                                                                                                         userName,
                                                                                                                                         userID,
                                                                                                                                         firstName,
                                                                                                                                         userInformationID,

                                                                                                                                         secondCycleName,

                                                                                                                                         firstDayNameInSecondCycle,
                                                                                                                                         secondDayNameInSecondCycle,

                                                                                                                                         firstExerciseNameOnFirstDayInSecondCycle,
                                                                                                                                         secondExerciseNameOnFirstDayInSecondCycle,
                                                                                                                                         firstExerciseNameOnSecondDayInSecondCycle,
                                                                                                                                         secondExerciseNameOnSecondDayInSecondCycle);

            string dateStr = DateTime.Now.ToString(CommonConsts.Common.DateFormat);

            // Нажатие на кнопку "Настройки"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct4AssertTestsHelper.AssertAct1);

            // Нажатие на кнопку "Настройка тренировочных циклов"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct4AssertTestsHelper.AssertAct2);

            // Нажатие на кнопку "Настройка существующих циклов"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct4AssertTestsHelper.AssertAct3);

            // Нажатие на кнопку "2"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct4AssertTestsHelper.AssertAct4);

            // Нажатие на кнопку "Сделать активным"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct4AssertTestsHelper.AssertAct5);

            // Нажатие на кнопку "Вернуться к главному меню"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(5, scenario1BusinessAct4AssertTestsHelper.AssertAct6);

            // Нажатие на кнопку "Начать тренировку"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct4AssertTestsHelper.AssertAct7);

            // Нажатие на кнопку "21". Выбор дня
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(2, scenario1BusinessAct4AssertTestsHelper.AssertAct8);

            // Нажатие на кнопку "211". Выбор упражнения
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(2, scenario1BusinessAct4AssertTestsHelper.AssertAct9);

            // Ввод результата для упраженения "211"
            await CreateMessageUpdateThenStartProcessThenAssertResult("20 30", scenario1BusinessAct4AssertTestsHelper.AssertAct10);

            // Нажатие на кнопку "Сохранить результаты"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct4AssertTestsHelper.AssertAct11);

            // Нажатие на кнопку "Последние результаты выбранного дня"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct4AssertTestsHelper.AssertAct12);

            // Нажатие на кнопку "212"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(3, scenario1BusinessAct4AssertTestsHelper.AssertAct13);

            // Ввод результата для упраженения "212"
            await CreateMessageUpdateThenStartProcessThenAssertResult("TestValue221", scenario1BusinessAct4AssertTestsHelper.AssertAct14);

            // Нажатие на кнопку "Сохранить результаты"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct4AssertTestsHelper.AssertAct15);

            // Нажатие на кнопку "Найти тренировку этого дня по дате"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct4AssertTestsHelper.AssertAct16);

            // Ввод даты поиска тренировки дня по дате
            await CreateMessageUpdateThenStartProcessThenAssertResult(dateStr, scenario1BusinessAct4AssertTestsHelper.AssertAct17);

            // Нажатие на кнопку "Назад"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct4AssertTestsHelper.AssertAct18);

            // Нажатие на кнопку "22"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(3, scenario1BusinessAct4AssertTestsHelper.AssertAct19);

            // Нажатие на кнопку "221"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(2, scenario1BusinessAct4AssertTestsHelper.AssertAct20);

            // Ввод результата для упраженения "221"
            await CreateMessageUpdateThenStartProcessThenAssertResult("20 40;30 60", scenario1BusinessAct4AssertTestsHelper.AssertAct21);

            // Нажатие на кнопку "Сохранить результаты"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct4AssertTestsHelper.AssertAct22);

            // Нажатие на кнопку "Последние результаты выбранного дня"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct4AssertTestsHelper.AssertAct23);

            // Нажатие на кнопку "222"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(3, scenario1BusinessAct4AssertTestsHelper.AssertAct24);

            // Ввод результата для упраженения "222"
            await CreateMessageUpdateThenStartProcessThenAssertResult("TestValue222", scenario1BusinessAct4AssertTestsHelper.AssertAct25);

            // Нажатие на кнопку "Сохранить результаты"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct4AssertTestsHelper.AssertAct26);

            // Нажатие на кнопку "Найти тренировку этого дня по дате"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct4AssertTestsHelper.AssertAct27);

            // Ввод даты поиска тренировки дня по дате
            await CreateMessageUpdateThenStartProcessThenAssertResult(dateStr, scenario1BusinessAct4AssertTestsHelper.AssertAct28);

            // Нажатие на кнопку "Назад"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct4AssertTestsHelper.AssertAct29);

            // Нажатие на кнопку "Последняя тренировка"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct4AssertTestsHelper.AssertAct30);

            // Нажатие на кнопку "Найти тренировку по дате"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct4AssertTestsHelper.AssertAct31);

            // Ввод даты поиска тренировки среди дней по дате
            await CreateMessageUpdateThenStartProcessThenAssertResult(dateStr, scenario1BusinessAct4AssertTestsHelper.AssertAct32);

            // Нажатие на кнопку "Назад"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct4AssertTestsHelper.AssertAct33);
        }

        /// <summary>
        /// Бизнес действие5: Переименование, изменение типа, архивирование, удаление результатов и удаления упражнения 222, перемещение упражнения 221 из дня 22 в день 21
        /// </summary>
        private async Task ExecuteAndAssertBusinessAct5()
        {
            Scenario1BusinessAct5AssertsTestsHelper scenario1BusinessAct5AssertTestsHelper = new Scenario1BusinessAct5AssertsTestsHelper(contextKeeper,
                                                                                                                                         entityContext,
                                                                                                                                         userName,
                                                                                                                                         userID,
                                                                                                                                         firstName,
                                                                                                                                         userInformationID,

                                                                                                                                         secondCycleName,
  
                                                                                                                                         firstDayNameInSecondCycle,
                                                                                                                                         secondDayNameInSecondCycle,

                                                                                                                                         firstExerciseNameOnSecondDayInSecondCycle,
                                                                                                                                         secondExerciseNameOnSecondDayInSecondCycle);

            // Нажатие на кнопку "Настройки"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct5AssertTestsHelper.AssertAct1);

            // Нажатие на кнопку "Настройка тренировочных циклов"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct5AssertTestsHelper.AssertAct2);

            // Нажатие на кнопку "Настройка существующих циклов"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct5AssertTestsHelper.AssertAct3);

            // Нажатие на кнопку "2"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct5AssertTestsHelper.AssertAct4);

            // Нажатие на кнопку "Настройка дней"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(4, scenario1BusinessAct5AssertTestsHelper.AssertAct5);

            // Нажатие на кнопку "Настройка существующих дней"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct5AssertTestsHelper.AssertAct6);

            // Нажатие на кнопку "22"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct5AssertTestsHelper.AssertAct7);

            // Нажатие на кнопку "Настройка упражнений"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(4, scenario1BusinessAct5AssertTestsHelper.AssertAct8);

            // Нажатие на кнопку "Настройка существующих упражнений"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct5AssertTestsHelper.AssertAct9);

            // Нажатие на кнопку "222"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct5AssertTestsHelper.AssertAct10);

            // Нажатие на кнопку "Сменить название"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct5AssertTestsHelper.AssertAct11);

            // Ввод нового названия для упражения "222"
            await CreateMessageUpdateThenStartProcessThenAssertResult("222ToDelete", scenario1BusinessAct5AssertTestsHelper.AssertAct12);

            // Нажатие на кнопку "Сменить тип"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct5AssertTestsHelper.AssertAct13);

            // Нажатие на кнопку "Режим: таймер"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(2, scenario1BusinessAct5AssertTestsHelper.AssertAct14);

            // Нажатие на кнопку "Добавить в архив"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(3, scenario1BusinessAct5AssertTestsHelper.AssertAct15);

            // Нажатие на кнопку "Назад"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct5AssertTestsHelper.AssertAct16);

            // Нажатие на кнопку "Вернуться к главному меню"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(2, scenario1BusinessAct5AssertTestsHelper.AssertAct17);

            // Нажатие на кнопку "Настройки"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct5AssertTestsHelper.AssertAct1);

            // Нажатие на кнопку "Архив"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct5AssertTestsHelper.AssertAct18);

            // Нажатие на кнопку "Архивированные упражнения"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(2, scenario1BusinessAct5AssertTestsHelper.AssertAct19);

            // Нажатие на кнопку "222ToDelete[Id:8]"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct5AssertTestsHelper.AssertAct20);

            // Нажатие на кнопку "Назад"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct5AssertTestsHelper.AssertAct1);

            // Нажатие на кнопку "Настройка тренировочных циклов"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct5AssertTestsHelper.AssertAct2);

            // Нажатие на кнопку "Настройка существующих циклов"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct5AssertTestsHelper.AssertAct3);

            // Нажатие на кнопку "2"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct5AssertTestsHelper.AssertAct4);

            // Нажатие на кнопку "Настройка дней"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(4, scenario1BusinessAct5AssertTestsHelper.AssertAct5);

            // Нажатие на кнопку "Настройка существующих дней"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct5AssertTestsHelper.AssertAct6);

            // Нажатие на кнопку "22"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct5AssertTestsHelper.AssertAct7);

            // Нажатие на кнопку "Настройка упражнений"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(4, scenario1BusinessAct5AssertTestsHelper.AssertAct8);

            // Нажатие на кнопку "Настройка существующих упражнений"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct5AssertTestsHelper.AssertAct9);

            // Нажатие на кнопку "222ToDelete"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct5AssertTestsHelper.AssertAct21);

            // Нажатие на кнопку "Удалить результаты"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(5, scenario1BusinessAct5AssertTestsHelper.AssertAct22);

            // Ввод кол-ва последних результатов упражения "222ToDelete" для удаления 
            await CreateMessageUpdateThenStartProcessThenAssertResult("1", scenario1BusinessAct5AssertTestsHelper.AssertAct23);

            // Нажатие на кнопку "Удалить"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(4, scenario1BusinessAct5AssertTestsHelper.AssertAct24);

            // Нажатие на кнопку "Да, удалить упражнение"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct5AssertTestsHelper.AssertAct25);

            // Нажатие на кнопку "221"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct5AssertTestsHelper.AssertAct26);

            // Нажатие на кнопку "Перенести упражнение"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(2, scenario1BusinessAct5AssertTestsHelper.AssertAct27);

            // Нажатие на кнопку "21[Id:3]"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(2, scenario1BusinessAct5AssertTestsHelper.AssertAct28);

            // Нажатие на кнопку "Назад"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct5AssertTestsHelper.AssertAct29);

            // Нажатие на кнопку "Вернуться к главному меню"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(2, scenario1BusinessAct5AssertTestsHelper.AssertAct30);
        }

        /// <summary>
        /// Бизнес действие6: Переименование, архивирование и удаления дня 222, перемещение дня 21 из цикла 2 в цикл 1
        /// </summary>
        private async Task ExecuteAndAssertBusinessAct6()
        {
            Scenario1BusinessAct6AssertsTestsHelper scenario1BusinessAct6AssertTestsHelper = new Scenario1BusinessAct6AssertsTestsHelper(contextKeeper,
                                                                                                                                         entityContext,
                                                                                                                                         userName,
                                                                                                                                         userID,
                                                                                                                                         firstName,
                                                                                                                                         userInformationID,

                                                                                                                                         firstCycleName,
                                                                                                                                         secondCycleName,

                                                                                                                                         firstDayNameInSecondCycle,
                                                                                                                                         secondDayNameInSecondCycle);

            // Нажатие на кнопку "Настройки"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct6AssertTestsHelper.AssertAct1);

            // Нажатие на кнопку "Настройка тренировочных циклов"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct6AssertTestsHelper.AssertAct2);

            // Нажатие на кнопку "Настройка существующих циклов"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct6AssertTestsHelper.AssertAct3);

            // Нажатие на кнопку "2"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct6AssertTestsHelper.AssertAct4);

            // Нажатие на кнопку "Настройка дней"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(4, scenario1BusinessAct6AssertTestsHelper.AssertAct5);

            // Нажатие на кнопку "Настройка существующих дней"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct6AssertTestsHelper.AssertAct6);

            // Нажатие на кнопку "22"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct6AssertTestsHelper.AssertAct7);

            // Нажатие на кнопку "Сменить название"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct6AssertTestsHelper.AssertAct8);

            // Ввод нового названия для дня "22"
            await CreateMessageUpdateThenStartProcessThenAssertResult("22ToDelete", scenario1BusinessAct6AssertTestsHelper.AssertAct9);

            // Нажатие на кнопку "Добавить в архив"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(2, scenario1BusinessAct6AssertTestsHelper.AssertAct10);

            // Нажатие на кнопку "Назад"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct6AssertTestsHelper.AssertAct11);

            // Нажатие на кнопку "Вернуться к главному меню"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(2, scenario1BusinessAct6AssertTestsHelper.AssertAct12);

            // Нажатие на кнопку "Настройки"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct6AssertTestsHelper.AssertAct1);

            // Нажатие на кнопку "Архив"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct6AssertTestsHelper.AssertAct13);

            // Нажатие на кнопку "Архивированные дни"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct6AssertTestsHelper.AssertAct14);

            // Нажатие на кнопку "22ToDelete[Id:4]"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct6AssertTestsHelper.AssertAct15);

            // Нажатие на кнопку "Назад"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct6AssertTestsHelper.AssertAct1);

            // Нажатие на кнопку "Настройка тренировочных циклов"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct6AssertTestsHelper.AssertAct2);

            // Нажатие на кнопку "Настройка существующих циклов"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct6AssertTestsHelper.AssertAct3);

            // Нажатие на кнопку "2"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct6AssertTestsHelper.AssertAct4);

            // Нажатие на кнопку "Настройка дней"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(4, scenario1BusinessAct6AssertTestsHelper.AssertAct5);

            // Нажатие на кнопку "Настройка существующих дней"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct6AssertTestsHelper.AssertAct6);

            // Нажатие на кнопку "22ToDelete"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct6AssertTestsHelper.AssertAct16);

            // Нажатие на кнопку "Удалить"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(3, scenario1BusinessAct6AssertTestsHelper.AssertAct17);

            // Нажатие на кнопку "Да, удалить день"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct6AssertTestsHelper.AssertAct18);

            // Нажатие на кнопку "21"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct6AssertTestsHelper.AssertAct19);

            // Нажатие на кнопку "Перенести день"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct6AssertTestsHelper.AssertAct20);

            // Нажатие на кнопку "1[Id:1]"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct6AssertTestsHelper.AssertAct21);

            // Нажатие на кнопку "Назад"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct6AssertTestsHelper.AssertAct22);

            // Нажатие на кнопку "Вернуться к главному меню"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(2, scenario1BusinessAct6AssertTestsHelper.AssertAct23);
        }

        /// <summary>
        /// Бизнес действие7: Переименование, архивирование и удаления цикла 2
        /// </summary>
        private async Task ExecuteAndAssertBusinessAct7()
        {
            Scenario1BusinessAct7AssertsTestsHelper scenario1BusinessAct7AssertTestsHelper = new Scenario1BusinessAct7AssertsTestsHelper(contextKeeper,
                                                                                                                                         entityContext,
                                                                                                                                         userName,
                                                                                                                                         userID,
                                                                                                                                         firstName,
                                                                                                                                         userInformationID,

                                                                                                                                         firstCycleName,
                                                                                                                                         secondCycleName);

            // Нажатие на кнопку "Настройки"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct7AssertTestsHelper.AssertAct1);

            // Нажатие на кнопку "Настройка тренировочных циклов"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct7AssertTestsHelper.AssertAct2);

            // Нажатие на кнопку "Настройка существующих циклов"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct7AssertTestsHelper.AssertAct3);

            // Нажатие на кнопку "1"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct7AssertTestsHelper.AssertAct4);

            // Нажатие на кнопку "Сделать активным"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct7AssertTestsHelper.AssertAct5);

            // Нажатие на кнопку "Назад"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct7AssertTestsHelper.AssertAct6);

            // Нажатие на кнопку "Настройка существующих циклов"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct7AssertTestsHelper.AssertAct3);

            // Нажатие на кнопку "2"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct7AssertTestsHelper.AssertAct7);

            // Нажатие на кнопку "Сменить название"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct7AssertTestsHelper.AssertAct8);

            // Ввод нового названия для цикла "2"
            await CreateMessageUpdateThenStartProcessThenAssertResult("2ToDelete", scenario1BusinessAct7AssertTestsHelper.AssertAct9);

            // Нажатие на кнопку "Добавить в архив"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(2, scenario1BusinessAct7AssertTestsHelper.AssertAct10);

            // Нажатие на кнопку "Назад"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct7AssertTestsHelper.AssertAct11);

            // Нажатие на кнопку "Назад"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct7AssertTestsHelper.AssertAct1);

            // Нажатие на кнопку "Архив"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct7AssertTestsHelper.AssertAct12);

            // Нажатие на кнопку "Архивированные циклы"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct7AssertTestsHelper.AssertAct13);

            // Нажатие на кнопку "2ToDelete[Id:2]"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct7AssertTestsHelper.AssertAct14);

            // Нажатие на кнопку "Назад"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct7AssertTestsHelper.AssertAct1);

            // Нажатие на кнопку "Настройка тренировочных циклов"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct7AssertTestsHelper.AssertAct2);

            // Нажатие на кнопку "Настройка существующих циклов"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct7AssertTestsHelper.AssertAct3);

            // Нажатие на кнопку "2ToDelete"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct7AssertTestsHelper.AssertAct15);

            // Нажатие на кнопку "Удалить"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(3, scenario1BusinessAct7AssertTestsHelper.AssertAct16);

            // Нажатие на кнопку "Да, удалить цикл"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct7AssertTestsHelper.AssertAct17);

            // Нажатие на кнопку "Назад"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct7AssertTestsHelper.AssertAct18);

            // Нажатие на кнопку "Вернуться к главному меню"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(2, scenario1BusinessAct7AssertTestsHelper.AssertAct19);
        }

        /// <summary>
        /// Бизнес действие8: По отдельности добавление цикла, потом к нему дня, потом к нему упражнения, потом к нему результата упражения
        /// </summary>
        private async Task ExecuteAndAssertBusinessAct8()
        {
            Scenario1BusinessAct8AssertsTestsHelper scenario1BusinessAct8AssertTestsHelper = new Scenario1BusinessAct8AssertsTestsHelper(contextKeeper,
                                                                                                                                         entityContext,
                                                                                                                                         userName,
                                                                                                                                         userID,
                                                                                                                                         firstName,
                                                                                                                                         userInformationID,

                                                                                                                                         thirdCycleName,

                                                                                                                                         firstDayNameInThirdCycle,

                                                                                                                                         firstExerciseNameOnFirstDayInThirdCycle);

            // Нажатие на кнопку "Настройки"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct8AssertTestsHelper.AssertAct1);

            // Нажатие на кнопку "Настройка тренировочных циклов"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct8AssertTestsHelper.AssertAct2);

            // Нажатие на кнопку "Добавить новый цикл"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct8AssertTestsHelper.AssertAct3);

            // Ввод названия третьего цикла
            await CreateMessageUpdateThenStartProcessThenAssertResult(thirdCycleName, scenario1BusinessAct8AssertTestsHelper.AssertAct4);

            // Нажатие на кнопку "Назад"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct8AssertTestsHelper.AssertAct5);

            // Нажатие на кнопку "Настройка тренировочных циклов"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct8AssertTestsHelper.AssertAct6);

            // Нажатие на кнопку "thirdCycle"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct8AssertTestsHelper.AssertAct7);

            // Нажатие на кнопку "Настройка дней"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(4, scenario1BusinessAct8AssertTestsHelper.AssertAct8);

            // Нажатие на кнопку "Добавить новый день в цикл"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct8AssertTestsHelper.AssertAct9);

            // Ввод названия третьего дня
            await CreateMessageUpdateThenStartProcessThenAssertResult(firstDayNameInThirdCycle, scenario1BusinessAct8AssertTestsHelper.AssertAct10);

            // Нажатие на кнопку "Назад"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct8AssertTestsHelper.AssertAct11);

            // Нажатие на кнопку "Настройка существующих дней"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct8AssertTestsHelper.AssertAct12);

            // Нажатие на кнопку "thirdDay"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct8AssertTestsHelper.AssertAct13);

            // Нажатие на кнопку "Настройка упражнений"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(4, scenario1BusinessAct8AssertTestsHelper.AssertAct14);

            // Нажатие на кнопку "Добавить новые упражнения в день"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct8AssertTestsHelper.AssertAct15);

            // Ввод названия третьего упражнения и его типа
            await CreateMessageUpdateThenStartProcessThenAssertResult($"{firstExerciseNameOnFirstDayInThirdCycle}-0", scenario1BusinessAct8AssertTestsHelper.AssertAct16);

            // Нажатие на кнопку "Сохранить упражнения"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct8AssertTestsHelper.AssertAct17);

            // Нажатие на кнопку "Вернуться к главному меню"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(2, scenario1BusinessAct8AssertTestsHelper.AssertAct18);

            // Нажатие на кнопку "Настройки"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct8AssertTestsHelper.AssertAct1);

            // Нажатие на кнопку "Настройка тренировочных циклов"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct8AssertTestsHelper.AssertAct2);

            // Нажатие на кнопку "Настройка существующих циклов"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct8AssertTestsHelper.AssertAct6);

            // Нажатие на кнопку "thirdCycle"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(1, scenario1BusinessAct8AssertTestsHelper.AssertAct7);

            // Нажатие на кнопку "Сделать активным"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct8AssertTestsHelper.AssertAct19);

            // Нажатие на кнопку "Вернуться к главному меню"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(5, scenario1BusinessAct8AssertTestsHelper.AssertAct20);

            // Нажатие на кнопку "Начать тренировку"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct8AssertTestsHelper.AssertAct21);

            // Нажатие на кнопку "tDay"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(2, scenario1BusinessAct8AssertTestsHelper.AssertAct22);

            // Нажатие на кнопку "tExercise"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(2, scenario1BusinessAct8AssertTestsHelper.AssertAct23);

            // Ввод названия третьего цикла
            await CreateMessageUpdateThenStartProcessThenAssertResult("40", scenario1BusinessAct8AssertTestsHelper.AssertAct24);

            // Нажатие на кнопку "Сохранить результаты"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct8AssertTestsHelper.AssertAct25);

            // Нажатие на кнопку "Последние результаты выбранного дня"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct8AssertTestsHelper.AssertAct26);

            // Нажатие на кнопку "Вернуться к главному меню"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(3, scenario1BusinessAct8AssertTestsHelper.AssertAct27);
        }

        /// <summary>
        /// Бизнес действие9: Самоудаление юзера TestUser
        /// Проверяем, что юзер TestUser успешно удалился вместе со своими циклами, днями упражнениями и результами упражнений и он может повторно пользоваться ботом
        /// </summary>
        private async Task ExecuteAndAssertBusinessAct9()
        {
            Scenario1BusinessAct9AssertsTestsHelper scenario1BusinessAct9AssertTestsHelper = new Scenario1BusinessAct9AssertsTestsHelper(contextKeeper,
                                                                                                                                         entityContext,
                                                                                                                                         userName,
                                                                                                                                         userID,
                                                                                                                                         firstName,
                                                                                                                                         userInformationID);

            // Нажатие на кнопку "Настройки"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(-1, scenario1BusinessAct9AssertTestsHelper.AssertAct1);

            // Нажатие на кнопку "Удалить свой аккаунт"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(3, scenario1BusinessAct9AssertTestsHelper.AssertAct2);

            // Нажатие на кнопку "Да, удалить аккаунт"
            await CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(0, scenario1BusinessAct9AssertTestsHelper.AssertAct3, false);

            // Любое обращение к боту после удаления аккаунта считается первым для юзера
            await CreateMessageUpdateThenStartProcessThenAssertResult("/Some", scenario1BusinessAct9AssertTestsHelper.AssertAct4);
        }

        private async Task CreateMessageUpdateThenStartProcessThenAssertResult(string text, Func<List<HandlerResult>, Task> assertAction)
        {
            Update update = commonTestsHelper.GetMessageUpdate(text);

            List<HandlerResult> handlerResults = await commonTestsHelper.StartProcess(update);

            await assertAction.Invoke(handlerResults);

            SetupLastSuggestedButtons(handlerResults);

            SetupLastSuggestedButtonsStr();
        }

        private async Task CreateCallBackUpdateBySpecifiedButtonIndexThenStartProcessThenAssertResults(int specifiedButtonIndex, 
                                                                                                       Func<List<HandlerResult>, Task> assertAction, 
                                                                                                       bool withSetupLastSuggestedButtons = true)
        {
            specifiedButtonIndex.Should().BeGreaterThanOrEqualTo(-1);

            string callBackData;

            if (specifiedButtonIndex == -1)
                callBackData = GetLastCallBackDataFromInlineKeyboardButtons(lastSuggestedButtons);
            else if (specifiedButtonIndex == 0)
                callBackData = GetFirstCallBackDataFromInlineKeyboardButtons(lastSuggestedButtons);
            else
                callBackData = GetSpecifiedCallBackDataFromInlineKeyboardButtons(lastSuggestedButtons, specifiedButtonIndex);

            await CreateCallBackUpdateThenStartProcessThenAssertResults(callBackData, assertAction, withSetupLastSuggestedButtons);
        }
        
        private async Task CreateCallBackUpdateThenStartProcessThenAssertResults(string data, Func<List<HandlerResult>, Task> assertAction, bool withSetupLastSuggestedButtons)
        {
            Update update = commonTestsHelper.GetCallBackUpdate(data);

            List<HandlerResult> handlerResults = await commonTestsHelper.StartProcess(update);

            await assertAction.Invoke(handlerResults);

            if (withSetupLastSuggestedButtons)
            {
                SetupLastSuggestedButtons(handlerResults);

                SetupLastSuggestedButtonsStr();
            }
        }

        private void SetupLastSuggestedButtons(List<HandlerResult> handlerResults)
        {
            UserContext? userContext = contextKeeper.GetContext(userID);
            userContext.Should().NotBeNull();

            AuthorizedHandlerResult? authorizedHandlerResult = handlerResults.LastOrDefault() as AuthorizedHandlerResult;
            authorizedHandlerResult.Should().NotBeNull();
            IInformationSet? informationSet = authorizedHandlerResult.InformationSet;
            informationSet.Should().NotBeNull();

            lastSuggestedButtons = commonTestsHelper.GetCallBackRoutingToNextActFromVerticalButtons(userContext, informationSet.ButtonsSets, informationSet.AdditionalParameters);
        }

        private void SetupLastSuggestedButtonsStr()
        {
            int buttonInd = 0;

            lastSuggestedButtonsStr = string.Join(Environment.NewLine, lastSuggestedButtons.Select(x =>
                {
                return $"{x.CallbackData} - {x.Text} ({buttonInd++})";
            }));
        }

        private string GetFirstCallBackDataFromInlineKeyboardButtons(IEnumerable<InlineKeyboardButton> inlineKeyboardButtons)
        {
            InlineKeyboardButton? inlineKeyboardButton = inlineKeyboardButtons.FirstOrDefault();
            inlineKeyboardButton.Should().NotBeNull();
            inlineKeyboardButton.CallbackData.Should().NotBeNullOrWhiteSpace();

            return inlineKeyboardButton.CallbackData;
        }

        private string GetSpecifiedCallBackDataFromInlineKeyboardButtons(IEnumerable<InlineKeyboardButton> inlineKeyboardButtons, int specifiedButtonIndex)
        {
            InlineKeyboardButton? inlineKeyboardButton = inlineKeyboardButtons.Skip(specifiedButtonIndex).FirstOrDefault();
            inlineKeyboardButton.Should().NotBeNull();
            inlineKeyboardButton.CallbackData.Should().NotBeNullOrWhiteSpace();

            return inlineKeyboardButton.CallbackData;
        }

        private string GetLastCallBackDataFromInlineKeyboardButtons(IEnumerable<InlineKeyboardButton> inlineKeyboardButtons)
        {
            InlineKeyboardButton? inlineKeyboardButton = inlineKeyboardButtons.LastOrDefault();
            inlineKeyboardButton.Should().NotBeNull();
            inlineKeyboardButton.CallbackData.Should().NotBeNullOrWhiteSpace();

            return inlineKeyboardButton.CallbackData;
        }

        public void Dispose()
        {
            dbConnection.Dispose();
            entityContext.Dispose();
            cts.Dispose();
        }
    }
}