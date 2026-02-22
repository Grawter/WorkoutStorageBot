using WorkoutStorageBot.BusinessLogic.Enums;

namespace WorkoutStorageBot.BusinessLogic.Context.StepStore
{
    internal static class StepStorage
    {
        private static readonly IReadOnlyDictionary<ButtonsSet, StepInformation> stepsInformation = new Dictionary<ButtonsSet, StepInformation>()
        { 
            #region workout area

            { ButtonsSet.Main, new StepInformation(QueryFrom.NoMatter, "Выберите интересующий раздел", ButtonsSet.Main, ButtonsSet.None) },
            { ButtonsSet.DaysListWithLastWorkout, new StepInformation(QueryFrom.NoMatter, "Выберите тренировочный день из цикла", ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main) },
            { ButtonsSet.ExercisesListWithLastWorkoutForDay, new StepInformation(QueryFrom.NoMatter, "Выберите упражнение из дня", ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout) },

            #endregion

            #region settings area

            { ButtonsSet.Settings, new StepInformation(QueryFrom.Settings, "Выберите интересующие настройки", ButtonsSet.Settings, ButtonsSet.Main) },

            { ButtonsSet.ArchiveList, new StepInformation(QueryFrom.Settings, "Выберите интересующий архив для разархивирования", ButtonsSet.ArchiveList, ButtonsSet.Settings) },
            { ButtonsSet.Export, new StepInformation(QueryFrom.Settings, "Выберите формат в котором экспортировать данные о ваших тренировках", ButtonsSet.Export, ButtonsSet.Settings) },


            { ButtonsSet.SettingCycles, new StepInformation(QueryFrom.Settings, "Выберите интересующие настройки для циклов", ButtonsSet.SettingCycles, ButtonsSet.Settings) },
            { ButtonsSet.CycleList, new StepInformation(QueryFrom.Settings, "Выберите интересующий цикл", ButtonsSet.CycleList, ButtonsSet.SettingCycles) },
            { ButtonsSet.SettingCycle, new StepInformation(QueryFrom.Settings, "Выберите интересующую настройку для цикла", ButtonsSet.SettingCycle, ButtonsSet.CycleList) },

            { ButtonsSet.SettingDays, new StepInformation(QueryFrom.Settings, "Выберите интересующие настройки для дней из цикла", ButtonsSet.SettingDays, ButtonsSet.SettingCycle) },
            { ButtonsSet.DaysList, new StepInformation(QueryFrom.Settings, "Выберите интересующий день из цикла", ButtonsSet.DaysList, ButtonsSet.SettingDays) },
            { ButtonsSet.SettingDay, new StepInformation(QueryFrom.Settings, "Выберите интересующую настройку для дня", ButtonsSet.SettingDay, ButtonsSet.DaysList) },

            { ButtonsSet.SettingExercises, new StepInformation(QueryFrom.Settings, "Выберите интересующие настройки для упражнений из дня", ButtonsSet.SettingExercises, ButtonsSet.SettingDay) },
            { ButtonsSet.ExercisesList, new StepInformation(QueryFrom.Settings, "Выберите интересующее упражнение из дня", ButtonsSet.ExercisesList, ButtonsSet.SettingExercises) } ,
            { ButtonsSet.SettingExercise, new StepInformation(QueryFrom.Settings, "Выберите интересующую настройку для упражнения", ButtonsSet.SettingExercise, ButtonsSet.ExercisesList) },

            #endregion

            #region admin area

            { ButtonsSet.Admin, new StepInformation(QueryFrom.NoMatter, "Выберите интересующее действие с админкой", ButtonsSet.Admin, ButtonsSet.Main) },
            { ButtonsSet.AdminLogs, new StepInformation(QueryFrom.NoMatter, "Выберите интересующее действие с логами", ButtonsSet.AdminLogs, ButtonsSet.Admin) },
            { ButtonsSet.AdminUsers, new StepInformation(QueryFrom.NoMatter, "Выберите интересующее действие с пользователями", ButtonsSet.AdminUsers, ButtonsSet.Admin) },

            #endregion 
        };

        internal static StepInformation GetStep(ButtonsSet buttonsSet)
            => stepsInformation.GetValueOrDefault(buttonsSet) ?? GetMainStep();

        internal static StepInformation GetMainStep()
            => stepsInformation.First().Value;
    }
}