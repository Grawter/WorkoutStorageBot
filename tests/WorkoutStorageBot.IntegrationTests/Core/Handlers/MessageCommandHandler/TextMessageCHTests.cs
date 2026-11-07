using FluentAssertions;
using FluentAssertions.Specialized;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Global;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Extensions;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.MessageCommandHandler.Context;
using WorkoutStorageBot.BusinessLogic.Helpers.Converters;
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
using WorkoutStorageModels.Entities.BusinessLogic;
using WorkoutStorageModels.Entities.Core.Logging;

namespace WorkoutStorageBot.IntegrationTests.Core.Handlers.MessageCommandHandler
{
    public class TextMessageCHTests : IDisposable
    {
        private readonly EntityContextBuilder builder;

        private readonly EntityContext entityContext;

        private readonly Mock<IContextKeeper> contextKeeperMock;

        private readonly Mock<IBotResponseSender> botResponseSenderMock;

        private readonly Mock<ICustomLoggerFactory> customLoggerFactoryMock;

        private readonly Mock<ILogger> loggerMock;

        private readonly CoreTools coreTools;

        public TextMessageCHTests()
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

            coreTools = new CoreTools()
            {
                ConfigurationData = new ConfigurationData() { DB = new DbSettings() { Database = "test" } },
                Db = entityContext,
                ContextKeeper = contextKeeperMock.Object,
                BotResponseSender = botResponseSenderMock.Object,
                LoggerFactory = customLoggerFactoryMock.Object,
            };
        }

        [Theory]
        [InlineData("/start", false)]
        [InlineData("/start", true)]
        [InlineData("Other", false)]
        public async Task GetInformationSet_WithDefaultNavigation_TargetShouldReturnExpectedIInformationSet(string data, bool hasActiveCycle)
        {
            // Arrange
            MessageNavigationTarget messageNavigationTarget = MessageNavigationTarget.Default;

            if (hasActiveCycle)
            {
                builder.WithCycle(isActive: hasActiveCycle);
            }

            DTOUserInformation localDTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();
            UserContext localUserContext = new UserContext(localDTOCurrentUser, Roles.User | Roles.Admin, false);

            Update update = new Update();

            CommandHandlerTools localCommandHandlerTools = new CommandHandlerTools()
            {
                CoreTools = coreTools,
                HandlerResult = new UpdateHandlerResult()
                {
                    CurrentUserContext = localUserContext,
                    Update = update,
                    ShortUpdateInfo = new ShortUpdateInfo(update, new User(), 1, "...", UpdateType.Message, true)
                },
                RepositoriesStore = new RepositoriesStore(entityContext),
            };

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetMessageNavigationTarget(messageNavigationTarget);

            MessageTextBuilder requestTextBuilder = new MessageTextBuilder(data);

            TextMessageCH textMessageCH = new TextMessageCH(localCommandHandlerTools, requestTextBuilder);

            // Act
            await textMessageCH.Process(TestContext.Current.CancellationToken);

            IInformationSet? informationSet = localCommandHandlerTools.HandlerResult.InformationSet;
            informationSet.Should().NotBeNull();

            // Assert
            if (data == "Other")
            {
                informationSet.Message.Should().Be(@$"Неизвестная команда: {data.AddBoldAndQuotes()}
======================

Для получения разделов воспользуйтесь командой ""<b>/Start</b>""
");
                informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));

                localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            }
            else
            {
                if (hasActiveCycle)
                {
                    informationSet.Message.Should().Be("Выберите интересующий раздел");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.Main, ButtonsSet.None));

