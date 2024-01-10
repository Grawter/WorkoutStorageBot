

namespace WorkoutStorageBot.BusinessLogic.Enums
{
    internal enum ButtonsSet
    {
        None,

        Main,
            DaysListWithLastWorkout,
            ExercisesListWithLastWorkoutForDay,
            SaveResultForExercise,

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
                                            SaveAddedExercise,
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