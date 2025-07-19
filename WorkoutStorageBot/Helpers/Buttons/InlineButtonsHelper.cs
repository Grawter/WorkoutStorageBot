#region using

using Telegram.Bot.Types.ReplyMarkups;
using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.SessionContext;

#endregion

namespace WorkoutStorageBot.Helpers.Buttons
{
    internal class InlineButtonsHelper
    {
        private UserContext CurrentUserContext { get; }

        internal InlineButtonsHelper(UserContext userContext)
        {
            CurrentUserContext = userContext;
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
            ButtonsFactory buttonsFactory = GetButtonsFactory(buttonsSet);

            return buttonsFactory.Create(backButtonsSet, additionalParameters);
        }

        private ButtonsFactory GetButtonsFactory(ButtonsSet buttonsSet)
        {
            ButtonsFactory buttonsFactory;

            switch (buttonsSet)
            {
                case ButtonsSet.Main:
                    buttonsFactory = new MainBF(CurrentUserContext);
                    break;

                #region Workout area

                case ButtonsSet.DaysListWithLastWorkout:
                    buttonsFactory = new DaysListWithLastWorkoutBF(CurrentUserContext);
                    break;

                case ButtonsSet.ExercisesListWithLastWorkoutForDay:
                    buttonsFactory = new ExercisesListWithLastWorkoutForDayBF(CurrentUserContext);
                    break;

                case ButtonsSet.SaveResultsExercise:
                    buttonsFactory = new SaveResultsExerciseBF(CurrentUserContext);
                    break;

                #endregion

                #region Settings area

                case ButtonsSet.Settings:
                    buttonsFactory = new SettingsBF(CurrentUserContext);
                    break;

                case ButtonsSet.SettingCycles:
                    buttonsFactory = new SettingCyclesBF(CurrentUserContext);
                    break;

                case ButtonsSet.CycleList:
                    buttonsFactory = new CycleListBF(CurrentUserContext);
                    break;

                case ButtonsSet.SettingCycle:
                    buttonsFactory = new SettingCycleBF(CurrentUserContext);
                    break;

                case ButtonsSet.SettingDays:
                    buttonsFactory = new SettingDaysBF(CurrentUserContext);
                    break;

                case ButtonsSet.DaysList:
                    buttonsFactory = new DaysListBF(CurrentUserContext);
                    break;

                case ButtonsSet.SettingDay:
                    buttonsFactory = new SettingDayBF(CurrentUserContext);
                    break;

                case ButtonsSet.ReplaceToCycle:
                    buttonsFactory = new ReplaceToCycleBF(CurrentUserContext);
                    break;

                case ButtonsSet.SettingExercises:
                    buttonsFactory = new SettingExercisesBF(CurrentUserContext);
                    break;

                case ButtonsSet.ExercisesList:
                    buttonsFactory = new ExercisesListBF(CurrentUserContext); 
                    break;

                case ButtonsSet.SettingExercise:
                    buttonsFactory = new SettingExerciseBF(CurrentUserContext);
                    break;

                case ButtonsSet.ChangeType:
                    buttonsFactory = new ChangeTypeBF(CurrentUserContext);
                    break;

                case ButtonsSet.ReplaceToDay:
                    buttonsFactory = new ReplaceToDayBF(CurrentUserContext);
                    break;

                #region Full adding cycle area

                case ButtonsSet.AddCycle:
                    buttonsFactory = new AddCycleBF(CurrentUserContext);
                    break;
                case ButtonsSet.AddDays:
                    buttonsFactory = new AddDaysBF(CurrentUserContext);
                    break;
                case ButtonsSet.AddExercises:
                    buttonsFactory = new AddExercisesBF(CurrentUserContext);
                    break;
                case ButtonsSet.SaveExercises:
                    buttonsFactory = new SaveExercisesBF(CurrentUserContext);
                    break;
                case ButtonsSet.RedirectAfterSaveExercise:
                    buttonsFactory = new RedirectAfterSaveExerciseBF(CurrentUserContext); 
                    break;

                #endregion

                #region Archiving area

                case ButtonsSet.ArchiveList:
                    buttonsFactory = new ArchiveListBF(CurrentUserContext);
                    break;
                case ButtonsSet.ArchiveCyclesList:
                    buttonsFactory = new ArchiveCyclesListBF(CurrentUserContext);
                    break;
                case ButtonsSet.ArchiveDaysList:
                    buttonsFactory = new ArchiveDaysListBF(CurrentUserContext);
                    break;
                case ButtonsSet.ArchiveExercisesList:
                    buttonsFactory = new ArchiveExercisesListBF(CurrentUserContext);
                    break;

                #endregion

                #region Export area

                case ButtonsSet.Export:
                    buttonsFactory = new ExportBF(CurrentUserContext);
                    break;

                #endregion

                #region Period area

                case ButtonsSet.Period:
                    buttonsFactory = new PeriodBF(CurrentUserContext);
                    break;
                #endregion

                #region Confirm delete area

                case ButtonsSet.ConfirmDelete:
                    buttonsFactory = new ConfirmDeleteBF(CurrentUserContext);
                    break;

                #endregion

                #endregion

                #region Admin area

                case ButtonsSet.Admin:
                    buttonsFactory = new AdminBF(CurrentUserContext);
                    break;

                case ButtonsSet.AdminLogs:
                    buttonsFactory = new AdminLogsBF(CurrentUserContext);
                    break;

                #endregion

                case ButtonsSet.None:
                    buttonsFactory = new EmptyBF(CurrentUserContext);
                    break;

                default:
                    throw new NotImplementedException($"Неожиданный buttonsSet: {buttonsSet}");
            }

            return buttonsFactory;
        }
    }
}