                    localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
                }
                else
                {
                    informationSet.Message.Should().Be("Начнём");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.AddCycle, ButtonsSet.None));

                    localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.Start);
                }

                localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                localCommandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                informationSet.ParseMode.Should().Be(ParseMode.Html);
                informationSet.AdditionalParameters.Should().BeNull();
            }
        }

        [Theory]
        [InlineData(true, false, QueryFrom.NoMatter)]
        [InlineData(true, true, QueryFrom.NoMatter)]
        [InlineData(true, false, QueryFrom.Start)]
        [InlineData(true, true, QueryFrom.Start)]
        [InlineData(true, false, QueryFrom.Settings)]
        [InlineData(true, true, QueryFrom.Settings)]
        [InlineData(false, false, QueryFrom.NoMatter)]
        [InlineData(false, true, QueryFrom.NoMatter)]
        [InlineData(false, false, QueryFrom.Start)]
        [InlineData(false, true, QueryFrom.Start)]
        [InlineData(false, false, QueryFrom.Settings)]
        [InlineData(false, true, QueryFrom.Settings)]
        public async Task GetInformationSet_WithAddCycleNavigationTarget_ShouldReturnExpectedIInformationSet(bool hasAlreadyExistDomain, bool hasActiveCycle, QueryFrom queryFrom)
        {
            // Arrange
            MessageNavigationTarget messageNavigationTarget = MessageNavigationTarget.AddCycle;

            if (hasAlreadyExistDomain || hasActiveCycle)
            {
                if (hasAlreadyExistDomain)
                    builder.WithCycle("newTestCycle", hasActiveCycle);
                else
                    builder.WithCycle(isActive: hasActiveCycle);
            }

            DTOUserInformation localDTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();
            UserContext localUserContext = new UserContext(localDTOCurrentUser, Roles.User | Roles.Admin, false);

            Update update = new Update();

            CommandHandlerTools localCommandHandlerTools = new CommandHandlerTools()
            {
                CoreTools = coreTools,
                HandlerResult = new UpdateHandlerResult()
                {
                    CurrentUserContext = localUserContext,
                    Update = update,
                    ShortUpdateInfo = new ShortUpdateInfo(update, new User(), 1, "...", UpdateType.Message, true)
                },
                RepositoriesStore = new RepositoriesStore(entityContext),
            };

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetMessageNavigationTarget(messageNavigationTarget);
            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetQueryFrom(queryFrom);

            string data = "newTestCycle";
            MessageTextBuilder requestTextBuilder = new MessageTextBuilder(data);

            TextMessageCH textMessageCH = new TextMessageCH(localCommandHandlerTools, requestTextBuilder);

            // Assert
            if (hasAlreadyExistDomain)
            {
                if (queryFrom == QueryFrom.NoMatter)
                {
                    // Act
                    Func<Task> func = async () => await textMessageCH.Process(TestContext.Current.CancellationToken);

                    ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                    exceptionAssertions.WithMessage($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {queryFrom}");
                }
                else
                {
                    // Act
                    await textMessageCH.Process(TestContext.Current.CancellationToken);

                    IInformationSet? informationSet = localCommandHandlerTools.HandlerResult.InformationSet;
                    informationSet.Should().NotBeNull();

                    informationSet.Message.Should().Be(@$"Ошибка при добавлении названия!
======================
Цикл с названием {data.AddBoldAndQuotes()} уже существует
======================

Ввведите другое название тренировочного цикла
");
                    if (queryFrom == QueryFrom.Start)
                        informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));
                    else if (queryFrom == QueryFrom.Settings)
                        informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingCycles));


                    if (hasActiveCycle)
                        localCommandHandlerTools.HandlerResult.CurrentUserContext.ActiveCycle.Should().NotBeNull();

                    localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(queryFrom);
                    localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(messageNavigationTarget);

                    localCommandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                    informationSet.ParseMode.Should().Be(ParseMode.Html);
                    informationSet.AdditionalParameters.Should().BeNull();
                }
            }
            else
            {
                if (queryFrom == QueryFrom.NoMatter)
                {
                    // Act
                    Func<Task> func = async () => await textMessageCH.Process(TestContext.Current.CancellationToken);

                    ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                    exceptionAssertions.WithMessage($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {queryFrom}");
                }
                else
                {
                    if (!hasActiveCycle)
                        localCommandHandlerTools.HandlerResult.CurrentUserContext.ActiveCycle.Should().BeNull();

                    entityContext.Cycles.FirstOrDefault(x => x.Name == data && x.UserInformationId == localDTOCurrentUser.Id).Should().BeNull();

                    // Act
                    await textMessageCH.Process(TestContext.Current.CancellationToken);

                    IInformationSet? informationSet = localCommandHandlerTools.HandlerResult.InformationSet;
                    informationSet.Should().NotBeNull();

                    if (queryFrom == QueryFrom.Start)
                    {
                        localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddDays);

                        informationSet.Message.Should().Be(@$"Цикл {data.AddBoldAndQuotes()} сохранён
======================

Введите название тренирочного дня для цикла {data.AddBoldAndQuotes()}
");
                        informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));
                    }
                    else
                    {
                        localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                        informationSet.Message.Should().Be(@$"Цикл {data.AddBoldAndQuotes()} сохранён
======================

Выберите дальнейшие действия
");
                        informationSet.ButtonsSets.Should().Be((ButtonsSet.AddDays, ButtonsSet.SettingCycles));
                    }

                    entityContext.Cycles.FirstOrDefault(x => x.Name == data && x.UserInformationId == localDTOCurrentUser.Id).Should().NotBeNull();

                    if (!hasActiveCycle)
                        localCommandHandlerTools.HandlerResult.CurrentUserContext.ActiveCycle.Should().NotBeNull();

                    localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(queryFrom);

                    localCommandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                    informationSet.ParseMode.Should().Be(ParseMode.Html);
                    informationSet.AdditionalParameters.Should().BeNull();
                }
            }
        }

        [Theory]
        [InlineData(true, QueryFrom.NoMatter)]
        [InlineData(true, QueryFrom.Start)]
        [InlineData(true, QueryFrom.Settings)]
        [InlineData(false, QueryFrom.NoMatter)]
        [InlineData(false, QueryFrom.Start)]
        [InlineData(false, QueryFrom.Settings)]
        public async Task GetInformationSet_WithAddDaysNavigationTarget_ShouldReturnExpectedIInformationSet(bool hasAlreadyExistDomain, QueryFrom queryFrom)
        {
            // Arrange
            MessageNavigationTarget messageNavigationTarget = MessageNavigationTarget.AddDays;

            builder.WithCycle();

            if (hasAlreadyExistDomain)
            {
                builder.WithDay("newTestDay");
            }

            DTOUserInformation localDTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();
            UserContext localUserContext = new UserContext(localDTOCurrentUser, Roles.User | Roles.Admin, false);

            Update update = new Update();

            CommandHandlerTools localCommandHandlerTools = new CommandHandlerTools()
            {
                CoreTools = coreTools,
                HandlerResult = new UpdateHandlerResult()
                {
                    CurrentUserContext = localUserContext,
                    Update = update,
                    ShortUpdateInfo = new ShortUpdateInfo(update, new User(), 1, "...", UpdateType.Message, true)
                },
                RepositoriesStore = new RepositoriesStore(entityContext),
            };

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetMessageNavigationTarget(messageNavigationTarget);
            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetQueryFrom(queryFrom);

            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestCycle.ToDTOCycle());

            string data = "newTestDay";
            MessageTextBuilder requestTextBuilder = new MessageTextBuilder(data);

            TextMessageCH textMessageCH = new TextMessageCH(localCommandHandlerTools, requestTextBuilder);

            // Assert
            if (hasAlreadyExistDomain)
            {
                if (queryFrom == QueryFrom.NoMatter)
                {
                    // Act
                    Func<Task> func = async () => await textMessageCH.Process(TestContext.Current.CancellationToken);

                    ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                    exceptionAssertions.WithMessage($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {queryFrom}");
                }
                else
                {
                    // Act
                    await textMessageCH.Process(TestContext.Current.CancellationToken);

                    IInformationSet? informationSet = localCommandHandlerTools.HandlerResult.InformationSet;
                    informationSet.Should().NotBeNull();

                    informationSet.Message.Should().Be(@$"Ошибка при сохранении!
======================
В этом цикле уже существует день с названием {data.AddBoldAndQuotes()}
======================

Ввведите другое название дня для цикла {builder.TestCycle.Name.AddBoldAndQuotes()}
");
                    if (queryFrom == QueryFrom.Start)
                        informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));
                    else if (queryFrom == QueryFrom.Settings)
                        informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingDays));

                    localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(queryFrom);
                    localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(messageNavigationTarget);

                    localCommandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                    informationSet.ParseMode.Should().Be(ParseMode.Html);
                    informationSet.AdditionalParameters.Should().BeNull();
                }
            }
            else
            {
                if (queryFrom == QueryFrom.NoMatter)
                {
                    // Act
                    Func<Task> func = async () => await textMessageCH.Process(TestContext.Current.CancellationToken);

                    ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                    exceptionAssertions.WithMessage($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {queryFrom}");
                }
                else
                {
                    entityContext.Days.FirstOrDefault(x => x.Name == data && x.CycleId == builder.TestCycle.Id).Should().BeNull();

                    // Act
                    await textMessageCH.Process(TestContext.Current.CancellationToken);

                    IInformationSet? informationSet = localCommandHandlerTools.HandlerResult.InformationSet;
                    informationSet.Should().NotBeNull();

                    if (queryFrom == QueryFrom.Start)
                    {
                        localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddExercises);

                        informationSet.Message.Should().Be(@$"День {data.AddBoldAndQuotes()} сохранён в цикле {builder.TestCycle.Name.AddBold()}
======================
Введите название упражения для этого дня.
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
                        informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));
                    }
                    else
                    {
                        localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                        informationSet.Message.Should().Be(@$"День {data.AddBoldAndQuotes()} сохранён в цикле {builder.TestCycle.Name.AddBold()}");
                        informationSet.ButtonsSets.Should().Be((ButtonsSet.AddExercises, ButtonsSet.SettingDays));
                    }

                    localCommandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                    entityContext.Days.FirstOrDefault(x => x.Name == data && x.CycleId == builder.TestCycle.Id).Should().NotBeNull();

                    informationSet.ParseMode.Should().Be(ParseMode.Html);
                    informationSet.AdditionalParameters.Should().BeNull();
                }
            }
        }

        [Theory]
        [InlineData("-", QueryFrom.NoMatter)]
        [InlineData("-", QueryFrom.Start)]
        [InlineData("-", QueryFrom.Settings)]
        [InlineData("-1", QueryFrom.NoMatter)]
        [InlineData("-1", QueryFrom.Start)]
        [InlineData("-1", QueryFrom.Settings)]
        [InlineData("Abc-", QueryFrom.NoMatter)]
        [InlineData("Abc-", QueryFrom.Start)]
        [InlineData("Abc-", QueryFrom.Settings)]
        [InlineData("Abc-100", QueryFrom.NoMatter)]
        [InlineData("Abc-100", QueryFrom.Start)]
        [InlineData("Abc-100", QueryFrom.Settings)]
        public async Task GetInformationSet_WithBadDataAddExercisesNavigationTarget_ShouldReturnExpectedIInformationSet(string data, QueryFrom queryFrom)
        {
            // Arrange
            MessageNavigationTarget messageNavigationTarget = MessageNavigationTarget.AddExercises;

            builder.WithCycle();
            builder.WithDay();

            DTOUserInformation localDTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();
            UserContext localUserContext = new UserContext(localDTOCurrentUser, Roles.User | Roles.Admin, false);

            Update update = new Update();

            CommandHandlerTools localCommandHandlerTools = new CommandHandlerTools()
            {
                CoreTools = coreTools,
                HandlerResult = new UpdateHandlerResult()
                {
                    CurrentUserContext = localUserContext,
                    Update = update,
                    ShortUpdateInfo = new ShortUpdateInfo(update, new User(), 1, "...", UpdateType.Message, true)
                },
                RepositoriesStore = new RepositoriesStore(entityContext),
            };

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetMessageNavigationTarget(messageNavigationTarget);
            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetQueryFrom(queryFrom);

            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestCycle.ToDTOCycle());
            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestDay.ToDTODay());

            MessageTextBuilder requestTextBuilder = new MessageTextBuilder(data);

            TextMessageCH textMessageCH = new TextMessageCH(localCommandHandlerTools, requestTextBuilder);

            // Assert
            if (queryFrom == QueryFrom.NoMatter)
            {
                // Act
                Func<Task> func = async () => await textMessageCH.Process(TestContext.Current.CancellationToken);

                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                exceptionAssertions.WithMessage($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {queryFrom}");
            }
            else
            {
                // Act
                await textMessageCH.Process(TestContext.Current.CancellationToken);

                IInformationSet? informationSet = localCommandHandlerTools.HandlerResult.InformationSet;
                informationSet.Should().NotBeNull();

                if (queryFrom == QueryFrom.Start)
                {
                    if (data == "-")
                    {
                        informationSet.Message.Should().Be(@$"Упражнение(я) не зафиксировано(ы)!
======================
Не удалось получилось название упражнения.
======================

Введите другое(ие) упражнение(й) для дня {builder.TestDay.Name.AddBoldAndQuotes()}

Формат общего ввода: [название]-[тип]. 
Пример единичного ввода: Жим лёжа-0
Пример множественного ввода: Жим лёжа-0;Становая тяга-0;Прыжки на скакалке-2;...
");
                    }
                    else if (data == "-1")
                    {
                        informationSet.Message.Should().Be(@$"Упражнение(я) не зафиксировано(ы)!
======================
Не удалось получить тип упражнения '1'
======================

Введите другое(ие) упражнение(й) для дня {builder.TestDay.Name.AddBoldAndQuotes()}

Формат общего ввода: [название]-[тип]. 
Пример единичного ввода: Жим лёжа-0
Пример множественного ввода: Жим лёжа-0;Становая тяга-0;Прыжки на скакалке-2;...
");
                    }
                    else if (data == "Abc-")
                    {
                        informationSet.Message.Should().Be(@$"Упражнение(я) не зафиксировано(ы)!
======================
Не удалось получить тип упражнения 'Abc'
======================

Введите другое(ие) упражнение(й) для дня {builder.TestDay.Name.AddBoldAndQuotes()}

Формат общего ввода: [название]-[тип]. 
Пример единичного ввода: Жим лёжа-0
Пример множественного ввода: Жим лёжа-0;Становая тяга-0;Прыжки на скакалке-2;...
");
                    }
                    else if (data == "Abc-100")
                    {
                        informationSet.Message.Should().Be(@$"Упражнение(я) не зафиксировано(ы)!
======================
Указанный тип (100) упражнения 'Abc' не относится к допустимым
======================

Введите другое(ие) упражнение(й) для дня {builder.TestDay.Name.AddBoldAndQuotes()}

Формат общего ввода: [название]-[тип]. 
Пример единичного ввода: Жим лёжа-0
Пример множественного ввода: Жим лёжа-0;Становая тяга-0;Прыжки на скакалке-2;...
");
                    }

                    informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));

                    informationSet.AdditionalParameters.Should().BeNullOrEmpty();
                }
                else if (queryFrom == QueryFrom.Settings)
                {
                    if (data == "-")
                    {
                        informationSet.Message.Should().Be(@$"Упражнение(я) не зафиксировано(ы)!
======================
Не удалось получилось название упражнения.
======================

Сбросьте текущие упражнения, чтобы вернуться обратно или введите другое(ие) упражнение(й) для дня {builder.TestDay.Name.AddBoldAndQuotes()}

Формат общего ввода: [название]-[тип]. 
Пример единичного ввода: Жим лёжа-0
Пример множественного ввода: Жим лёжа-0;Становая тяга-0;Прыжки на скакалке-2;...
");
                    }
                    else if (data == "-1")
                    {
                        informationSet.Message.Should().Be(@$"Упражнение(я) не зафиксировано(ы)!
======================
Не удалось получить тип упражнения '1'
======================

Сбросьте текущие упражнения, чтобы вернуться обратно или введите другое(ие) упражнение(й) для дня {builder.TestDay.Name.AddBoldAndQuotes()}

Формат общего ввода: [название]-[тип]. 
Пример единичного ввода: Жим лёжа-0
Пример множественного ввода: Жим лёжа-0;Становая тяга-0;Прыжки на скакалке-2;...
");
                    }
                    else if (data == "Abc-")
                    {
                        informationSet.Message.Should().Be(@$"Упражнение(я) не зафиксировано(ы)!
======================
Не удалось получить тип упражнения 'Abc'
======================

Сбросьте текущие упражнения, чтобы вернуться обратно или введите другое(ие) упражнение(й) для дня {builder.TestDay.Name.AddBoldAndQuotes()}

Формат общего ввода: [название]-[тип]. 
Пример единичного ввода: Жим лёжа-0
Пример множественного ввода: Жим лёжа-0;Становая тяга-0;Прыжки на скакалке-2;...
");
                    }
                    else if (data == "Abc-100")
                    {
                        informationSet.Message.Should().Be(@$"Упражнение(я) не зафиксировано(ы)!
======================
Указанный тип (100) упражнения 'Abc' не относится к допустимым
======================

Сбросьте текущие упражнения, чтобы вернуться обратно или введите другое(ие) упражнение(й) для дня {builder.TestDay.Name.AddBoldAndQuotes()}

Формат общего ввода: [название]-[тип]. 
Пример единичного ввода: Жим лёжа-0
Пример множественного ввода: Жим лёжа-0;Становая тяга-0;Прыжки на скакалке-2;...
");
                    }

                    informationSet.ButtonsSets.Should().Be((ButtonsSet.ResetTempDomains, ButtonsSet.None));

                    informationSet.AdditionalParameters.Should().NotBeNull();
                    KeyValuePair<string, string> item = informationSet.AdditionalParameters.First();
                    item.Should().NotBeNull();
                    item.Key.Should().Be("type");
                    item.Value.Should().Be("Exercise");
                }

                localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(queryFrom);
                localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(messageNavigationTarget);

                localCommandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                informationSet.ParseMode.Should().Be(ParseMode.Html);
            }
        }

        [Theory]
        [InlineData(false, false, QueryFrom.NoMatter)]
        [InlineData(false, false, QueryFrom.Start)]
        [InlineData(false, false, QueryFrom.Settings)]
        [InlineData(true, false, QueryFrom.NoMatter)]
        [InlineData(true, false, QueryFrom.Start)]
        [InlineData(true, false, QueryFrom.Settings)]
        [InlineData(false, true, QueryFrom.NoMatter)]
        [InlineData(false, true, QueryFrom.Start)]
        [InlineData(false, true, QueryFrom.Settings)]
        [InlineData(true, true, QueryFrom.NoMatter)]
        [InlineData(true, true, QueryFrom.Start)]
        [InlineData(true, true, QueryFrom.Settings)]
        public async Task GetInformationSet_WithAddExercisesNavigationTarget_ShouldReturnExpectedIInformationSet(bool hasAlreadyExistDomain, bool hasAlreadyExistTempDomain, QueryFrom queryFrom)
        {
            // Arrange
            MessageNavigationTarget messageNavigationTarget = MessageNavigationTarget.AddExercises;

            builder.WithCycle();
            builder.WithDay();

            if (hasAlreadyExistDomain)
                builder.WithExercise("newTestExercise");
            else
                builder.WithExercise();

            DTOUserInformation localDTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();
            UserContext localUserContext = new UserContext(localDTOCurrentUser, Roles.User | Roles.Admin, false);

            Update update = new Update();

            CommandHandlerTools localCommandHandlerTools = new CommandHandlerTools()
            {
                CoreTools = coreTools,
                HandlerResult = new UpdateHandlerResult()
                {
                    CurrentUserContext = localUserContext,
                    Update = update,
                    ShortUpdateInfo = new ShortUpdateInfo(update, new User(), 1, "...", UpdateType.Message, true)
                },
                RepositoriesStore = new RepositoriesStore(entityContext),
            };

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetMessageNavigationTarget(messageNavigationTarget);
            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetQueryFrom(queryFrom);

            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestCycle.ToDTOCycle());
            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestDay.ToDTODay());

            string newExerciseName = "newTestExercise";

            if (hasAlreadyExistTempDomain)
            {
                localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.TryAddTempExercises([new DTOExercise() { Name = newExerciseName, Mode = 0, DayId = builder.TestDay.Id }], out string? errMess);
                errMess.Should().BeNullOrWhiteSpace();
            }

            string data = $"{newExerciseName}-0";
            MessageTextBuilder requestTextBuilder = new MessageTextBuilder(data);

            TextMessageCH textMessageCH = new TextMessageCH(localCommandHandlerTools, requestTextBuilder);

            // Assert
            if ((hasAlreadyExistDomain || hasAlreadyExistTempDomain) && queryFrom == QueryFrom.NoMatter)
            {
                // Act
                Func<Task> func = async () => await textMessageCH.Process(TestContext.Current.CancellationToken);

                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                exceptionAssertions.WithMessage($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {queryFrom}");
            }
            else
            {
                // Act
                await textMessageCH.Process(TestContext.Current.CancellationToken);

                IInformationSet? informationSet = localCommandHandlerTools.HandlerResult.InformationSet;
                informationSet.Should().NotBeNull();

                if (hasAlreadyExistDomain)
                {
                    if (queryFrom == QueryFrom.Start)
                    {
                        informationSet.Message.Should().Be(@$"Упражнение(я) не зафиксировано(ы)!
======================
В этом дне уже существует упражнение с названием {builder.TestExercise.Name.AddBoldAndQuotes()}
======================

Введите другое(ие) упражнение(й) для дня {builder.TestDay.Name.AddBoldAndQuotes()}

Формат общего ввода: [название]-[тип]. 
Пример единичного ввода: Жим лёжа-0
Пример множественного ввода: Жим лёжа-0;Становая тяга-0;Прыжки на скакалке-2;...
");
                        informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));

                        informationSet.AdditionalParameters.Should().BeNull();
                    }
                    else if (queryFrom == QueryFrom.Settings)
                    {
                        informationSet.Message.Should().Be(@$"Упражнение(я) не зафиксировано(ы)!
======================
В этом дне уже существует упражнение с названием {builder.TestExercise.Name.AddBoldAndQuotes()}
======================

Сбросьте текущие упражнения, чтобы вернуться обратно или введите другое(ие) упражнение(й) для дня {builder.TestDay.Name.AddBoldAndQuotes()}

Формат общего ввода: [название]-[тип]. 
Пример единичного ввода: Жим лёжа-0
Пример множественного ввода: Жим лёжа-0;Становая тяга-0;Прыжки на скакалке-2;...
");
                        informationSet.ButtonsSets.Should().Be((ButtonsSet.ResetTempDomains, ButtonsSet.None));

                        informationSet.AdditionalParameters.Should().NotBeNull();
                        KeyValuePair<string, string> item = informationSet.AdditionalParameters.First();
                        item.Should().NotBeNull();
                        item.Key.Should().Be("type");
                        item.Value.Should().Be("Exercise");
                    }
                }
                else if (hasAlreadyExistTempDomain)
                {
                    if (queryFrom == QueryFrom.Start)
                    {
                        informationSet.Message.Should().Be(@$"Упражнение(я) не зафиксировано(ы)!
======================
В списке фиксаций уже существует упражнение с названием {newExerciseName.AddBoldAndQuotes()}
======================

Введите другое(ие) упражнение(й) для дня {builder.TestDay.Name.AddBoldAndQuotes()}

Формат общего ввода: [название]-[тип]. 
Пример единичного ввода: Жим лёжа-0
Пример множественного ввода: Жим лёжа-0;Становая тяга-0;Прыжки на скакалке-2;...
");
                        informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));

                        informationSet.AdditionalParameters.Should().BeNull();
                    }
                    else if (queryFrom == QueryFrom.Settings)
                    {
                        informationSet.Message.Should().Be(@$"Упражнение(я) не зафиксировано(ы)!
======================
В списке фиксаций уже существует упражнение с названием {newExerciseName.AddBoldAndQuotes()}
======================

Сбросьте текущие упражнения, чтобы вернуться обратно или введите другое(ие) упражнение(й) для дня {builder.TestDay.Name.AddBoldAndQuotes()}

Формат общего ввода: [название]-[тип]. 
Пример единичного ввода: Жим лёжа-0
Пример множественного ввода: Жим лёжа-0;Становая тяга-0;Прыжки на скакалке-2;...
");
                        informationSet.ButtonsSets.Should().Be((ButtonsSet.ResetTempDomains, ButtonsSet.None));

                        informationSet.AdditionalParameters.Should().NotBeNull();
                        KeyValuePair<string, string> item = informationSet.AdditionalParameters.First();
                        item.Should().NotBeNull();
                        item.Key.Should().Be("type");
                        item.Value.Should().Be("Exercise");
                    }
                }
                else
                {
                    informationSet.Message.Should().Be(@$"Упражнение(я) зафиксировано(ы)
======================

Введите след. упражнение(я) для дня {builder.TestDay.Name.AddBoldAndQuotes()} либо нажмите ""Сохранить"" для сохранения зафиксированных упражнений
");
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.SaveExercises, ButtonsSet.None));

                    informationSet.AdditionalParameters.Should().BeNull();
                }

                localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(queryFrom);
                localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(messageNavigationTarget);

                localCommandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                informationSet.ParseMode.Should().Be(ParseMode.Html);
            }
        }

        //FreeResult сломать в принципе невозможно в AddResultForExercise, поэтому в неудачных кейсах его нет
        //Timer удачно обработать в принципе невозможно в AddResultForExercise, поэтому в удачных кейсах его нет
        [Theory]
        [InlineData("Other", ExercisesMods.Count)]
        [InlineData("Other", ExercisesMods.WeightCount)]
        [InlineData("Other", ExercisesMods.Timer)]
        [InlineData("10 20", ExercisesMods.Count)]
        [InlineData("10 20; 30 40", ExercisesMods.WeightCount)]
        [InlineData("123", ExercisesMods.FreeResult)]
        public async Task GetInformationSet_WithAddResultForExerciseNavigationTarget_ShouldReturnExpectedIInformationSet(string data, ExercisesMods exercisesMods)
        {
            // Arrange
            MessageNavigationTarget messageNavigationTarget = MessageNavigationTarget.AddResultForExercise;

            builder.WithCycle();
            builder.WithDay();
            builder.WithExercise(mode: exercisesMods);

            DTOUserInformation localDTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();
            UserContext localUserContext = new UserContext(localDTOCurrentUser, Roles.User | Roles.Admin, false);

            Update update = new Update();

            CommandHandlerTools localCommandHandlerTools = new CommandHandlerTools()
            {
                CoreTools = coreTools,
                HandlerResult = new UpdateHandlerResult()
                {
                    CurrentUserContext = localUserContext,
                    Update = update,
                    ShortUpdateInfo = new ShortUpdateInfo(update, new User(), 1, "...", UpdateType.Message, true)
                },
                RepositoriesStore = new RepositoriesStore(entityContext),
            };

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetMessageNavigationTarget(messageNavigationTarget);
            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetQueryFrom(QueryFrom.NoMatter);

            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestCycle.ToDTOCycle());
            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestDay.ToDTODay());
            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestExercise.ToDTOExercise());

            MessageTextBuilder requestTextBuilder = new MessageTextBuilder(data);

            TextMessageCH textMessageCH = new TextMessageCH(localCommandHandlerTools, requestTextBuilder);

            // Act
            await textMessageCH.Process(TestContext.Current.CancellationToken);

            IInformationSet? informationSet = localCommandHandlerTools.HandlerResult.InformationSet;
            informationSet.Should().NotBeNull();

            // Assert
            if (data != "Other")
            {
                informationSet.Message.Should().Be(@$"Подход(ы) зафиксирован(ы)
======================

Введите результат след. подхода для упражения {builder.TestExercise.Name.AddBoldAndQuotes()}
либо нажмите ""Сохранить"" для сохранения указанных подходов
");

                informationSet.ButtonsSets.Should().Be((ButtonsSet.SaveResultsExercise, ButtonsSet.None));
            }
            else
            {
                string title, content;

                if (exercisesMods == ExercisesMods.Count)
                {
                    title = "Указанное количество повторений не является цифрой";
                    content = CommonConsts.ResultExercise.InputFormatExerciseResultCount;
                }
                else if (exercisesMods == ExercisesMods.WeightCount)
                {
                    title = "Некорректный ввод. Ожидаемое кол-во аргументов больше 2 для упражнения с типом \"WeightCount\"";
                    content = CommonConsts.ResultExercise.InputFormatExerciseResultWeightCount;
                }
                else
                {
                    title = "Неподдерживаемый тип упражнения: \"Timer\"";
                    content = CommonConsts.ResultExercise.InputFormatExerciseResultTimer;
                }

                informationSet.Message.Should().Be(@$"{title}
======================
{content}
======================

Сбросьте текущие результаты, чтобы вернуться обратно или введите результат заново для упражения {builder.TestExercise.Name.AddBoldAndQuotes()}
");

                informationSet.ButtonsSets.Should().Be((ButtonsSet.ResetResultsExercise, ButtonsSet.None));
            }

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(messageNavigationTarget);

            localCommandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSet_WithAddCommentForExerciseTimerNavigationTarget_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            string freeResult = "Abc";
            string data = "123";
            
            MessageNavigationTarget messageNavigationTarget = MessageNavigationTarget.AddCommentForExerciseTimer;

            builder.WithCycle();
            builder.WithDay();
            builder.WithExercise();

            DTOUserInformation localDTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();
            UserContext localUserContext = new UserContext(localDTOCurrentUser, Roles.User | Roles.Admin, false);

            Update update = new Update();

            CommandHandlerTools localCommandHandlerTools = new CommandHandlerTools()
            {
                CoreTools = coreTools,
                HandlerResult = new UpdateHandlerResult()
                {
                    CurrentUserContext = localUserContext,
                    Update = update,
                    ShortUpdateInfo = new ShortUpdateInfo(update, new User(), 1, "...", UpdateType.Message, true)
                },
                RepositoriesStore = new RepositoriesStore(entityContext),
            };

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetMessageNavigationTarget(messageNavigationTarget);
            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetQueryFrom(QueryFrom.NoMatter);

            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestCycle.ToDTOCycle());
            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestDay.ToDTODay());
            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestExercise.ToDTOExercise());

            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.AddTempResultsExercise([new DTOResultExercise() { ExerciseId = builder.TestExercise .Id, FreeResult = freeResult }]);

            MessageTextBuilder requestTextBuilder = new MessageTextBuilder(data);

            TextMessageCH textMessageCH = new TextMessageCH(localCommandHandlerTools, requestTextBuilder);

            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().ResultsExercise.Should().BeEmpty();

            // Act
            await textMessageCH.Process(TestContext.Current.CancellationToken);

            IInformationSet? informationSet = localCommandHandlerTools.HandlerResult.InformationSet;
            informationSet.Should().NotBeNull();

            // Assert
            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().ResultsExercise.Should().HaveCount(1);

            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().ResultsExercise.First().ExerciseId.Should().Be(builder.TestExercise.Id);

            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.TempResultsExercise.Should().BeNull();

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            informationSet.Message.Should().Be(@$"<b>{freeResult} / {data}</b>
======================
Введённые данные сохранены
======================

Выберите упражнение или посмотрите предыдущие результаты дня {localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()}
");

            informationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout));

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);

            localCommandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Theory]
        [InlineData(false, MessageNavigationTarget.ChangeNameCycle, QueryFrom.NoMatter)]
        [InlineData(false, MessageNavigationTarget.ChangeNameCycle, QueryFrom.Start)]
        [InlineData(false, MessageNavigationTarget.ChangeNameCycle, QueryFrom.Settings)]
        [InlineData(false, MessageNavigationTarget.ChangeNameDay, QueryFrom.NoMatter)]
        [InlineData(false, MessageNavigationTarget.ChangeNameDay, QueryFrom.Start)]
        [InlineData(false, MessageNavigationTarget.ChangeNameDay, QueryFrom.Settings)]
        [InlineData(false, MessageNavigationTarget.ChangeNameExercise, QueryFrom.NoMatter)]
        [InlineData(false, MessageNavigationTarget.ChangeNameExercise, QueryFrom.Start)]
        [InlineData(false, MessageNavigationTarget.ChangeNameExercise, QueryFrom.Settings)]
        [InlineData(true, MessageNavigationTarget.ChangeNameCycle, QueryFrom.NoMatter)]
        [InlineData(true, MessageNavigationTarget.ChangeNameCycle, QueryFrom.Start)]
        [InlineData(true, MessageNavigationTarget.ChangeNameCycle, QueryFrom.Settings)]
        [InlineData(true, MessageNavigationTarget.ChangeNameDay, QueryFrom.NoMatter)]
        [InlineData(true, MessageNavigationTarget.ChangeNameDay, QueryFrom.Start)]
        [InlineData(true, MessageNavigationTarget.ChangeNameDay, QueryFrom.Settings)]
        [InlineData(true, MessageNavigationTarget.ChangeNameExercise, QueryFrom.NoMatter)]
        [InlineData(true, MessageNavigationTarget.ChangeNameExercise, QueryFrom.Start)]
        [InlineData(true, MessageNavigationTarget.ChangeNameExercise, QueryFrom.Settings)]
        public async Task GetInformationSet_WithChangeNameDomainNavigationTarget_ShouldReturnExpectedIInformationSet(bool hasAlreadyExistDomain, MessageNavigationTarget messageNavigationTarget, QueryFrom queryFrom)
        {
            // Arrange
            string data = "NewName";

            if (hasAlreadyExistDomain)
            {
                if (messageNavigationTarget == MessageNavigationTarget.ChangeNameCycle)
                {
                    builder.WithCycle(name: "NewName");
                    builder.WithDay();
                    builder.WithExercise();

                    builder.TestCycle.Name.Should().Be(data);
                }
                else if (messageNavigationTarget == MessageNavigationTarget.ChangeNameDay)
                {
                    builder.WithCycle();
                    builder.WithDay(name: "NewName");
                    builder.WithExercise();

                    builder.TestDay.Name.Should().Be(data);
                }
                else if (messageNavigationTarget == MessageNavigationTarget.ChangeNameExercise)
                {
                    builder.WithCycle();
                    builder.WithDay();
                    builder.WithExercise(name: "NewName");

                    builder.TestExercise.Name.Should().Be(data);
                }
            }
            else
            {
                builder.WithCycle();
                builder.WithDay();
                builder.WithExercise();

                builder.TestCycle.Name.Should().NotBe(data);
                builder.TestDay.Name.Should().NotBe(data);
                builder.TestExercise.Name.Should().NotBe(data);
            }

            DTOUserInformation localDTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();
            UserContext localUserContext = new UserContext(localDTOCurrentUser, Roles.User | Roles.Admin, false);

            Update update = new Update();

            CommandHandlerTools localCommandHandlerTools = new CommandHandlerTools()
            {
                CoreTools = coreTools,
                HandlerResult = new UpdateHandlerResult()
                {
                    CurrentUserContext = localUserContext,
                    Update = update,
                    ShortUpdateInfo = new ShortUpdateInfo(update, new User(), 1, "...", UpdateType.Message, true)
                },
                RepositoriesStore = new RepositoriesStore(entityContext),
            };

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetMessageNavigationTarget(messageNavigationTarget);
            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetQueryFrom(queryFrom);

            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestCycle.ToDTOCycle());
            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestDay.ToDTODay());
            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestExercise.ToDTOExercise());

            MessageTextBuilder requestTextBuilder = new MessageTextBuilder(data);

            TextMessageCH textMessageCH = new TextMessageCH(localCommandHandlerTools, requestTextBuilder);

            if (!hasAlreadyExistDomain)
            {
                localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.Should().NotBe(data);
                localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.Should().NotBe(data);
                localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().Name.Should().NotBe(data);
            }

            if (queryFrom == QueryFrom.NoMatter && hasAlreadyExistDomain)
            {
                // Act
                Func<Task> func = async () => await textMessageCH.Process(TestContext.Current.CancellationToken);

                // Assert
                ExceptionAssertions<NotImplementedException> exceptionAssertions = await func.Should().ThrowAsync<NotImplementedException>();
                exceptionAssertions.WithMessage($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {queryFrom}");
            }
            else
            {
                // Act
                await textMessageCH.Process(TestContext.Current.CancellationToken);

                IInformationSet? informationSet = localCommandHandlerTools.HandlerResult.InformationSet;
                informationSet.Should().NotBeNull();

                // Assert
                if (hasAlreadyExistDomain)
                {
                    if (messageNavigationTarget == MessageNavigationTarget.ChangeNameCycle)
                    {
                        informationSet.Message.Should().Be(@$"Ошибка при добавлении названия!
======================
Цикл с названием {data.AddBoldAndQuotes()} уже существует
======================

Ввведите другое название тренировочного цикла
");

                        if (queryFrom == QueryFrom.Start)
                        {
                            informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));
                        }
                        else if (queryFrom == QueryFrom.Settings)
                        {
                            informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingCycles));
                        }
                    }
                    else if (messageNavigationTarget == MessageNavigationTarget.ChangeNameDay)
                    {
                        informationSet.Message.Should().Be(@$"Ошибка при сохранении!
======================
В этом цикле уже существует день с названием {data.AddBoldAndQuotes()}
======================

Ввведите другое название дня для цикла {builder.TestCycle.Name.AddBoldAndQuotes()}
");

                        if (queryFrom == QueryFrom.Start)
                        {
                            informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));
                        }
                        else if (queryFrom == QueryFrom.Settings)
                        {
                            informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingDays));
                        }
                    }
                    else if (messageNavigationTarget == MessageNavigationTarget.ChangeNameExercise)
                    {
                        informationSet.Message.Should().Be(@$"Ошибка при сохранении!
======================
В этом дне уже существует упражнение с названием {data.AddBoldAndQuotes()}
======================

Введите другое(ие) название(я) упражнение(ий) для дня {builder.TestDay.Name.AddBoldAndQuotes()}
");

                        if (queryFrom == QueryFrom.Start)
                        {
                            informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));
                        }
                        else if (queryFrom == QueryFrom.Settings)
                        {
                            informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingExercises));
                        }
                    }

                    localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(messageNavigationTarget);
                }
                else
                {
                    if (messageNavigationTarget == MessageNavigationTarget.ChangeNameCycle)
                    {
                        localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.Should().Be(data);

                        informationSet.Message.Should().Be(@$"Название цикла сохранено!");

                        informationSet.ButtonsSets.Should().Be((ButtonsSet.SettingCycle, ButtonsSet.CyclesList));
                    }
                    else if (messageNavigationTarget == MessageNavigationTarget.ChangeNameDay)
                    {
                        localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.Should().Be(data);

                        informationSet.Message.Should().Be(@$"Название дня сохранено!
======================

Выберите интересующую настройку для указанного дня
");

                        informationSet.ButtonsSets.Should().Be((ButtonsSet.SettingDay, ButtonsSet.DaysList));
                    }
                    else if (messageNavigationTarget == MessageNavigationTarget.ChangeNameExercise)
                    {
                        localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().Name.Should().Be(data);

                        informationSet.Message.Should().Be(@$"Название упражнения сохранено!
======================

Выберите интересующую настройку для указанного упражнения
");

                        informationSet.ButtonsSets.Should().Be((ButtonsSet.SettingExercise, ButtonsSet.ExercisesList));
                    }

                    localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);
                }

                localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(queryFrom);

                localCommandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

                informationSet.ParseMode.Should().Be(ParseMode.Html);
                informationSet.AdditionalParameters.Should().BeNull();
            }
        }

        [Theory]
        [InlineData(false, "Other")]
        [InlineData(true, "Other")]
        [InlineData(false, "02.06.2020")]
        [InlineData(true, "02.06.2020")]
        public async Task GetInformationSet_WithFindResultsByDateNavigationTarget_ShouldReturnExpectedIInformationSet(bool isNeedFindByCurrentDay, string data)
        {
            // Arrange
            if(isNeedFindByCurrentDay)
                builder.WithCycle();
            else
                builder.WithCycle(isActive: true);

            builder.WithDay();
            builder.WithExercise();

            if (data != "Other")
            {
                ResultExercise resultExercise1 = new ResultExercise()
                {
                    DateTime = new DateTime(2020, 6, 2),
                    ExerciseId = builder.TestExercise.Id,
                    Count = 10
                };

                entityContext.Add(resultExercise1);
                entityContext.SaveChanges();
            }

            DTOUserInformation localDTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();
            UserContext localUserContext = new UserContext(localDTOCurrentUser, Roles.User | Roles.Admin, false);

            Update update = new Update();

            CommandHandlerTools localCommandHandlerTools = new CommandHandlerTools()
            {
                CoreTools = coreTools,
                HandlerResult = new UpdateHandlerResult()
                {
                    CurrentUserContext = localUserContext,
                    Update = update,
                    ShortUpdateInfo = new ShortUpdateInfo(update, new User(), 1, "...", UpdateType.Message, true)
                },
                RepositoriesStore = new RepositoriesStore(entityContext),
            };

            QueryFrom queryFrom = QueryFrom.NoMatter;

            MessageNavigationTarget messageNavigationTarget = isNeedFindByCurrentDay 
                ? MessageNavigationTarget.FindResultsByDateInDay
                : MessageNavigationTarget.FindResultsByDate;

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetMessageNavigationTarget(messageNavigationTarget);
            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetQueryFrom(queryFrom);

            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestCycle.ToDTOCycle());
            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestDay.ToDTODay());
            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestExercise.ToDTOExercise());

            MessageTextBuilder requestTextBuilder = new MessageTextBuilder(data);

            TextMessageCH textMessageCH = new TextMessageCH(localCommandHandlerTools, requestTextBuilder);

            // Act
            await textMessageCH.Process(TestContext.Current.CancellationToken);

            IInformationSet? informationSet = localCommandHandlerTools.HandlerResult.InformationSet;
            informationSet.Should().NotBeNull();

            // Assert
            if (data == "Other")
            {
                informationSet.Message.Should().Be(@$"Не удалось получить дату из сообщения 'Other', для корректного поиска придерживайтесь допустимого формата
======================
{CommonConsts.Exercise.FindResultsByDateFormat}
======================

Введите дату искомой тренировки
");
                if (isNeedFindByCurrentDay)
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay));
                else
                    informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.DaysListWithLastWorkout));

                localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(messageNavigationTarget);
            }
            else
            {
                if (isNeedFindByCurrentDay)
                {
                    informationSet.Message.Should().Be(@$"Найденная тренировка:
======================
Дата: {data}
Упражнение: {builder.TestExercise.Name.AddBoldAndQuotes()}
Повторения: (10)
======================

Выберите упражнение или посмотрите предыдущие результаты дня {builder.TestDay.Name.AddBoldAndQuotes()} ({builder.TestCycle.Name.AddBold()})
");

                    informationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout));
                }
                else
                {
                    informationSet.Message.Should().Be(@$"Найденная тренировка:
======================
Дата: {data}
Упражнение: {builder.TestExercise.Name.AddBoldAndQuotes()}
Повторения: (10)
======================

Выберите тренировочный день или посмотрите предыдущие результаты из цикла {builder.TestCycle.Name.AddBoldAndQuotes()}
");

                    informationSet.ButtonsSets.Should().Be((ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main));
                }

                localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);
            }

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(queryFrom);

            localCommandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Theory]
        [InlineData(false, "Other")]
        [InlineData(true, "Other")]
        [InlineData(false, "9999")]
        [InlineData(true, "9999")]
        [InlineData(false, "1")]
        [InlineData(true, "1")]
        public async Task GetInformationSet_WithFindLogByIDNavigationTarget_ShouldReturnExpectedIInformationSet(bool byEventID, string data)
        {
            // Arrange
            if (data == "1")
            {
                Log log;

                if (byEventID)
                    log = new Log() { LogLevel = LogLevel.Information.ToString(), Message = "12", SourceContext = "Tests", EventID = int.Parse(data) };
                else
                    log = new Log() { LogLevel = LogLevel.Information.ToString(), Message = "123", SourceContext = "Tests" };

                entityContext.Logs.Add(log);
                entityContext.SaveChanges();
            }

            DTOUserInformation localDTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();
            UserContext localUserContext = new UserContext(localDTOCurrentUser, Roles.User | Roles.Admin, false);

            Update update = new Update();

            CommandHandlerTools localCommandHandlerTools = new CommandHandlerTools()
            {
                CoreTools = coreTools,
                HandlerResult = new UpdateHandlerResult()
                {
                    CurrentUserContext = localUserContext,
                    Update = update,
                    ShortUpdateInfo = new ShortUpdateInfo(update, new User(), 1, "...", UpdateType.Message, true)
                },
                RepositoriesStore = new RepositoriesStore(entityContext),
            };

            QueryFrom queryFrom = QueryFrom.NoMatter;

            MessageNavigationTarget messageNavigationTarget = byEventID
                ? MessageNavigationTarget.FindLogByEventID
                : MessageNavigationTarget.FindLogByID;

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetMessageNavigationTarget(messageNavigationTarget);
            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetQueryFrom(queryFrom);

            MessageTextBuilder requestTextBuilder = new MessageTextBuilder(data);

            TextMessageCH textMessageCH = new TextMessageCH(localCommandHandlerTools, requestTextBuilder);

            string identifierType = byEventID
                             ? "eventId"
                             : "Id";

            // Act
            await textMessageCH.Process(TestContext.Current.CancellationToken);

            IInformationSet? informationSet = localCommandHandlerTools.HandlerResult.InformationSet;
            informationSet.Should().NotBeNull();

            // Assert
            if (data == "Other")
            {
                informationSet.Message.Should().Be(@$"Передан некорректный {identifierType}: ""<b>Other</b>""
======================

Выберите интересующее действие
");
                informationSet.ParseMode.Should().Be(ParseMode.Html);
            }
            else if (data == "15")
            {
                informationSet.Message.Should().Be(@$"Не удалось найти лог с {identifierType}: {data.ToString().AddBoldAndQuotes()}
======================

Выберите интересующее действие
");
                informationSet.ParseMode.Should().Be(ParseMode.Html);
            }
            else if (data == "1")
            {
                if (byEventID)
                {
                    informationSet.Message.Should().Be(@$"Найденный лог:
======================
[1] [Information] [1] [01.01.0001 00:00:00] [Null] [Tests]:
12
======================

Выберите интересующее действие
");
                }
                else
                {
                    informationSet.Message.Should().Be(@$"Найденный лог:
======================
[1] [Information] [] [01.01.0001 00:00:00] [Null] [Tests]:
123
======================

Выберите интересующее действие
");
                }

                informationSet.ParseMode.Should().Be(ParseMode.None);
            }

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);
            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(queryFrom);

            localCommandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

            informationSet.ButtonsSets.Should().Be((ButtonsSet.AdminLogs, ButtonsSet.Admin));

            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Theory]
        [InlineData("Other-Bad-Arguments")]
        [InlineData("22-NotFoundUser")]
        [InlineData("@NotFoundUser-text")]
        [InlineData("1-text")]
        [InlineData("@TestUsername-text")]
        public async Task GetInformationSet_WithSendMessageToUserNavigationTarget_ShouldReturnExpectedIInformationSet(string data)
        {
            // Arrange
            string[] parameters = data.Split("-", StringSplitOptions.RemoveEmptyEntries);

            string userIdentity = parameters[0];
            string text = parameters[1];

            DTOUserInformation localDTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();
            UserContext localUserContext = new UserContext(localDTOCurrentUser, Roles.User | Roles.Admin, false);

            Update update = new Update();

            CommandHandlerTools localCommandHandlerTools = new CommandHandlerTools()
            {
                CoreTools = coreTools,
                HandlerResult = new UpdateHandlerResult()
                {
                    CurrentUserContext = localUserContext,
                    Update = update,
                    ShortUpdateInfo = new ShortUpdateInfo(update, new User(), 1, "...", UpdateType.Message, true)
                },
                RepositoriesStore = new RepositoriesStore(entityContext),
            };

            QueryFrom queryFrom = QueryFrom.NoMatter;
            MessageNavigationTarget messageNavigationTarget = MessageNavigationTarget.SendMessageToUser;

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetMessageNavigationTarget(messageNavigationTarget);
            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetQueryFrom(queryFrom);

            MessageTextBuilder requestTextBuilder = new MessageTextBuilder(data);

            TextMessageCH textMessageCH = new TextMessageCH(localCommandHandlerTools, requestTextBuilder);

            // Act
            await textMessageCH.Process(TestContext.Current.CancellationToken);

            IInformationSet? informationSet = localCommandHandlerTools.HandlerResult.InformationSet;
            informationSet.Should().NotBeNull();

            // Assert
            if (data == "Other-Bad-Arguments")
            {
                informationSet.Message.Should().Be(@$"Некорректные параметры для отправки сообщения пользователю
======================
Пример: @TestUser-Тест
======================

Введите параметры повторно
");
                informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.AdminUsers));

                botResponseSenderMock.Verify(x => x.SendSimpleNotification(It.IsAny<long>(), It.IsAny<string>(), TestContext.Current.CancellationToken), Times.Never);
            }
            else if (data.Contains("NotFoundUser"))
            {
                informationSet.Message.Should().Be(@$"Пользователь {userIdentity.AddBoldAndQuotes()} не найден!
======================

Введите параметры для поиска другого пользователя
");

                informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.AdminUsers));

                botResponseSenderMock.Verify(x => x.SendSimpleNotification(It.IsAny<long>(), It.IsAny<string>(), TestContext.Current.CancellationToken), Times.Never);
            }
            else
            {
                informationSet.Message.Should().Be(@$"Сообщение отправлено пользователю {userIdentity.AddBoldAndQuotes()}
======================

Выберите интересующее действие
");
                informationSet.ButtonsSets.Should().Be((ButtonsSet.AdminUsers, ButtonsSet.Admin));

                botResponseSenderMock.Verify(x => x.SendSimpleNotification(It.IsAny<long>(), It.IsAny<string>(), TestContext.Current.CancellationToken), Times.Once);
            }

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(messageNavigationTarget);
            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(queryFrom);

            localCommandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task GetInformationSet_WithMassiveSendMessagesToUsersNavigationTarget_ShouldReturnExpectedIInformationSet(bool isNeedFromDB)
        {
            // Arrange
            string data = "SomeText";

            if (isNeedFromDB)
            {
                UserInformation newUserInformation = new UserInformation() { FirstName = "NewUserInfo", Username = "@NewUserInfo" };
                await entityContext.UsersInformation.AddAsync(newUserInformation, TestContext.Current.CancellationToken);
                await entityContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }
            else
                this.contextKeeperMock.Setup(x => x.GetAllKeys()).Returns([1, 2]);

            DTOUserInformation localDTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();
            UserContext localUserContext = new UserContext(localDTOCurrentUser, Roles.User | Roles.Admin, false);

            Update update = new Update();

            CommandHandlerTools localCommandHandlerTools = new CommandHandlerTools()
            {
                CoreTools = coreTools,
                HandlerResult = new UpdateHandlerResult()
                {
                    CurrentUserContext = localUserContext,
                    Update = update,
                    ShortUpdateInfo = new ShortUpdateInfo(update, new User(), 1, "...", UpdateType.Message, true)
                },
                RepositoriesStore = new RepositoriesStore(entityContext),
            };

            QueryFrom queryFrom = QueryFrom.NoMatter;

            MessageNavigationTarget messageNavigationTarget = isNeedFromDB 
                ? MessageNavigationTarget.SendMessagesToAllUsers
                : MessageNavigationTarget.SendMessagesToActiveUsers;

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetMessageNavigationTarget(messageNavigationTarget);
            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetQueryFrom(queryFrom);

            MessageTextBuilder requestTextBuilder = new MessageTextBuilder(data);

            TextMessageCH textMessageCH = new TextMessageCH(localCommandHandlerTools, requestTextBuilder);

            // Act
            await textMessageCH.Process(TestContext.Current.CancellationToken);

            IInformationSet? informationSet = localCommandHandlerTools.HandlerResult.InformationSet;
            informationSet.Should().NotBeNull();

            // Assert
            informationSet.Message.Should().Be(@$"Сообщение отправлено {2} пользователям
======================

Выберите интересующее действие
");
            informationSet.ButtonsSets.Should().Be((ButtonsSet.AdminUsers, ButtonsSet.Admin));

            botResponseSenderMock.Verify(x => x.SendSimpleNotification(It.IsAny<long>(), It.IsAny<string>(), TestContext.Current.CancellationToken), Times.Exactly(2));

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(messageNavigationTarget);
            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(queryFrom);

            localCommandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Theory]
        [InlineData("Other-Bad-Arguments")]
        [InlineData("22-wl")]
        [InlineData("22-bl")]
        [InlineData("@NotFoundUser-wl")]
        [InlineData("@NotFoundUser-bl")]
        [InlineData("1-wl")]
        [InlineData("1-bl")]
        [InlineData("@TestUsername-wl")]
        [InlineData("@TestUsername-bl")]
        public async Task GetInformationSet_WithChangeUserStateNavigationTarget_ShouldReturnExpectedIInformationSet(string data)
        {
            // Arrange
            string[] parameters = data.Split("-", StringSplitOptions.RemoveEmptyEntries);

            string userIdentity = parameters[0];
            string text = parameters[1];

            DTOUserInformation localDTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();
            UserContext localUserContext = new UserContext(localDTOCurrentUser, Roles.User | Roles.Admin, false);

            Update update = new Update();

            CommandHandlerTools localCommandHandlerTools = new CommandHandlerTools()
            {
                CoreTools = coreTools,
                HandlerResult = new UpdateHandlerResult()
                {
                    CurrentUserContext = localUserContext,
                    Update = update,
                    ShortUpdateInfo = new ShortUpdateInfo(update, new User(), 1, "...", UpdateType.Message, true)
                },
                RepositoriesStore = new RepositoriesStore(entityContext),
            };

            QueryFrom queryFrom = QueryFrom.NoMatter;
            MessageNavigationTarget messageNavigationTarget = MessageNavigationTarget.ChangeUserState;

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetMessageNavigationTarget(messageNavigationTarget);
            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetQueryFrom(queryFrom);

            MessageTextBuilder requestTextBuilder = new MessageTextBuilder(data);

            TextMessageCH textMessageCH = new TextMessageCH(localCommandHandlerTools, requestTextBuilder);

            contextKeeperMock.Setup(x => x.GetContext(builder.TestUserInformation.UserId)).Returns(localCommandHandlerTools.HandlerResult.CurrentUserContext);

            localUserContext.UserInformation.WhiteList.Should().BeFalse();
            localUserContext.UserInformation.BlackList.Should().BeFalse();

            builder.TestUserInformation.WhiteList.Should().BeFalse();
            builder.TestUserInformation.BlackList.Should().BeFalse();

            // Act
            await textMessageCH.Process(TestContext.Current.CancellationToken);

            IInformationSet? informationSet = localCommandHandlerTools.HandlerResult.InformationSet;
            informationSet.Should().NotBeNull();

            // Assert
            if (data == "Other-Bad-Arguments")
            {
                informationSet.Message.Should().Be(@$"Некорректные параметры для изменения состояния пользователя
======================
Пример: @TestUser-bl
======================

Введите параметры повторно
");
                informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.AdminUsers));

                localUserContext.UserInformation.WhiteList.Should().BeFalse();
                localUserContext.UserInformation.BlackList.Should().BeFalse();

                builder.TestUserInformation.WhiteList.Should().BeFalse();
                builder.TestUserInformation.BlackList.Should().BeFalse();
            }
            else if (data.Contains("22") || data.Contains("NotFoundUser"))
            {
                informationSet.Message.Should().Be(@$"Пользователь {userIdentity.AddBoldAndQuotes()} не найден!
======================

Введите параметры для поиска другого пользователя
");

                informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.AdminUsers));

                localUserContext.UserInformation.WhiteList.Should().BeFalse();
                localUserContext.UserInformation.BlackList.Should().BeFalse();

                builder.TestUserInformation.WhiteList.Should().BeFalse();
                builder.TestUserInformation.BlackList.Should().BeFalse();
            }
            else if (data.Contains("1") || data.Contains("TestUsername"))
            {
                if (text == "bl")
                {
                    informationSet.Message.Should().Be(@$"BlackList для {builder.TestUserInformation.Username.AddBoldAndQuotes()} ({builder.TestUserInformation.UserId}) установлен в: {builder.TestUserInformation.BlackList.ToString().AddBold()}
======================

Выберите интересующее действие
");
                    localUserContext.UserInformation.WhiteList.Should().BeFalse();
                    localUserContext.UserInformation.BlackList.Should().BeTrue();

                    builder.TestUserInformation.WhiteList.Should().BeFalse();
                    builder.TestUserInformation.BlackList.Should().BeTrue();
                }
                else if (text == "wl")
                {
                    informationSet.Message.Should().Be(@$"WhiteList для {builder.TestUserInformation.Username.AddBoldAndQuotes()} ({builder.TestUserInformation.UserId}) установлен в: {builder.TestUserInformation.WhiteList.ToString().AddBold()}
======================

Выберите интересующее действие
");
                    localUserContext.UserInformation.WhiteList.Should().BeTrue();
                    localUserContext.UserInformation.BlackList.Should().BeFalse();

                    builder.TestUserInformation.WhiteList.Should().BeTrue();
                    builder.TestUserInformation.BlackList.Should().BeFalse();
                }

                informationSet.ButtonsSets.Should().Be((ButtonsSet.AdminUsers, ButtonsSet.Admin));
            }

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(messageNavigationTarget);
            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(queryFrom);

            localCommandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Theory]
        //[InlineData("")] // Невозможно затестить, т.к. MessageTextBuilder не допускает пустой строки
        [InlineData("22")]
        [InlineData("@NotFoundUser")]
        [InlineData("1")]
        [InlineData("@TestUsername")]
        public async Task GetInformationSet_WithDeleteUserNavigationTarget_ShouldReturnExpectedIInformationSet(string data)
        {
            // Arrange
            DTOUserInformation localDTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();
            UserContext localUserContext = new UserContext(localDTOCurrentUser, Roles.User | Roles.Admin, false);

            Update update = new Update();

            CommandHandlerTools localCommandHandlerTools = new CommandHandlerTools()
            {
                CoreTools = coreTools,
                HandlerResult = new UpdateHandlerResult()
                {
                    CurrentUserContext = localUserContext,
                    Update = update,
                    ShortUpdateInfo = new ShortUpdateInfo(update, new User(), 1, "...", UpdateType.Message, true)
                },
                RepositoriesStore = new RepositoriesStore(entityContext),
            };

            QueryFrom queryFrom = QueryFrom.NoMatter;
            MessageNavigationTarget messageNavigationTarget = MessageNavigationTarget.DeleteUser;

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetMessageNavigationTarget(messageNavigationTarget);
            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetQueryFrom(queryFrom);

            MessageTextBuilder requestTextBuilder = new MessageTextBuilder(data);

            TextMessageCH textMessageCH = new TextMessageCH(localCommandHandlerTools, requestTextBuilder);

            UserInformation? userInformationForDelete = await entityContext.UsersInformation.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
            userInformationForDelete.Should().NotBeNull();

            // Act
            await textMessageCH.Process(TestContext.Current.CancellationToken);

            IInformationSet? informationSet = localCommandHandlerTools.HandlerResult.InformationSet;
            informationSet.Should().NotBeNull();

            // Assert
            if (string.IsNullOrEmpty(data)) // Невозможно затестить, т.к. MessageTextBuilder не допускает пустой строки
            {
                informationSet.Message.Should().Be(@$"Некорректные параметры для изменения состояния пользователя
======================
Пример: @TestUser-bl
======================

Введите параметры повторно
");
                informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.AdminUsers));

                userInformationForDelete = await entityContext.UsersInformation.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
                userInformationForDelete.Should().NotBeNull();

            }
            else if (data.Contains("22") || data.Contains("NotFoundUser"))
            {
                informationSet.Message.Should().Be(@$"Пользователь {data.AddBoldAndQuotes()} не найден!
======================

Введите параметры для поиска другого пользователя
");

                informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.AdminUsers));

                userInformationForDelete = await entityContext.UsersInformation.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
                userInformationForDelete.Should().NotBeNull();
            }
            else if (data.Contains("1") || data.Contains("TestUsername"))
            {
                informationSet.Message.Should().Be(@$"Пользователь {builder.TestUserInformation.Username.AddBoldAndQuotes()} ({builder.TestUserInformation.Id}) был успешно удалён
======================

Выберите интересующее действие
");

                informationSet.ButtonsSets.Should().Be((ButtonsSet.AdminUsers, ButtonsSet.Admin));

                userInformationForDelete = await entityContext.UsersInformation.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
                userInformationForDelete.Should().BeNull();
            }

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(messageNavigationTarget);
            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(queryFrom);

            localCommandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Theory]
        [InlineData("Other")]
        [InlineData("0")]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("5")]
        public async Task GetInformationSet_WithDeleteResultsExerciseNavigationTarget_ShouldReturnExpectedIInformationSet(string data)
        {
            // Arrange
            builder.WithCycle();
            builder.WithDay();
            builder.WithExercise();
            builder.WithResultExercises();

            DTOUserInformation localDTOCurrentUser = entityContext.UsersInformation.First().ToDTOUserInformation();
            UserContext localUserContext = new UserContext(localDTOCurrentUser, Roles.User | Roles.Admin, false);

            Update update = new Update();

            CommandHandlerTools localCommandHandlerTools = new CommandHandlerTools()
            {
                CoreTools = coreTools,
                HandlerResult = new UpdateHandlerResult()
                {
                    CurrentUserContext = localUserContext,
                    Update = update,
                    ShortUpdateInfo = new ShortUpdateInfo(update, new User(), 1, "...", UpdateType.Message, true)
                },
                RepositoriesStore = new RepositoriesStore(entityContext),
            };

            QueryFrom queryFrom = QueryFrom.NoMatter;
            MessageNavigationTarget messageNavigationTarget = MessageNavigationTarget.DeleteResultsExercise;

            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetMessageNavigationTarget(messageNavigationTarget);
            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.SetQueryFrom(queryFrom);

            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestCycle.ToDTOCycle());
            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestDay.ToDTODay());
            localCommandHandlerTools.HandlerResult.CurrentUserContext.DataManager.SetCurrentDomain(builder.TestExercise.ToDTOExercise());

            int allResultsCount = 3;

            int resultCount = await entityContext.ResultsExercises.CountAsync(TestContext.Current.CancellationToken);
            resultCount.Should().Be(allResultsCount);

            MessageTextBuilder requestTextBuilder = new MessageTextBuilder(data);

            TextMessageCH textMessageCH = new TextMessageCH(localCommandHandlerTools, requestTextBuilder);

            // Act
            await textMessageCH.Process(TestContext.Current.CancellationToken);

            IInformationSet? informationSet = localCommandHandlerTools.HandlerResult.InformationSet;
            informationSet.Should().NotBeNull();

            // Assert
            if (data == "Other")
            {
                informationSet.Message.Should().Be(@$"Не удалось получить кол-во записей для удаления из текста '{data}'
======================

Для удаления введите кол-во последних записей, которые требуется удалить
");
                informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingExercise));

                localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(messageNavigationTarget);

                resultCount = await entityContext.ResultsExercises.CountAsync(TestContext.Current.CancellationToken);
                resultCount.Should().Be(allResultsCount);
            }
            else if (data == "0")
            {
                informationSet.Message.Should().Be(@$"Нельзя удалить меньше одной записи
======================

Для удаления введите кол-во последних записей, которые требуется удалить
");

                informationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingExercise));


                localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(messageNavigationTarget);

                resultCount = await entityContext.ResultsExercises.CountAsync(TestContext.Current.CancellationToken);
                resultCount.Should().Be(allResultsCount);
            }
            else
            {
                int dataInt = int.Parse(data);
                int remainingResultExercisesCount = allResultsCount - dataInt > 0
                    ? allResultsCount - dataInt
                    : 0;

                int resultExercisesCountToDelete = allResultsCount - remainingResultExercisesCount;

                informationSet.Message.Should().Be(@$"Записей удалено: {resultExercisesCountToDelete}
======================

Выберите интересуюущую настройку для упражения {localUserContext.DataManager.CurrentExercise.ThrowIfNull().Name.AddBoldAndQuotes()}
");

                informationSet.ButtonsSets.Should().Be((ButtonsSet.SettingExercise, ButtonsSet.ExercisesList));

                localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

                resultCount = await entityContext.ResultsExercises.CountAsync(TestContext.Current.CancellationToken);
                resultCount.Should().Be(remainingResultExercisesCount);
            }
                
            localCommandHandlerTools.HandlerResult.CurrentUserContext.Navigation.QueryFrom.Should().Be(queryFrom);

            localCommandHandlerTools.HandlerResult.IsNeedSendResponse.Should().BeTrue();

            informationSet.ParseMode.Should().Be(ParseMode.Html);
            informationSet.AdditionalParameters.Should().BeNull();
        }

        public void Dispose()
        {
            builder.Dispose();
        }
    }
}