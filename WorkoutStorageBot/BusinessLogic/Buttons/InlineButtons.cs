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

        internal IReplyMarkup GetInlineButtons(ButtonsSet buttonsSet, Dictionary<string, string> additionalParameters = null)
        {
            return new InlineKeyboardMarkup(GetButtons(buttonsSet, ButtonsSet.None, additionalParameters));
        }

        internal IReplyMarkup GetInlineButtons((ButtonsSet buttonsSet, ButtonsSet backButtonsSet) buttonsSets, Dictionary<string, string> additionalParameters = null)
        {
            return new InlineKeyboardMarkup(GetButtons(buttonsSets.buttonsSet, buttonsSets.backButtonsSet, additionalParameters));
        }

        private IEnumerable<IEnumerable<InlineKeyboardButton>> GetButtons(ButtonsSet buttonsSet, ButtonsSet backButtonsSet, Dictionary<string, string> additionalParameters)
        {
            switch (buttonsSet)
            {
                case ButtonsSet.Main:
                    AddInlineButton($"Начать тренировку", "1|Workout");
                    AddInlineButton($"Настройки", "3|Settings");
                    break;

                #region Workout area
                case ButtonsSet.DaysListWithLastWorkout:
                    AddInlineButton("Последняя тренировка", "1|LastResult|Exercises");
                    GetDomainsInButtons(CurrentUserContext.ActiveCycle.Days.Where(d => !d.IsArchive), "Selected");
                    break;

                case ButtonsSet.ExercisesListWithLastWorkoutForDay:
                    AddInlineButton("Последние результаты выбранного дня", $"1|LastResult|Day");
                    GetDomainsInButtons(CurrentUserContext.DataManager.CurrentDay.Exercises.Where(e => !e.IsArchive), "Selected");
                    break;

                case ButtonsSet.SaveResultsExercise:
                    AddInlineButton("Сохранить результаты", "1|SaveResultsExercise");
                    break;
                #endregion

                #region Settings area
                case ButtonsSet.Settings:
                    AddInlineButton("Настройка тренировочных циклов", "3|Setting|Cycles");
                    AddInlineButton("Архив", "3|ArchiveStore");
                    AddInlineButton("Экспорт тренировок", "3|Export");
                    AddInlineButton("Удалить свой аккаунт", "3|Delete|Account");
                    break;

                case ButtonsSet.SettingCycles:
                    AddInlineButton("Добавить новый цикл", "3|Add|Cycle");
                    AddInlineButton("Настройка существующих циклов", "3|SettingExisting|Cycles");
                    AddInlineButton("Вернуться к главному меню", "0|ToMain");
                    break;

                case ButtonsSet.CycleList:
                    GetDomainsInButtons(CurrentUserContext.UserInformation.Cycles.Where(c => !c.IsArchive), "Selected");
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
                    GetDomainsInButtons(CurrentUserContext.DataManager.CurrentCycle.Days.Where(d => !d.IsArchive), "Selected");
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
                    GetDomainsInButtons(CurrentUserContext.UserInformation.Cycles.Where(c => !c.IsArchive), "ReplaceTo");
                    break;

                case ButtonsSet.SettingExercises:
                    AddInlineButton("Добавить новые упражнения в день", "3|Add|Exercise");
                    AddInlineButton("Настройка существующих упражнений", "3|SettingExisting|Exercises");
                    AddInlineButton("Вернуться к главному меню", "0|ToMain");
                    break;

                case ButtonsSet.ExercisesList:
                    GetDomainsInButtons(CurrentUserContext.DataManager.CurrentDay.Exercises.Where(e => !e.IsArchive), "Selected");
                    break;

                case ButtonsSet.SettingExercise:
                    AddInlineButton("Сменить название", "3|ChangeName|Exercise");
                    AddInlineButton("Перенести упражнение", "3|Replace|Exercise");
                    AddInlineButton("Добавить в архив", $"3|Archiving|Exercise");
                    AddInlineButton("Удалить", "3|Delete|Exercise");
                    AddInlineButton("Вернуться к главному меню", "0|ToMain");
                    break;

                case ButtonsSet.ReplaceToDay:
                    foreach (Cycle cycle in CurrentUserContext.UserInformation.Cycles.Where(c => !c.IsArchive))
                    {
                        GetDomainsInButtons(cycle.Days.Where(c => !c.IsArchive), "ReplaceTo");
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
                    GetDomainsInButtons(CurrentUserContext.UserInformation.Cycles.Where(c => c.IsArchive), "UnArchive");
                    break;
                case ButtonsSet.ArchiveDaysList:
                    foreach(Cycle cycle in CurrentUserContext.UserInformation.Cycles.Where(c => !c.IsArchive))
                    {
                        GetDomainsInButtons(cycle.Days.Where(d => d.IsArchive), "UnArchive");
                    }
                    break;
                case ButtonsSet.ArchiveExercisesList:
                    foreach (Cycle cycle in CurrentUserContext.UserInformation.Cycles.Where(c => !c.IsArchive))
                    {
                        foreach (Day day in cycle.Days.Where(c => !c.IsArchive))
                        {
                            GetDomainsInButtons(day.Exercises.Where(e => e.IsArchive), "UnArchive");
                        }
                    }
                    break;
                #endregion

                #region Export area
                case ButtonsSet.Export:
                    AddInlineButton("Экспортировать тренировки в Excel", "3|ExportTo|Excel");
                    AddInlineButton("Экспортировать тренировки в JSON", "3|ExportTo|JSON");
                    break;

                #endregion

                #region Period area

                case ButtonsSet.Period:
                    AddInlineButton("Месяц", $"3|Period||||{additionalParameters["Act"]}|1");
                    AddInlineButton("Квартал", $"3|Period||||{additionalParameters["Act"]}|3");
                    AddInlineButton("Полгода", $"3|Period||||{additionalParameters["Act"]}|6");
                    AddInlineButton("Год", $"3|Period||||{additionalParameters["Act"]}|12");
                    AddInlineButton("За всё время", $"3|Period||||{additionalParameters["Act"]}|0");
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
            string callBackData = AddCallBackSetId(callBackDataWithoutSetId);

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
            StringBuilder sb = new StringBuilder(callBackData);

            sb.Append("|");
            sb.Append(CurrentUserContext.CallBackId);

            return sb.ToString();
        }

        private void GetDomainsInButtons(IEnumerable<IDomain> source, string subDirection)
        {
            string domainType = CurrentUserContext.DataManager.GetDomainType(source.First()).ToString();

            foreach (IDomain domain in source)
            {
                AddInlineButton(domain.Name, $"3|{subDirection}|{domainType}|{domain.Id}");
            }
        }
    }
}