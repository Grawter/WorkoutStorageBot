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
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.SaveResultForExercise, backButtonsSet: ButtonsSet.Main));
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
                                                    replyMarkup: buttons.GetInlineButtons(buttonsSet: ButtonsSet.SaveAddedExercise));
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
                    ProcessAbstractCallBack(update, callbackQueryParser, buttons);
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

        private void ProcessAbstractCallBack(Update update, CallbackQueryParser callbackQueryParser, InlineButtons buttons)
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
                    
                    botClient.SendTextMessageAsync( update.CallbackQuery.Message.Chat.Id,
                                                    message,
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: previousStep.ButtonsSet, backButtonsSet: previousStep.BackButtonsSet));
                    break;

                case "ToMain":
                    CurrentUserContext.DataManager.ResetAll();

                    var mainStep = StepStorage.GetMainStep();

                    CurrentUserContext.Navigation.QueryFrom = mainStep.QueryFrom;
                    CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.None;

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    mainStep.Message,
                                                    replyMarkup: buttons.GetInlineButtons(buttonsSet: mainStep.ButtonsSet));
                    break;

                default:
                    throw new NotImplementedException($"Неожиданный CallbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");

                    #endregion
            }
        }

        private void ProcessWorkoutCallBack(Update update, CallbackQueryParser callbackQueryParser, InlineButtons buttons)
        {
            ResponseConverter responseConverter;

            switch (callbackQueryParser.SubDirection)
            {
                #region Workout area

                case "Workout":
                    CurrentUserContext.Navigation.QueryFrom = QueryFrom.NoMatter; // not necessary, but just in case

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    "Выберите тренировочный день",
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.DaysListWithLastWorkout, backButtonsSet: ButtonsSet.Main));
                    break;

                case "LastWorkout":
                    var exercises = QueriesStorage.GetExercisesWithDaysIds(CurrentUserContext.ActiveCycle.Days.Where(d => !d.IsArchive).Select(d => d.Id), db.GetExercisesFromQuery);
                    var resultExercisesForLastWorkout = QueriesStorage.GetLastResultsExercisesWithExercisesIds(exercises.Select(e => e.Id), db.GetResultExercisesFromQuery);

                    var informationAboutLastWorkout = ResponseConverter.GetInformationAboutLastWorkout(exercises, resultExercisesForLastWorkout);
                    responseConverter = new ResponseConverter("Последняя тренировка:", informationAboutLastWorkout, "Выберите тренировочный день");

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    responseConverter.Convert(),
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.DaysListWithLastWorkout, backButtonsSet: ButtonsSet.Main));
                    break;

                case "GetLastResultForThisDay":
                    var day = CurrentUserContext.ActiveCycle.Days.FirstOrDefault(d => d.Id == callbackQueryParser.ObjectId);
                    var lastResultsForExercises = QueriesStorage.GetLastResultsForExercisesWithExercisesIds(day.Exercises.Where(e => !e.IsArchive).Select(d => d.Id), db.GetResultExercisesFromQuery);

                    var informationAboutLastDay = ResponseConverter.GetInformationAboutLastDay(day.Exercises, lastResultsForExercises);
                    responseConverter = new ResponseConverter("Последняя результаты упражений из этого дня:", informationAboutLastDay, "Выберите упражнение");

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    responseConverter.Convert(),
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.ExercisesListWithLastWorkoutForDay, backButtonsSet: ButtonsSet.DaysListWithLastWorkout));
                    break;

                case "SaveResultForExercise":
                    db.ResultsExercises.AddRange(CurrentUserContext.DataManager.ResultExercises);
                    db.SaveChanges();

                    CurrentUserContext.DataManager.ResetResultExercises();

                    CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.None;

                    responseConverter = new ResponseConverter("Введённые данные сохранены!", "Выберите упраженение");

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    responseConverter.Convert(),
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.ExercisesListWithLastWorkoutForDay, backButtonsSet: ButtonsSet.DaysList));
                    break;

                default:
                    throw new NotImplementedException($"Неожиданный CallbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");

                    #endregion
            }
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
            ResponseConverter responseConverter;

            switch (callbackQueryParser.SubDirection)
            {
                #region Settings area

                case "Settings":
                    CurrentUserContext.Navigation.QueryFrom = QueryFrom.Settings;
                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    "Выберите интересующие настройки",
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.Settings, backButtonsSet: ButtonsSet.Main));
                    break;

                case "SettingArchive":
                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    "Выберите интересующий архив для разархивирования",
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.ArchiveList, backButtonsSet: ButtonsSet.Settings));
                    break;

                case "ArchiveCyclesList":
                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    "Выберите архивный цикл для разархивирования",
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.ArchiveCyclesList, backButtonsSet: ButtonsSet.ArchiveList));
                    break;

                case "ArchiveDaysList":
                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    "Выберите архивный день для разархивирования",
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.ArchiveDaysList, backButtonsSet: ButtonsSet.ArchiveList));
                    break;

                case "ArchiveExercisesList":
                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    "Выберите архивное упражнение для разархивирования",
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.ArchiveExercisesList, backButtonsSet: ButtonsSet.ArchiveList));
                    break;

                case "UnArchive":

                    IDomain domain = callbackQueryParser.ObjectType switch
                    {
                        "Cycle"
                            => CurrentUserContext.UserInformation.Cycles.First(c => c.Id == callbackQueryParser.ObjectId),
                        "Day"
                            => db.Days.First(d => d.Id == callbackQueryParser.ObjectId),
                        "Exercise"
                            => db.Exercises.First(e => e.Id == callbackQueryParser.ObjectId),
                         _=> throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectId: {callbackQueryParser.ObjectId}")
                    };
                    
                    domain.IsArchive = false;

                    db.Update(domain);
                    db.SaveChanges();

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    $"{domain.Name} разархивирован!",
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.ArchiveList, backButtonsSet: ButtonsSet.Settings));
                    break;

                case "DeleteAccount":
                    responseConverter = new ResponseConverter("Вы уверены?", "Удаление аккаунта приведёт к полной и безвозвратной потере информации о ваших тренировках");

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    responseConverter.Convert(),
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.ConfirmDeleteAccount, backButtonsSet: ButtonsSet.Settings));
                    break;

                case "ConfirmDeleteAccount":
                    primaryProcessUpdate.DeleteContext(CurrentUserContext.UserInformation.UserId);
                    db.UsersInformation.Remove(CurrentUserContext.UserInformation);
                    db.SaveChanges();

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    "Аккаунт успешно удалён");
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CallbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");

                #region Full add cycle area

                case "AddCycle":
                    CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.AddCycle;

                    switch (CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.Start:
                            botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "Введите название тренировочного цикла");
                            break;

                        case QueryFrom.Settings:
                            botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                        "Введите название тренировочного цикла",
                                                        replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.None, backButtonsSet: ButtonsSet.SettingCycles));
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {CurrentUserContext.Navigation.QueryFrom}");
                    }

                    break;

                case "AddDays":
                    CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.AddDays;

                    switch (CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.Start:
                            botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                            $"Введите название тренирочного дня для цикла {CurrentUserContext.DataManager.CurrentCycle.Name}");
                            break;

                        case QueryFrom.Settings:
                            botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                            $"Введите название тренирочного дня для цикла {CurrentUserContext.DataManager.CurrentCycle.Name}",
                                                            replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.None, backButtonsSet: ButtonsSet.SettingDay));
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {CurrentUserContext.Navigation.QueryFrom}");
                    }

                    break;

                case "AddExercises":
                    CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.AddExercises;

                    switch (CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.Start:
                            botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                            $"Введите название упражнение дня для цикла {CurrentUserContext.DataManager.CurrentDay.Name}");
                            break;

                        case QueryFrom.Settings:
                            botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                            $"Введите название упражнение дня для цикла {CurrentUserContext.DataManager.CurrentDay.Name}",
                                                            replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.None, backButtonsSet: ButtonsSet.SettingExercise));
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {CurrentUserContext.Navigation.QueryFrom}");
                    }

                    break;

                case "SaveAddedExercise":
                    CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.None;

                    db.Exercises.AddRange(CurrentUserContext.DataManager.Exercises);
                    db.SaveChanges();

                    CurrentUserContext.DataManager.ResetExercises();

                    switch (CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.Start:
                            responseConverter = new ResponseConverter("Упражнения сохранены!", "Выберите дальнейшее действие");
                            botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                            responseConverter.Convert(),
                                                            replyMarkup: buttons.GetInlineButtons(buttonsSet: ButtonsSet.RedirectAfterSaveExercise));
                            break;

                        case QueryFrom.Settings:
                            responseConverter = new ResponseConverter("Упражнения сохранены!", "Выберите интересующие настройки для упражнений");
                            botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                            responseConverter.Convert(),
                                                            replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.SettingExercises, backButtonsSet: ButtonsSet.SettingDays));
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {CurrentUserContext.Navigation.QueryFrom}");
                    }


                    break;

                #endregion

                #region Cycles settings
                case "SettingCycles":
                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    "Выберите интересующие настройки для циклов",
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.SettingCycles, backButtonsSet: ButtonsSet.Settings));
                    break;
                
                case "SettingExistingCycles":
                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    "Выберите интересующий цикл",
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.CycleList, backButtonsSet: ButtonsSet.SettingCycles));
                    break;

                case "SelectedCycle":
                    CurrentUserContext.DataManager.SetCycle(CurrentUserContext.UserInformation.Cycles.First(c => c.Id == callbackQueryParser.ObjectId));

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    $"Выберите интересующую настройку для цикла {CurrentUserContext.DataManager.CurrentCycle.Name}",
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.SettingCycle, backButtonsSet: ButtonsSet.CycleList));
                    break;

                case "ChangeActiveCycle":
                    if (CurrentUserContext.DataManager.CurrentCycle.IsActive)
                    {
                        responseConverter = new ResponseConverter($"Выбранный цикл {CurrentUserContext.ActiveCycle.Name} уже являается активным!", 
                            $"Выберите интересующую настройку для цикла {CurrentUserContext.DataManager.CurrentCycle.Name}");

                        botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    responseConverter.Convert(),
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.SettingCycle, backButtonsSet: ButtonsSet.SettingCycles));
                        break;
                    }

                    CurrentUserContext.ActiveCycle.IsActive = false;
                    db.Cycles.Update(CurrentUserContext.ActiveCycle);
                    CurrentUserContext.UdpateCycleForce(CurrentUserContext.DataManager.CurrentCycle);
                    db.Cycles.Update(CurrentUserContext.ActiveCycle);
                    db.SaveChanges();

                    responseConverter = new ResponseConverter($"Активный цикл изменён на {CurrentUserContext.ActiveCycle.Name}",
                           $"Выберите интересующую настройку для цикла {CurrentUserContext.DataManager.CurrentCycle.Name}");

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    responseConverter.Convert(),
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.SettingCycle, backButtonsSet: ButtonsSet.SettingCycles));
                    break;

                case "ChangeNameCycle":
                    CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.ChangeNameCycle;

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    $"Введите новоё название для цикла {CurrentUserContext.DataManager.CurrentCycle.Name}",
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.None, backButtonsSet: ButtonsSet.SettingCycle));
                    break;

                case "CycleArchiving":
                if (CurrentUserContext.DataManager.CurrentCycle.IsActive)
                {
                    responseConverter = new ResponseConverter("Ошибка при архивации!", "Нельзя архивировать активный цикл!",
                        $"Выберите интересующую настройку для цикла {CurrentUserContext.DataManager.CurrentCycle.Name}");

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                responseConverter.Convert(),
                                                replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.SettingCycle, backButtonsSet: ButtonsSet.CycleList));
                    break;
                }

                CurrentUserContext.DataManager.CurrentCycle.IsArchive = true;
                db.Cycles.Update(CurrentUserContext.DataManager.CurrentCycle);
                db.SaveChanges();

                responseConverter = new ResponseConverter($"Цикл {CurrentUserContext.DataManager.CurrentCycle.Name} был добавлен в архив",
                    $"Выберите интересующий цикл");

                CurrentUserContext.DataManager.ResetCycle();

                botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                responseConverter.Convert(),
                                                replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.CycleList, backButtonsSet: ButtonsSet.SettingCycles));
                break;

                case "DeleteCycle":
                    responseConverter = new ResponseConverter("Вы уверены?", "Удаление цикла приведёт к полной и безвозвратной потере информации о ваших тренировках в этом цикле");

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    responseConverter.Convert(),
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.ConfirmDeleteCycle, backButtonsSet: ButtonsSet.SettingCycle));
                    break;

                case "ConfirmDeleteCycle":
                    if (CurrentUserContext.DataManager.CurrentCycle.IsActive)
                    {
                        responseConverter = new ResponseConverter("Ошибка при удалении!", "Нельзя удалить активный цикл!", 
                            $"Выберите интересующую настройку для цикла {CurrentUserContext.DataManager.CurrentCycle.Name}");

                        botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    responseConverter.Convert(),
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.SettingCycle, backButtonsSet: ButtonsSet.CycleList));
                        break;
                    }

                    responseConverter = new ResponseConverter($"Цикл {CurrentUserContext.DataManager.CurrentCycle.Name} удалён!", "Выберите интересующий цикл");

                    db.Cycles.Remove(CurrentUserContext.DataManager.CurrentCycle);
                    db.SaveChanges();

                    CurrentUserContext.DataManager.ResetCycle();

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    responseConverter.Convert(),
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.CycleList, backButtonsSet: ButtonsSet.SettingCycles));
                    break;
                #endregion

                #region Days settings
                case "SettingDays":
                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    "Выберите интересующие настройки для дней",
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.SettingDays, backButtonsSet: ButtonsSet.SettingCycle));
                    break;

                case "SettingExistingDays":
                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    "Выберите интересующий день",
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.DaysList, backButtonsSet: ButtonsSet.SettingDays));
                    break;

                case "SelectedDay":
                    switch (CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.NoMatter:
                            CurrentUserContext.DataManager.SetDay(CurrentUserContext.ActiveCycle.Days.First(d => d.Id == callbackQueryParser.ObjectId));

                            botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                            "Выберите упраженение",
                                                            replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.ExercisesListWithLastWorkoutForDay, backButtonsSet: ButtonsSet.DaysListWithLastWorkout));
                            break;

                        case QueryFrom.Settings:
                            CurrentUserContext.DataManager.SetDay(CurrentUserContext.DataManager.CurrentCycle.Days.First(d => d.Id == callbackQueryParser.ObjectId));

                            botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                            $"Выберите интересующую настройку для дня {CurrentUserContext.DataManager.CurrentDay.Name}",
                                                            replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.SettingDay, backButtonsSet: ButtonsSet.DaysList));
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {CurrentUserContext.Navigation.QueryFrom}");
                    }

                    break;

                case "ChangeNameDay":
                    CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.ChangeNameDay;

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    $"Введите новоё название для дня {CurrentUserContext.DataManager.CurrentDay.Name}",
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.None, backButtonsSet: ButtonsSet.SettingDay));
                    break;

                case "DayArchiving":
                    CurrentUserContext.DataManager.CurrentDay.IsArchive = true;
                    db.Days.Update(CurrentUserContext.DataManager.CurrentDay);
                    db.SaveChanges();

                    responseConverter = new ResponseConverter($"День {CurrentUserContext.DataManager.CurrentDay.Name} был добавлен в архив",
                            $"Выберите интересующий день");

                    CurrentUserContext.DataManager.ResetDay();

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    responseConverter.Convert(),
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.DaysList, backButtonsSet: ButtonsSet.SettingDays));
                    break;

                case "DeleteDay":
                    responseConverter = new ResponseConverter("Вы уверены?", "Удаление дня приведёт к полной и безвозвратной потере информации о ваших тренировках в этом дне");

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    responseConverter.Convert(),
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.ConfirmDeleteDay, backButtonsSet: ButtonsSet.SettingDay));
                    break;

                case "ConfirmDeleteDay":
                    responseConverter = new ResponseConverter($"День {CurrentUserContext.DataManager.CurrentDay.Name} удалён!", "Выберите интересующий день");

                    db.Days.Remove(CurrentUserContext.DataManager.CurrentDay);
                    db.SaveChanges();

                    CurrentUserContext.DataManager.ResetDay();

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    responseConverter.Convert(),
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.DaysList, backButtonsSet: ButtonsSet.SettingDays));
                    break;
                #endregion

                #region Exercises settings
                case "SettingExercises":
                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    "Выберите интересующие настройки для упражнений",
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.SettingExercises, backButtonsSet: ButtonsSet.SettingDay));
                    break;

                case "SettingExistingExercises":
                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    "Выберите интересующее упражнение",
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.ExercisesList, backButtonsSet: ButtonsSet.SettingExercises));
                    break;

                case "SelectedExercise":
                    CurrentUserContext.DataManager.SetExercise(CurrentUserContext.DataManager.CurrentDay.Exercises.First(e => e.Id == callbackQueryParser.ObjectId));

                    switch (CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.NoMatter:
                            CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.AddResultForExercise;

                            botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                            "Введите вес и количество повторений",
                                                            replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.None, backButtonsSet: ButtonsSet.ExercisesListWithLastWorkoutForDay));
                            break;

                        case QueryFrom.Settings:
                            botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                            $"Выберите интересующую настройку для упражнения {CurrentUserContext.DataManager.CurrentExercise.Name}",
                                                            replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.SettingExercise, backButtonsSet: ButtonsSet.ExercisesList));
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {CurrentUserContext.Navigation.QueryFrom}");
                    }

                    break;

                case "ChangeNameExercise":
                    CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.ChangeNameExercise;

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    $"Введите новоё название для упражнения {CurrentUserContext.DataManager.CurrentExercise.Name}",
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.None, backButtonsSet: ButtonsSet.SettingExercise));
                    break;

                case "ExerciseArchiving":
                    CurrentUserContext.DataManager.CurrentExercise.IsArchive = true;
                    db.Exercises.Update(CurrentUserContext.DataManager.CurrentExercise);
                    db.SaveChanges();

                    responseConverter = new ResponseConverter($"Упражнение {CurrentUserContext.DataManager.CurrentExercise.Name} было добавлено в архив",
                            $"Выберите интересующее упражнение");

                    CurrentUserContext.DataManager.ResetExercise();

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    responseConverter.Convert(),
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.ExercisesList, backButtonsSet: ButtonsSet.SettingExercises));
                    break;

                case "DeleteExercise":
                    responseConverter = new ResponseConverter("Вы уверены?", "Удаление упражнения приведёт к полной и безвозвратной потере информации о ваших тренировках с этим упражнением");

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    responseConverter.Convert(),
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.ConfirmDeleteExercise, backButtonsSet: ButtonsSet.SettingExercise));
                    break;

                case "ConfirmDeleteExercise":
                    responseConverter = new ResponseConverter($"Упражнение {CurrentUserContext.DataManager.CurrentExercise.Name} удалёно!", "Выберите интересующее упражнение");

                    db.Exercises.Remove(CurrentUserContext.DataManager.CurrentExercise);
                    db.SaveChanges();

                    CurrentUserContext.DataManager.ResetExercise();

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    responseConverter.Convert(),
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.ExercisesList, backButtonsSet: ButtonsSet.SettingExercises));
                    break;
                #endregion

                #endregion
            }
        }
    }
}