

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
                        SettingDays,
                            AddDays,
                            DaysList,
                                SettingDay,
                                    ReplaceToCycle,
                                    ArchiveDaysList,
                                    SettingExercises,
                                        AddExercises,
                                            SaveExercises,
                                                 RedirectAfterSaveExercise,
                                        ExercisesList,
                                            SettingExercise,
                                                ReplaceToDay,
                                                ArchiveExercisesList,
                                                
            ArchiveList,
            Export,
            ConfirmDelete,

        Period,
        
        Admin,
            AdminLogs,
    }
}