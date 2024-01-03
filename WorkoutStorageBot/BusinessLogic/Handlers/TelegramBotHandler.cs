#region using
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.Helpers.Logger;
using WorkoutStorageBot.Helpers.CallbackQueryParser;
using WorkoutStorageBot.Helpers.ResponseGenerator;
using WorkoutStorageBot.Helpers.UserMessageConverter;
using WorkoutStorageBot.BusinessLogic.StepStore;
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.BusinessLogic.Buttons;
using WorkoutStorageBot.BusinessLogic.SQLiteQueries;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Helpers.Crypto;
using WorkoutStorageBot.Model;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers
{
    internal class TelegramBotHandler
    {
        private readonly ITelegramBotClient botClient;
        private readonly ApplicationContext db;
        private readonly PrimaryUpdateHandler primaryProcessUpdate;
        private UserContext? currentUserContext;
        private UserContext? CurrentUserContext { get => currentUserContext; set => currentUserContext = value; }
        internal bool WhiteList { get => primaryProcessUpdate.WhiteList;  set => primaryProcessUpdate.WhiteList = value;  }

        internal TelegramBotHandler(ITelegramBotClient botClient, DbContextOptions<ApplicationContext> options, ILogger logger)
        {
            this.botClient = botClient;
            db = new(options);

            primaryProcessUpdate = new PrimaryUpdateHandler(in logger, in this.botClient, in db);
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
            }
        }

        private void ProcessMessage(Update update)
        {
            var requestConverter = new UserMessageConverter(update.Message.Text);
            
            ResponseGenerator responseGenerator;

            var buttons = new InlineButtons(CurrentUserContext);

            switch (CurrentUserContext.NavigationType)
            {
                #region Workout area

                #region Set based cycle

                case NavigationType.SetNameCycle:
                    requestConverter.RemoveCompletely().WithoutServiceSymbol();

                    CurrentUserContext.DataManager.SetCycle(requestConverter.Convert(), CurrentUserContext.UserInformation.Id);

                    CurrentUserContext.NavigationType = NavigationType.SetExercise;

                    botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                    $"Введите упражнение для дня № {CurrentUserContext.DataManager.NumberDay}",
                                                    replyMarkup: buttons.GetInlineButtons(ButtonsSet.SetCycle));
                    break;

                case NavigationType.SetExercise:
                    requestConverter.RemoveCompletely().WithoutServiceSymbol();

                    CurrentUserContext.DataManager.AddExercise(requestConverter.Convert());

                    responseGenerator = new ResponseGenerator("Упражнение зафиксировано",
                        $"Введите след. упражнение для дня № {CurrentUserContext.DataManager.NumberDay} либо нажмите \"Сохранить\" для сохранения текущего списка фиксаций");
                    botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                    responseGenerator.Generate(),
                                                    replyMarkup: buttons.GetInlineButtons(ButtonsSet.SetCycle));
                    break;

                #endregion

                case NavigationType.SetResultForExercise:
                    requestConverter.RemoveCompletely(20).WithoutServiceSymbol();

                    responseGenerator = new ResponseGenerator("Подход зафиксирован",
                        "Введите вес и кол-во повторений след. подхода либо нажмите \"Сохранить\" для сохранения указанных подходов");

                    try
                    {
                        CurrentUserContext.DataManager.AddResultExercise(requestConverter.GetResultExercise());
                    }
                    catch (FormatException)
                    {
                        responseGenerator = new ResponseGenerator("Неожиданный формат результата",
                            "Введите результат заново. Пример ожидаемого ввода: 50 10");
                        botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                        responseGenerator.Generate(),
                                                        replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.None, backButtonsSet: ButtonsSet.WorkoutExercises));
                        break;
                    }

                    botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                    responseGenerator.Generate(),
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.SaveResultForExercise, backButtonsSet: ButtonsSet.Main));
                    break;

                #endregion

                #region Start

                case NavigationType.None:
                    var text = requestConverter.RemoveCompletely().Convert();

                    switch (text.ToLower())
                    {
                        case "/start":
                            if (CurrentUserContext.Cycle != null)
                            {
                                botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                                "Выберите интересующий вас раздел",
                                                                replyMarkup: buttons.GetInlineButtons(ButtonsSet.Main));
                            }
                            else
                            {
                                botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                                                "Начнём",
                                                                replyMarkup: buttons.GetInlineButtons(ButtonsSet.StartSetCycle));
                            }
                            break;

                        default:
                            botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Неизвестная команда: {text} \nДля получения разделов воспользуйтесь командой /Start");
                            break;
                    }
                    break;

                #endregion
            }
        }

        private void ProcessCallbackQuery(Update update)
        {
            CallbackQueryParser.TryParse(update.CallbackQuery.Data);

            if (!ProcessRelevanceCallBackQueryId())
                return;

            ResponseGenerator responseGenerator;

            var buttons = new InlineButtons(CurrentUserContext);

            switch (CallbackQueryParser.Direction)
            {
                #region Workout area

                #region Set based cycle

                case "#SetCycle":
                    CurrentUserContext.NavigationType = NavigationType.SetNameCycle;

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "Придумайте и введите название тренировочного цикла");
                    break;

                case "#SwitchOnNewDay":
                    CurrentUserContext.DataManager.AddDay();

                    responseGenerator = new ResponseGenerator("День зафиксирован",
                        $"Введите след. упражнение для дня № {CurrentUserContext.DataManager.NumberDay} либо нажмите \"Сохранить\" для сохранения текущего списка фиксаций");

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    responseGenerator.Generate(),
                                                    replyMarkup: buttons.GetInlineButtons(ButtonsSet.SetCycle));
                    break;

                case "#SaveCycle":
                    var currentCycle = CurrentUserContext.DataManager.GetCycleWithUiId(CurrentUserContext.UserInformation.Id);
                    CurrentUserContext.RefleshCycleForce(currentCycle);
                    db.Cycles.Add(currentCycle);
                    db.SaveChanges();

                    CurrentUserContext.DataManager.ResetCycle();

                    CurrentUserContext.NavigationType = NavigationType.None;

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    "Цикл создан, можно приступать к тренировкам!",
                                                    replyMarkup: buttons.GetInlineButtons(ButtonsSet.Main));
                    break;

                case "#StartWorkout":
                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    "Выберите тренировочный день",
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.WorkoutDays, backButtonsSet: ButtonsSet.Main));
                    break;

                #endregion

                case "#LastWorkout":
                    var exercises = QueriesStorage.GetExercisesWithDaysIds(CurrentUserContext.Cycle.Days.Select(d => d.Id), db.GetExercisesFromQuery);
                    var resultExercisesForLastWorkout = QueriesStorage.GetLastResultsExercisesWithExercisesIds(exercises.Select(e => e.Id), db.GetResultExercisesFromQuery);

                    var informationAboutLastWorkout = ResponseGenerator.GetInformationAboutLastWorkout(exercises, resultExercisesForLastWorkout);
                    responseGenerator = new ResponseGenerator("Последняя тренировка:", informationAboutLastWorkout, "Выберите тренировочный день");

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    responseGenerator.Generate(),
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.WorkoutDays, backButtonsSet: ButtonsSet.Main));
                    break;

                case "#SelectedDay":
                    CurrentUserContext.DataManager.SetDay(CallbackQueryParser.ObjectName, CallbackQueryParser.ObjectId);

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    "Выберите упраженение",
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.WorkoutExercises, backButtonsSet: ButtonsSet.WorkoutDays));
                    break;

                case "#GetLastResultForThisDay":
                    var day = CurrentUserContext.Cycle.Days.FirstOrDefault(d => d.NameDay == CallbackQueryParser.ObjectName);
                    var lastResultsForExercises = QueriesStorage.GetLastResultsForExercisesWithExercisesIds(day.Exercises.Select(d => d.Id), db.GetResultExercisesFromQuery);

                    var informationAboutLastDay = ResponseGenerator.GetInformationAboutLastDay(day.Exercises, lastResultsForExercises);
                    responseGenerator = new ResponseGenerator("Последняя результаты упражений из этого дня:", informationAboutLastDay, "Выберите упражнение");

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    responseGenerator.Generate(),
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.WorkoutExercises, backButtonsSet: ButtonsSet.WorkoutDays));
                    break;
                case "#SetResultForExercise":
                    CurrentUserContext.DataManager.SetExercise(CallbackQueryParser.ObjectName, CallbackQueryParser.ObjectId);

                    CurrentUserContext.NavigationType = NavigationType.SetResultForExercise;

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    "Введите вес и количество повторений",
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.None, backButtonsSet: ButtonsSet.WorkoutExercises));
                    break;

                case "#SaveResultForExercise":
                    db.ResultsExercises.AddRange(CurrentUserContext.DataManager.ResultExercises);
                    db.SaveChanges();

                    CurrentUserContext.DataManager.ResetResultExercises();

                    CurrentUserContext.NavigationType = NavigationType.None;

                    responseGenerator = new ResponseGenerator("Введённые данные сохранены!", "Выберите упраженение");

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    responseGenerator.Generate(),
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: ButtonsSet.WorkoutExercises, backButtonsSet: ButtonsSet.WorkoutDays));
                    break;

                #endregion

                #region Settings area



                #endregion

                #region BackStep

                case "#Back":
                    var previousStep = StepStorage.GetStep(CallbackQueryParser.Args[1]);

                    CurrentUserContext.NavigationType = previousStep.NavigationType;

                    botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                                    previousStep.Message,
                                                    replyMarkup: buttons.GetInlineButtonsWithBack(buttonsSet: previousStep.ButtonsSet, backButtonsSet: previousStep.BackButtonsSet));
                    break;

                #endregion
            }
        }

        private bool ProcessRelevanceCallBackQueryId()
        {
            if (CurrentUserContext.CallBackSetId?.ToString() != CallbackQueryParser.CallBackSetId)
            {
                botClient.SendTextMessageAsync(CurrentUserContext.UserInformation.UserId, "Действие невыполнено. Устаревшая информация");

                return false;
            }

            CurrentUserContext.CallBackSetId = Convert.ToBase64String(Cryptography.CreateRandomByteArray());

            return true;
        }
    }
}