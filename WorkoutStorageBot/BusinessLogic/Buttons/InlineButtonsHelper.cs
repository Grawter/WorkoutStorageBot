#region using

using Telegram.Bot.Types.ReplyMarkups;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.Extenions;
using WorkoutStorageBot.Helpers.Crypto;
using WorkoutStorageBot.Model.Domain;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons
{
    internal class InlineButtonsHelper
    {
        private List<List<InlineKeyboardButton>> inlineKeyboardButtonsMain;
        private List<InlineKeyboardButton> inlineKeyboardButtons;

        private UserContext CurrentUserContext { get; }

        internal InlineButtonsHelper(UserContext userContext)
        {
            CurrentUserContext = userContext;

            CurrentUserContext.CallBackId = CryptographyHelper.CreateRandomCallBackQueryId();

            inlineKeyboardButtonsMain = new();
        }

        internal ReplyMarkup GetInlineButtons(ButtonsSet buttonsSet, Dictionary<string, string> additionalParameters = null)
        {
            return new InlineKeyboardMarkup(GetButtons(buttonsSet, ButtonsSet.None, additionalParameters));
        }

        internal ReplyMarkup GetInlineButtons((ButtonsSet buttonsSet, ButtonsSet backButtonsSet) buttonsSets, Dictionary<string, string> additionalParameters = null)
        {
            return new InlineKeyboardMarkup(GetButtons(buttonsSets.buttonsSet, buttonsSets.backButtonsSet, additionalParameters));
        }

        private IEnumerable<IEnumerable<InlineKeyboardButton>> GetButtons(ButtonsSet buttonsSet, ButtonsSet backButtonsSet, Dictionary<string, string> additionalParameters)
        {
            switch (buttonsSet)
            {
                case ButtonsSet.Main:
                    AddInlineButton($"Начать тренировку", "1|Workout");
                    AddInlineButton($"Настройки", "2|Settings");

                    if (CurrentUserContext.Roles == Roles.Admin)
                        AddInlineButton("Админка", "3|Admin");

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
                    AddInlineButton("Настройка тренировочных циклов", "2|Setting|Cycles");
                    AddInlineButton("Архив", "2|ArchiveStore");
                    AddInlineButton("Экспорт тренировок", "2|Export");
                    AddInlineButton("Удалить свой аккаунт", "2|Delete|Account");
                    break;

                case ButtonsSet.SettingCycles:
                    AddInlineButton("Добавить новый цикл", "2|Add|Cycle");
                    AddInlineButton("Настройка существующих циклов", "2|SettingExisting|Cycles");
                    AddInlineButton("Вернуться к главному меню", "0|ToMain");
                    break;

                case ButtonsSet.CycleList:
                    GetDomainsInButtons(CurrentUserContext.UserInformation.Cycles.Where(c => !c.IsArchive), "Selected");
                    break;

                case ButtonsSet.SettingCycle:
                    AddInlineButton("Сделать активным", "2|ChangeActive|Cycle");
                    AddInlineButton("Сменить название", "2|ChangeName|Cycle");
                    AddInlineButton("Добавить в архив", "2|Archiving|Cycle");
                    AddInlineButton("Удалить", "2|Delete|Cycle");
                    AddInlineButton("Настройка дней", "2|Setting|Days");
                    AddInlineButton("Вернуться к главному меню", "0|ToMain");
                    break;

                case ButtonsSet.SettingDays:
                    AddInlineButton("Добавить новый день в цикл", "2|Add|Day");
                    AddInlineButton("Настройка существующих дней", "2|SettingExisting|Days");
                    AddInlineButton("Вернуться к главному меню", "0|ToMain");
                    break;

                case ButtonsSet.DaysList:
                    GetDomainsInButtons(CurrentUserContext.DataManager.CurrentCycle.Days.Where(d => !d.IsArchive), "Selected");
                    break;

                case ButtonsSet.SettingDay:
                    AddInlineButton("Сменить название", "2|ChangeName|Day");
                    AddInlineButton("Перенести день", "2|Replace|Day");
                    AddInlineButton("Добавить в архив", "2|Archiving|Day");
                    AddInlineButton("Удалить", "2|Delete|Day");
                    AddInlineButton("Настройка упражнений", "2|Setting|Exercises");
                    AddInlineButton("Вернуться к главному меню", "0|ToMain");
                    break;

                case ButtonsSet.ReplaceToCycle:
                    GetDomainsInButtons(CurrentUserContext.UserInformation.Cycles.Where(c => !c.IsArchive), "ReplaceTo");
                    break;

                case ButtonsSet.SettingExercises:
                    AddInlineButton("Добавить новые упражнения в день", "2|Add|Exercise");
                    AddInlineButton("Настройка существующих упражнений", "2|SettingExisting|Exercises");
                    AddInlineButton("Вернуться к главному меню", "0|ToMain");
                    break;

                case ButtonsSet.ExercisesList:
                    GetDomainsInButtons(CurrentUserContext.DataManager.CurrentDay.Exercises.Where(e => !e.IsArchive), "Selected");
                    break;

                case ButtonsSet.SettingExercise:
                    AddInlineButton("Сменить название", "2|ChangeName|Exercise");
                    AddInlineButton("Сменить тип", "2|ChangeMode|Exercise");
                    AddInlineButton("Перенести упражнение", "2|Replace|Exercise");
                    AddInlineButton("Добавить в архив", $"2|Archiving|Exercise");
                    AddInlineButton("Удалить", "2|Delete|Exercise");
                    AddInlineButton("Вернуться к главному меню", "0|ToMain");
                    break;

                case ButtonsSet.ChangeType:
                    AddInlineButton("Режим только повторения", "2|ChangedMode|Exercise|||0");
                    AddInlineButton("Режим повторения и вес", "2|ChangedMode|Exercise|||1");
                    AddInlineButton("Режим свободного формата результата", "2|ChangedMode|Exercise|||2");
                    break;

                case ButtonsSet.ReplaceToDay:
                    foreach (Cycle cycle in CurrentUserContext.UserInformation.Cycles.Where(c => !c.IsArchive))
                    {
                        GetDomainsInButtons(cycle.Days.Where(c => !c.IsArchive), "ReplaceTo");
                    }
                    break;

                #region Full adding cycle area

                case ButtonsSet.AddCycle:
                    AddInlineButton("Добавить цикл", "2|Add|Cycle");
                    break;
                case ButtonsSet.AddDays:
                    AddInlineButton($"Добавить дни", "2|Add|Day");
                    break;
                case ButtonsSet.AddExercises:
                    AddInlineButton($"Добавить упражнения", "2|Add|Exercise");
                    break;
                case ButtonsSet.SaveExercises:
                    AddInlineButton($"Сохранить упражнения", "2|SaveExercises");
                    break;
                case ButtonsSet.RedirectAfterSaveExercise:
                    AddInlineButton($"Добавить новый день", "2|Add|Day");
                    AddInlineButton($"Перейти в главное меню", "0|ToMain");
                    break;

                #endregion

                #region Archiving area

                case ButtonsSet.ArchiveList:
                    AddInlineButton("Архивированные циклы", "2|Archive|Cycles");
                    AddInlineButton("Архивированные дни", "2|Archive|Days");
                    AddInlineButton("Архивированные упражнения", "2|Archive|Exercises");
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
                    AddInlineButton("Экспортировать тренировки в Excel", "2|ExportTo|Excel");
                    AddInlineButton("Экспортировать тренировки в JSON", "2|ExportTo|JSON");
                    break;

                #endregion

                #region Period area

                case ButtonsSet.Period:
                    AddInlineButton("Месяц", $"2|Period||||{additionalParameters["Act"]}|1");
                    AddInlineButton("Квартал", $"2|Period||||{additionalParameters["Act"]}|3");
                    AddInlineButton("Полгода", $"2|Period||||{additionalParameters["Act"]}|6");
                    AddInlineButton("Год", $"2|Period||||{additionalParameters["Act"]}|12");
                    AddInlineButton("За всё время", $"2|Period||||{additionalParameters["Act"]}|0");
                    break;
                #endregion

                #region Confirm delete area

                case ButtonsSet.ConfirmDelete:
                    AddInlineButton($"Да, удалить {additionalParameters["Name"]}", $"2|ConfirmDelete|{additionalParameters["DomainType"]}");
                    break;
                case ButtonsSet.None:
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный buttonsSet: {buttonsSet}");

                #endregion

                #endregion

                #region Admin area

                case ButtonsSet.Admin:

                    if (CurrentUserContext.Roles == Roles.Admin)
                    {
                        AddInlineButton("Логи", "3|Logs");
                        AddInlineButton("Показать стартовую настройку", "3|ShowStartConfiguration");
                        AddInlineButton("Сменить режим использования лимитов", "3|ChangeLimitsMods");
                        AddInlineButton("Сменить режим белого списка", "3|ChangeWhiteListMode");
                        AddInlineButton("Сменить white/black list у пользователя", "3|ChangeUserState");
                        AddInlineButton("Удалить пользователя", "3|RemoveUser");
                        AddInlineButton("Выключить бота", "3|DisableBot");
                    }
                        
                    break;

                case ButtonsSet.AdminLogs:
                    
                    if (CurrentUserContext.Roles == Roles.Admin)
                    {
                        AddInlineButton("Показать последний лог", "3|ShowLastLog");
                        AddInlineButton("Показать последние логи ошибок", "3|ShowLastExceptionLogs");
                        AddInlineButton("Найти лог по ID", "3|FindLogByID");
                        AddInlineButton("Найти лог по eventID", "3|FindLogByEventID");
                    }
                        
                    break;

                #endregion
            }

            if (backButtonsSet != ButtonsSet.None)
                AddInlineButton("Назад", $"0|Back|{backButtonsSet}");

            return inlineKeyboardButtonsMain;
        }

        private void AddInlineButton(string titleButton, string callBackDataWithoutId, bool onNewLine = true)
        {
            string callBackData = AddCallBackId(callBackDataWithoutId);

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

        private string AddCallBackId(string callBackDataWithoutId)
        {
            callBackDataWithoutId += $"|{CurrentUserContext.CallBackId}";

            return callBackDataWithoutId;
        }

        private void GetDomainsInButtons(IEnumerable<IDomain> source, string subDirection)
        {
            if (source.HasItemsInCollection())
            {
                string domainType = CurrentUserContext.DataManager.GetDomainType(source.First()).ToString();

                foreach (IDomain domain in source)
                {
                    AddInlineButton(domain.Name, $"2|{subDirection}|{domainType}|{domain.Id}");
                }
            }
        }
    }
}