using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Exceptions;
using WorkoutStorageBot.BusinessLogic.Extensions;
using WorkoutStorageBot.BusinessLogic.Helpers.Converters;
using WorkoutStorageBot.BusinessLogic.Helpers.SharedBusinessLogic;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.BusinessLogic.Repositories;
using WorkoutStorageBot.Core.Extensions;
using WorkoutStorageBot.Core.Helpers;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.Interfaces;
using WorkoutStorageModels.Entities.BusinessLogic;
using WorkoutStorageModels.Entities.Core.Logging;

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.MessageCommandHandler.Context
{
    internal class TextMessageCH : MessageCH
    {
        private static IReadOnlyDictionary<MessageNavigationTarget, Func<TextMessageCH, Task<IInformationSet>>> commandMap
           = new Dictionary<MessageNavigationTarget, Func<TextMessageCH, Task<IInformationSet>>>
           {
               { MessageNavigationTarget.Default, (x) => Task.FromResult(x.DefaultCommand()) },
               { MessageNavigationTarget.AddCycle, (x) => x.AddCycleCommand() },
               { MessageNavigationTarget.AddDays, (x) => x.AddDaysCommand() },
               { MessageNavigationTarget.AddExercises, (x) => Task.FromResult(x.AddExercisesCommand()) },
               { MessageNavigationTarget.AddResultForExercise, (x) => Task.FromResult(x.AddResultForExerciseCommand()) },
               { MessageNavigationTarget.AddCommentForExerciseTimer, (x) => x.AddCommentForExerciseTimerCommand() },
               { MessageNavigationTarget.ChangeNameCycle, (x) => x.ChangeNameCommand(domainType: CommonConsts.DomainsAndEntities.Cycle) },
               { MessageNavigationTarget.ChangeNameDay, (x) => x.ChangeNameCommand(domainType: CommonConsts.DomainsAndEntities.Day) },
               { MessageNavigationTarget.ChangeNameExercise, (x) => x.ChangeNameCommand(domainType: CommonConsts.DomainsAndEntities.Exercise) },
               { MessageNavigationTarget.FindResultsByDate, (x) => x.FindResultByDateCommand(isNeedFindByCurrentDay: false) },
               { MessageNavigationTarget.FindResultsByDateInDay, (x) => x.FindResultByDateCommand(isNeedFindByCurrentDay: true) },
               { MessageNavigationTarget.FindLogByID, (x) => x.FindLogByIDCommand(isEventID: false) },
               { MessageNavigationTarget.FindLogByEventID, (x) => x.FindLogByIDCommand(isEventID: true) },
               { MessageNavigationTarget.SendMessageToUser, (x) => x.SendMessageToUserCommand() },
               { MessageNavigationTarget.SendMessagesToActiveUsers, (x) => x.MassiveSendMessagesToUsers(isNeedFromDB: false) },
               { MessageNavigationTarget.SendMessagesToAllUsers, (x) => x.MassiveSendMessagesToUsers(isNeedFromDB: true) },
               { MessageNavigationTarget.ChangeUserState, (x) => x.ChangeUserStateCommand() },
               { MessageNavigationTarget.DeleteUser, (x) => x.DeleteUserCommand() },
               { MessageNavigationTarget.DeleteResultsExercise, (x) => x.DeleteResultsExerciseCommand() },
           };

        internal TextMessageCH(CommandHandlerTools commandHandlerTools, MessageTextBuilder requestTextBuilder) : base(commandHandlerTools, requestTextBuilder)
        { }

        internal override async Task<IInformationSet> GetInformationSet()
        {
            Func<TextMessageCH, Task<IInformationSet>>? selectedCommand = commandMap.GetValueOrDefault(this.CurrentUserContext.Navigation.MessageNavigationTarget)
                ?? throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.MessageNavigationTarget: {this.CurrentUserContext.Navigation.MessageNavigationTarget}");

            IInformationSet informationSet = await selectedCommand(this);

            CheckInformationSet(informationSet);

            return informationSet;
        }

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

        private async Task<IInformationSet> AddCycleCommand()
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
                buttonsSets = (ButtonsSet.None, ButtonsSet.SettingCycles);

                informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

                return informationSet;
            }

            bool hasActiveCycle = this.CurrentUserContext.ActiveCycle == null ? false : true;
            DTOCycle currentCycle = this.CurrentUserContext.DataManager.SetCurrentCycle(requestTextBuilder.Build(), !hasActiveCycle, this.CurrentUserContext.UserInformation);

            if (!hasActiveCycle)
                this.CurrentUserContext.UpdateActiveCycleForce(currentCycle);

            this.CurrentUserContext.UserInformation.Cycles.Add(currentCycle);
            await this.Db.AddEntity(currentCycle);

            this.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull();

            switch (this.CurrentUserContext.Navigation.QueryFrom)
            {
                case QueryFrom.Start:
                    this.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.AddDays);

                    responseTextBuilder = new ResponseTextBuilder($"Цикл {this.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()} сохранён!",
                        $"Введите название тренирочного дня для цикла {this.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                    break;

                case QueryFrom.Settings:
                    this.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

                    responseTextBuilder = new ResponseTextBuilder($"Цикл {this.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()} сохранён!",
                        "Выберите дальнейшие действия");
                    buttonsSets = (ButtonsSet.AddDays, ButtonsSet.SettingCycles);

                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CurrentUserContext.Navigation.QueryFrom}");
            }

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> AddDaysCommand()
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

            DTODay currentDay = this.CurrentUserContext.DataManager.SetCurrentDay(domainName);

            this.CurrentUserContext.DataManager.CurrentCycle.Days.Add(currentDay);

            await this.Db.AddEntity(currentDay);

            switch (this.CurrentUserContext.Navigation.QueryFrom)
            {
                case QueryFrom.Start:
                    this.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.AddExercises);

                    responseTextBuilder = new ResponseTextBuilder($"День {currentDay.Name.AddBoldAndQuotes()} сохранён!",
                        $"Введите название упражения для этого дня.{Environment.NewLine}{CommonConsts.Exercise.ExamplesTypesExercise}", 
                        CommonConsts.Exercise.InputFormatExercise);
                    buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                    break;

                case QueryFrom.Settings:
                    this.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

                    responseTextBuilder = new ResponseTextBuilder($"День {currentDay.Name.AddBoldAndQuotes()} сохранён!");
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

            Dictionary<string, string>? additionalParameters = new Dictionary<string, string>();

            requestTextBuilder.RemoveCompletely(80).WithoutServiceSymbol();

            List<DTOExercise> exercises = new List<DTOExercise>();

            string exceptionMessage = string.Empty;

            try
            {
                exercises = SharedExercisesAndResultsLogicHelper.GetExercisesFromText(requestTextBuilder.Build());
            }
            catch (CreateExerciseException ex)
            {
                exceptionMessage = ex.Message;
            }

            // Сначала нужно чтобы была общая проверка, есть ли в текущем дне добавляемое упражнение
            foreach (DTOExercise exercise in exercises)
            {
                if (AlreadyExistDomainWithName(exercise.Name, DomainType.Exercise))
                    exceptionMessage = $"В этом дне уже существует упражнение с названием {exercise.Name.AddBoldAndQuotes()}";
            }

            if (string.IsNullOrWhiteSpace(exceptionMessage))
            {
                if (!this.CurrentUserContext.DataManager.TryAddTempExercises(exercises, out string existingExerciseName))
                    exceptionMessage = $"В списке фиксаций уже существует упражнение с названием {existingExerciseName.AddBoldAndQuotes()}";
            }

            this.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull();

            if (string.IsNullOrWhiteSpace(exceptionMessage))
            {
                responseTextBuilder = new ResponseTextBuilder("Упражнение(я) зафиксировано(ы)!",
                    $"Введите след. упражнение(я) для дня {this.CurrentUserContext.DataManager.CurrentDay.Name.AddBoldAndQuotes()} либо нажмите {"Сохранить".AddQuotes()} для сохранения зафиксированных упражнений");
                buttonsSets = (ButtonsSet.SaveExercises, ButtonsSet.None);
            }
            else
            {
                responseTextBuilder = new ResponseTextBuilder("Упражнение(я) не зафиксировано(ы)!",
                    exceptionMessage, string.Empty);

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

                        additionalParameters.Add("type", CommonConsts.DomainsAndEntities.Exercise);

                        break;

                    default:
                        throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CurrentUserContext.Navigation.QueryFrom}");
                }
            }

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets, additionalParameters);

            return informationSet;
        }

        private IInformationSet AddResultForExerciseCommand()
        {
            requestTextBuilder.RemoveCompletely(80).WithoutServiceSymbol();

            DTOExercise currentExercise = this.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull();

            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;

            try
            {
                List<DTOResultExercise> resultsExercise = SharedExercisesAndResultsLogicHelper.GetResultsExerciseFromText(requestTextBuilder.Build(), currentExercise.Mode);

                this.CurrentUserContext.DataManager.AddTempResultsExercise(resultsExercise);

                responseTextBuilder = new ResponseTextBuilder("Подход(ы) зафиксирован(ы)",
                @$"Введите результат след. подхода для упражения {this.CurrentUserContext.DataManager.CurrentExercise.Name.AddBoldAndQuotes()} 
либо нажмите {"Сохранить".AddQuotes()} для сохранения указанных подходов");

                buttonsSets = (ButtonsSet.SaveResultsExercise, ButtonsSet.None);
            }
            catch (CreateResultExerciseException ex)
            {
                string inputFormatExerciseResult = currentExercise.Mode switch
                {
                    ExercisesMods.Count => CommonConsts.ResultExercise.InputFormatExerciseResultCount,
                    ExercisesMods.WeightCount => CommonConsts.ResultExercise.InputFormatExerciseResultWeightCount,
                    ExercisesMods.Timer => CommonConsts.ResultExercise.InputFormatExerciseResultTimer,
                    ExercisesMods.FreeResult => CommonConsts.ResultExercise.InputFormatExerciseResultFreeResult,
                    _ => throw new NotImplementedException($"Неожиданный тип упражнения: {currentExercise.Mode.ToString()}")
                };

                responseTextBuilder = new ResponseTextBuilder(ex.Message,
                    inputFormatExerciseResult,
                    $"Сбросьте текущие результаты, чтобы вернуться обратно или введите результат заново для упражения {this.CurrentUserContext.DataManager.CurrentExercise.Name.AddBoldAndQuotes()}");
                buttonsSets = (ButtonsSet.ResetResultsExercise, ButtonsSet.None);
            }

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> AddCommentForExerciseTimerCommand()
        {
            string comment = requestTextBuilder.RemoveCompletely(50).WithoutServiceSymbol().Build();

            DTOResultExercise resultExercise = this.CurrentUserContext.DataManager.TempResultsExercise.ThrowIfNull().Single();
            resultExercise.FreeResult += $" / {comment}";

            resultExercise.Exercise = this.CurrentUserContext.DataManager.CurrentExercise;
            this.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().ResultsExercise.Add(resultExercise);
            await this.Db.AddEntity(resultExercise);

            this.CurrentUserContext.DataManager.ResetTempResultsExercise();

            this.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder(resultExercise.FreeResult.AddBold(), "Введённые данные сохранены!", "Выберите упраженение");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout);

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> ChangeNameCommand(string domainType)
        {
            requestTextBuilder.RemoveCompletely(25).WithoutServiceSymbol();

            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;
            IInformationSet informationSet;

            string domainName = requestTextBuilder.Build();

            switch (domainType)
            {
                case CommonConsts.DomainsAndEntities.Cycle:
                    if (AlreadyExistDomainWithName(domainName, DomainType.Cycle))
                    {
                        responseTextBuilder = new ResponseTextBuilder("Ошибка при добавлении названия!", $"Цикл с названием {domainName.AddBoldAndQuotes()} уже существует",
                            "Ввведите другое название тренировочного цикла");
                        buttonsSets = (ButtonsSet.None, ButtonsSet.SettingCycles);

                        informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

                        return informationSet;
                    }

                    responseTextBuilder = new ResponseTextBuilder("Название цикла сохранено!");
                    buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.CycleList);
                    break;

                case CommonConsts.DomainsAndEntities.Day:
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

                case CommonConsts.DomainsAndEntities.Exercise:

                    if (AlreadyExistDomainWithName(domainName, DomainType.Exercise))
                    {
                        responseTextBuilder = new ResponseTextBuilder("Ошибка при сохранении!", $"В этом дне уже существует упражнение с названием {domainName.AddBoldAndQuotes()}",
                        $"Введите другое(ие) название(я) упражнение(ий) для дня {this.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name}");

                        buttonsSets = GetButtonsSetIfFailedSaveNewDomainValue(ButtonsSet.SettingExercises);

                        informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

                        return informationSet;
                    }

                    responseTextBuilder = new ResponseTextBuilder("Название сохранено!", "Выберите интересующую настройку для указанного упражнения");
                    buttonsSets = (ButtonsSet.SettingExercise, ButtonsSet.ExercisesList);
                    break;
                default:
                    throw new InvalidOperationException($"Неожиданный {nameof(domainType)} : {domainType}");
            }

            IDTODomain DTODomain = this.CurrentUserContext.DataManager.GetRequiredCurrentDomain(domainType);
            DTODomain.Name = domainName;

            await this.Db.UpdateEntity(DTODomain);

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

        private async Task<IInformationSet> FindResultByDateCommand(bool isNeedFindByCurrentDay)
        {
            string finderDate = requestTextBuilder.RemoveCompletely(10).Build();

            IInformationSet informationSet = await SharedExercisesAndResultsLogicHelper.FindResultByDateCommand(this.Db, this.CurrentUserContext, finderDate, isNeedFindByCurrentDay);

            return informationSet;
        }

        private async Task<IInformationSet> FindLogByIDCommand(bool isEventID)
        {
            this.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            LogsRepository logsRepository = this.GetRequiredRepository<LogsRepository>();

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
                log = await logsRepository.GetLogs(Id, 1).FirstOrDefaultAsync();
            else
                log = await logsRepository.GetLogById(Id, 1);

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

        private async Task<IInformationSet> SendMessageToUserCommand()
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

            AdminRepository adminRepository = this.GetRequiredRepository<AdminRepository>();

            if (long.TryParse(userIdentity, out long userID))
                user = await adminRepository.GetUserInformation(userID);
            else
                user = await adminRepository.GetUserInformation(userIdentity);

            if (user == null)
            {
                responseTextBuilder = new ResponseTextBuilder($"Пользователь {userIdentity.AddBoldAndQuotes()} не найден!", 
                    "Введите параметры для поиска другого пользователя");
                buttonsSets = (ButtonsSet.None, ButtonsSet.AdminUsers);

                informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);
                return informationSet;
            }

            await this.SimpleSendNotification(user.UserId, text);

            responseTextBuilder = new ResponseTextBuilder($"Сообщение отправлено пользователю {userIdentity.AddBoldAndQuotes()}",
                    "Выберите интересующее действие");
            buttonsSets = (ButtonsSet.AdminUsers, ButtonsSet.Admin);

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> MassiveSendMessagesToUsers(bool isNeedFromDB)
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            string text = requestTextBuilder.RemoveCompletely(300).WithoutServiceSymbol().Build();

            int counter = 0;
            foreach (var userID in isNeedFromDB 
                ? this.Db.UsersInformation.AsNoTracking().Select(x => x.UserId)
                : this.ContextKeeper.GetAllKeys())
            {
                await this.SimpleSendNotification(userID, text);
                ++counter;
            }

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder(@$"Сообщение отправлено {counter} пользователям",
                "Выберите интересующее действие");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.AdminUsers, ButtonsSet.Admin);

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> ChangeUserStateCommand()
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;

            requestTextBuilder.RemoveCompletely(35).WithoutServiceSymbol();

            string[] parameters = requestTextBuilder.Build().Split(" ", StringSplitOptions.RemoveEmptyEntries);

            bool isInvalidParameters = parameters.Length != 2 || string.IsNullOrWhiteSpace(parameters[0]) || string.IsNullOrWhiteSpace(parameters[1]);

            if (isInvalidParameters)
            {
                responseTextBuilder = new ResponseTextBuilder("Некорректные параметры для изменения состояния пользователя", "Введите параметры повторно");
                buttonsSets = (ButtonsSet.None, ButtonsSet.AdminUsers);
                
                informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);
                return informationSet;
            }

            string userIdentity = parameters[0];
            string list = parameters[1];

            UserInformation? user;

            AdminRepository adminRepository = this.GetRequiredRepository<AdminRepository>();

            if (long.TryParse(userIdentity, out long userID))
                user = await adminRepository.GetUserInformation(userID);
            else
                user = await adminRepository.GetUserInformation(userIdentity);

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

                    await adminRepository.ChangeWhiteListByUser(user);

                    responseTextBuilder = new ResponseTextBuilder($"WhiteList для {user.Username.AddBoldAndQuotes()} ({user.UserId}) установлен в: {user.WhiteList.ToString().AddBold()}",
                        "Выберите интересующее действие");

                    break;
                case "bl":

                    if (userContext != null)
                        userContext.UserInformation.BlackList = !userContext.UserInformation.BlackList;

                    await adminRepository.ChangeBlackListByUser(user);

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

        private async Task<IInformationSet> DeleteUserCommand()
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;

            string userIdentity = requestTextBuilder.RemoveCompletely(35).WithoutServiceSymbol().Build();

            if (string.IsNullOrWhiteSpace(userIdentity))
                throw new NotImplementedException("Некорректные параметры для изменения состояния пользователя");

            UserInformation? user;

            AdminRepository adminRepository = this.GetRequiredRepository<AdminRepository>();

            if (long.TryParse(userIdentity, out long userID))
                user = await adminRepository.GetUserInformation(userID);
            else
                user = await adminRepository.GetUserInformation(userIdentity);

            if (user == null)
            {
                responseTextBuilder = new ResponseTextBuilder($"Пользователь {userIdentity.AddBoldAndQuotes()} не найден!", "Введите параметры для поиска другого пользователя");
                buttonsSets = (ButtonsSet.None, ButtonsSet.AdminUsers);

                informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);
                return informationSet;
            }

            this.ContextKeeper.RemoveContext(user.UserId);

            await adminRepository.DeleteAccount(user);
            responseTextBuilder = new ResponseTextBuilder($"Пользователь {user.Username.AddBoldAndQuotes()} ({user.UserId}) был успешно удалён", "Выберите интересующее действие");

            buttonsSets = (ButtonsSet.AdminUsers, ButtonsSet.Admin);

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> DeleteResultsExerciseCommand()
        {
            string countToDeleteStr = requestTextBuilder.RemoveCompletely(35).WithoutServiceSymbol().Build();

            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;

            if (!int.TryParse(countToDeleteStr, out int countToDelete))
            {
                responseTextBuilder = new ResponseTextBuilder($"Не удалось получить кол-во записей для удаления из текста '{countToDeleteStr}'",
                    "Для удаления введите кол-во последних записей, которые требуется удалить");

                buttonsSets = (ButtonsSet.None, ButtonsSet.SettingExercise);
            }
            else
            {
                DTOExercise currentExercise = this.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull();

                string dbProvider = this.Db.GetDBProvider();

                if (dbProvider == "Microsoft.EntityFrameworkCore.Sqlite")
                {
                    string sqlQuery = $@"
DELETE FROM [ResultsExercises]
WHERE Id IN (
    SELECT Id 
    FROM [ResultsExercises]
    WHERE ExerciseId = {currentExercise.Id}
    ORDER BY DateTime DESC, ID DESC
    LIMIT {countToDelete}
)";

                    int numberOfRowsAffected = await this.Db.ExecuteSQL(sqlQuery);

                    responseTextBuilder = new ResponseTextBuilder($"Было удалено {numberOfRowsAffected} строк",
                        $"Выберите интересуюущую настройку для упражения {currentExercise.Name.AddBoldAndQuotes()}");

                    buttonsSets = (ButtonsSet.SettingExercise, ButtonsSet.ExercisesList);
                }
                else
                    throw new NotImplementedException($"Операция не поддерживается для DBProvider {dbProvider}");
            }

            this.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

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