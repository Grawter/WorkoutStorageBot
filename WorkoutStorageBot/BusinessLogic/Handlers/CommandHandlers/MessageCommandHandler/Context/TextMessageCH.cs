using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Exceptions;
using WorkoutStorageBot.BusinessLogic.Extensions;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.SharedCommandHandler;
using WorkoutStorageBot.BusinessLogic.Helpers.Converters;
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
        internal TextMessageCH(CommandHandlerData commandHandlerTools, TextMessageConverter requestConverter) : base(commandHandlerTools, requestConverter)
        { }

        internal override async Task<IInformationSet> GetInformationSet()
        {
            IInformationSet informationSet;

            switch (CommandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget)
            {
                case MessageNavigationTarget.Default:
                    informationSet = DefaultCommand();

                    break;

                case MessageNavigationTarget.AddCycle:
                    informationSet = await AddCycleCommand();

                    break;

                case MessageNavigationTarget.AddDays:
                    informationSet = await AddDaysCommand();

                    break;

                case MessageNavigationTarget.AddExercises:
                    informationSet = AddExercisesCommand();

                    break;

                case MessageNavigationTarget.AddResultForExercise:
                    informationSet = AddResultForExerciseCommand();

                    break;

                case MessageNavigationTarget.AddCommentForExerciseTimer:
                    informationSet = await AddCommentForExerciseTimerCommand();

                    break;

                case MessageNavigationTarget.ChangeNameCycle:
                    informationSet = await ChangeNameCommand(CommonConsts.DomainsAndEntities.Cycle);

                    break;

                case MessageNavigationTarget.ChangeNameDay:
                    informationSet = await ChangeNameCommand(CommonConsts.DomainsAndEntities.Day);

                    break;

                case MessageNavigationTarget.ChangeNameExercise:
                    informationSet = await ChangeNameCommand(CommonConsts.DomainsAndEntities.Exercise);

                    break;

                case MessageNavigationTarget.FindResultsByDate:
                    informationSet = await FindResultByDateCommand(false);
                    break;

                case MessageNavigationTarget.FindResultsByDateInDay:
                    informationSet = await FindResultByDateCommand(true);
                    break;

                case MessageNavigationTarget.FindLogByID:
                    informationSet = await FindLogByIDCommand(isEventID: false);

                    break;

                case MessageNavigationTarget.FindLogByEventID:
                    informationSet = await FindLogByIDCommand(isEventID: true);

                    break;

                case MessageNavigationTarget.ChangeUserState:
                    informationSet = await ChangeUserStateCommand();

                    break;

                case MessageNavigationTarget.DeleteUser:
                    informationSet = await DeleteUserCommand();

                    break;

                case MessageNavigationTarget.DeleteResultsExercise:
                    informationSet = await DeleteResultsExerciseCommand();

                    break;

                default:
                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.MessageNavigationTarget: {CommandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget}!");
            }

            CheckInformationSet(informationSet);

            return informationSet;
        }

        private IInformationSet DefaultCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (requestConverter.RemoveCompletely().Convert().ToLower())
            {
                case "/start":
                    if (this.CommandHandlerTools.CurrentUserContext.ActiveCycle != null)
                    {
                        responseConverter = new ResponseTextConverter("Выберите интересующий раздел");
                        buttonsSets = (ButtonsSet.Main, ButtonsSet.None);
                    }
                    else
                    {
                        this.CommandHandlerTools.CurrentUserContext.Navigation.SetQueryFrom(QueryFrom.Start);

                        responseConverter = new ResponseTextConverter("Начнём");
                        buttonsSets = (ButtonsSet.AddCycle, ButtonsSet.None);
                    }
                    break;

                default:
                    responseConverter = new ResponseTextConverter($"Неизвестная команда: {requestConverter.Convert().AddBoldAndQuotes()}", 
                        $"Для получения разделов воспользуйтесь командой {"/Start".AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                    break;
            }

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> AddCycleCommand()
        {
            requestConverter.RemoveCompletely().WithoutServiceSymbol();

            string domainName = requestConverter.Convert();

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;
            IInformationSet informationSet;

            if (AlreadyExistDomainWithName(domainName, DomainType.Cycle))
            {
                responseConverter = new ResponseTextConverter("Ошибка при добавлении названия!", $"Цикл с названием {domainName.AddBoldAndQuotes()} уже существует",
                    "Ввведите другое название тренировочного цикла");
                buttonsSets = (ButtonsSet.None, ButtonsSet.SettingCycles);

                informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                return informationSet;
            }

            bool hasActiveCycle = this.CommandHandlerTools.CurrentUserContext.ActiveCycle == null ? false : true;
            DTOCycle currentCycle = this.CommandHandlerTools.CurrentUserContext.DataManager.SetCurrentCycle(requestConverter.Convert(), !hasActiveCycle, this.CommandHandlerTools.CurrentUserContext.UserInformation);

            if (!hasActiveCycle)
                this.CommandHandlerTools.CurrentUserContext.UdpateActiveCycleForce(currentCycle);

            this.CommandHandlerTools.CurrentUserContext.UserInformation.Cycles.Add(currentCycle);
            await this.CommandHandlerTools.Db.AddEntity(currentCycle);

            this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull();

            switch (this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom)
            {
                case QueryFrom.Start:
                    this.CommandHandlerTools.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.AddDays);

                    responseConverter = new ResponseTextConverter($"Цикл {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()} сохранён!",
                        $"Введите название тренирочного дня для цикла {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                    break;

                case QueryFrom.Settings:
                    this.CommandHandlerTools.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

                    responseConverter = new ResponseTextConverter($"Цикл {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()} сохранён!",
                        "Выберите дальнейшие действия");
                    buttonsSets = (ButtonsSet.AddDays, ButtonsSet.SettingCycles);

                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom}");
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> AddDaysCommand()
        {
            requestConverter.RemoveCompletely().WithoutServiceSymbol();

            string domainName = requestConverter.Convert();

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;
            IInformationSet informationSet;

            this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull();

            if (AlreadyExistDomainWithName(domainName, DomainType.Day))
            {
                responseConverter = new ResponseTextConverter("Ошибка при сохранении!", $"В этом цикле уже существует день с названием {domainName.AddBoldAndQuotes()}",
            $"Ввведите другое название дня для цикла {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}");

                buttonsSets = GetButtonsSetIfFailedSaveNewDomainValue(ButtonsSet.SettingDays);

                informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                return informationSet;
            }

            DTODay currentDay = this.CommandHandlerTools.CurrentUserContext.DataManager.SetCurrentDay(domainName);

            this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Days.Add(currentDay);

            await this.CommandHandlerTools.Db.AddEntity(currentDay);

            switch (this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom)
            {
                case QueryFrom.Start:
                    this.CommandHandlerTools.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.AddExercises);

                    responseConverter = new ResponseTextConverter($"День {currentDay.Name.AddBoldAndQuotes()} сохранён!",
                        $"Введите название упражения для этого дня.{Environment.NewLine}{CommonConsts.Exercise.ExamplesTypesExercise}", 
                        CommonConsts.Exercise.InputFormatExercise);
                    buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                    break;

                case QueryFrom.Settings:
                    this.CommandHandlerTools.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

                    responseConverter = new ResponseTextConverter($"День {currentDay.Name.AddBoldAndQuotes()} сохранён!");
                    buttonsSets = (ButtonsSet.AddExercises, ButtonsSet.SettingDays);

                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom}");
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet AddExercisesCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            Dictionary<string, string>? additionalParameters = new Dictionary<string, string>();

            requestConverter.RemoveCompletely(80).WithoutServiceSymbol();

            List<DTOExercise> exercises = new List<DTOExercise>();

            string exceptionMessage = string.Empty;

            try
            {
                exercises = requestConverter.GetExercises();
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
                if (!this.CommandHandlerTools.CurrentUserContext.DataManager.TryAddTempExercises(exercises, out string existingExerciseName))
                    exceptionMessage = $"В списке фиксаций уже существует упражнение с названием {existingExerciseName.AddBoldAndQuotes()}";
            }

            this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull();

            if (string.IsNullOrWhiteSpace(exceptionMessage))
            {
                responseConverter = new ResponseTextConverter("Упражнение(я) зафиксировано(ы)!",
                    $"Введите след. упражнение(я) для дня {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Name.AddBoldAndQuotes()} либо нажмите {"Сохранить".AddQuotes()} для сохранения зафиксированных упражнений");
                buttonsSets = (ButtonsSet.SaveExercises, ButtonsSet.None);
            }
            else
            {
                responseConverter = new ResponseTextConverter("Упражнение(я) не зафиксировано(ы)!",
                    exceptionMessage, string.Empty);

                switch (this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom)
                {
                    case QueryFrom.Start:

                        responseConverter.ResetTarget(@$"Введите другое(ие) упражнение(й) для дня {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Name.AddBoldAndQuotes()}

{CommonConsts.Exercise.InputFormatExercise}");
                        buttonsSets = (ButtonsSet.None, ButtonsSet.None);

                        break;

                    case QueryFrom.Settings:

                        responseConverter.ResetTarget(@$"Сбросьте текущие упражнения, чтобы вернуться обратно или введите другое(ие) упражнение(й) для дня {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Name.AddBoldAndQuotes()}

{CommonConsts.Exercise.InputFormatExercise}");
                        buttonsSets = (ButtonsSet.ResetTempDomains, ButtonsSet.None);

                        additionalParameters.Add("type", CommonConsts.DomainsAndEntities.Exercise);

                        break;

                    default:
                        throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom}");
                }
            }

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets, additionalParameters);

            return informationSet;
        }

        private IInformationSet AddResultForExerciseCommand()
        {
            requestConverter.RemoveCompletely(80).WithoutServiceSymbol();

            DTOExercise currentExercise = this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull();

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            try
            {
                List<DTOResultExercise> resultsExercise = requestConverter.GetResultsExercise(currentExercise.Mode);

                this.CommandHandlerTools.CurrentUserContext.DataManager.AddTempResultsExercise(resultsExercise);

                responseConverter = new ResponseTextConverter("Подход(ы) зафиксирован(ы)",
                @$"Введите результат след. подхода для упражения {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.Name.AddBoldAndQuotes()} 
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

                responseConverter = new ResponseTextConverter(ex.Message,
                    inputFormatExerciseResult,
                    $"Сбросьте текущие результаты, чтобы вернуться обратно или введите результат заново для упражения {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.Name.AddBoldAndQuotes()}");
                buttonsSets = (ButtonsSet.ResetResultsExercise, ButtonsSet.None);
            }

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> AddCommentForExerciseTimerCommand()
        {
            string comment = requestConverter.RemoveCompletely(50).WithoutServiceSymbol().Convert();

            DTOResultExercise resultExercise = this.CommandHandlerTools.CurrentUserContext.DataManager.TempResultsExercise.ThrowIfNull().Single();
            resultExercise.FreeResult += $" / {comment}";

            resultExercise.Exercise = this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise;
            this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().ResultsExercise.Add(resultExercise);
            await this.CommandHandlerTools.Db.AddEntity(resultExercise);

            this.CommandHandlerTools.CurrentUserContext.DataManager.ResetTempResultsExercise();

            this.CommandHandlerTools.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

            ResponseTextConverter responseConverter = new ResponseTextConverter(resultExercise.FreeResult.AddBold(), "Введённые данные сохранены!", "Выберите упраженение");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout);

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> ChangeNameCommand(string domainType)
        {
            requestConverter.RemoveCompletely(25).WithoutServiceSymbol();

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;
            IInformationSet informationSet;

            string domainName = requestConverter.Convert();

            switch (domainType)
            {
                case CommonConsts.DomainsAndEntities.Cycle:
                    if (AlreadyExistDomainWithName(domainName, DomainType.Cycle))
                    {
                        responseConverter = new ResponseTextConverter("Ошибка при добавлении названия!", $"Цикл с названием {domainName.AddBoldAndQuotes()} уже существует",
                            "Ввведите другое название тренировочного цикла");
                        buttonsSets = (ButtonsSet.None, ButtonsSet.SettingCycles);

                        informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                        return informationSet;
                    }

                    responseConverter = new ResponseTextConverter("Название цикла сохранено!");
                    buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.CycleList);
                    break;

                case CommonConsts.DomainsAndEntities.Day:
                    if (AlreadyExistDomainWithName(domainName, DomainType.Day))
                    {
                        responseConverter = new ResponseTextConverter("Ошибка при сохранении!", $"В этом цикле уже существует день с названием {domainName.AddBoldAndQuotes()}",
                    $"Ввведите другое название дня для цикла {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}");

                        buttonsSets = GetButtonsSetIfFailedSaveNewDomainValue(ButtonsSet.SettingDays);

                        informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                        return informationSet;
                    }

                    responseConverter = new ResponseTextConverter("Название дня сохранено!", "Выберите интересующую настройку для указанного дня");
                    buttonsSets = (ButtonsSet.SettingDay, ButtonsSet.DaysList);
                    break;

                case CommonConsts.DomainsAndEntities.Exercise:

                    if (AlreadyExistDomainWithName(domainName, DomainType.Exercise))
                    {
                        responseConverter = new ResponseTextConverter("Ошибка при сохранении!", $"В этом дне уже существует упражнение с названием {domainName.AddBoldAndQuotes()}",
                        $"Введите другое(ие) название(я) упражнение(ий) для дня {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name}");

                        buttonsSets = GetButtonsSetIfFailedSaveNewDomainValue(ButtonsSet.SettingExercises);

                        informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                        return informationSet;
                    }

                    responseConverter = new ResponseTextConverter("Название сохранено!", "Выберите интересующую настройку для указанного упражнения");
                    buttonsSets = (ButtonsSet.SettingExercise, ButtonsSet.ExercisesList);
                    break;
                default:
                    throw new InvalidOperationException($"Неожиданный {nameof(domainType)} : {domainType}");
            }

            IDTODomain DTODomain = this.CommandHandlerTools.CurrentUserContext.DataManager.GetRequiredCurrentDomain(domainType);
            DTODomain.Name = domainName;

            await this.CommandHandlerTools.Db.UpdateEntity(DTODomain);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            this.CommandHandlerTools.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

            return informationSet;
        }

        private bool AlreadyExistDomainWithName(string name, DomainType domainType)
        {
            switch (domainType)
            {
                case DomainType.Cycle:
                    return this.CommandHandlerTools.CurrentUserContext.UserInformation.Cycles.Any(c => c.Name == name);
                
                case DomainType.Day:
                    return this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Days.Any(d => d.Name == name);

                case DomainType.Exercise:
                    return this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Exercises.Any(e => e.Name == name);

                default:
                    throw new NotSupportedException($"Неподдерживаемый тип домена: {domainType.ToString()}");
            }
        }

        private (ButtonsSet, ButtonsSet) GetButtonsSetIfFailedSaveNewDomainValue(ButtonsSet backButtonForSetting)
        {
            switch (this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom)
            {
                case QueryFrom.Start:
                    return (ButtonsSet.None, ButtonsSet.None);
        
                case QueryFrom.Settings:
                    return (ButtonsSet.None, backButtonForSetting);

                default:
                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom}");
            }
        }

        private async Task<IInformationSet> FindResultByDateCommand(bool isNeedFindByCurrentDay)
        {
            SharedCH sharedCH = new SharedCH(this.CommandHandlerTools);

            string findedDate = requestConverter.RemoveCompletely(10).Convert();

            IInformationSet informationSet = await sharedCH.FindResultByDateCommand(findedDate, isNeedFindByCurrentDay);

            return informationSet;
        }

        private async Task<IInformationSet> FindLogByIDCommand(bool isEventID)
        {
            this.CommandHandlerTools.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            LogsRepository logsRepository = this.CommandHandlerTools.ParentHandler.CoreManager.GetRequiredRepository<LogsRepository>();

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.AdminLogs, ButtonsSet.Admin);

            string identifierType = isEventID
                             ? "eventId"
                             : "Id";

            string IdStr = requestConverter.RemoveCompletely(10).WithoutServiceSymbol().Convert();

            if (!int.TryParse(IdStr, out int Id))
            {
                responseConverter = new ResponseTextConverter($"Передан некорректный {identifierType}: {IdStr.AddBoldAndQuotes()}", "Выберите интересующее действие");

                informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                return informationSet;
            }

            Log? log;

            if (isEventID)
                log = await logsRepository.GetLogs(Id, 1).FirstOrDefaultAsync();
            else
                log = await logsRepository.GetLogById(Id, 1);

            if (log == null)
                responseConverter = new ResponseTextConverter($"Не удалось найти лог с {identifierType}: {Id.ToString().AddBoldAndQuotes()}", "Выберите интересующее действие");
            else
            {
                string logStr = LogFormatter.ConvertLogToStr(log);

                responseConverter = new ResponseTextConverter("Найденный лог:", logStr, "Выберите интересующее действие");
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> ChangeUserStateCommand()
        {
            this.CommandHandlerTools.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            AdminRepository adminRepository = this.CommandHandlerTools.ParentHandler.CoreManager.GetRequiredRepository<AdminRepository>();

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            requestConverter.RemoveCompletely(35).WithoutServiceSymbol();

            string[] parameters = requestConverter.Convert().Split(" ", StringSplitOptions.RemoveEmptyEntries);

            bool isInvalidParameters = parameters.Length != 2 || string.IsNullOrWhiteSpace(parameters[0]) || string.IsNullOrWhiteSpace(parameters[1]);

            if (isInvalidParameters)
                responseConverter = new ResponseTextConverter("Некорректные параметры для изменения состояния пользователя", "Выберите интересующее действие");
            else
            {
                string userIdentity = parameters[0];
                string list = parameters[1];

                UserInformation? user;

                if (long.TryParse(userIdentity, out long userID))
                    user = await adminRepository.GetUserInformation(userID);
                else
                    user = await adminRepository.GetUserInformation(userIdentity);

                if (user == null)
                    responseConverter = new ResponseTextConverter($"Пользователь '{userIdentity.AddBoldAndQuotes()}' не найден!", "Выберите интересующее действие");
                else
                {
                    UserContext? userContext = this.CommandHandlerTools.ParentHandler.CoreManager.ContextKeeper.GetContext(user.UserId);

                    switch (list)
                    {
                        case "wl":

                            if (userContext != null)
                                userContext.UserInformation.WhiteList = !userContext.UserInformation.WhiteList;

                            await adminRepository.ChangeWhiteListByUser(user);

                            responseConverter = new ResponseTextConverter($"WhiteList для {user.Username.AddBoldAndQuotes()} ({user.UserId}) установлен в: {user.WhiteList.ToString().AddBold()}",
                                "Выберите интересующее действие");

                            break;
                        case "bl":

                            if (userContext != null)
                                userContext.UserInformation.BlackList = !userContext.UserInformation.BlackList;

                            await adminRepository.ChangeBlackListByUser(user);

                            responseConverter = new ResponseTextConverter($"BlackList для {user.Username.AddBoldAndQuotes()} ({user.UserId}) установлен в: {user.BlackList.ToString().AddBold()}",
                                "Выберите интересующее действие");
                            break;
                        default:
                            throw new InvalidOperationException($"Неизвестный для изменения список: {list}");
                    }
                }
            }

            buttonsSets = (ButtonsSet.Admin, ButtonsSet.Main);
            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> DeleteUserCommand()
        {
            this.CommandHandlerTools.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            AdminRepository adminRepository = this.CommandHandlerTools.ParentHandler.CoreManager.GetRequiredRepository<AdminRepository>();

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            string userIdentity = requestConverter.RemoveCompletely(35).WithoutServiceSymbol().Convert();

            if (string.IsNullOrWhiteSpace(userIdentity))
                throw new NotImplementedException("Некорректные параметры для изменения состояния пользователя");

            UserInformation? user;

            if (long.TryParse(userIdentity, out long userID))
                user = await adminRepository.GetUserInformation(userID);
            else
                user = await adminRepository.GetUserInformation(userIdentity);

            if (user == null)
                responseConverter = new ResponseTextConverter($"Пользователь '{userIdentity.AddBoldAndQuotes()}' не найден!", "Выберите интересующее действие");
            else
            {
                this.CommandHandlerTools.ParentHandler.CoreManager.ContextKeeper.RemoveContext(user.UserId);

                await adminRepository.DeleteAccount(user);
                responseConverter = new ResponseTextConverter($"Пользователь {user.Username.AddBoldAndQuotes()} ({user.UserId}) был успешно удалён", "Выберите интересующее действие");
            }

            buttonsSets = (ButtonsSet.Admin, ButtonsSet.Main);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> DeleteResultsExerciseCommand()
        {
            string countToDeleteStr = requestConverter.RemoveCompletely(35).WithoutServiceSymbol().Convert();

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            if (!int.TryParse(countToDeleteStr, out int countToDelete))
            {
                responseConverter = new ResponseTextConverter($"Не удалось получить кол-во записей для удаления из текста '{countToDeleteStr}'",
                    "Для удаления введите кол-во последних записей, которые требуется удалить");

                buttonsSets = (ButtonsSet.None, ButtonsSet.SettingExercise);
            }
            else
            {
                DTOExercise currentExercise = this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull();

                string dbProvider = this.CommandHandlerTools.Db.GetDBProvider();

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

                    int numberOfRowsAffected = await this.CommandHandlerTools.Db.ExecuteSQL(sqlQuery);

                    responseConverter = new ResponseTextConverter($"Было удалено {numberOfRowsAffected} строк",
                        $"Выберите интересуюущую настройку для упражения {currentExercise.Name.AddBoldAndQuotes()}");

                    buttonsSets = (ButtonsSet.SettingExercise, ButtonsSet.ExercisesList);
                }
                else
                    throw new NotImplementedException($"Операция не поддерживается для DBProvider {dbProvider}");
            }

            this.CommandHandlerTools.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private bool AccessDenied([NotNullWhen(true)] out IInformationSet? informationSet)
        {
            informationSet = null;

            if (!this.CommandHandlerTools.CurrentUserContext.IsAdmin())
            {
                SharedCH sharedCH = new SharedCH(this.CommandHandlerTools);

                informationSet = sharedCH.GetAccessDeniedMessageInformationSet();

                this.CommandHandlerTools.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

                return true;
            }

            return false;
        }
    }
}