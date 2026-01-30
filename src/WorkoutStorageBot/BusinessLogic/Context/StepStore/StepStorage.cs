using WorkoutStorageBot.BusinessLogic.Enums;

namespace WorkoutStorageBot.BusinessLogic.Context.StepStore
{
    internal static class StepStorage
    {
        private static readonly IReadOnlyDictionary<ButtonsSet, StepInformation> stepsInformation = new Dictionary<ButtonsSet, StepInformation>()
        { 
            #region workout area

            { ButtonsSet.Main, new StepInformation(QueryFrom.NoMatter, "Выберите интересующий раздел", ButtonsSet.Main, ButtonsSet.None) },
            { ButtonsSet.DaysListWithLastWorkout, new StepInformation(QueryFrom.NoMatter, "Выберите тренировочный день", ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main) },
            { ButtonsSet.ExercisesListWithLastWorkoutForDay, new StepInformation(QueryFrom.NoMatter, "Выберите упражнение", ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout) },

            #endregion

            #region settings area

            { ButtonsSet.Settings, new StepInformation(QueryFrom.Settings, "Выберите интересующие настройки", ButtonsSet.Settings, ButtonsSet.Main) },

            { ButtonsSet.SettingCycles, new StepInformation(QueryFrom.Settings, "Выберите интересующие настройки для циклов", ButtonsSet.SettingCycles, ButtonsSet.Settings) },
            { ButtonsSet.ArchiveList, new StepInformation(QueryFrom.Settings, "Выберите интересующий архив для разархивирования", ButtonsSet.ArchiveList, ButtonsSet.Settings) },

            { ButtonsSet.CycleList, new StepInformation(QueryFrom.Settings, "Выберите интересующий цикл", ButtonsSet.CycleList, ButtonsSet.SettingCycles) },
            { ButtonsSet.SettingCycle, new StepInformation(QueryFrom.Settings, "Выберите интересующую настройку для указанного цикла", ButtonsSet.SettingCycle, ButtonsSet.CycleList) },

            { ButtonsSet.SettingDays, new StepInformation(QueryFrom.Settings, "Выберите интересующие настройки для дней", ButtonsSet.SettingDays, ButtonsSet.SettingCycle) },
            { ButtonsSet.DaysList, new StepInformation(QueryFrom.Settings, "Выберите интересующий день", ButtonsSet.DaysList, ButtonsSet.SettingDays) },
            { ButtonsSet.SettingDay, new StepInformation(QueryFrom.Settings, "Выберите интересующую настройку для указанного дня", ButtonsSet.SettingDay, ButtonsSet.DaysList) },

            { ButtonsSet.SettingExercises, new StepInformation(QueryFrom.Settings, "Выберите интересующие настройки для упражнений", ButtonsSet.SettingExercises, ButtonsSet.SettingDay) },
            { ButtonsSet.ExercisesList, new StepInformation(QueryFrom.Settings, "Выберите интересующее упражнение", ButtonsSet.ExercisesList, ButtonsSet.SettingExercises) } ,
            { ButtonsSet.SettingExercise, new StepInformation(QueryFrom.Settings, "Выберите интересующую настройку для указанного упражнения", ButtonsSet.SettingExercise, ButtonsSet.ExercisesList) },

            #endregion

            #region admin area

            { ButtonsSet.Admin, new StepInformation(QueryFrom.NoMatter, "Выберите интересующее действие", ButtonsSet.Admin, ButtonsSet.Main) },
            { ButtonsSet.AdminLogs, new StepInformation(QueryFrom.NoMatter, "Выберите интересующее действие", ButtonsSet.AdminLogs, ButtonsSet.Admin) },
            { ButtonsSet.AdminUsers, new StepInformation(QueryFrom.NoMatter, "Выберите интересующее действие", ButtonsSet.AdminUsers, ButtonsSet.Admin) },

            #endregion 
        };

        internal static StepInformation GetStep(ButtonsSet buttonsSet)
            => stepsInformation.GetValueOrDefault(buttonsSet) ?? GetMainStep();

        internal static StepInformation GetMainStep()
            => stepsInformation.First().Value;
    }
}