using System.Collections.Immutable;
using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Storage
{
    internal static class ButtonsStorage
    {
        private readonly static IReadOnlyDictionary<ButtonsSet, Func<UserContext, ButtonsFactory>> buttonsStorage
           = new Dictionary<ButtonsSet, Func<UserContext, ButtonsFactory>>
           {
               { ButtonsSet.Main, (x) => new MainBF(x) },

               #region Workout area

               {  ButtonsSet.FoundResultsByDate, (x) => new FoundResultsByDateBF(x) },
               {  ButtonsSet.DaysListWithLastWorkout, (x) => new DaysListWithLastWorkoutBF(x) },
               {  ButtonsSet.ExercisesListWithLastWorkoutForDay, (x) => new ExercisesListWithLastWorkoutForDayBF(x) },

               #region Timer area

               {  ButtonsSet.EnableExerciseTimer, (x) => new EnableExerciseTimerBF(x) },
               {  ButtonsSet.FixExerciseTimer, (x) => new FixExerciseTimerBF(x) },

               #endregion

               {  ButtonsSet.ResetResultsExercise, (x) => new ResetResultsExerciseBF(x) },
               {  ButtonsSet.SaveResultsExercise, (x) => new SaveResultsExerciseBF(x) },

               #endregion

               #region Settings area

               {  ButtonsSet.Settings, (x) => new SettingsBF(x) },
               {  ButtonsSet.SettingCycles, (x) => new SettingCyclesBF(x) },
               {  ButtonsSet.CycleList, (x) => new CycleListBF(x) },
               {  ButtonsSet.SettingCycle, (x) => new SettingCycleBF(x) },
               {  ButtonsSet.SettingDays, (x) => new SettingDaysBF(x) },
               {  ButtonsSet.DaysList, (x) => new DaysListBF(x) },
               {  ButtonsSet.SettingDay, (x) => new SettingDayBF(x) },
               {  ButtonsSet.ReplaceToCycle, (x) => new ReplaceToCycleBF(x) },
               {  ButtonsSet.SettingExercises, (x) => new SettingExercisesBF(x) },
               {  ButtonsSet.ExercisesList, (x) => new ExercisesListBF(x) },
               {  ButtonsSet.SettingExercise, (x) => new SettingExerciseBF(x) },
               {  ButtonsSet.ChangeType, (x) => new ChangeTypeBF(x) },
               {  ButtonsSet.ReplaceToDay, (x) => new ReplaceToDayBF(x) },

               #region Full adding cycle area

               {  ButtonsSet.AddCycle, (x) => new AddCycleBF(x) },
               {  ButtonsSet.AddDays, (x) => new AddDaysBF(x) },
               {  ButtonsSet.AddExercises, (x) => new AddExercisesBF(x) },
               {  ButtonsSet.ResetTempDomains, (x) => new ResetTempDomainsBF(x) },
               {  ButtonsSet.SaveExercises, (x) => new SaveExercisesBF(x) },
               {  ButtonsSet.RedirectAfterSaveExercise, (x) => new RedirectAfterSaveExerciseBF(x) },

               #endregion

               #region Archiving area

               {  ButtonsSet.ArchiveList, (x) => new ArchiveListBF(x) },
               {  ButtonsSet.ArchiveCyclesList, (x) => new ArchiveCyclesListBF(x) },
               {  ButtonsSet.ArchiveDaysList, (x) => new ArchiveDaysListBF(x) },
               {  ButtonsSet.ArchiveExercisesList, (x) => new ArchiveExercisesListBF(x) },

               #endregion

               #region Export area

               {  ButtonsSet.Export, (x) => new ExportBF(x) },

               #endregion

               #region Period area

               {  ButtonsSet.Period, (x) => new PeriodBF(x) },

               #endregion

               #region Confirm delete area
            
               {  ButtonsSet.ConfirmDelete, (x) => new ConfirmDeleteBF(x) },

               #endregion

               #endregion

               #region Admin area

               {  ButtonsSet.Admin, (x) => new AdminBF(x) },
               {  ButtonsSet.AdminLogs, (x) => new AdminLogsBF(x) },
               {  ButtonsSet.AdminUsers, (x) => new AdminUsersBF(x) },

               #endregion

               {  ButtonsSet.None, (x) => new EmptyBF(x) },
           };

        internal static ButtonsFactory GetButtonsFactory(ButtonsSet buttonsSet, UserContext userContext)
        {
            Func<UserContext, ButtonsFactory> func = buttonsStorage.GetValueOrDefault(buttonsSet)
                ?? throw new NotImplementedException($"Неожиданный buttonsSet: {buttonsSet}");

            return func(userContext);
        }
    }
}