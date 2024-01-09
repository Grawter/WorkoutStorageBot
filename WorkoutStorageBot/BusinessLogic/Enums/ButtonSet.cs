

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
                SettingCycle,
                    CycleList,
                        ConfirmDeleteCycle,
                        SettingDays,
                            AddDays,
                            SettingDay,
                                DaysList,
                                    ConfirmDeleteDay,
                                    SettingExercises,
                                        AddExercises,
                                            SaveAddedExercise,
                                                 RedirectAfterSaveExercise,
                                        SettingExercise,
                                            ExercisesList,
                                                ConfirmDeleteExercise,
            ConfirmDeleteAccount,
    }
}