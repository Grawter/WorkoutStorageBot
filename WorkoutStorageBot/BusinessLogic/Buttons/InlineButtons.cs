#region using
using System.Text;
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.BusinessLogic.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WorkoutStorageBot.Model;
using WorkoutStorageBot.Helpers.Crypto;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons
{
    internal class InlineButtons
    {
        private List<List<InlineKeyboardButton>> inlineKeyboardButtonsMain;
        private List<InlineKeyboardButton> inlineKeyboardButtons;

        private UserContext CurrentUserContext { get; }

        internal InlineButtons(UserContext userNavigator)
        {
            CurrentUserContext = userNavigator;

            CurrentUserContext.CallBackId = Cryptography.CreateRandomCallBackQueryId();

            inlineKeyboardButtonsMain = new();
        }

        internal IReplyMarkup GetInlineButtons(ButtonsSet buttonsSet)
        {
            return new InlineKeyboardMarkup(GetButtons(buttonsSet));
        }

        internal IReplyMarkup GetInlineButtonsWithBack(ButtonsSet buttonsSet, ButtonsSet backButtonsSet)
        {
            return new InlineKeyboardMarkup(GetButtons(buttonsSet, backButtonsSet));
        }

        private IEnumerable<IEnumerable<InlineKeyboardButton>> GetButtons(ButtonsSet buttonsSet, ButtonsSet backButtonsSet = ButtonsSet.None)
        {
            switch (buttonsSet)
            {
                case ButtonsSet.Main:
                    AddInlineButton($"Начать тренировку", "1|Workout");
                    AddInlineButton($"Аналитика", "2|Analytics");
                    AddInlineButton($"Настройки", "3|Settings");
                    break;

                #region Workout area
                case ButtonsSet.DaysListWithLastWorkout:
                    AddInlineButton("Последняя тренировка", "1|LastWorkout");
                    GetDaysInButtons(CurrentUserContext.ActiveCycle.Days.Where(d => !d.IsArchive), ActionForList.Select);
                    break;

                case ButtonsSet.ExercisesListWithLastWorkoutForDay:
                    AddInlineButton("Последние результаты выбранного дня", $"1|GetLastResultForThisDay|{CurrentUserContext.DataManager.CurrentDay.Id}");
                    GetExercisesInButtons(CurrentUserContext.DataManager.CurrentDay.Exercises.Where(e => !e.IsArchive), ActionForList.Select);
                    break;

                case ButtonsSet.SaveResultForExercise:
                    AddInlineButton("Сохранить результаты", "1|SaveResultForExercise");
                    break;
                #endregion

                #region Analytics area
                #endregion

                #region Settings area
                case ButtonsSet.Settings:
                    AddInlineButton("Настройка тренировочных циклов", "3|SettingCycles");
                    AddInlineButton("Настройка архива", "3|SettingArchive");
                    AddInlineButton("Удалить свой аккаунт", "3|DeleteAccount");
                    break;

                case ButtonsSet.SettingCycles:
                    AddInlineButton("Добавить новый цикл", "3|AddCycle");
                    AddInlineButton("Настройка существующих циклов", "3|SettingExistingCycles");
                    AddInlineButton("Вернуться к главному меню", "0|ToMain");
                    break;

                case ButtonsSet.CycleList:
                    GetCyclesInButtons(CurrentUserContext.UserInformation.Cycles.Where(c => !c.IsArchive), ActionForList.Select);
                    break;

                case ButtonsSet.SettingCycle:
                    AddInlineButton("Сделать активным", "3|ChangeActiveCycle");
                    AddInlineButton("Сменить название", "3|ChangeNameCycle");
                    AddInlineButton("Добавить в архив", "3|CycleArchiving");
                    AddInlineButton("Удалить", "3|DeleteCycle");
                    AddInlineButton("Настройка дней", "3|SettingDays");
                    AddInlineButton("Вернуться к главному меню", "0|ToMain");
                    break;

                case ButtonsSet.SettingDays:
                    AddInlineButton("Добавить новые дни в цикл", "3|AddDays");
                    AddInlineButton("Настройка существующих дней", "3|SettingExistingDays");
                    AddInlineButton("Вернуться к главному меню", "0|ToMain");
                    break;

                case ButtonsSet.DaysList:
                    GetDaysInButtons(CurrentUserContext.DataManager.CurrentCycle.Days.Where(d => !d.IsArchive), ActionForList.Select);
                    break;

                case ButtonsSet.SettingDay:
                    AddInlineButton("Сменить название", "3|ChangeNameDay");
                    AddInlineButton("Перенести день", "3|Replace||Day");
                    AddInlineButton("Добавить в архив", "3|DayArchiving");
                    AddInlineButton("Удалить", "3|DeleteDay");
                    AddInlineButton("Настройка упражнений", "3|SettingExercises");
                    AddInlineButton("Вернуться к главному меню", "0|ToMain");
                    break;

                case ButtonsSet.ReplaceToCycle:
                    GetCyclesInButtons(CurrentUserContext.UserInformation.Cycles.Where(c => !c.IsArchive), ActionForList.Move);
                    break;

                case ButtonsSet.SettingExercises:
                    AddInlineButton("Добавить новые упражнения в день", "3|AddExercises");
                    AddInlineButton("Настройка существующих упражнений", "3|SettingExistingExercises");
                    AddInlineButton("Вернуться к главному меню", "0|ToMain");
                    break;

                case ButtonsSet.ExercisesList:
                    GetExercisesInButtons(CurrentUserContext.DataManager.CurrentDay.Exercises.Where(e => !e.IsArchive), ActionForList.Select);
                    break;

                case ButtonsSet.SettingExercise:
                    AddInlineButton("Сменить название", "3|ChangeNameExercise");
                    AddInlineButton("Перенести упражнение", "3|Replace||Exercise");
                    AddInlineButton("Добавить в архив", "3|ExerciseArchiving");
                    AddInlineButton("Удалить", "3|DeleteExercise");
                    AddInlineButton("Вернуться к главному меню", "0|ToMain");
                    break;

                case ButtonsSet.ReplaceToDay:
                    foreach (var cycle in CurrentUserContext.UserInformation.Cycles.Where(c => !c.IsArchive))
                    {
                        GetDaysInButtons(cycle.Days.Where(c => !c.IsArchive), ActionForList.Move);
                    }
                    break;

                #region Full adding cycle area
                case ButtonsSet.AddCycle:
                    AddInlineButton("Добавить цикл", "3|AddCycle");
                    break;
                case ButtonsSet.AddDays:
                    AddInlineButton($"Добавить дни", "3|AddDays");
                    break;
                case ButtonsSet.AddExercises:
                    AddInlineButton($"Добавить упражнеия", "3|AddExercises");
                    break;
                case ButtonsSet.SaveAddedExercise:
                    AddInlineButton($"Сохранить упражнения", "3|SaveAddedExercise");
                    break;
                case ButtonsSet.RedirectAfterSaveExercise:
                    AddInlineButton($"Добавить новый день", "3|AddDays");
                    AddInlineButton($"Перейти в главное меню", "0|ToMain");
                    break;
                #endregion

                #region Archiving area
                case ButtonsSet.ArchiveList:
                    AddInlineButton("Архивированные циклы", "3|ArchiveCyclesList");
                    AddInlineButton("Архивированные дни", "3|ArchiveDaysList");
                    AddInlineButton("Архивированные упражнения", "3|ArchiveExercisesList");
                    break;
                case ButtonsSet.ArchiveCyclesList:
                    GetCyclesInButtons(CurrentUserContext.UserInformation.Cycles.Where(c => c.IsActive), ActionForList.UnArchive);
                    break;
                case ButtonsSet.ArchiveDaysList:
                    foreach(var cycle in CurrentUserContext.UserInformation.Cycles.Where(c => !c.IsArchive))
                    {
                        GetDaysInButtons(cycle.Days.Where(d => d.IsArchive), ActionForList.UnArchive);
                    }
                    break;
                case ButtonsSet.ArchiveExercisesList:
                    foreach (var cycle in CurrentUserContext.UserInformation.Cycles.Where(c => !c.IsArchive))
                    {
                        foreach (var day in cycle.Days.Where(c => !c.IsArchive))
                        {
                            GetExercisesInButtons(day.Exercises.Where(e => e.IsArchive), ActionForList.UnArchive);
                        }
                    }
                    break;
                #endregion

                #region Confirm delete area
                case ButtonsSet.ConfirmDeleteExercise:
                    AddInlineButton("Да, удалить упражнение", "3|ConfirmDeleteExercise");
                    break;

                case ButtonsSet.ConfirmDeleteDay:
                    AddInlineButton("Да, удалить день", "3|ConfirmDeleteDay");
                    break;

                case ButtonsSet.ConfirmDeleteCycle:
                    AddInlineButton("Да, удалить цикл", "3|ConfirmDeleteCycle");
                    break;

                case ButtonsSet.ConfirmDeleteAccount:
                    AddInlineButton("Да, удалить аккаунт", "3|ConfirmDeleteAccount");
                    break;
                case ButtonsSet.None:
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный buttonsSet: {buttonsSet}");
                #endregion

                #endregion
            }

            if (backButtonsSet != ButtonsSet.None)
                AddInlineButton("Назад", $"0|Back|{backButtonsSet}");

            return inlineKeyboardButtonsMain;
        }

        private void AddInlineButton(string titleButton, string callBackDataWithoutSetId, bool onNewLine = true)
        {
            var callBackData = AddCallBackSetId(callBackDataWithoutSetId);

            if (onNewLine)
            {
                inlineKeyboardButtons = new() { InlineKeyboardButton.WithCallbackData(titleButton, callBackData) };

                inlineKeyboardButtonsMain.Add(inlineKeyboardButtons);
            }
            else
            {
                if (inlineKeyboardButtons is null)
                    inlineKeyboardButtons = new();

                inlineKeyboardButtons.Add(InlineKeyboardButton.WithCallbackData(titleButton, callBackData));
            }
        }

        private string AddCallBackSetId(string callBackData)
        {
            var sb = new StringBuilder(callBackData)
                                                .Append("|")
                                                .Append(CurrentUserContext.CallBackId);
            return sb.ToString();
        }

        private void GetCyclesInButtons(IEnumerable<Cycle> source, ActionForList actionForList)
        {
            switch (actionForList)
            {
                case ActionForList.Select:
                    foreach (var cycle in source)
                    {
                        AddInlineButton(cycle.Name, $"3|SelectedCycle|{cycle.Id}");
                    }
                    break;
                case ActionForList.UnArchive:
                    foreach (var cycle in source)
                    {
                        AddInlineButton(cycle.Name, $"3|UnArchive|{cycle.Id}|Cycle");
                    }
                    break;
                case ActionForList.Move:
                    foreach (var cycle in source)
                    {
                        AddInlineButton(cycle.Name, $"3|ReplaceTo|{cycle.Id}|Cycle|{cycle.Name}");
                    }
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный actionForList: {actionForList}");

            }
        }

        private void GetDaysInButtons(IEnumerable<Day> source, ActionForList actionForList)
        {
            switch (actionForList)
            {
                case ActionForList.Select:
                    foreach (var day in source)
                    {
                        AddInlineButton(day.Name, $"3|SelectedDay|{day.Id}");
                    }
                    break;
                case ActionForList.UnArchive:
                    foreach (var day in source)
                    {
                        AddInlineButton(day.Name, $"3|UnArchive|{day.Id}|Day");
                    }
                    break;
                case ActionForList.Move:
                    foreach (var day in source)
                    {
                        AddInlineButton(day.Name, $"3|ReplaceTo|{day.Id}|Day|{day.Name}");
                    }
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный actionForList: {actionForList}");

            }    

        }

        private void GetExercisesInButtons(IEnumerable<Exercise> source, ActionForList actionForList)
        {
            switch (actionForList)
            {
                case ActionForList.Select:
                    foreach (var exercise in source)
                    {
                        AddInlineButton(exercise.Name, $"3|SelectedExercise|{exercise.Id}");
                    }
                    break;
                case ActionForList.UnArchive:
                    foreach (var exercise in source)
                    {
                        AddInlineButton(exercise.Name, $"3|UnArchive|{exercise.Id}|Exercise");
                    }
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный actionForList: {actionForList}");

            }
        }
    }
}