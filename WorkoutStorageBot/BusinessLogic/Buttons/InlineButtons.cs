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

        internal IReplyMarkup GetInlineButtons(ButtonsSet buttonsSet, ButtonsSet backButtonsSet = ButtonsSet.None)
        {
            return new InlineKeyboardMarkup(GetButtons(buttonsSet, backButtonsSet));
        }

        private IEnumerable<IEnumerable<InlineKeyboardButton>> GetButtons(ButtonsSet buttonsSet, ButtonsSet backButtonsSet)
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
                    AddInlineButton("Последняя тренировка", "1|LastResult|Exercises");
                    GetDaysInButtons(CurrentUserContext.ActiveCycle.Days.Where(d => !d.IsArchive), ActionForList.Select);
                    break;

                case ButtonsSet.ExercisesListWithLastWorkoutForDay:
                    AddInlineButton("Последние результаты выбранного дня", $"1|LastResult|Day");
                    GetExercisesInButtons(CurrentUserContext.DataManager.CurrentDay.Exercises.Where(e => !e.IsArchive), ActionForList.Select);
                    break;

                case ButtonsSet.SaveResultsExercise:
                    AddInlineButton("Сохранить результаты", "1|SaveResultsExercise");
                    break;
                #endregion

                #region Analytics area
                #endregion

                #region Settings area
                case ButtonsSet.Settings:
                    AddInlineButton("Настройка тренировочных циклов", "3|Setting|Cycles");
                    AddInlineButton("Архив", "3|ArchiveStore");
                    AddInlineButton("Удалить свой аккаунт", "3|Delete|Account");
                    break;

                case ButtonsSet.SettingCycles:
                    AddInlineButton("Добавить новый цикл", "3|Add|Cycle");
                    AddInlineButton("Настройка существующих циклов", "3|SettingExisting|Cycles");
                    AddInlineButton("Вернуться к главному меню", "0|ToMain");
                    break;

                case ButtonsSet.CycleList:
                    GetCyclesInButtons(CurrentUserContext.UserInformation.Cycles.Where(c => !c.IsArchive), ActionForList.Select);
                    break;

                case ButtonsSet.SettingCycle:
                    AddInlineButton("Сделать активным", "3|ChangeActive|Cycle");
                    AddInlineButton("Сменить название", "3|ChangeName|Cycle");
                    AddInlineButton("Добавить в архив", "3|Archiving|Cycle");
                    AddInlineButton("Удалить", "3|Delete|Cycle");
                    AddInlineButton("Настройка дней", "3|Setting|Days");
                    AddInlineButton("Вернуться к главному меню", "0|ToMain");
                    break;

                case ButtonsSet.SettingDays:
                    AddInlineButton("Добавить новые дни в цикл", "3|Add|Day");
                    AddInlineButton("Настройка существующих дней", "3|SettingExisting|Days");
                    AddInlineButton("Вернуться к главному меню", "0|ToMain");
                    break;

                case ButtonsSet.DaysList:
                    GetDaysInButtons(CurrentUserContext.DataManager.CurrentCycle.Days.Where(d => !d.IsArchive), ActionForList.Select);
                    break;

                case ButtonsSet.SettingDay:
                    AddInlineButton("Сменить название", "3|ChangeName|Day");
                    AddInlineButton("Перенести день", "3|Replace|Day");
                    AddInlineButton("Добавить в архив", "3|Archiving|Day");
                    AddInlineButton("Удалить", "3|Delete|Day");
                    AddInlineButton("Настройка упражнений", "3|Setting|Exercises");
                    AddInlineButton("Вернуться к главному меню", "0|ToMain");
                    break;

                case ButtonsSet.ReplaceToCycle:
                    GetCyclesInButtons(CurrentUserContext.UserInformation.Cycles.Where(c => !c.IsArchive), ActionForList.Move);
                    break;

                case ButtonsSet.SettingExercises:
                    AddInlineButton("Добавить новые упражнения в день", "3|Add|Exercise");
                    AddInlineButton("Настройка существующих упражнений", "3|SettingExisting|Exercises");
                    AddInlineButton("Вернуться к главному меню", "0|ToMain");
                    break;

                case ButtonsSet.ExercisesList:
                    GetExercisesInButtons(CurrentUserContext.DataManager.CurrentDay.Exercises.Where(e => !e.IsArchive), ActionForList.Select);
                    break;

                case ButtonsSet.SettingExercise:
                    AddInlineButton("Сменить название", "3|ChangeName|Exercise");
                    AddInlineButton("Перенести упражнение", "3|Replace|Exercise");
                    AddInlineButton("Добавить в архив", $"3|Archiving|Exercise");
                    AddInlineButton("Удалить", "3|Delete|Exercise");
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
                    AddInlineButton("Добавить цикл", "3|Add|Cycle");
                    break;
                case ButtonsSet.AddDays:
                    AddInlineButton($"Добавить дни", "3|Add|Day");
                    break;
                case ButtonsSet.AddExercises:
                    AddInlineButton($"Добавить упражнеия", "3|Add|Exercise");
                    break;
                case ButtonsSet.SaveExercises:
                    AddInlineButton($"Сохранить упражнения", "3|SaveExercises");
                    break;
                case ButtonsSet.RedirectAfterSaveExercise:
                    AddInlineButton($"Добавить новый день", "3|Add|Day");
                    AddInlineButton($"Перейти в главное меню", "0|ToMain");
                    break;
                #endregion

                #region Archiving area
                case ButtonsSet.ArchiveList:
                    AddInlineButton("Архивированные циклы", "3|Archive|Cycles");
                    AddInlineButton("Архивированные дни", "3|Archive|Days");
                    AddInlineButton("Архивированные упражнения", "3|Archive|Exercises");
                    break;
                case ButtonsSet.ArchiveCyclesList:
                    GetCyclesInButtons(CurrentUserContext.UserInformation.Cycles.Where(c => c.IsArchive), ActionForList.UnArchive);
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
                    AddInlineButton("Да, удалить упражнение", "3|ConfirmDelete|Exercise");
                    break;

                case ButtonsSet.ConfirmDeleteDay:
                    AddInlineButton("Да, удалить день", "3|ConfirmDelete|Day");
                    break;

                case ButtonsSet.ConfirmDeleteCycle:
                    AddInlineButton("Да, удалить цикл", "3|ConfirmDelete|Cycle");
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
                        AddInlineButton(cycle.Name, $"3|Selected|Cycle|{cycle.Id}");
                    }
                    break;
                case ActionForList.UnArchive:
                    foreach (var cycle in source)
                    {
                        AddInlineButton(cycle.Name, $"3|UnArchive|Cycle|{cycle.Id}");
                    }
                    break;
                case ActionForList.Move:
                    foreach (var cycle in source)
                    {
                        AddInlineButton(cycle.Name, $"3|ReplaceTo|Cycle|{cycle.Id}|{cycle.Name}");
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
                        AddInlineButton(day.Name, $"3|Selected|Day|{day.Id}");
                    }
                    break;
                case ActionForList.UnArchive:
                    foreach (var day in source)
                    {
                        AddInlineButton(day.Name, $"3|UnArchive|Day|{day.Id}");
                    }
                    break;
                case ActionForList.Move:
                    foreach (var day in source)
                    {
                        AddInlineButton(day.Name, $"3|ReplaceTo|Day|{day.Id}|{day.Name}");
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
                        AddInlineButton(exercise.Name, $"3|Selected|Exercise|{exercise.Id}");
                    }
                    break;
                case ActionForList.UnArchive:
                    foreach (var exercise in source)
                    {
                        AddInlineButton(exercise.Name, $"3|UnArchive|Exercise|{exercise.Id}");
                    }
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный actionForList: {actionForList}");
            }
        }
    }
}