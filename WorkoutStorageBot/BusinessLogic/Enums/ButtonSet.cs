

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
                                    ArchiveDaysList,
                                    ConfirmDeleteDay,
                                    SettingExercises,
                                        AddExercises,
                                            SaveAddedExercise,
                                                 RedirectAfterSaveExercise,
                                        ExercisesList,
                                            SettingExercise,
                                                ArchiveExercisesList,
                                                ConfirmDeleteExercise,
            ArchiveList,
            ConfirmDeleteAccount,
    }
}