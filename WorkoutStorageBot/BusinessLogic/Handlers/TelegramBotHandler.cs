#region using
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.Helpers.Logger;
using WorkoutStorageBot.Helpers.CallbackQueryParser;
using WorkoutStorageBot.BusinessLogic.StepStore;
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.BusinessLogic.Buttons;
using WorkoutStorageBot.BusinessLogic.SQLiteQueries;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Model;
using WorkoutStorageBot.Helpers.Converters;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers
{
    internal class TelegramBotHandler
    {
        private readonly ITelegramBotClient botClient;
        private readonly EntityContext db;
        private readonly PrimaryUpdateHandler primaryProcessUpdate;
        private UserContext? currentUserContext;
        private UserContext? CurrentUserContext { get => currentUserContext; set => currentUserContext = value; }
        internal bool WhiteList { get => primaryProcessUpdate.WhiteList;  set => primaryProcessUpdate.WhiteList = value;  }

        internal TelegramBotHandler(ITelegramBotClient botClient, DbContextOptions<EntityContext> options, ILogger logger)
        {
            this.botClient = botClient;
            db = new(options);

            primaryProcessUpdate = new PrimaryUpdateHandler(logger, botClient, db);
        }

        internal async Task ProcessUpdate(Update update)
        {
            primaryProcessUpdate.Execute(update, out currentUserContext);
            if (!primaryProcessUpdate.HasAccess || primaryProcessUpdate.IsNewContext)
                return;

            switch (update.Type)
            {
                case UpdateType.Message:
                    ProcessMessage(update);
                    break;
                case UpdateType.CallbackQuery:
                    ProcessCallbackQuery(update);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный update.type: {update.Type}");
            }
        }

        private void ProcessMessage(Update update)
        {
            var requestConverter = new UserMessageConverter(update.Message.Text);
            
            ResponseConverter? responseConverter;

            var buttons = new InlineButtons(CurrentUserContext);

            switch (CurrentUserContext.Navigation.MessageNavigationTarget)
            {
                #region Post Start area

                case MessageNavigationTarget.None:
                    var text = requestConverter.RemoveCompletely().Convert();

                    switch (text.ToLower())
                    {
                        case "/start":
                            if (CurrentUserContext.ActiveCycle != null)
                            {
                                botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                                "Выберите интересующий раздел",
                                                                replyMarkup: buttons.GetInlineButtons(ButtonsSet.Main));
                            }
                            else
                            {
                                CurrentUserContext.Navigation.QueryFrom = QueryFrom.Start;
                                botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                                "Начнём",
                                                                replyMarkup: buttons.GetInlineButtons(ButtonsSet.AddCycle));
                            }
                            break;

                        default:
                            botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Неизвестная команда: {text} \nДля получения разделов воспользуйтесь командой /Start");
                            break;
                    }
                    break;

                #endregion

                #region Workout area

                case MessageNavigationTarget.AddResultForExercise:
                    requestConverter.RemoveCompletely(20).WithoutServiceSymbol();

                    try
                    {
                        CurrentUserContext.DataManager.AddResultExercise(requestConverter.GetResultExercise());
                    }
                    catch (FormatException)
                    {
                        responseConverter = new ResponseConverter("Неожиданный формат результата",
                            "Введите результат заново. Пример ожидаемого ввода: 50 10");

                        botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                        responseConverter.Convert(),
                                                        replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.None, backButtonsSet: ButtonsSet.ExercisesList));
                        break;
                    }

                    responseConverter = new ResponseConverter("Подход зафиксирован",
                        "Введите вес и кол-во повторений след. подхода либо нажмите \"Сохранить\" для сохранения указанных подходов");

                    botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                    responseConverter.Convert(),
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.SaveResultsExercise, backButtonsSet: ButtonsSet.Main));
                    break;

                #endregion

                #region Settings area

                #region Full adding cycle area

                case MessageNavigationTarget.AddCycle:
                    requestConverter.RemoveCompletely().WithoutServiceSymbol();

                    var hasNameCycleExists = CurrentUserContext.UserInformation.Cycles.Any(c => c.Name == requestConverter.Convert());
                    if (hasNameCycleExists)
                    {
                        responseConverter = new ResponseConverter("Цикл не сохранён!", $"Цикл с названием {requestConverter.Convert()} уже существует",
                            "Ввведите другое название тренировочного цикла");

                        botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                        responseConverter.Convert(),
                                                        replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.None, backButtonsSet: ButtonsSet.SettingCycles));
                    }
                    else
                    {
                        var hasActiveCycle = CurrentUserContext.ActiveCycle == null ? false : true;
                        CurrentUserContext.DataManager.SetCycle(requestConverter.Convert(), !hasActiveCycle, CurrentUserContext.UserInformation.Id);

                        db.Cycles.Add(CurrentUserContext.DataManager.CurrentCycle);
                        db.SaveChanges();

                        if (!hasActiveCycle)
                            CurrentUserContext.UdpateCycleForce(CurrentUserContext.DataManager.CurrentCycle);

                        switch (CurrentUserContext.Navigation.QueryFrom)
                        {
                            case QueryFrom.Start:
                                CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.AddDays;

                                responseConverter = new ResponseConverter($"Цикл {CurrentUserContext.DataManager.CurrentCycle.Name} сохранён!",
                                    $"Введите название тренирочного дня для цикла {CurrentUserContext.DataManager.CurrentCycle.Name}");

                                botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                                responseConverter.Convert());
                                break;

                            case QueryFrom.Settings:
                                CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.None;

                                botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                                $"Цикл {CurrentUserContext.DataManager.CurrentCycle.Name} сохранён!",
                                                                replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.AddDays, backButtonsSet: ButtonsSet.SettingCycles));
                                break;
                            default:
                                throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {CurrentUserContext.Navigation.QueryFrom}");
                        }
                    }
                    break;

                case MessageNavigationTarget.AddDays:
                    requestConverter.RemoveCompletely().WithoutServiceSymbol();

                    var hasNameDayExists = CurrentUserContext.DataManager.CurrentCycle.Days.Any(d => d.Name == requestConverter.Convert());
                    if (hasNameDayExists)
                    {
                        responseConverter = new ResponseConverter("День не сохранён!", $"День с названием {requestConverter.Convert()} уже существует в этом цикле",
                            "Ввведите другое название тренировочного дня");

                        switch (CurrentUserContext.Navigation.QueryFrom)
                        {
                            case QueryFrom.Start:

                                botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                                responseConverter.Convert());
                                break;

                            case QueryFrom.Settings:

                                botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                                responseConverter.Convert(),
                                                                replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.None, backButtonsSet: ButtonsSet.SettingDays));
                                break;
                            default:
                                throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {CurrentUserContext.Navigation.QueryFrom}");

                        }
                        break;
                    }

                    CurrentUserContext.DataManager.SetDay(requestConverter.Convert());

                    db.Days.Add(CurrentUserContext.DataManager.CurrentDay);
                    db.SaveChanges();

                    switch (CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.Start:
                            CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.AddExercises;

                            responseConverter = new ResponseConverter($"День {CurrentUserContext.DataManager.CurrentDay.Name} сохранён!",
                                "Введите название упражения для этого дня");
                            botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                            responseConverter.Convert());
                            break;

                        case QueryFrom.Settings:
                            CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.None;

                            botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                            $"День {CurrentUserContext.DataManager.CurrentDay.Name} сохранён!",
                                                            replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.AddExercises, backButtonsSet: ButtonsSet.SettingDays));
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {CurrentUserContext.Navigation.QueryFrom}");
                    }

                    break;

                case MessageNavigationTarget.AddExercises:
                    requestConverter.RemoveCompletely().WithoutServiceSymbol();

                    var hasNameExerciseExists = CurrentUserContext.DataManager.CurrentDay.Exercises.Any(e => e.Name == requestConverter.Convert());
                    if (hasNameExerciseExists)
                    {
                        responseConverter = new ResponseConverter("Упражнение не зафиксировано!", "В этом дне уже существует упражнение с таким названием",
                            $"Введите другое название упражнение для дня {CurrentUserContext.DataManager.CurrentDay.Name}");

                        switch (CurrentUserContext.Navigation.QueryFrom)
                        {
                            case QueryFrom.Start:

                                botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                                responseConverter.Convert());
                                break;

                            case QueryFrom.Settings:

                                botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                                responseConverter.Convert(),
                                                                replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.None, backButtonsSet: ButtonsSet.SettingExercises));
                                break;
                            default:
                                throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {CurrentUserContext.Navigation.QueryFrom}");
                        }
                        break;
                    }

                    if (!CurrentUserContext.DataManager.TryAddExercise(requestConverter.Convert()))
                    {
                        responseConverter = new ResponseConverter("Упражнение не зафиксировано!", "В списке фиксаций уже существует упражнение с таким названием",
                            $"Введите другое название упражнение для дня {CurrentUserContext.DataManager.CurrentDay.Name}");

                        switch (CurrentUserContext.Navigation.QueryFrom)
                        {
                            case QueryFrom.Start:

                                botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                                responseConverter.Convert());
                                break;

                            case QueryFrom.Settings:

                                botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                                responseConverter.Convert(),
                                                                replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.None, backButtonsSet: ButtonsSet.SettingExercises));
                                break;
                            default:
                                throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {CurrentUserContext.Navigation.QueryFrom}");
                        }
                        break;
                    }

                    responseConverter = new ResponseConverter("Упражнение зафиксировано!",
                        $"Введите след. упражнение для дня {CurrentUserContext.DataManager.CurrentDay.Name} либо нажмите \"Сохранить\" для сохранения зафиксированных упражнений");

                    botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                    responseConverter.Convert(),
                                                    replyMarkup: buttons.GetInlineButtons(buttonsSet: ButtonsSet.SaveExercises));
                    break;

                #endregion

                case MessageNavigationTarget.ChangeNameCycle:
                    requestConverter.RemoveCompletely(25).WithoutServiceSymbol();

                    CurrentUserContext.DataManager.CurrentCycle.Name = requestConverter.Convert();

                    db.Cycles.Update(CurrentUserContext.DataManager.CurrentCycle);
                    db.SaveChanges();

                    CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.None;

                    botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                    "Название цикла сохранено!",
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.SettingCycle, backButtonsSet: ButtonsSet.CycleList));
                    break;

                case MessageNavigationTarget.ChangeNameDay:
                    requestConverter.RemoveCompletely(25).WithoutServiceSymbol();

                    CurrentUserContext.DataManager.CurrentDay.Name = requestConverter.Convert();

                    db.Days.Update(CurrentUserContext.DataManager.CurrentDay);
                    db.SaveChanges();

                    CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.None;

                    botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                    "Название дня сохранено!",
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.SettingDay, backButtonsSet: ButtonsSet.DaysList));
                    break;

                case MessageNavigationTarget.ChangeNameExercise:
                    requestConverter.RemoveCompletely(25).WithoutServiceSymbol();

                    CurrentUserContext.DataManager.CurrentExercise.Name = requestConverter.Convert();

                    db.Exercises.Update(CurrentUserContext.DataManager.CurrentExercise);
                    db.SaveChanges();

                    CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.None;

                    botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                    "Название упражнения сохранено!",
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.SettingExercise, backButtonsSet: ButtonsSet.ExercisesList));
                    break;

                #endregion

                default:
                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.MessageNavigationTarget: {CurrentUserContext.Navigation.MessageNavigationTarget}!");
            }
        }

        private void ProcessCallbackQuery(Update update)
        {
            var callbackQueryParser = new CallbackQueryParser();

            if (!callbackQueryParser.TryParse(update.CallbackQuery.Data))
                return;

            if (!CheckingComplianceCallBackId(callbackQueryParser.CallBackId))
                return;

            var buttons = new InlineButtons(CurrentUserContext);

            switch((CallBackNavigationTarget)callbackQueryParser.Direction)
            {
                case CallBackNavigationTarget.None:
                    ProcessCommonCallBack(update, callbackQueryParser, buttons);
                    break;
                case CallBackNavigationTarget.Workout:
                    ProcessWorkoutCallBack(update, callbackQueryParser, buttons);
                    break;
                case CallBackNavigationTarget.Analytics:
                    ProcessAnalyticsCallBack(update, callbackQueryParser, buttons);
                    break;
                case CallBackNavigationTarget.Settings:
                    ProcessSettingsCallBack(update, callbackQueryParser, buttons);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CallBackNavigationTarget: {(CallBackNavigationTarget)callbackQueryParser.Direction}");
            }
        }

        private bool CheckingComplianceCallBackId(string currentCallBackId)
        {
            if (CurrentUserContext.CallBackId != currentCallBackId)
            {
                var responseConverter = new ResponseConverter("Действие не может быть выполнено, т.к. информация устарела",
                        "Для продолжения работы используйте последний диалог или введите команду /Start");

                botClient.SendTextMessageAsync(CurrentUserContext.UserInformation.UserId, responseConverter.Convert());

                return false;
            }

            return true;
        }

        private void ProcessCommonCallBack(Update update, CallbackQueryParser callbackQueryParser, InlineButtons buttons)
        {
            switch (callbackQueryParser.SubDirection)
            {
                #region Back area

                case "Back":
                    var previousStep = StepStorage.GetStep(callbackQueryParser.Args[2]);

                    CurrentUserContext.Navigation.QueryFrom = previousStep.QueryFrom;
                    CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.None;

                    var message = previousStep.ButtonsSet switch // optional additional information
                    {
                        ButtonsSet.SettingCycle
                            => previousStep.Message + " " + CurrentUserContext.DataManager.CurrentCycle.Name,
                        ButtonsSet.SettingDay
                            => previousStep.Message + " " +CurrentUserContext.DataManager.CurrentDay.Name,
                        ButtonsSet.SettingExercise
                            => previousStep.Message + " " + CurrentUserContext.DataManager.CurrentExercise.Name,
                        _ => previousStep.Message
                    };

                    SendResponse(update.CallbackQuery.Message.Chat.Id, message, (previousStep.ButtonsSet, previousStep.BackButtonsSet));
                    break;

                case "ToMain":
                    CurrentUserContext.DataManager.ResetAll();

                    var mainStep = StepStorage.GetMainStep();

                    CurrentUserContext.Navigation.QueryFrom = mainStep.QueryFrom;
                    CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.None;

                    SendResponse(update.CallbackQuery.Message.Chat.Id, mainStep.Message, (mainStep.ButtonsSet, mainStep.BackButtonsSet));
                    break;

                default:
                    throw new NotImplementedException($"Неожиданный CallbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");

                    #endregion
            }
        }

        private void ProcessWorkoutCallBack(Update update, CallbackQueryParser callbackQueryParser, InlineButtons buttons)
        {
            ResponseConverter responseConverter;
            (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) ButtonsSets;

            switch (callbackQueryParser.SubDirection)
            {
                #region Workout area

                case "Workout":
                    CurrentUserContext.Navigation.QueryFrom = QueryFrom.NoMatter; // not necessary, but just in case

                    responseConverter = new ResponseConverter("Выберите тренировочный день");
                    ButtonsSets = (ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main);
                    break;

                case "LastResultFor":
                    IEnumerable<ResultExercise> trainingIndicators;
                    string information;

                    switch (callbackQueryParser.ObjectType) 
                    {
                        case "Exercises":
                            var exercises = QueriesStorage.GetExercisesWithDaysIds(CurrentUserContext.ActiveCycle.Days.Where(d => !d.IsArchive).Select(d => d.Id), db.GetExercisesFromQuery);
                            trainingIndicators = QueriesStorage.GetLastResultsExercisesWithExercisesIds(exercises.Select(e => e.Id), db.GetResultExercisesFromQuery);

                            information = ResponseConverter.GetInformationAboutLastExercises(exercises, trainingIndicators);
                            responseConverter = new ResponseConverter("Последняя тренировка:", information, "Выберите тренировочный день");
                            ButtonsSets = (ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main);
                            break;
                        case "Day":
                            trainingIndicators = QueriesStorage.GetLastResultsForExercisesWithExercisesIds(CurrentUserContext.DataManager.CurrentDay.Exercises.Where(e => !e.IsArchive).Select(d => d.Id), db.GetResultExercisesFromQuery);

                            information = ResponseConverter.GetInformationAboutLastDay(CurrentUserContext.DataManager.CurrentDay.Exercises, trainingIndicators);
                            responseConverter = new ResponseConverter("Последняя результаты упражений из этого дня:", information, "Выберите упражнение");
                            ButtonsSets = (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CallbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
                    }
                    break;

                case "SaveResultsExercise":
                    db.ResultsExercises.AddRange(CurrentUserContext.DataManager.ResultExercises);
                    db.SaveChanges();

                    CurrentUserContext.DataManager.ResetResultExercises();

                    CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.None;

                    responseConverter = new ResponseConverter("Введённые данные сохранены!", "Выберите упраженение");
                    ButtonsSets = (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout);
                    break;

                default:
                    throw new NotImplementedException($"Неожиданный CallbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");

                    #endregion
            }

            SendResponse(update.CallbackQuery.Message.Chat.Id, responseConverter.Convert(), ButtonsSets);
        }

        private void ProcessAnalyticsCallBack(Update update, CallbackQueryParser callbackQueryParser, InlineButtons buttons)
        {
            ResponseConverter responseConverter;

            switch (callbackQueryParser.SubDirection)
            {
                default:
                    throw new NotImplementedException($"Неожиданный CallbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
            }
        }

        private void ProcessSettingsCallBack(Update update, CallbackQueryParser callbackQueryParser, InlineButtons buttons)
        {
            IDomain domain;

            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.SubDirection)
            {
                case "Settings":
                    CurrentUserContext.Navigation.QueryFrom = QueryFrom.Settings;

                    responseConverter = new ResponseConverter("Выберите интересующие настройки");
                    buttonsSets = (ButtonsSet.Settings, ButtonsSet.Main);
                    break;

                case "ArchiveStore":
                    responseConverter = new ResponseConverter("Выберите интересующий архив для разархивирования");
                    buttonsSets = (ButtonsSet.ArchiveList, ButtonsSet.Settings);
                    break;

                case "Archive":
                    switch (callbackQueryParser.ObjectType)
                    {
                        case "Cycles":
                            responseConverter = new ResponseConverter("Выберите архивный цикл для разархивирования");
                            buttonsSets = (ButtonsSet.ArchiveCyclesList, ButtonsSet.ArchiveList);
                            break;
                        case "Days":
                            responseConverter = new ResponseConverter("Выберите архивный день для разархивирования");
                            buttonsSets = (ButtonsSet.ArchiveDaysList, ButtonsSet.ArchiveList);
                            break;
                        case "Exercises":
                            responseConverter = new ResponseConverter("Выберите архивное упражнение для разархивирования");
                            buttonsSets = (ButtonsSet.ArchiveExercisesList, ButtonsSet.ArchiveList);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.ObjectType}");
                    }
                    break;

                case "UnArchive":

                    domain = GetDomainWithId(callbackQueryParser.ObjectId, callbackQueryParser.ObjectType);
                    domain.IsArchive = false;

                    db.Update(domain);
                    db.SaveChanges();

                    responseConverter = new ResponseConverter($"{domain.Name} разархивирован!");
                    buttonsSets = (ButtonsSet.ArchiveList, ButtonsSet.Settings);
                    break;

                case "Setting":
                    switch (callbackQueryParser.ObjectType)
                    {
                        case "Cycles":
                            responseConverter = new ResponseConverter("Выберите интересующие настройки для циклов");
                            buttonsSets = (ButtonsSet.SettingCycles, ButtonsSet.Settings);
                            break;
                        case "Days":
                            responseConverter = new ResponseConverter("Выберите интересующие настройки для дней");
                            buttonsSets = (ButtonsSet.SettingDays, ButtonsSet.SettingCycle);
                            break;
                        case "Exercises":
                            responseConverter = new ResponseConverter("Выберите интересующие настройки для упражнений");
                            buttonsSets = (ButtonsSet.SettingExercises, ButtonsSet.SettingDay);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.ObjectType}");
                    }
                    break;

                case "Add":
                    switch (callbackQueryParser.ObjectType)
                    {
                        case "Cycle":
                            CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.AddCycle;

                            responseConverter = new ResponseConverter("Введите название тренировочного цикла");

                            switch (CurrentUserContext.Navigation.QueryFrom)
                            {
                                case QueryFrom.Start:
                                    buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                                    break;

                                case QueryFrom.Settings:
                                    buttonsSets = (ButtonsSet.None, ButtonsSet.SettingCycles);
                                    break;
                                default:
                                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {CurrentUserContext.Navigation.QueryFrom}");
                            }
                            break;

                        case "Days":
                            CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.AddDays;

                            responseConverter = new ResponseConverter($"Введите название тренирочного дня для цикла {CurrentUserContext.DataManager.CurrentCycle.Name}");

                            switch (CurrentUserContext.Navigation.QueryFrom)
                            {
                                case QueryFrom.Start:
                                    buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                                    break;

                                case QueryFrom.Settings:
                                    buttonsSets = (ButtonsSet.None, ButtonsSet.SettingDay);
                                    break;
                                default:
                                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {CurrentUserContext.Navigation.QueryFrom}");
                            }
                            break;

                        case "Exercises":
                            CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.AddExercises;

                            responseConverter = new ResponseConverter($"Введите название упражнение дня для цикла {CurrentUserContext.DataManager.CurrentDay.Name}");
                            switch (CurrentUserContext.Navigation.QueryFrom)
                            {
                                case QueryFrom.Start:
                                    buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                                    break;

                                case QueryFrom.Settings:
                                    buttonsSets = (ButtonsSet.None, ButtonsSet.SettingExercise);
                                    break;
                                default:
                                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {CurrentUserContext.Navigation.QueryFrom}");
                            }
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CallbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
                    }
                    break;

                case "SaveExercises":
                    db.Exercises.AddRange(CurrentUserContext.DataManager.Exercises);
                    db.SaveChanges();

                    CurrentUserContext.DataManager.ResetExercises();

                    CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.None;

                    switch (CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.Start:
                            responseConverter = new ResponseConverter("Упражнения сохранены!", "Выберите дальнейшее действие");
                            buttonsSets = (ButtonsSet.RedirectAfterSaveExercise, ButtonsSet.None);
                            break;

                        case QueryFrom.Settings:
                            responseConverter = new ResponseConverter("Упражнения сохранены!", "Выберите интересующие настройки для упражнений");
                            buttonsSets = (ButtonsSet.SettingExercises, ButtonsSet.SettingDays);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {CurrentUserContext.Navigation.QueryFrom}");
                    }
                    break;

                case "SettingExisting":
                    switch (callbackQueryParser.ObjectType)
                    {
                        case "Cycles":
                            responseConverter = new ResponseConverter("Выберите интересующий цикл");
                            buttonsSets = (ButtonsSet.CycleList, ButtonsSet.SettingCycles);
                            break;
                        case "Days":
                            responseConverter = new ResponseConverter("Выберите интересующий день");
                            buttonsSets = (ButtonsSet.DaysList, ButtonsSet.SettingDays);
                            break;
                        case "Exercises":
                            responseConverter = new ResponseConverter("Выберите интересующее упражнение");
                            buttonsSets = (ButtonsSet.ExercisesList, ButtonsSet.SettingExercises);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.ObjectType}");
                    }
                    break;

                case "Selected":
                    switch (callbackQueryParser.ObjectType)
                    {
                        case "Cycle":
                            CurrentUserContext.DataManager.SetCycle(CurrentUserContext.UserInformation.Cycles.First(c => c.Id == callbackQueryParser.ObjectId));

                            responseConverter = new ResponseConverter($"Выберите интересующую настройку для цикла {CurrentUserContext.DataManager.CurrentCycle.Name}");
                            buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.CycleList);
                            break;

                        case "Day":
                            switch (CurrentUserContext.Navigation.QueryFrom)
                            {
                                case QueryFrom.NoMatter:
                                    CurrentUserContext.DataManager.SetDay(CurrentUserContext.ActiveCycle.Days.First(d => d.Id == callbackQueryParser.ObjectId));

                                    responseConverter = new ResponseConverter("Выберите упраженение");
                                    buttonsSets = (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout);
                                    break;

                                case QueryFrom.Settings:
                                    CurrentUserContext.DataManager.SetDay(CurrentUserContext.DataManager.CurrentCycle.Days.First(d => d.Id == callbackQueryParser.ObjectId));

                                    responseConverter = new ResponseConverter($"Выберите интересующую настройку для дня {CurrentUserContext.DataManager.CurrentDay.Name}");
                                    buttonsSets = (ButtonsSet.SettingDay, ButtonsSet.DaysList);
                                    break;
                                default:
                                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {CurrentUserContext.Navigation.QueryFrom}");
                            }
                            break;

                        case "Exercise":
                            CurrentUserContext.DataManager.SetExercise(CurrentUserContext.DataManager.CurrentDay.Exercises.First(e => e.Id == callbackQueryParser.ObjectId));

                            switch (CurrentUserContext.Navigation.QueryFrom)
                            {
                                case QueryFrom.NoMatter:
                                    CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.AddResultForExercise;

                                    responseConverter = new ResponseConverter("Введите вес и количество повторений");
                                    buttonsSets = (ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay);
                                    break;

                                case QueryFrom.Settings:
                                    responseConverter = new ResponseConverter($"Выберите интересующую настройку для упражнения {CurrentUserContext.DataManager.CurrentExercise.Name}");
                                    buttonsSets = (ButtonsSet.SettingExercise, ButtonsSet.ExercisesList);
                                    break;
                                default:
                                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {CurrentUserContext.Navigation.QueryFrom}");
                            }
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.ObjectType}");
                    }
                    break;

                case "ChangeActive":
                    switch (callbackQueryParser.ObjectType)
                    {
                        case "Cycle":
                            if (CurrentUserContext.DataManager.CurrentCycle.IsActive)
                            {
                                responseConverter = new ResponseConverter($"Выбранный цикл {CurrentUserContext.ActiveCycle.Name} уже являается активным!",
                                    $"Выберите интересующую настройку для цикла {CurrentUserContext.DataManager.CurrentCycle.Name}");
                                buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.SettingCycles);

                                SendResponse(update.CallbackQuery.Message.Chat.Id, responseConverter.Convert(), buttonsSets);
                                return;
                            }

                            CurrentUserContext.ActiveCycle.IsActive = false;
                            db.Cycles.Update(CurrentUserContext.ActiveCycle);
                            CurrentUserContext.UdpateCycleForce(CurrentUserContext.DataManager.CurrentCycle);
                            db.Cycles.Update(CurrentUserContext.ActiveCycle);
                            db.SaveChanges();

                            responseConverter = new ResponseConverter($"Активный цикл изменён на {CurrentUserContext.ActiveCycle.Name}",
                           $"Выберите интересующую настройку для цикла {CurrentUserContext.DataManager.CurrentCycle.Name}");
                            buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.SettingCycles);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.ObjectType}");
                    }
                    break;

                case "Archiving":
                    switch (callbackQueryParser.ObjectType)
                    {
                        case "Cycle":
                            if (CurrentUserContext.DataManager.CurrentCycle.IsActive)
                            {
                                responseConverter = new ResponseConverter("Ошибка при архивации!", "Нельзя архивировать активный цикл!",
                                    $"Выберите интересующую настройку для цикла {CurrentUserContext.DataManager.CurrentCycle.Name}");
                                buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.CycleList);

                                SendResponse(update.CallbackQuery.Message.Chat.Id, responseConverter.Convert(), buttonsSets);
                                return;
                            }

                            domain = GetDomainFromDataManager(DomainType.Cycle);

                            responseConverter = new ResponseConverter($"Цикл {domain.Name} был добавлен в архив", $"Выберите интересующий цикл");
                            buttonsSets = (ButtonsSet.CycleList, ButtonsSet.SettingCycles);
                            break;

                        case "Day":
                            domain = GetDomainFromDataManager(DomainType.Day);

                            responseConverter = new ResponseConverter($"День {domain.Name} был добавлен в архив", $"Выберите интересующий день");
                            buttonsSets = (ButtonsSet.DaysList, ButtonsSet.SettingDays);
                            break;

                        case "Exercise":
                            domain = GetDomainFromDataManager(DomainType.Exercise);

                            responseConverter = new ResponseConverter($"Упражнение {domain.Name} было добавлено в архив", $"Выберите интересующее упражнение");
                            buttonsSets = (ButtonsSet.ExercisesList, ButtonsSet.SettingExercises);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.ObjectType}");
                    }

                    domain.IsArchive = true;

                    db.Update(domain);
                    db.SaveChanges();

                    CurrentUserContext.DataManager.ResetDomain(domain);

                    break;

                case "Replace":
                    switch (callbackQueryParser.ObjectType)
                    {
                        case "Day":
                            responseConverter = new ResponseConverter($"Выберите цикл, в который хотите перенести день {CurrentUserContext.DataManager.CurrentDay.Name}");
                            buttonsSets = (ButtonsSet.ReplaceToCycle, ButtonsSet.SettingDay);
                            break;
                        case "Exercise":
                            responseConverter = new ResponseConverter($"Выберите день, в который хотите перенести упражнение {CurrentUserContext.DataManager.CurrentExercise.Name}");
                            buttonsSets = (ButtonsSet.ReplaceToDay, ButtonsSet.SettingExercise);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.ObjectType}");
                    }
                    break;

                case "ReplaceTo":
                    switch (callbackQueryParser.ObjectType)
                    {
                        case "Cycle":
                            if (CurrentUserContext.DataManager.CurrentDay.CycleId == callbackQueryParser.ObjectId)
                            {
                                responseConverter = new ResponseConverter($"Ошибка при переносе дня!", "Нельзя перенести день в тот же самый цикл",
                                    $"Выберите цикл, в который хотите перенести день {CurrentUserContext.DataManager.CurrentDay.Name}");
                                buttonsSets = (ButtonsSet.ReplaceToCycle, ButtonsSet.SettingDay);
                                return;
                            }

                            CurrentUserContext.DataManager.CurrentDay.CycleId = callbackQueryParser.ObjectId;
                            db.Days.Update(CurrentUserContext.DataManager.CurrentDay);

                            responseConverter = new ResponseConverter($"День {CurrentUserContext.DataManager.CurrentDay.Name}, перенесён в цикл {callbackQueryParser.ObjectName}",
                                "Выберите интересующий цикл");
                            
                            break;
                        case "Day":
                            if (CurrentUserContext.DataManager.CurrentExercise.DayId == callbackQueryParser.ObjectId)
                            {
                                responseConverter = new ResponseConverter($"Ошибка при переносе упражнения!", "Нельзя перенести упражнение в тот же самый день",
                                    $"Выберите день, в который хотите перенести упражнение {CurrentUserContext.DataManager.CurrentExercise.Name}");
                                buttonsSets = (ButtonsSet.ReplaceToDay, ButtonsSet.SettingExercise);
                                return;
                            }

                            CurrentUserContext.DataManager.CurrentExercise.DayId = callbackQueryParser.ObjectId;
                            db.Exercises.Update(CurrentUserContext.DataManager.CurrentExercise);

                            responseConverter = new ResponseConverter($"Упражнение {CurrentUserContext.DataManager.CurrentExercise.Name}, перенесёно в день {callbackQueryParser.ObjectName}",
                                "Выберите интересующий цикл");
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.ObjectType}");
                    }

                    db.SaveChanges();

                    buttonsSets = (ButtonsSet.CycleList, ButtonsSet.SettingCycles);
                    break;

                case "ChangeName":
                    switch (callbackQueryParser.ObjectType)
                    {
                        case "Cycle":
                            CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.ChangeNameCycle;

                            responseConverter = new ResponseConverter($"Введите новоё название для цикла {CurrentUserContext.DataManager.CurrentCycle.Name}");
                            buttonsSets = (ButtonsSet.None, ButtonsSet.SettingCycle);
                            break;

                        case "ChangeNameDay":
                            CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.ChangeNameDay;

                            responseConverter = new ResponseConverter($"Введите новоё название для дня {CurrentUserContext.DataManager.CurrentDay.Name}");
                            buttonsSets = (ButtonsSet.None, ButtonsSet.SettingDay);
                            break;

                        case "Exercise":
                            CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.ChangeNameExercise;

                            responseConverter = new ResponseConverter($"Введите новоё название для упражнения {CurrentUserContext.DataManager.CurrentExercise.Name}");
                            buttonsSets = (ButtonsSet.None, ButtonsSet.SettingExercise);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.ObjectType}");
                    }
                    break;

                case "Delete":
                    switch (callbackQueryParser.ObjectType)
                    {
                        case "Account":
                            responseConverter = new ResponseConverter("Вы уверены?", "Удаление аккаунта приведёт к полной и безвозвратной потере информации о ваших тренировках");
                            buttonsSets = (ButtonsSet.ConfirmDeleteAccount, ButtonsSet.Settings);
                            break;

                        case "Cycle":
                            responseConverter = new ResponseConverter("Вы уверены?", "Удаление цикла приведёт к полной и безвозвратной потере информации о ваших тренировках в этом цикле");
                            buttonsSets = (ButtonsSet.ConfirmDeleteCycle, ButtonsSet.SettingCycle);
                            break;

                        case "Day":
                            responseConverter = new ResponseConverter("Вы уверены?", "Удаление дня приведёт к полной и безвозвратной потере информации о ваших тренировках в этом дне");
                            buttonsSets = (ButtonsSet.ConfirmDeleteDay, ButtonsSet.SettingDay);
                            break;

                        case "Exercise":
                            responseConverter = new ResponseConverter("Вы уверены?", "Удаление упражнения приведёт к полной и безвозвратной потере информации о ваших тренировках с этим упражнением");
                            buttonsSets = (ButtonsSet.ConfirmDeleteExercise, ButtonsSet.SettingExercise);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.ObjectType}");
                    }
                    break;

                case "ConfirmDelete":
                    switch (callbackQueryParser.ObjectType)
                    {
                        case "Account":
                            primaryProcessUpdate.DeleteContext(CurrentUserContext.UserInformation.UserId);
                            db.UsersInformation.Remove(CurrentUserContext.UserInformation);
                            db.SaveChanges();

                            responseConverter = new ResponseConverter("Аккаунт успешно удалён");
                            buttonsSets = (ButtonsSet.None, ButtonsSet.None);

                            SendResponse(update.CallbackQuery.Message.Chat.Id, responseConverter.Convert(), buttonsSets);
                            return;

                        case "Cycle":
                            if (CurrentUserContext.DataManager.CurrentCycle.IsActive)
                            {
                                responseConverter = new ResponseConverter("Ошибка при удалении!", "Нельзя удалить активный цикл!",
                                    $"Выберите интересующую настройку для цикла {CurrentUserContext.DataManager.CurrentCycle.Name}");
                                buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.CycleList);

                                SendResponse(update.CallbackQuery.Message.Chat.Id, responseConverter.Convert(), buttonsSets);
                                return;
                            }

                            domain = GetDomainFromDataManager(DomainType.Cycle);

                            responseConverter = new ResponseConverter($"Цикл {domain.Name} удалён!", "Выберите интересующий цикл");
                            buttonsSets = (ButtonsSet.CycleList, ButtonsSet.SettingCycles);
                            break;

                        case "Day":
                            domain = GetDomainFromDataManager(DomainType.Day);

                            responseConverter = new ResponseConverter($"День {domain.Name} удалён!", "Выберите интересующий день");
                            buttonsSets = (ButtonsSet.DaysList, ButtonsSet.SettingDays);
                            break;

                        case "Exercise":
                            domain = GetDomainFromDataManager(DomainType.Exercise);

                            responseConverter = new ResponseConverter($"Упражнение {domain.Name} удалёно!", "Выберите интересующее упражнение");
                            buttonsSets = (ButtonsSet.ExercisesList, ButtonsSet.SettingExercises);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.ObjectType}");
                    }

                    db.Remove(domain);
                    db.SaveChanges();

                    CurrentUserContext.DataManager.ResetDomain(domain);

                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
            }

            SendResponse(update.CallbackQuery.Message.Chat.Id, responseConverter.Convert(), buttonsSets);
        }

        IDomain? GetDomainWithId(int id, string domainType)
        {
            return domainType switch
            {
                "Cycle"
                    => GetDomainWithId(id, DomainType.Cycle),
                "Day"
                    => GetDomainWithId(id, DomainType.Day),
               "Exercise"
                    => GetDomainWithId(id, DomainType.Exercise),
                _ => throw new NotImplementedException($"Неожиданный domainTyped: {domainType}")
            };
        }

        IDomain? GetDomainWithId(int id, DomainType domainType)
        {
            return domainType switch
            {
                DomainType.Cycle
                    => CurrentUserContext.UserInformation.Cycles.First(c => c.Id == id),
                DomainType.Day
                    => db.Days.First(d => d.Id == id),
                DomainType.Exercise
                    => db.Exercises.First(e => e.Id == id),
                _ => throw new NotImplementedException($"Неожиданный domainTyped: {domainType}")
            };
        }

        IDomain? GetDomainFromDataManager(DomainType domainType)
        {
            return domainType switch
            {
                DomainType.Cycle
                    => CurrentUserContext.DataManager.CurrentCycle,
                DomainType.Day
                    => CurrentUserContext.DataManager.CurrentDay,
                DomainType.Exercise
                    => CurrentUserContext.DataManager.CurrentExercise,
                _ => throw new NotImplementedException($"Неожиданный domainTyped: {domainType}")
            };
        }

        async Task SendResponse(long chatId, string message, (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) ButtonsSets)
        {
            var buttons = new InlineButtons(CurrentUserContext);

            botClient.SendTextMessageAsync(chatId,
                                            message,
                                            replyMarkup: buttons.GetInlineButtonsWithBack(ButtonsSets.buttonsSet, ButtonsSets.backButtonsSet));
        }
    }
}