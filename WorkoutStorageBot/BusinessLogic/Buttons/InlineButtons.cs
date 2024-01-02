#region using
using System.Text;
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.BusinessLogic.Enums;
using Telegram.Bot.Types.ReplyMarkups;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons
{
    internal class InlineButtons
    {
        private UserContext CurrentUserContext { get; }

        private List<List<InlineKeyboardButton>> inlineKeyboardButtonsMain;
        private List<InlineKeyboardButton> inlineKeyboardButtons;

        internal InlineButtons(UserContext userNavigator)
        {
            CurrentUserContext = userNavigator;

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
                case ButtonsSet.StartSetCycle:
                    AddInlineButton("Установить цикл", "#SetCycle");
                    break;
                case ButtonsSet.SetCycle:
                    AddInlineButton($"Зафиксировать упражнения для дня № {CurrentUserContext.DataManager.NumberDay}", "#SwitchOnNewDay");
                    AddInlineButton("Cохранить цикл", "#SaveCycle");
                    break;
                case ButtonsSet.Main:
                    AddInlineButton($"Начать тренировку", "#StartWorkout");
                    AddInlineButton($"Аналитика", "#Analytics");
                    AddInlineButton($"Настройки", "#Settings");
                    break;
                case ButtonsSet.WorkoutDays:
                    GetWorkoutDaysInButtons();
                    break;
                case ButtonsSet.WorkoutExercises:
                    GetWorkoutExercisesInButtons();
                    break;
                case ButtonsSet.SaveResultForExercise:
                    AddInlineButton("Сохранить результаты", "#SaveResultForExercise");
                    break;
                default:
                    break;
            }

            if (backButtonsSet != ButtonsSet.None)
                AddInlineButton("Назад", $"#Back|{backButtonsSet}");

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
                                                    .Append(CurrentUserContext.CallBackSetId);
            return sb.ToString();
        }

        private void GetWorkoutDaysInButtons(bool showLastWorkoutButton = true)
        {
            if (showLastWorkoutButton)
                AddInlineButton("Последняя тренировка", "#LastWorkout");

            if (CurrentUserContext.Cycle != null) 
            {
                foreach (var day in CurrentUserContext.Cycle.Days)
                {
                    AddInlineButton(day.NameDay, $"#SelectedDay|{day.NameDay}|{day.Id}");
                }
            }
        }

        private void GetWorkoutExercisesInButtons(bool showLastResultForThisDayButton = true)
        {
            if (showLastResultForThisDayButton)
                AddInlineButton("Последние результаты выбранного дня", $"#GetLastResultForThisDay|{CurrentUserContext.DataManager.CurrentDay.NameDay}|{CurrentUserContext.DataManager.CurrentDay.Id}");

            if (CurrentUserContext.Cycle != null)
            {
                foreach (var exercise in CurrentUserContext.Cycle.Days.FirstOrDefault(d => d.Id == CurrentUserContext.DataManager.CurrentDay.Id).Exercises)
                {
                    AddInlineButton(exercise.NameExercise, $"#SetResultForExercise|{exercise.NameExercise}|{exercise.Id}");
                }
            }
        }
    }
}