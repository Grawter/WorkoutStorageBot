#region using
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.CoreRepositories.Repositories;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.Extenions;
using WorkoutStorageBot.Helpers.Common;
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.Model.Domain;
using WorkoutStorageBot.Model.HandlerData;
using WorkoutStorageBot.Model.Logging;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.MessageCommandHandler.Context
{
    internal class TextMessageCH : MessageCH
    {
        internal TextMessageCH(CommandHandlerData commandHandlerTools, TextMessageConverter requestConverter) : base(commandHandlerTools, requestConverter)
        { }

        internal override TextMessageCH Expectation(params HandlerAction[] handlerActions)
        {
            this.HandlerActions = handlerActions;

            return this;
        }

        internal TextMessageCH DefaultCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (requestConverter.RemoveCompletely().Convert().ToLower())
            {
                case "/start":
                    if (CommandHandlerTools.CurrentUserContext.ActiveCycle != null)
                    {
                        responseConverter = new ResponseTextConverter("Выберите интересующий раздел");
                        buttonsSets = (ButtonsSet.Main, ButtonsSet.None);
                    }
                    else
                    {
                        CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom = QueryFrom.Start;

                        responseConverter = new ResponseTextConverter("Начнём");
                        buttonsSets = (ButtonsSet.AddCycle, ButtonsSet.None);
                    }
                    break;

                default:
                    responseConverter = new ResponseTextConverter($"Неизвестная команда: {requestConverter.Convert()}", "Для получения разделов воспользуйтесь командой /Start");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                    break;
            }

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal TextMessageCH AddCycleCommand()
        {
            requestConverter.RemoveCompletely().WithoutServiceSymbol();

            if (!TryCheckingCycleName(requestConverter.Convert()))
                return this;

            bool hasActiveCycle = CommandHandlerTools.CurrentUserContext.ActiveCycle == null ? false : true;
            this.Domain = CommandHandlerTools.CurrentUserContext.DataManager.SetCycle(requestConverter.Convert(), !hasActiveCycle, CommandHandlerTools.CurrentUserContext.UserInformation.Id);

            if (!hasActiveCycle)
                CommandHandlerTools.CurrentUserContext.UdpateCycleForce((Cycle)this.Domain);

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom)
            {
                case QueryFrom.Start:
                    CommandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.AddDays;

                    responseConverter = new ResponseTextConverter($"Цикл {CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Name} сохранён!",
                        $"Введите название тренирочного дня для цикла {CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Name}");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                    break;

                case QueryFrom.Settings:
                    CommandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.Default;

                    responseConverter = new ResponseTextConverter($"Цикл {CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Name} сохранён!",
                        "Выберите дальнейшие действия");
                    buttonsSets = (ButtonsSet.AddDays, ButtonsSet.SettingCycles);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom}");
            }

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal TextMessageCH AddDaysCommand()
        {
            requestConverter.RemoveCompletely().WithoutServiceSymbol();

            if (!TryCheckingDayName(requestConverter.Convert()))
                return this;

            this.Domain = CommandHandlerTools.CurrentUserContext.DataManager.SetDay(requestConverter.Convert());

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom)
            {
                case QueryFrom.Start:
                    CommandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.AddExercises;

                    responseConverter = new ResponseTextConverter($"День {CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Name} сохранён!",
                        "Введите название упражения для этого дня");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                    break;

                case QueryFrom.Settings:
                    CommandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.Default;

                    responseConverter = new ResponseTextConverter($"День {CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Name} сохранён!");
                    buttonsSets = (ButtonsSet.AddExercises, ButtonsSet.SettingDays);

                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom}");
            }

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal TextMessageCH AddExercisesCommand()
        {
            requestConverter.RemoveCompletely().WithoutServiceSymbol();

            string[] exercisesNames = requestConverter.GetExercises();

            if (!TryCheckingExercisesNames(exercisesNames))
                return this;

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            if (!CommandHandlerTools.CurrentUserContext.DataManager.TryAddExercise(exercisesNames))
            {
                responseConverter = new ResponseTextConverter("Упражнение(я) не зафиксировано(ы)!", "В списке фиксаций уже существует(ют) упражнение(я) с таким(и) названием(ями)",
                    $"Введите другое(ие) название(я) упражнение(й) для дня {CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Name}");

                switch (CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom)
                {
                    case QueryFrom.Start:
                        buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                        break;

                    case QueryFrom.Settings:
                        buttonsSets = (ButtonsSet.None, ButtonsSet.SettingExercises);
                        break;
                    default:
                        throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom}");
                }

                this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                return this;
            }

            responseConverter = new ResponseTextConverter("Упражнение(я) зафиксировано(ы)!",
                $"Введите след. упражнение(я) для дня {CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Name} либо нажмите \"Сохранить\" для сохранения зафиксированных упражнений");
            buttonsSets = (ButtonsSet.SaveExercises, ButtonsSet.None);

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal TextMessageCH AddResultForExerciseCommand()
        {
            requestConverter.RemoveCompletely(45).WithoutServiceSymbol();

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            try
            {
                CommandHandlerTools.CurrentUserContext.DataManager.AddResultsExercise(requestConverter.GetResultsExercise());
            }
            catch (Exception)
            {
                responseConverter = new ResponseTextConverter("Неожиданный формат результата",
                    $"Введите результат заново для упражения {CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.Name}. Пример ожидаемого ввода: 50 10 или 50 10;50 10;50 10... для множественного ввода");
                buttonsSets = (ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay);

                this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                return this;
            }

            responseConverter = new ResponseTextConverter("Подход(ы) зафиксирован(ы)",
                $"Введите вес и кол-во повторений след. подхода для упражения {CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.Name} " +
                $"либо нажмите \"Сохранить\" для сохранения указанных подходов");
            buttonsSets = (ButtonsSet.SaveResultsExercise, ButtonsSet.Main);

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal TextMessageCH ChangeNameCommand(string domainType)
        {
            this.Domain = CommandHandlerTools.CurrentUserContext.GetCurrentDomainFromDataManager(domainType);

            requestConverter.RemoveCompletely(25).WithoutServiceSymbol();

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (domainType)
            {
                case "Cycle":
                    if (!TryCheckingCycleName(requestConverter.Convert()))
                        return this;

                    responseConverter = new ResponseTextConverter("Название цикла сохранено!");
                    buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.CycleList);
                    break;

                case "Day":
                    if (!TryCheckingDayName(requestConverter.Convert()))
                        return this;

                    responseConverter = new ResponseTextConverter("Название дня сохранено!");
                    buttonsSets = (ButtonsSet.SettingDay, ButtonsSet.DaysList);
                    break;

                case "Exercise":

                    if (!TryCheckingExercisesNames(requestConverter.Convert()))
                        return this;

                    responseConverter = new ResponseTextConverter("Название сохранено!", "Выберите интересующую настройку для указанного упражнения");
                    buttonsSets = (ButtonsSet.SettingExercise, ButtonsSet.ExercisesList);
                    break;
                default:
                    throw new InvalidOperationException($"Неожиданный {nameof(domainType)} : {domainType}");
            }

            this.Domain.Name = requestConverter.Convert();

            this.CommandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.Default;

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        private bool TryCheckingCycleName(string name)
        {
            if (CommandHandlerTools.CurrentUserContext.UserInformation.Cycles.Any(c => c.Name == name))
            {
                ResponseTextConverter responseConverter = new ResponseTextConverter("Ошибка при добавлении названия!", $"Цикл с названием {requestConverter.Convert()} уже существует",
                    "Ввведите другое название тренировочного цикла");
                (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.None, ButtonsSet.SettingCycles);

                ClearHandlerAction();

                this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                return false;
            }

            return true;
        }

        private bool TryCheckingDayName(string name)
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            if (CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Days.Any(d => d.Name == name))
            {
                responseConverter = new ResponseTextConverter("Ошибка при сохранении!", "В этом цикле уже существует день с таким названием",
                    $"Ввведите другое название дня для цикла {CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Name}");

                switch (CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom)
                {
                    case QueryFrom.Start:
                        buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                        break;

                    case QueryFrom.Settings:
                        buttonsSets = (ButtonsSet.None, ButtonsSet.SettingDays);
                        break;
                    default:
                        throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom}");
                }

                ClearHandlerAction();

                this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                return false;
            }

            return true;
        }

        private bool TryCheckingExercisesNames(params string[] names)
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            foreach (string name in names)
            {
                if (CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Exercises.Any(e => e.Name == name))
                {
                    responseConverter = new ResponseTextConverter("Ошибка при сохранении!", "В этом дне уже существует упражнение(я) с таким(и) названием(ями)",
                        $"Введите другое(ие) название(я) упражнение(ий) для дня {CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Name}");

                    switch (CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.Start:
                            buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                            break;

                        case QueryFrom.Settings:
                            buttonsSets = (ButtonsSet.None, ButtonsSet.SettingExercises);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom}");
                    }

                    ClearHandlerAction();

                    this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                    return false;
                }
            }
            return true;
        }

        internal TextMessageCH FindLogByIDCommand(bool isEventID)
        {
            if (AccessDenied())
                return this;

            LogsRepository logsRepository = this.CommandHandlerTools.ParentHandler.CoreManager.GetRepository<LogsRepository>()!;

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.AdminLogs, ButtonsSet.Admin);

            string identifierType = isEventID
                             ? "eventId"
                             : "Id";

            string IdStr = requestConverter.RemoveCompletely(10).WithoutServiceSymbol().Convert();

            if (!int.TryParse(IdStr, out int Id))
            {
                responseConverter = new ResponseTextConverter($"Передан некорректный {identifierType}: {IdStr}", "Выберите интересующее действие");

                this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                return this;
            }

            Log? log;

            if (isEventID)
                log = logsRepository.GetLogs(Id, 1).FirstOrDefault();
            else
                log = logsRepository.GetLogsById(Id, 1).FirstOrDefault();

            if (log == null)
                responseConverter = new ResponseTextConverter($"Не удалось найти лог с {identifierType}: {Id}", "Выберите интересующее действие");
            else
            {
                string logStr = LogFormatter.ConvertLogToStr(log);

                responseConverter = new ResponseTextConverter("Найденный лог:", logStr, "Выберите интересующее действие");
            }

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal TextMessageCH ChangeUserStateCommand()
        {
            if (AccessDenied())
                return this;

            AdminRepository adminRepository = this.CommandHandlerTools.ParentHandler.CoreManager.GetRepository<AdminRepository>()!;

            ResponseTextConverter responseConverter = default;
            (ButtonsSet, ButtonsSet) buttonsSets;

            requestConverter.RemoveCompletely(35).WithoutServiceSymbol();

            string[] parameters = requestConverter.Convert().Split(" ", StringSplitOptions.RemoveEmptyEntries);

            bool parametersIsValid = false;

            if (parameters.Length != 2 || string.IsNullOrEmpty(parameters[0]) || string.IsNullOrEmpty(parameters[1]))
                responseConverter = new ResponseTextConverter("Некорректные параметры для изменения состояния пользователя", "Выберите интересующее действие");
            else
                parametersIsValid = true;

            if (parametersIsValid)
            {
                string userIdentity = parameters[0];
                string list = parameters[1];

                UserInformation? user = adminRepository.GetUserInformation(userIdentity);

                if (user == null)
                    responseConverter = new ResponseTextConverter($"Пользователь '{userIdentity}' не найден!", "Выберите интересующее действие");
                else
                {
                    switch (list)
                    {
                        case "wl":
                            adminRepository.ChangeWhiteListByUser(user);

                            responseConverter = new ResponseTextConverter($"WhiteList для {user.Username} ({user.UserId}) установлен на {user.WhiteList}",
                                "Выберите интересующее действие");

                            break;
                        case "bl":
                            adminRepository.ChangeBlackListByUser(user);

                            responseConverter = new ResponseTextConverter($"BlackList для {user.Username} ({user.UserId}) установлен на {user.BlackList}",
                                "Выберите интересующее действие");
                            break;
                        default:
                            throw new InvalidOperationException($"Неизвестный для изменения список: {list}");
                    }
                }
            }

            buttonsSets = (ButtonsSet.Admin, ButtonsSet.Main);
            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal TextMessageCH DeleteUserCommand()
        {
            if (AccessDenied())
                return this;

            AdminRepository adminRepository = this.CommandHandlerTools.ParentHandler.CoreManager.GetRepository<AdminRepository>()!;

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            string userIdentity = requestConverter.RemoveCompletely(35).WithoutServiceSymbol().Convert();

            if (string.IsNullOrEmpty(userIdentity))
                throw new NotImplementedException("Некорректные параметры для изменения состояния пользователя");

            UserInformation? user = adminRepository.GetUserInformation(userIdentity);

            if (user == null)
                responseConverter = new ResponseTextConverter($"Пользователь '{userIdentity}' не найден!", "Выберите интересующее действие");
            else
            {
                adminRepository.DeleteAccount(user);
                responseConverter = new ResponseTextConverter($"Пользователь {user.Username} ({user.UserId}) был успешно удалён", "Выберите интересующее действие");
            }

            buttonsSets = (ButtonsSet.Admin, ButtonsSet.Main);

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        private bool AccessDenied()
        {
            if (!this.CommandHandlerTools.CurrentUserContext.IsAdmin())
            {
                this.InformationSet = GetAccessDeniedMessageInformationSet();
                return true;
            }

            return false;
        }

        private MessageInformationSet GetAccessDeniedMessageInformationSet()
        {
            ResponseTextConverter responseConverter = new ResponseTextConverter("Отказано в действии");

            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Main, ButtonsSet.None);

            return new MessageInformationSet(responseConverter.Convert(), buttonsSets);
        }
    }
}