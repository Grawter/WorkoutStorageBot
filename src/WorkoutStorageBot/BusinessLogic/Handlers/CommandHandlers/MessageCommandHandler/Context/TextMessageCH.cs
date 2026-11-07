using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Extensions;
using WorkoutStorageBot.BusinessLogic.Helpers.Converters;
using WorkoutStorageBot.BusinessLogic.Helpers.SharedBusinessLogic;
using WorkoutStorageBot.BusinessLogic.Repositories;
using WorkoutStorageBot.Core.Extensions;
using WorkoutStorageBot.Core.Helpers;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.InformationSetForSend;
using WorkoutStorageBot.Model.Interfaces;
using WorkoutStorageModels.Entities.BusinessLogic;
using WorkoutStorageModels.Entities.Core.Logging;

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.MessageCommandHandler.Context
{
    internal class TextMessageCH : MessageCH
    {
        private static IReadOnlyDictionary<MessageNavigationTarget, Func<TextMessageCH, CancellationToken, Task<IInformationSet>>> commandMap
           = new Dictionary<MessageNavigationTarget, Func<TextMessageCH, CancellationToken, Task<IInformationSet>>>
           {
               { MessageNavigationTarget.Default, (x, y) => Task.FromResult(x.DefaultCommand()) },
               { MessageNavigationTarget.AddCycle, (x, y)  => x.AddCycleCommand(y) },
               { MessageNavigationTarget.AddDays, (x, y)  => x.AddDaysCommand(y) },
               { MessageNavigationTarget.AddExercises, (x, y)  => Task.FromResult(x.AddExercisesCommand()) },
               { MessageNavigationTarget.AddResultForExercise, (x, y)  => Task.FromResult(x.AddResultForExerciseCommand()) },
               { MessageNavigationTarget.AddCommentForExerciseTimer, (x, y)  => x.AddCommentForExerciseTimerCommand(y) },
               { MessageNavigationTarget.ChangeNameCycle, (x, y)  => x.ChangeNameCommand(domainType: DomainType.Cycle, y) },
               { MessageNavigationTarget.ChangeNameDay, (x, y)  => x.ChangeNameCommand(domainType: DomainType.Day, y) },
               { MessageNavigationTarget.ChangeNameExercise, (x, y)  => x.ChangeNameCommand(domainType: DomainType.Exercise, y) },
               { MessageNavigationTarget.FindResultsByDate, (x, y)  => x.FindResultsByDateCommand(isNeedFindByCurrentDay: false, y) },
               { MessageNavigationTarget.FindResultsByDateInDay, (x, y)  => x.FindResultsByDateCommand(isNeedFindByCurrentDay: true, y) },
               { MessageNavigationTarget.FindLogByID, (x, y)  => x.FindLogByIDCommand(isEventID: false, y) },
               { MessageNavigationTarget.FindLogByEventID, (x, y)  => x.FindLogByIDCommand(isEventID: true, y) },
               { MessageNavigationTarget.SendMessageToUser, (x, y)  => x.SendMessageToUserCommand(y) },
               { MessageNavigationTarget.SendMessagesToActiveUsers, (x, y)  => x.MassiveSendMessagesToUsersCommand(isNeedFromDB: false, y) },
               { MessageNavigationTarget.SendMessagesToAllUsers, (x, y)  => x.MassiveSendMessagesToUsersCommand(isNeedFromDB: true, y) },
               { MessageNavigationTarget.ChangeUserState, (x, y)  => x.ChangeUserStateCommand(y) },
               { MessageNavigationTarget.DeleteUser, (x, y)  => x.DeleteUserCommand(y) },
               { MessageNavigationTarget.DeleteResultsExercise, (x, y)  => x.DeleteResultsExerciseCommand(y) },
           };

        internal TextMessageCH(CommandHandlerTools commandHandlerTools, MessageTextBuilder requestTextBuilder) : base(commandHandlerTools, requestTextBuilder)
        { }

        protected override async Task<IInformationSet> GetInformationSet(CancellationToken cancellationToken)
        {
            Func<TextMessageCH, CancellationToken, Task<IInformationSet>> selectedCommand = GetSelectedCommand();

            IInformationSet informationSet = await selectedCommand(this, cancellationToken);

            CheckInformationSet(informationSet);

            return informationSet;
        }

        protected Func<TextMessageCH, CancellationToken, Task<IInformationSet>> GetSelectedCommand()
            => commandMap.GetValueOrDefault(this.CurrentUserContext.Navigation.MessageNavigationTarget) 
            ?? throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.MessageNavigationTarget: {this.CurrentUserContext.Navigation.MessageNavigationTarget}");
        
        private IInformationSet DefaultCommand()
        {
            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (requestTextBuilder.RemoveCompletely().Build().ToLower())
            {
                case "/start":
                    if (this.CurrentUserContext.ActiveCycle != null)
                    {
                        responseTextBuilder = new ResponseTextBuilder("Выберите интересующий раздел");
                        buttonsSets = (ButtonsSet.Main, ButtonsSet.None);
                    }
                    else
                    {
                        this.CurrentUserContext.Navigation.SetQueryFrom(QueryFrom.Start);

                        responseTextBuilder = new ResponseTextBuilder("Начнём");
                        buttonsSets = (ButtonsSet.AddCycle, ButtonsSet.None);
                    }
                    break;

                default:
                    responseTextBuilder = new ResponseTextBuilder($"Неизвестная команда: {requestTextBuilder.Build().AddBoldAndQuotes()}", 
                        $"Для получения разделов воспользуйтесь командой {"/Start".AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                    break;
            }

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> AddCycleCommand(CancellationToken cancellationToken)
        {
            requestTextBuilder.RemoveCompletely().WithoutServiceSymbol();

            string domainName = requestTextBuilder.Build();

            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;
            IInformationSet informationSet;

            if (AlreadyExistDomainWithName(domainName, DomainType.Cycle))
            {
                responseTextBuilder = new ResponseTextBuilder("Ошибка при добавлении названия!", $"Цикл с названием {domainName.AddBoldAndQuotes()} уже существует",
                    "Ввведите другое название тренировочного цикла");
                buttonsSets = GetButtonsSetIfFailedSaveNewDomainValue(ButtonsSet.SettingCycles);

                informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

                return informationSet;
            }

            bool isNeedSetActiveCycle = this.CurrentUserContext.ActiveCycle == null ? true : false;
            DTOCycle currentCycle = this.CurrentUserContext.DataManager.CreateAndSetCurrentCycle(requestTextBuilder.Build(), isNeedSetActiveCycle, this.CurrentUserContext.UserInformation);

            if (isNeedSetActiveCycle)
                this.CurrentUserContext.SetNewActiveCycleForce(currentCycle);

            this.CurrentUserContext.UserInformation.Cycles.Add(currentCycle);
            await this.Db.AddEntity(currentCycle, cancellationToken);

            this.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull();

            switch (this.CurrentUserContext.Navigation.QueryFrom)
            {
                case QueryFrom.Start:
                    this.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.AddDays);

                    responseTextBuilder = new ResponseTextBuilder($"Цикл {this.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()} сохранён",
                        $"Введите название тренирочного дня для цикла {this.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                    break;

                case QueryFrom.Settings:
                    this.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

                    responseTextBuilder = new ResponseTextBuilder($"Цикл {this.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()} сохранён",
                        "Выберите дальнейшие действия");
                    buttonsSets = (ButtonsSet.AddDays, ButtonsSet.SettingCycles);

                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CurrentUserContext.Navigation.QueryFrom}");
            }

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> AddDaysCommand(CancellationToken cancellationToken)
        {
            requestTextBuilder.RemoveCompletely().WithoutServiceSymbol();

            string domainName = requestTextBuilder.Build();

            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;
            IInformationSet informationSet;

            this.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull();

            if (AlreadyExistDomainWithName(domainName, DomainType.Day))
            {
                responseTextBuilder = new ResponseTextBuilder("Ошибка при сохранении!", $"В этом цикле уже существует день с названием {domainName.AddBoldAndQuotes()}",
            $"Ввведите другое название дня для цикла {this.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}");

                buttonsSets = GetButtonsSetIfFailedSaveNewDomainValue(ButtonsSet.SettingDays);

                informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

                return informationSet;
            }

            DTODay currentDay = this.CurrentUserContext.DataManager.CreateAndSetCurrentDay(domainName);

            this.CurrentUserContext.DataManager.CurrentCycle.Days.Add(currentDay);

            await this.Db.AddEntity(currentDay, cancellationToken);

            switch (this.CurrentUserContext.Navigation.QueryFrom)
            {
                case QueryFrom.Start:
                    this.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.AddExercises);

                    responseTextBuilder = new ResponseTextBuilder($"День {currentDay.Name.AddBoldAndQuotes()} сохранён в цикле {this.CurrentUserContext.DataManager.CurrentCycle.Name.AddBold()}",
                        $"Введите название упражения для этого дня.{Environment.NewLine}{CommonConsts.Exercise.ExamplesTypesExercise}", 
                        CommonConsts.Exercise.InputFormatExercise);
                    buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                    break;

                case QueryFrom.Settings:
                    this.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

                    responseTextBuilder = new ResponseTextBuilder($"День {currentDay.Name.AddBoldAndQuotes()} сохранён в цикле {this.CurrentUserContext.DataManager.CurrentCycle.Name.AddBold()}");
                    buttonsSets = (ButtonsSet.AddExercises, ButtonsSet.SettingDays);

                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CurrentUserContext.Navigation.QueryFrom}");
            }

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet AddExercisesCommand()
        {
            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;
            IInformationSet informationSet;

            this.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull();

            requestTextBuilder.RemoveCompletely(80).WithoutServiceSymbol();

            string? error = null;

            (List<DTOExercise> Exercises, string? Error) resultGettingExerciseFromText = SharedExercisesAndResultsLogicHelper.GetExercisesFromText(requestTextBuilder.Build());
            error = resultGettingExerciseFromText.Error;

            if (string.IsNullOrWhiteSpace(error))
            {
                // Сначала нужна общая проверка, есть ли в текущем дне добавляемое упражнение
                foreach (DTOExercise exercise in resultGettingExerciseFromText.Exercises)
                {
                    if (AlreadyExistDomainWithName(exercise.Name, DomainType.Exercise))
                    {
                        error = $"В этом дне уже существует упражнение с названием {exercise.Name.AddBoldAndQuotes()}";
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(error) && this.CurrentUserContext.DataManager.TryAddTempExercises(resultGettingExerciseFromText.Exercises, out error))
            {
                responseTextBuilder = new ResponseTextBuilder("Упражнение(я) зафиксировано(ы)",
                    $"Введите след. упражнение(я) для дня {this.CurrentUserContext.DataManager.CurrentDay.Name.AddBoldAndQuotes()} либо нажмите {"Сохранить".AddQuotes()} для сохранения зафиксированных упражнений");
                buttonsSets = (ButtonsSet.SaveExercises, ButtonsSet.None);
            }
            else
            {
                responseTextBuilder = new ResponseTextBuilder("Упражнение(я) не зафиксировано(ы)!", error, "textForReplace");

                switch (this.CurrentUserContext.Navigation.QueryFrom)
                {
                    case QueryFrom.Start:

                        responseTextBuilder.ResetTarget(@$"Введите другое(ие) упражнение(й) для дня {this.CurrentUserContext.DataManager.CurrentDay.Name.AddBoldAndQuotes()}

{CommonConsts.Exercise.InputFormatExercise}");
                        buttonsSets = (ButtonsSet.None, ButtonsSet.None);

                        break;

                    case QueryFrom.Settings:

                        responseTextBuilder.ResetTarget(@$"Сбросьте текущие упражнения, чтобы вернуться обратно или введите другое(ие) упражнение(й) для дня {this.CurrentUserContext.DataManager.CurrentDay.Name.AddBoldAndQuotes()}

{CommonConsts.Exercise.InputFormatExercise}");
                        buttonsSets = (ButtonsSet.ResetTempDomains, ButtonsSet.None);

                        Dictionary<string, string> additionalParameters = new Dictionary<string, string>() { { "type", CommonConsts.DomainsAndEntities.Exercise } };

                        informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets, additionalParameters);

                        return informationSet;

                    default:
                        throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CurrentUserContext.Navigation.QueryFrom}");
                }
            }

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet AddResultForExerciseCommand()
        {
            requestTextBuilder.RemoveCompletely(80).WithoutServiceSymbol();

            DTOExercise currentExercise = this.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull();

            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;

            (List<DTOResultExercise> ResultsExercise, string? Error) result = SharedExercisesAndResultsLogicHelper.GetResultsExerciseFromText(requestTextBuilder.Build(), currentExercise.Mode);

            if (string.IsNullOrWhiteSpace(result.Error))
            {
                this.CurrentUserContext.DataManager.AddTempResultsExercise(result.ResultsExercise);

                responseTextBuilder = new ResponseTextBuilder("Подход(ы) зафиксирован(ы)",
                @$"Введите результат след. подхода для упражения {this.CurrentUserContext.DataManager.CurrentExercise.Name.AddBoldAndQuotes()}
либо нажмите {"Сохранить".AddQuotes()} для сохранения указанных подходов");

                buttonsSets = (ButtonsSet.SaveResultsExercise, ButtonsSet.None);
            }
            else
            {
                string inputFormatExerciseResult = currentExercise.Mode switch
                {
                    ExercisesMods.Count => CommonConsts.ResultExercise.InputFormatExerciseResultCount,
                    ExercisesMods.WeightCount => CommonConsts.ResultExercise.InputFormatExerciseResultWeightCount,
                    ExercisesMods.Timer => CommonConsts.ResultExercise.InputFormatExerciseResultTimer,
                    ExercisesMods.FreeResult => CommonConsts.ResultExercise.InputFormatExerciseResultFreeResult,
                    _ => throw new NotImplementedException($"Неожиданный тип упражнения: {currentExercise.Mode.ToString()}")
                };

                responseTextBuilder = new ResponseTextBuilder(result.Error,
                    inputFormatExerciseResult,
                    $"Сбросьте текущие результаты, чтобы вернуться обратно или введите результат заново для упражения {this.CurrentUserContext.DataManager.CurrentExercise.Name.AddBoldAndQuotes()}");
                buttonsSets = (ButtonsSet.ResetResultsExercise, ButtonsSet.None);
            }

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> AddCommentForExerciseTimerCommand(CancellationToken cancellationToken)
        {
            string comment = requestTextBuilder.RemoveCompletely(50).WithoutServiceSymbol().Build();

            DTOResultExercise resultExercise = this.CurrentUserContext.DataManager.TempResultsExercise.ThrowIfNull().Single();
            resultExercise.FreeResult += $" / {comment}";

            resultExercise.Exercise = this.CurrentUserContext.DataManager.CurrentExercise;
            this.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().ResultsExercise.Add(resultExercise);
            await this.Db.AddEntity(resultExercise, cancellationToken);

            this.CurrentUserContext.DataManager.ResetTempResultsExercise();

            this.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder(resultExercise.FreeResult.AddBold(), "Введённые данные сохранены", 
                $"Выберите упражнение или посмотрите предыдущие результаты дня {this.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()}");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout);

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> ChangeNameCommand(DomainType domainType, CancellationToken cancellationToken)
        {
            requestTextBuilder.RemoveCompletely(25).WithoutServiceSymbol();

            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;
            IInformationSet informationSet;

            string domainName = requestTextBuilder.Build();

            switch (domainType)
            {
                case DomainType.Cycle:
                    if (AlreadyExistDomainWithName(domainName, DomainType.Cycle))
                    {
                        responseTextBuilder = new ResponseTextBuilder("Ошибка при добавлении названия!", $"Цикл с названием {domainName.AddBoldAndQuotes()} уже существует",
                            "Ввведите другое название тренировочного цикла");
                        buttonsSets = GetButtonsSetIfFailedSaveNewDomainValue(ButtonsSet.SettingCycles);

                        informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

                        return informationSet;
                    }

                    responseTextBuilder = new ResponseTextBuilder("Название цикла сохранено!");
                    buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.CyclesList);
                    break;

                case DomainType.Day:
                    if (AlreadyExistDomainWithName(domainName, DomainType.Day))
                    {
                        responseTextBuilder = new ResponseTextBuilder("Ошибка при сохранении!", $"В этом цикле уже существует день с названием {domainName.AddBoldAndQuotes()}",
                    $"Ввведите другое название дня для цикла {this.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}");

                        buttonsSets = GetButtonsSetIfFailedSaveNewDomainValue(ButtonsSet.SettingDays);

                        informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

                        return informationSet;
                    }

                    responseTextBuilder = new ResponseTextBuilder("Название дня сохранено!", "Выберите интересующую настройку для указанного дня");
                    buttonsSets = (ButtonsSet.SettingDay, ButtonsSet.DaysList);
                    break;

                case DomainType.Exercise:

                    if (AlreadyExistDomainWithName(domainName, DomainType.Exercise))
                    {
                        responseTextBuilder = new ResponseTextBuilder("Ошибка при сохранении!", $"В этом дне уже существует упражнение с названием {domainName.AddBoldAndQuotes()}",
                        $"Введите другое(ие) название(я) упражнение(ий) для дня {this.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()}");

                        buttonsSets = GetButtonsSetIfFailedSaveNewDomainValue(ButtonsSet.SettingExercises);

                        informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

                        return informationSet;
                    }

                    responseTextBuilder = new ResponseTextBuilder("Название упражнения сохранено!", "Выберите интересующую настройку для указанного упражнения");
                    buttonsSets = (ButtonsSet.SettingExercise, ButtonsSet.ExercisesList);
                    break;
                default:
                    throw new InvalidOperationException($"Неожиданный {nameof(domainType)} : {domainType}");
            }

            IDTODomain DTODomain = this.CurrentUserContext.DataManager.GetRequiredCurrentDomain(domainType);
            DTODomain.Name = domainName;

            await this.Db.UpdateEntity(DTODomain, cancellationToken);

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            this.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

            return informationSet;
        }

        private bool AlreadyExistDomainWithName(string name, DomainType domainType)
        {
            switch (domainType)
            {
                case DomainType.Cycle:
                    return this.CurrentUserContext.UserInformation.Cycles.Any(c => c.Name == name);
                
                case DomainType.Day:
                    return this.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Days.Any(d => d.Name == name);

                case DomainType.Exercise:
                    return this.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Exercises.Any(e => e.Name == name);

                default:
                    throw new NotSupportedException($"Неподдерживаемый тип домена: {domainType.ToString()}");
            }
        }

        private (ButtonsSet, ButtonsSet) GetButtonsSetIfFailedSaveNewDomainValue(ButtonsSet backButtonForSetting)
        {
            switch (this.CurrentUserContext.Navigation.QueryFrom)
            {
                case QueryFrom.Start:
                    return (ButtonsSet.None, ButtonsSet.None);
        
                case QueryFrom.Settings:
                    return (ButtonsSet.None, backButtonForSetting);

                default:
                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CurrentUserContext.Navigation.QueryFrom}");
            }
        }

        private async Task<IInformationSet> FindResultsByDateCommand(bool isNeedFindByCurrentDay, CancellationToken cancellationToken)
        {
            IInformationSet informationSet;

            string finderDate = requestTextBuilder.RemoveCompletely(10).Build();

            if (!DateTime.TryParseExact(finderDate, CommonConsts.Exercise.ValidDateFormats, null, System.Globalization.DateTimeStyles.None, out DateTime dateTime))
            {
                ResponseTextBuilder responseConverter = new ResponseTextBuilder(
                    $"Не удалось получить дату из сообщения '{finderDate}', для корректного поиска придерживайтесь допустимого формата",
                    CommonConsts.Exercise.FindResultsByDateFormat,
                    "Введите дату искомой тренировки");

                (ButtonsSet, ButtonsSet) buttonsSets = isNeedFindByCurrentDay
                    ? (ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay)
                    : (ButtonsSet.None, ButtonsSet.DaysListWithLastWorkout);

                informationSet = new MessageInformationSet(responseConverter.Build(), buttonsSets);

                return informationSet;
            }

            informationSet = await SharedExercisesAndResultsLogicHelper.GetInformationSetWithTextResultsByDateCommand(this.Db, 
                                                                                                                      this.CurrentUserContext, 
                                                                                                                      dateTime, 
                                                                                                                      isNeedFindByCurrentDay,
                                                                                                                      cancellationToken);

            return informationSet;
        }

        private async Task<IInformationSet> FindLogByIDCommand(bool isEventID, CancellationToken cancellationToken)
        {
            this.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            LogsRepository logsRepository = this.RepositoriesHub.GetRequiredRepository<LogsRepository>();

            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.AdminLogs, ButtonsSet.Admin);

            string identifierType = isEventID
                             ? "eventId"
                             : "Id";

            string IdStr = requestTextBuilder.RemoveCompletely(10).WithoutServiceSymbol().Build();

            if (!int.TryParse(IdStr, out int Id))
            {
                responseTextBuilder = new ResponseTextBuilder($"Передан некорректный {identifierType}: {IdStr.AddBoldAndQuotes()}", "Выберите интересующее действие");

                informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

                return informationSet;
            }

            Log? log;

            if (isEventID)
                log = await logsRepository.GetLogByEventId(Id, cancellationToken);
            else
                log = await logsRepository.GetLogById(Id, cancellationToken);

            if (log == null)
            {
                responseTextBuilder = new ResponseTextBuilder($"Не удалось найти лог с {identifierType}: {Id.ToString().AddBoldAndQuotes()}", "Выберите интересующее действие");

                informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

                return informationSet;
            }

            string logStr = LogFormatter.ConvertLogToStr(log);

            responseTextBuilder = new ResponseTextBuilder("Найденный лог:", logStr, "Выберите интересующее действие");

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets, ParseMode.None);

            return informationSet;
        }

        private async Task<IInformationSet> SendMessageToUserCommand(CancellationToken cancellationToken)
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;

            requestTextBuilder.RemoveCompletely(300).WithoutServiceSymbol();

            string[] parameters = requestTextBuilder.Build().Split("-", StringSplitOptions.RemoveEmptyEntries);

            bool isInvalidParameters = parameters.Length != 2 || string.IsNullOrWhiteSpace(parameters[0]) || string.IsNullOrWhiteSpace(parameters[1]);

            if (isInvalidParameters)
            {
                responseTextBuilder = new ResponseTextBuilder("Некорректные параметры для отправки сообщения пользователю", "Пример: @TestUser-Тест",
                    "Введите параметры повторно");
                buttonsSets = (ButtonsSet.None, ButtonsSet.AdminUsers);
                
                informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);
                return informationSet;
            }

            string userIdentity = parameters[0];
            string text = parameters[1];

            UserInformation? user;

            UserInformationRepository userInformationRepository = this.RepositoriesHub.GetRequiredRepository<UserInformationRepository>();

            if (long.TryParse(userIdentity, out long userID))
                user = await userInformationRepository.GetUserInformation(userID, cancellationToken);
            else
                user = await userInformationRepository.GetUserInformation(userIdentity, cancellationToken);

            if (user == null)
            {
                responseTextBuilder = new ResponseTextBuilder($"Пользователь {userIdentity.AddBoldAndQuotes()} не найден!", 
                    "Введите параметры для поиска другого пользователя");
                buttonsSets = (ButtonsSet.None, ButtonsSet.AdminUsers);

                informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);
                return informationSet;
            }

            await this.BotResponseSender.SendSimpleNotification(user.UserId, text, cancellationToken);

            responseTextBuilder = new ResponseTextBuilder($"Сообщение отправлено пользователю {userIdentity.AddBoldAndQuotes()}",
                    "Выберите интересующее действие");
            buttonsSets = (ButtonsSet.AdminUsers, ButtonsSet.Admin);

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> MassiveSendMessagesToUsersCommand(bool isNeedFromDB, CancellationToken cancellationToken)
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            string text = requestTextBuilder.RemoveCompletely(300).WithoutServiceSymbol().Build();

            int counter = 0;

            foreach (var userID in isNeedFromDB 
                ? await this.Db.UsersInformation.AsNoTracking().Select(x => x.UserId).ToListAsync(cancellationToken)
                : this.ContextKeeper.GetAllKeys())
            {
                await this.BotResponseSender.SendSimpleNotification(userID, text, cancellationToken);
                ++counter;
            }

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder(@$"Сообщение отправлено {counter} пользователям",
                "Выберите интересующее действие");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.AdminUsers, ButtonsSet.Admin);

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> ChangeUserStateCommand(CancellationToken cancellationToken)
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;

            requestTextBuilder.RemoveCompletely(35).WithoutServiceSymbol();

            string[] parameters = requestTextBuilder.Build().Split("-", StringSplitOptions.RemoveEmptyEntries);

            bool isInvalidParameters = parameters.Length != 2 || string.IsNullOrWhiteSpace(parameters[0]) || string.IsNullOrWhiteSpace(parameters[1]);

            if (isInvalidParameters)
            {
                responseTextBuilder = new ResponseTextBuilder("Некорректные параметры для изменения состояния пользователя", "Пример: @TestUser-bl", 
                    "Введите параметры повторно");
                buttonsSets = (ButtonsSet.None, ButtonsSet.AdminUsers);
                
                informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);
                return informationSet;
            }

            string userIdentity = parameters[0];
            string list = parameters[1];

            UserInformation? user;

            AdminWrapper adminWrapper = this.RepositoriesHub.InitRepository(x => new AdminWrapper(x, this.GetConfigurationData(), this.CreateLogger<AdminWrapper>()));

            if (long.TryParse(userIdentity, out long userID))
                user = await adminWrapper.GetUserInformation(userID, cancellationToken);
            else
                user = await adminWrapper.GetUserInformation(userIdentity, cancellationToken);

            if (user == null)
            {
                responseTextBuilder = new ResponseTextBuilder($"Пользователь {userIdentity.AddBoldAndQuotes()} не найден!", "Введите параметры для поиска другого пользователя");
                buttonsSets = (ButtonsSet.None, ButtonsSet.AdminUsers);

                informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);
                return informationSet;
            }

            UserContext? userContext = this.ContextKeeper.GetContext(user.UserId);

            switch (list)
            {
                case "wl":

                    if (userContext != null)
                        userContext.UserInformation.WhiteList = !userContext.UserInformation.WhiteList;

                    await adminWrapper.ChangeWhiteListByUser(user, cancellationToken);

                    responseTextBuilder = new ResponseTextBuilder($"WhiteList для {user.Username.AddBoldAndQuotes()} ({user.UserId}) установлен в: {user.WhiteList.ToString().AddBold()}",
                        "Выберите интересующее действие");

                    break;
                case "bl":

                    if (userContext != null)
                        userContext.UserInformation.BlackList = !userContext.UserInformation.BlackList;

                    await adminWrapper.ChangeBlackListByUser(user, cancellationToken);

                    responseTextBuilder = new ResponseTextBuilder($"BlackList для {user.Username.AddBoldAndQuotes()} ({user.UserId}) установлен в: {user.BlackList.ToString().AddBold()}",
                        "Выберите интересующее действие");
                    break;
                default:
                    throw new InvalidOperationException($"Неизвестный для изменения список: {list}");
            }

            buttonsSets = (ButtonsSet.AdminUsers, ButtonsSet.Admin);
            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> DeleteUserCommand(CancellationToken cancellationToken)
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;

            string userIdentity = requestTextBuilder.RemoveCompletely(35).WithoutServiceSymbol().Build();

            if (string.IsNullOrWhiteSpace(userIdentity))
            {
                responseTextBuilder = new ResponseTextBuilder("Некорректные параметры для удаления пользователя", "Пример: @TestUser",
                    "Введите параметры повторно");
                buttonsSets = (ButtonsSet.None, ButtonsSet.AdminUsers);

                informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);
                return informationSet;
            }

            UserInformation? user;

            AdminWrapper adminWrapper = this.RepositoriesHub.InitRepository(x => new AdminWrapper(x, this.GetConfigurationData(), this.CreateLogger<AdminWrapper>()));

            if (long.TryParse(userIdentity, out long userID))
                user = await adminWrapper.GetUserInformation(userID, cancellationToken);
            else
                user = await adminWrapper.GetUserInformation(userIdentity, cancellationToken);

            if (user == null)
            {
                responseTextBuilder = new ResponseTextBuilder($"Пользователь {userIdentity.AddBoldAndQuotes()} не найден!", "Введите параметры для поиска другого пользователя");
                buttonsSets = (ButtonsSet.None, ButtonsSet.AdminUsers);

                informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);
                return informationSet;
            }

            this.ContextKeeper.RemoveContext(user.UserId);
            await adminWrapper.DeleteAccount(user, cancellationToken);

            responseTextBuilder = new ResponseTextBuilder($"Пользователь {user.Username.AddBoldAndQuotes()} ({user.UserId}) был успешно удалён", "Выберите интересующее действие");
            buttonsSets = (ButtonsSet.AdminUsers, ButtonsSet.Admin);

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> DeleteResultsExerciseCommand(CancellationToken cancellationToken)
        {
            string countToDeleteStr = requestTextBuilder.RemoveCompletely(35).WithoutServiceSymbol().Build();

            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;

            IInformationSet informationSet;

            if (!int.TryParse(countToDeleteStr, out int countToDelete))
            {
                responseTextBuilder = new ResponseTextBuilder($"Не удалось получить кол-во записей для удаления из текста '{countToDeleteStr}'",
                    "Для удаления введите кол-во последних записей, которые требуется удалить");
                buttonsSets = (ButtonsSet.None, ButtonsSet.SettingExercise);

                informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);
                return informationSet;
            }

            if (countToDelete < 1)
            {
                responseTextBuilder = new ResponseTextBuilder($"Нельзя удалить меньше одной записи",
                    "Для удаления введите кол-во последних записей, которые требуется удалить");
                buttonsSets = (ButtonsSet.None, ButtonsSet.SettingExercise);

                informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);
                return informationSet;
            }

            DTOExercise currentExercise = this.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull();

            List<int> idsToDelete = await this.Db.ResultsExercises.Where(x => x.ExerciseId == currentExercise.Id)
                                                                  .OrderByDescending(x => x.DateTime)
                                                                  .ThenByDescending(x => x.Id)
                                                                  .Take(countToDelete)
                                                                  .Select(x => x.Id)
                                                                  .ToListAsync(cancellationToken);

            int numberOfRowsAffected = 0;

            if (idsToDelete.Count > 0)
            {
                numberOfRowsAffected = await this.Db.ResultsExercises.Where(x => idsToDelete.Contains(x.Id))
                                                                     .ExecuteDeleteAsync(cancellationToken);

                var tracked = this.Db.ChangeTracker.Entries<ResultExercise>()
                                                   .Where(e => idsToDelete.Contains(e.Entity.Id))
                                                   .ToList();

                foreach (var entry in tracked)
                    entry.State = EntityState.Detached;

                this.CurrentUserContext.DataManager.CurrentExercise.ResultsExercise.RemoveAll(x => idsToDelete.Contains(x.Id));
            }

            responseTextBuilder = new ResponseTextBuilder($"Записей удалено: {numberOfRowsAffected}",
                $"Выберите интересуюущую настройку для упражения {currentExercise.Name.AddBoldAndQuotes()}");

            buttonsSets = (ButtonsSet.SettingExercise, ButtonsSet.ExercisesList);

            this.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private bool AccessDenied([NotNullWhen(true)] out IInformationSet? informationSet)
        {
            informationSet = null;

            if (!this.CurrentUserContext.IsAdmin())
            {
                informationSet = SharedCommonLogicHelper.GetAccessDeniedMessageInformationSet();

                this.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

                return true;
            }

            return false;
        }
    }
}