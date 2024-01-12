

namespace WorkoutStorageBot.BusinessLogic.Enums
{
    internal enum ButtonsSet
    {
        None,

        Main,
            DaysListWithLastWorkout,
            ExercisesListWithLastWorkoutForDay,
            SaveResultsExercise,

        Settings,
            SettingCycles,
                AddCycle,
                CycleList, 
                    SettingCycle,
                        ArchiveCyclesList,
                        ConfirmDeleteCycle,
                        SettingDays,
                            AddDays,
                            DaysList,
                                SettingDay,
                                    ReplaceToCycle,
                                    ArchiveDaysList,
                                    ConfirmDeleteDay,
                                    SettingExercises,
                                        AddExercises,
                                            SaveExercises,
                                                 RedirectAfterSaveExercise,
                                        ExercisesList,
                                            SettingExercise,
                                                ReplaceToDay,
                                                ArchiveExercisesList,
                                                ConfirmDeleteExercise,
            ArchiveList,
            ConfirmDeleteAccount,
    }
}